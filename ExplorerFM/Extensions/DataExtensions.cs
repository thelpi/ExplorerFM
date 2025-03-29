using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon.Runtime.Internal.Util;
using ExplorerFM.Datas;
using ExplorerFM.FieldsAttributes;
using ExplorerFM.Properties;
using ExplorerFM.RuleEngine;
using ExplorerFM.UiDatas;

namespace ExplorerFM.Extensions
{
    public static class DataExtensions
    {
        private static readonly DateTime _startDate = new DateTime(2001, 7, 1);

        public static int? GetAge(this Player player)
        {
            return player.ActualDateOfBirth.HasValue
                ? _startDate.Year - player.ActualDateOfBirth.Value.Year
                : default(int?);
        }

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
            int maxTheoreticalRate, bool potentialRate)
        {
            int rate;
            if (Settings.Default.UseSaveFile)
            {
                rate = p.GetPositionFinalRate((position, side));
            }
            else
            {
                var ability = potentialRate
                    ? p.GetFixedPotentialAbility()
                    : p.CurrentAbility.GetValueOrDefault(100);
                rate = (int)Math.Round(p.GetAttributesTotal() * (ability / (decimal)200) * p.GetPositionSideRate(position, side) / 20);
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

        public static List<(Position, Side, PlayerRateUiData)> GetBestLineUp(this Tactic tactic,
            List<Player> sourcePlayers, int maxTheoreticalRate, bool usePotentialAbility)
        {
#if DEBUG
            var start = DateTime.Now;
#endif
            if (sourcePlayers.Count < 11)
            {
                return new List<(Position, Side, PlayerRateUiData)>();
            }

            var players = new List<Player>(sourcePlayers);
            var positions = new List<(Position, Side)>(tactic.Positions);

            PlayerRateUiData pGk = null;
            foreach (var p in players)
            {
                var ppr = p.ToRateItemData(Position.GoalKeeper, Side.Center, maxTheoreticalRate, usePotentialAbility);
                if (pGk == null || ppr.Rate > pGk.Rate)
                    pGk = ppr;
            }
            players.Remove(pGk.Player);
            positions.RemoveAt(positions.IndexOf((Position.GoalKeeper, Side.Center)));

            List<(Position, Side, PlayerRateUiData)> globalTeamPlayers = null;
            var globalTeamRate = pGk.Rate;

            var posGroup = positions.GroupBy(x => x).Select(x => (x.Key.Item1, x.Key.Item2, x.Count())).ToList();
            RecursiveCrawlBestTeam(players, posGroup, 0, new List<int>(), ref globalTeamRate, ref globalTeamPlayers, maxTheoreticalRate, usePotentialAbility);

#if DEBUG
            var end = DateTime.Now;
            System.Diagnostics.Debug.WriteLine($"[TIME] {(end - start).TotalMilliseconds}");
#endif

            globalTeamPlayers.Add((Position.GoalKeeper, Side.Center, pGk));

            return globalTeamPlayers;
        }

        private static Comparer<PlayerRateUiData> _playerComparer = Comparer<PlayerRateUiData>.Create((x, y) => x.Rate.CompareTo(y.Rate));

        private static void RecursiveCrawlBestTeam(List<Player> players,
            List<(Position, Side, int)> posGroup,
            int currentDepth,
            List<int> aboveIndexes,
            ref int currentBestScore,
            ref List<(Position, Side, PlayerRateUiData)> currentBestTeam,
            int maxTheoreticalRate,
            bool potentialRate)
        {
            for (var i = 0; i < posGroup.Count; i++)
            {
                if (aboveIndexes.Contains(i)) continue;

                var allIndexes = new List<int>(aboveIndexes) { i };
                if (currentDepth == posGroup.Count - 1)
                {
                    var localScore = 0;
                    var localBestTeam = new List<(Position, Side, PlayerRateUiData)>(10);
                    var usedPlayers = new List<Player>(10);
                    foreach (var k in allIndexes)
                    {
                        var psgPos = posGroup[k].Item1;
                        var psgSid = posGroup[k].Item2;
                        var psgCount = posGroup[k].Item3;
                        var selectedPlayers = new List<PlayerRateUiData>(psgCount);
                        var localSumRate = 0;
                        foreach (var p in players)
                        {
                            if (usedPlayers.Contains(p)) continue;

                            var pData = p.ToRateItemData(psgPos, psgSid, maxTheoreticalRate, potentialRate);
                            if (selectedPlayers.Count < psgCount)
                            {
                                selectedPlayers.Add(pData);
                                selectedPlayers.Sort(_playerComparer);
                                usedPlayers.Add(p);
                                localSumRate += pData.Rate;
                            }
                            else if (selectedPlayers[0].Rate < pData.Rate)
                            {
                                var removedP = selectedPlayers[0];
                                selectedPlayers.RemoveAt(0);
                                selectedPlayers.Add(pData);
                                selectedPlayers.Sort(_playerComparer);
                                usedPlayers.Remove(removedP.Player);
                                usedPlayers.Add(p);
                                localSumRate -= removedP.Rate;
                                localSumRate += pData.Rate;
                            }
                        }

                        localBestTeam.AddRange(selectedPlayers.Select(x => (psgPos, psgSid, x)));
                        localScore += localSumRate;
                    }

                    if (localScore > currentBestScore)
                    {
                        currentBestScore = localScore;
                        currentBestTeam = localBestTeam;
                    }
                }
                else
                {
                    RecursiveCrawlBestTeam(players, posGroup, currentDepth + 1, allIndexes, ref currentBestScore, ref currentBestTeam, maxTheoreticalRate, potentialRate);
                }
            }
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
