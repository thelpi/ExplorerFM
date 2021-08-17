using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using ExplorerFM.Datas;
using ExplorerFM.FieldsAttributes;
using ExplorerFM.RuleEngine;

namespace ExplorerFM
{
    public static class Extensions
    {
        public static string ToCode(this Side side)
        {
            return side.ToString().Substring(0, 1);
        }

        public static string ToCode(this Position position)
        {
            switch (position)
            {
                case Position.GoalKeeper:
                    return "GK";
                case Position.Sweeper:
                    return "SW";
                case Position.DefensiveMidfielder:
                    return "DM";
                case Position.OffensiveMidfielder:
                    return "OM";
                case Position.FreeRole:
                    return "FR";
                default:
                    return position.ToString().Substring(0, 1);
            }
        }

        public static T Get<T>(this IDataReader reader, string columnName)
        {
            var sourceValue = reader[reader.GetOrdinal(columnName)];

            var forcedValue = sourceValue == DBNull.Value || sourceValue == null
                ? default(T)
                : sourceValue;

            return (T)Convert.ChangeType(forcedValue, typeof(T));
        }

        public static T? GetNull<T>(this IDataReader reader, string columnName)
            where T : struct
        {
            var sourceValue = reader[reader.GetOrdinal(columnName)];

            var forcedValue = sourceValue == DBNull.Value || sourceValue == null
                ? null
                : sourceValue;

            return forcedValue != null
                ? (T)Convert.ChangeType(forcedValue, typeof(T))
                : (T?)null;
        }

        public static T? ToEnum<T>(this int? value) where T : struct
        {
            return value.HasValue
                ? (T)Enum.ToObject(typeof(T), value.Value)
                : default(T?);
        }

        public static List<T> GetSubList<T>(this List<int> sourceDatas, List<T> fullDatas)
            where T : BaseData
        {
            return sourceDatas
                .Select(r => fullDatas.Find(c => c.Id == r))
                .Where(c => c != null)
                .ToList();
        }

        public static List<int> GetIdList(this IDataReader reader, string columnNameTemplate)
        {
            return Enumerable.Range(1, 3)
                .Select(x => reader.GetNull<int>(string.Format(columnNameTemplate, x)))
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();
        }

        public static string ToSymbol(this Comparator comparator)
        {
            switch (comparator)
            {
                case Comparator.Equal: return "=";
                case Comparator.Greater: return ">";
                case Comparator.GreaterEqual: return ">=";
                case Comparator.Lower: return "<";
                case Comparator.LowerEqual: return "<=";
                case Comparator.NotEqual: return "!=";
                case Comparator.Like: return "LIKE";
                case Comparator.NotLike: return "NOT LIKE";
                default: throw new NotSupportedException();
            }
        }

        public static bool IsStringSymbol(this Comparator comparator)
        {
            return comparator == Comparator.Like
                || comparator == Comparator.NotLike;
        }

        public static List<PropertyInfo> GetAttributeProperties<T>(this Type t) where T : System.Attribute
        {
            return t.GetProperties().Where(p => p.GetCustomAttributes(typeof(T), true).Length > 0).ToList();
        }

        // not my code
        public static void RemoveRoutedEventHandlers(this UIElement element, RoutedEvent routedEvent)
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

        public static T Find<T>(this FrameworkElement element, string name) where T : UIElement
        {
            return element.FindName(name) as T;
        }

        public static bool IsNullOrContainsNull(this object value)
        {
            return value == null
                || (value is object[]
                    && (value as object[]).Contains(null));
        }

        public static Type GetUnderlyingNotNullType(this Type propType)
        {
            var underlyingType = propType;
            if (typeof(IDictionary).IsAssignableFrom(underlyingType) && underlyingType.IsGenericType)
                underlyingType = underlyingType.GenericTypeArguments[1];
            else if (typeof(IList).IsAssignableFrom(underlyingType) && underlyingType.IsGenericType)
                underlyingType = underlyingType.GenericTypeArguments[0];

            return Nullable.GetUnderlyingType(underlyingType) ?? underlyingType;
        }

        public static IEnumerable<T> Yield<T>(this T value, params T[] values)
        {
            return new[] { value }.Concat(values ?? Enumerable.Empty<T>());
        }

        public static IEnumerable<Comparator> GetComparators(this Type type, FieldAttribute fieldAttribute)
        {
            if (type == typeof(string))
                return Enum.GetValues(typeof(Comparator)).Cast<Comparator>();
            else if (type.IsClass || type == typeof(bool) || fieldAttribute.IsTripleIdentifier)
                return new[] { Comparator.Equal, Comparator.NotEqual };
            else
                return Enum.GetValues(typeof(Comparator)).Cast<Comparator>().Where(_ => !_.IsStringSymbol());
        }
    }
}
