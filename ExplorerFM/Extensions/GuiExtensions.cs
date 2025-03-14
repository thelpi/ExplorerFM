using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ExplorerFM.FieldsAttributes;
using ExplorerFM.Providers;
using Xceed.Wpf.Toolkit;

namespace ExplorerFM.Extensions
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

        public static IEnumerable<KeyValuePair<GridViewColumn, double>> GetAttributeColumns(
            bool fromPlayerRateUiData, Action<GridViewAttribute, PropertyInfo> headerColumnClick)
        {
            var columns = new List<KeyValuePair<GridViewColumn, double>>();

            var allColumnFields = DataProvider.GetAllAttribute<GridViewAttribute>();
            foreach (var columnField in allColumnFields)
            {
                var fullPath = columnField.GetPlayerPropertyPath();
                if (fromPlayerRateUiData)
                    fullPath = string.Concat(nameof(UiDatas.PlayerRateUiData.Player), ".", fullPath);
                var attributes = columnField.GetCustomAttributes<GridViewAttribute>();
                foreach (var attribute in attributes)
                {
                    columns.Add(new KeyValuePair<GridViewColumn, double>(
                        attribute.GetAttributeColumn(columnField, fullPath, headerColumnClick),
                        attribute.Priority));
                }
            }

            return columns.OrderBy(_ => _.Value);
        }

        private static GridViewColumn GetAttributeColumn(this GridViewAttribute attribute,
            PropertyInfo columnField, string fullPath, Action<GridViewAttribute, PropertyInfo> headerColumnClick)
        {
            var columnHeader = new GridViewColumnHeader
            {
                Content = attribute.Name,
            };
            if (headerColumnClick != null)
                columnHeader.Click += (_1, _2) => headerColumnClick(attribute, columnField);

            var column = new GridViewColumn
            {
                Header = columnHeader,
                DisplayMemberBinding = new Binding
                {
                    Path = attribute.NoPath ? null : new PropertyPath(fullPath),
                    Converter = attribute.Converter,
                    ConverterParameter = attribute.ConverterParameter
                }
            };
            return column;
        }
    }
}
