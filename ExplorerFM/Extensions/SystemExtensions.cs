using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace ExplorerFM.Extensions
{
    internal static class SystemExtensions
    {
        public static T? ToEnum<T>(this int? value) where T : struct
        {
            return value.HasValue
                ? (T)Enum.ToObject(typeof(T), value.Value)
                : default(T?);
        }

        public static List<PropertyInfo> GetAttributeProperties<T>(this Type t) where T : System.Attribute
        {
            return t.GetProperties().Where(p => p.GetCustomAttributes(typeof(T), true).Length > 0).ToList();
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

        public static IEnumerable<T> WithNullEntry<T>(this IEnumerable<T> sourceCollection, bool last = false)
            where T : class
        {
            var collectionCopy = new List<T>(sourceCollection);
            collectionCopy.Insert(last ? collectionCopy.Count : 0, null);
            return collectionCopy;
        }

        public static bool IsNumeric(this object value)
        {
            return value is sbyte
                || value is byte
                || value is short
                || value is ushort
                || value is int
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is double
                || value is decimal;
        }
    }
}
