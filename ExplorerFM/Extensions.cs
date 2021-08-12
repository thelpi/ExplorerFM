using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xml;
using ExplorerFM.Datas;
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

        public static T Get<T>(this IDataReader reader, string columnName, T defaultValue = default(T), params T[] ignored)
        {
            var sourceValue = reader[reader.GetOrdinal(columnName)];

            var forcedValue = sourceValue == DBNull.Value || sourceValue == null
                ? defaultValue
                : sourceValue;

            var value = (T)Convert.ChangeType(forcedValue, typeof(T));

            return ignored?.Contains(value) == true
                ? defaultValue
                : value;
        }

        public static T? GetNull<T>(this IDataReader reader, string columnName, T? defaultValue = null, params T[] ignored)
            where T : struct
        {
            var sourceValue = reader[reader.GetOrdinal(columnName)];

            var forcedValue = sourceValue == DBNull.Value || sourceValue == null
                ? defaultValue
                : sourceValue;

            T? value = null;
            if (forcedValue != null)
            {
                value = (T)Convert.ChangeType(forcedValue, typeof(T));
            }

            return value.HasValue && ignored?.Contains(value.Value) == true
                ? defaultValue
                : value;
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

        public static List<Comparator> GetComparators(this Type t)
        {
            if ((t != typeof(string) && t.GetInterfaces().Contains(typeof(IEnumerable)))
                || t == typeof(Club)
                || t == typeof(Country)
                || t == typeof(Confederation))
            {
                return new List<Comparator>
                {
                    Comparator.Equal,
                    Comparator.NotEqual
                };
            }

            var comparators = new List<Comparator>
            {
                Comparator.Equal,
                Comparator.NotEqual,
                Comparator.Greater,
                Comparator.GreaterEqual,
                Comparator.Lower,
                Comparator.LowerEqual
            };

            if (t == typeof(string))
            {
                comparators.Add(Comparator.Like);
                comparators.Add(Comparator.NotLike);
            }
            else if (t == typeof(bool) || (!typeof(IComparable).IsAssignableFrom(t)
                && !(t.IsGenericType && typeof(IComparable).IsAssignableFrom(t.GenericTypeArguments[0]))))
            {
                comparators.Remove(Comparator.Greater);
                comparators.Remove(Comparator.GreaterEqual);
                comparators.Remove(Comparator.Lower);
                comparators.Remove(Comparator.LowerEqual);
            }

            return comparators;
        }

        public static List<PropertyInfo> GetAttributeProperties(this Type t)
        {
            return t.GetProperties().Where(p => p.GetCustomAttributes(typeof(FieldAttribute), true).Length > 0).ToList();
        }

        public static System.Windows.DataTemplate ToDataTemplate(this string xamlContent)
        {
            return XamlReader.Load(
                XmlReader.Create(
                    new StringReader(
                        string.Concat(
                            "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">",
                            xamlContent,
                            "</DataTemplate>"))))
                as System.Windows.DataTemplate;
        }
    }
}
