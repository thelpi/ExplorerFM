using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ExplorerFM.Extensions
{
    internal static class SqlExtensions
    {
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

        public static List<int> GetIdList(this IDataReader reader, string columnNameTemplate)
        {
            return Enumerable.Range(1, 3)
                .Select(x => reader.GetNull<int>(string.Format(columnNameTemplate, x)))
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToList();
        }
    }
}
