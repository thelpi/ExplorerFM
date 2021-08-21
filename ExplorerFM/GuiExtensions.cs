using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ExplorerFM.FieldsAttributes;
using Xceed.Wpf.Toolkit;

namespace ExplorerFM
{
    internal static class GuiExtensions
    {
        // not my code
        public static void RemoveRoutedEventHandlers(
            this UIElement element,
            RoutedEvent routedEvent)
        {
            var eventHandlersStore = typeof(UIElement)
                .GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(element, null);

            if (eventHandlersStore != null)
            {
                var routedEventHandlers = (RoutedEventHandlerInfo[])eventHandlersStore
                    .GetType()
                    .GetMethod("GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Invoke(eventHandlersStore, new object[] { routedEvent });

                if (routedEventHandlers != null)
                {
                    foreach (var routedEventHandler in routedEventHandlers)
                        element.RemoveHandler(routedEvent, routedEventHandler.Handler);
                }
            }
        }

        public static T Find<T>(
            this FrameworkElement element,
            string name)
            where T : UIElement
        {
            return element.FindName(name) as T;
        }

        public static CommonNumericUpDown<T> WithMinMaxFromAttribute<T>(
            this CommonNumericUpDown<T> element,
            FieldAttribute attribute)
            where T : struct, IFormattable, IComparable<T>
        {
            element.Minimum = (T)Convert.ChangeType(attribute.Min, typeof(T));
            element.Maximum = (T)Convert.ChangeType(attribute.Max, typeof(T));
            return element;
        }

        public static T GetByTemplateKey<T>(
            this FrameworkElement window,
            string key)
            where T : UIElement
        {
            return (window.FindResource(key) as ControlTemplate).LoadContent() as T;
        }

        public static ComboBox WithBindingMaxFromAttribute(
            this ComboBox valueComboBox,
            SelectorFieldAttribute realAttribute,
            DataProvider dataProvider)
        {
            valueComboBox.ItemsSource = realAttribute.GetValuesFunc(dataProvider);
            if (realAttribute.HasDisplayPropertyName)
                valueComboBox.DisplayMemberPath = realAttribute.DisplayPropertyName;
            return valueComboBox;
        }

        public static CommonNumericUpDown<T> GetNumericUpDown<T>(
            this FrameworkElement element,
            string key,
            FieldAttribute attribute)
            where T : struct, IFormattable, IComparable<T>
        {
            return element.GetByTemplateKey<CommonNumericUpDown<T>>(key).WithMinMaxFromAttribute(attribute);
        }

        public static Color GetColorFromRate(int rate, int maxRate = 20)
        {
            var switchStop = maxRate / (decimal)3;

            var blue = rate > switchStop
                ? 0
                : 255 - (rate / switchStop * 255);

            var green = rate <= switchStop
                ? 255
                : 255 - ((rate - switchStop) / (switchStop * 2) * 255);

            return Color.FromArgb(byte.MaxValue, byte.MaxValue, (byte)green, (byte)blue);
        }

        public static void HideWorkAndDisplay<T>(this ProgressBar pgb,
            Func<T> backgroundFunc,
            Action<T> foregroundFunc,
            params UIElement[] uiElementsToHide)
        {
            foreach (var uiElem in uiElementsToHide)
                uiElem.Visibility = Visibility.Collapsed;
            pgb.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                var result = backgroundFunc();
                pgb.Dispatcher.Invoke(() =>
                {
                    foregroundFunc(result);
                    foreach (var uiElem in uiElementsToHide)
                        uiElem.Visibility = Visibility.Visible;
                    pgb.Visibility = Visibility.Collapsed;
                });
            });
        }
    }
}
