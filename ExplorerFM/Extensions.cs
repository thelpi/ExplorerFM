using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using ExplorerFM.Datas;
using ExplorerFM.FieldsAttributes;
using ExplorerFM.RuleEngine;

namespace ExplorerFM
{
    public static class Extensions
    {
        public static readonly Side[] OrderedSides = new Side[] { Side.Left, Side.Center, Side.Right };
        
        // ignore wing back and free role
        public static readonly Position[] OrderedPositions = new Position[]
        {
            Position.Striker, Position.OffensiveMidfielder, Position.Midfielder, Position.DefensiveMidfielder,
            Position.Defender, Position.Sweeper, Position.GoalKeeper
        };

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

        public static string GetPlayerPropertyPath(this PropertyInfo columnField)
        {
            var fullPath = columnField.Name;
            if (columnField.DeclaringType == typeof(Confederation))
                fullPath = string.Concat(nameof(Player.Nationality), ".", nameof(Country.Confederation), ".", fullPath);
            else if (columnField.DeclaringType == typeof(Country))
                fullPath = string.Concat(nameof(Player.Nationality), ".", fullPath);
            else if (columnField.DeclaringType == typeof(Club))
                fullPath = string.Concat(nameof(Player.ClubContract), ".", fullPath);
            return fullPath;
        }

        public static List<PropertyInfo> GetAllAttribute<T>() where T : System.Attribute
        {
            return typeof(Player).GetAttributeProperties<T>()
                .Concat(typeof(Club).GetAttributeProperties<T>())
                .Concat(typeof(Country).GetAttributeProperties<T>())
                .Concat(typeof(Confederation).GetAttributeProperties<T>())
                .ToList();
        }

        public static PlayerRateItemData ToRateItemData(this Player p,
            Position position, Side side,
            int maxTheoreticalRate, bool potentialRate,
            NullRateBehavior nullRateBehavior = NullRateBehavior.Minimal)
        {
            var ability = potentialRate
                ? p.GetFixedPotentialAbility()
                : p.CurrentAbility.GetValueOrDefault(100);

            return new PlayerRateItemData
            {
                Rate = (int)Math.Round(p.AttributesTotal * (ability / (decimal)200) * p.GetPositionSideRate(position, side, nullRateBehavior) / 20),
                Player = p,
                PositionRate = p.Positions[position] ?? nullRateBehavior.ToRate(),
                SideRate = position == Position.GoalKeeper ? 20 : (p.Sides[side] ?? nullRateBehavior.ToRate()),
                MaxTheoreticalRate = maxTheoreticalRate
            };
        }

        public static int ToRate(this NullRateBehavior nullRateBehavior, int max = 20, params int[] otherRates)
        {
            switch (nullRateBehavior)
            {
                case NullRateBehavior.Minimal:
                    return 1;
                case NullRateBehavior.GlobalAverage:
                    return max / 2;
                case NullRateBehavior.Random:
                    return App.Randomizer.Next(1, max + 1);
                case NullRateBehavior.LocalAverage:
                    if (otherRates.Length == 0)
                        throw new NotSupportedException();
                    else
                        return (int)Math.Round(otherRates.Average());
                default:
                    throw new NotSupportedException();
            }
        }

        public static List<Tuple<Position, Side, PlayerRateItemData>> GetBestLineUp(this Tactic tactic, List<Player> sourcePlayers, int maxTheoreticalRate)
        {
            var playerByPos = new List<Tuple<Position, Side, PlayerRateItemData>>();

            // copy
            var players = new List<Player>(sourcePlayers);
            var positions = new List<Tuple<Position, Side>>(tactic.Positions);

            while (positions.Count > 0 && sourcePlayers.Count > 0)
            {
                var positionsBest = new List<Tuple<Position, Side, PlayerRateItemData, decimal>>();
                foreach (var position in positions.Distinct())
                {
                    var pDatas = players
                        .Select(p => p.ToRateItemData(position.Item1, position.Item2, maxTheoreticalRate, false))
                        .OrderByDescending(p => p.Rate)
                        .ToList();
                    var bestP = pDatas.First();
                    var ratePercent = bestP.Rate / (decimal)pDatas.Sum(p => p.Rate);
                    positionsBest.Add(
                        new Tuple<Position, Side, PlayerRateItemData, decimal>(
                            position.Item1, position.Item2, bestP, ratePercent));
                }

                var bestPositionBest = positionsBest
                    .OrderByDescending(p => p.Item4)
                    .First();

                players.Remove(bestPositionBest.Item3.Player);
                positions.RemoveAt(
                    positions.IndexOf(
                        new Tuple<Position, Side>(
                            bestPositionBest.Item1, bestPositionBest.Item2)));
                playerByPos.Add(
                    new Tuple<Position, Side, PlayerRateItemData>(
                        bestPositionBest.Item1, bestPositionBest.Item2, bestPositionBest.Item3));
            }

            return playerByPos;
        }
    }
}
