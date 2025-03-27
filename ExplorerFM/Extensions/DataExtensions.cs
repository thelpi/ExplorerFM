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
            var start = DateTime.Now;

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

            var localTeamPlayers = new (Position, Side, PlayerRateUiData)[10];

            for (var a = 0; a < 10; a++)
            {
                for (var b = 0; b < 10; b++)
                {
                    if (b == a) continue;
                    for (var c = 0; c < 10; c++)
                    {
                        if (c == a || c == b) continue;
                        for (var d = 0; d < 10; d++)
                        {
                            if (d == a || d == b || d == c) continue;
                            for (var e = 0; e < 10; e++)
                            {
                                if (e == a || e == b || e == c || e == d) continue;
                                for (var f = 0; f < 10; f++)
                                {
                                    if (f == a || f == b || f == c || f == d || f == e) continue;
                                    for (var g = 0; g < 10; g++)
                                    {
                                        if (g == a || g == b || g == c || g == d || g == e || g == f) continue;
                                        for (var h = 0; h < 10; h++)
                                        {
                                            if (h == a || h == b || h == c || h == d || h == e || h == f || h == g) continue;
                                            for (var i = 0; i < 10; i++)
                                            {
                                                if (i == a || i == b || i == c || i == d || i == e || i == f || i == g || i == h) continue;
                                                for (var j = 0; j < 10; j++)
                                                {
                                                    if (j == a || j == b || j == c || j == d || j == e || j == f || j == g || j == h || j == i) continue;

                                                    var localPlayers = new Player[10];
                                                    var localTeamRate = -1;

                                                    ComputeBestLocalPlayerForPosition(players, localPlayers, localTeamPlayers, positions, maxTheoreticalRate, usePotentialAbility, a, 0, ref localTeamRate);
                                                    ComputeBestLocalPlayerForPosition(players, localPlayers, localTeamPlayers, positions, maxTheoreticalRate, usePotentialAbility, b, 1, ref localTeamRate);
                                                    ComputeBestLocalPlayerForPosition(players, localPlayers, localTeamPlayers, positions, maxTheoreticalRate, usePotentialAbility, c, 2, ref localTeamRate);
                                                    ComputeBestLocalPlayerForPosition(players, localPlayers, localTeamPlayers, positions, maxTheoreticalRate, usePotentialAbility, d, 3, ref localTeamRate);
                                                    ComputeBestLocalPlayerForPosition(players, localPlayers, localTeamPlayers, positions, maxTheoreticalRate, usePotentialAbility, e, 4, ref localTeamRate);
                                                    ComputeBestLocalPlayerForPosition(players, localPlayers, localTeamPlayers, positions, maxTheoreticalRate, usePotentialAbility, f, 5, ref localTeamRate);
                                                    ComputeBestLocalPlayerForPosition(players, localPlayers, localTeamPlayers, positions, maxTheoreticalRate, usePotentialAbility, g, 6, ref localTeamRate);
                                                    ComputeBestLocalPlayerForPosition(players, localPlayers, localTeamPlayers, positions, maxTheoreticalRate, usePotentialAbility, h, 7, ref localTeamRate);
                                                    ComputeBestLocalPlayerForPosition(players, localPlayers, localTeamPlayers, positions, maxTheoreticalRate, usePotentialAbility, i, 8, ref localTeamRate);
                                                    ComputeBestLocalPlayerForPosition(players, localPlayers, localTeamPlayers, positions, maxTheoreticalRate, usePotentialAbility, j, 9, ref localTeamRate);

                                                    if (localTeamRate > globalTeamRate)
                                                    {
                                                        globalTeamRate = localTeamRate;
                                                        globalTeamPlayers = localTeamPlayers.ToList();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var end = DateTime.Now;
            System.Diagnostics.Debug.WriteLine($"[TIME] {(end - start).TotalMilliseconds}");

            globalTeamPlayers.Add((Position.GoalKeeper, Side.Center, pGk));

            return globalTeamPlayers;
        }

        private static void ComputeBestLocalPlayerForPosition(List<Player> sourcePlayer,
            Player[] usedPlayer,
            (Position, Side, PlayerRateUiData)[] lineUp,
            List<(Position, Side)> positions,
            int maxTheoreticalRate,
            bool usePotentialAbility,
            int index,
            int index2,
            ref int totalRate)
        {
            PlayerRateUiData pBest = null;
            foreach (var p in sourcePlayer)
            {
                if (usedPlayer.Contains(p)) continue;

                var pData = p.ToRateItemData(positions[index].Item1, positions[index].Item2, maxTheoreticalRate, usePotentialAbility);
                if (pBest == null || pData.Rate > pBest.Rate)
                    pBest = pData;
            }

            totalRate += pBest.Rate;
            usedPlayer[index2] = pBest.Player;
            lineUp[index2] = (positions[index].Item1, positions[index].Item2, pBest);
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
