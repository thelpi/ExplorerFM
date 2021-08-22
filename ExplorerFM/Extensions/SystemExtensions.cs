using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
    }
}
