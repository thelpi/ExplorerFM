﻿using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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
    }
}