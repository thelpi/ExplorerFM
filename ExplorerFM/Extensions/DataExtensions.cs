using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExplorerFM.Datas;
using ExplorerFM.FieldsAttributes;
using ExplorerFM.Properties;
using ExplorerFM.RuleEngine;
using ExplorerFM.UiDatas;

namespace ExplorerFM.Extensions
{
    public static class DataExtensions
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

        public static List<T> GetSubList<T>(this List<int> sourceDatas, List<T> fullDatas)
            where T : BaseData
        {
            return sourceDatas
                .Select(r => fullDatas.Find(c => c.Id == r))
                .Where(c => c != null)
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

        public static IEnumerable<Comparator> GetComparators(this Type type, FieldAttribute fieldAttribute)
        {
            if (type == typeof(string))
                return Enum.GetValues(typeof(Comparator)).Cast<Comparator>();
            else if (type.IsClass || type == typeof(bool) || fieldAttribute.IsTripleIdentifier)
                return new[] { Comparator.Equal, Comparator.NotEqual };
            else
                return Enum.GetValues(typeof(Comparator)).Cast<Comparator>().Where(_ => !_.IsStringSymbol());
        }

        public static PlayerRateUiData ToRateItemData(this Player p,
            Position position, Side side,
            int maxTheoreticalRate, bool potentialRate,
            NullRateBehavior nullRateBehavior = NullRateBehavior.Minimal)
        {
            int rate;
            if (Settings.Default.UseSaveFile)
            {
                var abilityDelta = potentialRate
                    ? p.GetFixedPotentialAbility() - p.CurrentAbility.GetValueOrDefault(100)
                    : 0;
                rate = (int)Math.Round((p.GetAttributesTotal(nullRateBehavior: nullRateBehavior) + abilityDelta) * p.GetPositionSideRate(position, side) / (decimal)20);
            }
            else
            {
                var ability = potentialRate
                    ? p.GetFixedPotentialAbility()
                    : p.CurrentAbility.GetValueOrDefault(100);
                rate = (int)Math.Round(p.GetAttributesTotal(nullRateBehavior: nullRateBehavior) * (ability / (decimal)200) * p.GetPositionSideRate(position, side) / 20);
            }

            return new PlayerRateUiData
            {
                Rate = rate,
                Player = p,
                PositionRate = p.Positions[position] ?? 1,
                SideRate = position == Position.GoalKeeper ? 20 : (p.Sides[side] ?? 1),
                Position = position,
                Side = side,
                MaxTheoreticalRate = maxTheoreticalRate
            };
        }

        public static int ToRate(this NullRateBehavior nullRateBehavior, int ratesCount, params int[] otherRates)
        {
            switch (nullRateBehavior)
            {
                case NullRateBehavior.Minimal:
                    return 1;
                case NullRateBehavior.GlobalAverage:
                    return 10;
                case NullRateBehavior.Random:
                    return App.Randomizer.Next(1, 21);
                case NullRateBehavior.LocalAverage:
                    if (otherRates.Length < ratesCount / 2) // at least half rates known
                        return 10;
                    else
                        return (int)Math.Round(otherRates.Average());
                default:
                    throw new NotSupportedException();
            }
        }

        public static List<Tuple<Position, Side, PlayerRateUiData>> GetBestLineUp(this Tactic tactic,
            List<Player> sourcePlayers, int maxTheoreticalRate, bool usePotentialAbility,
            NullRateBehavior nullRateBehavior)
        {
            var playerByPos = new List<Tuple<Position, Side, PlayerRateUiData>>();

            // copy
            var players = new List<Player>(sourcePlayers);
            var positions = new List<Tuple<Position, Side>>(tactic.Positions);

            while (positions.Count > 0 && players.Count > 0)
            {
                var positionsBest = new List<Tuple<Position, Side, PlayerRateUiData, decimal>>();
                foreach (var position in positions.Distinct())
                {
                    var pDatas = players
                        .Select(p => p.ToRateItemData(position.Item1, position.Item2, maxTheoreticalRate, usePotentialAbility, nullRateBehavior))
                        .OrderByDescending(p => p.Rate)
                        .ToList();
                    var bestP = pDatas.First();
                    var ratePercent = bestP.Rate / (decimal)pDatas.Sum(p => p.Rate);
                    positionsBest.Add(
                        new Tuple<Position, Side, PlayerRateUiData, decimal>(
                            position.Item1, position.Item2, bestP, ratePercent));
                }

                var bestPositionBest = positionsBest.First(x => x.Item4 == positionsBest.Max(y => y.Item4));

                players.Remove(bestPositionBest.Item3.Player);
                positions.RemoveAt(
                    positions.IndexOf(
                        new Tuple<Position, Side>(
                            bestPositionBest.Item1, bestPositionBest.Item2)));
                playerByPos.Add(
                    new Tuple<Position, Side, PlayerRateUiData>(
                        bestPositionBest.Item1, bestPositionBest.Item2, bestPositionBest.Item3));
            }

            return playerByPos;
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
    }
}
