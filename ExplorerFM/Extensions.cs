using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ExplorerFM.Datas;

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
    }
}
