using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ExplorerFM.Datas;
using ExplorerFM.Extensions;

namespace ExplorerFM.Converters
{
    class PositioningDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var player = value.GetType() == typeof(UiDatas.PlayerRateUiData)
                ? (value as UiDatas.PlayerRateUiData).Player
                :  value as Player;
            var isBest = (bool)parameter;

            var bestPositions = GetPositioning(player.Positions, true, null);
            var bestSides = GetPositioning(player.Sides, true, null);

            if (isBest)
                return GetFullString(
                    bestPositions,
                    bestSides);
            else
                return GetFullString(
                    GetPositioning(player.Positions, false, bestPositions),
                    GetPositioning(player.Sides, false, bestSides));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string GetFullString(List<Position> positions, List<Side> sides)
        {
            return string
                .Concat(
                    GetPositioningString(positions, "/", _ => _.ToCode()),
                    " ",
                    GetPositioningString(sides, "", _ => _.ToCode()))
                .Trim();
        }

        private static string GetPositioningString<T>(
            List<T> values,
            string separator,
            Func<T, string> toStringFunc)
        {
            return string.Join(separator, values.Select(p => toStringFunc(p)));
        }

        private static List<T> GetPositioning<T>(
            Dictionary<T, int?> rates,
            bool getBestOnly,
            List<T> excludeAsBest)
        {
            var selectedValues = new List<T>();
            var positionString = string.Empty;

            int? bestPositionRate = null;
            var positions = new List<T>();
            foreach (var position in rates.Keys)
            {
                bool isSuperior = (rates[position] ?? 0) > (bestPositionRate ?? 0);
                if ((rates[position] ?? 0) >= (getBestOnly ? (bestPositionRate ?? 0) : Player.PositioningTolerance))
                {
                    bestPositionRate = rates[position];
                    if (isSuperior && getBestOnly)
                        positions.Clear();
                    positions.Add(position);
                }
            }

            if (excludeAsBest != null)
                positions.RemoveAll(_ => excludeAsBest.Contains(_));

            if (!bestPositionRate.HasValue || bestPositionRate.Value < Player.PositioningTolerance)
                positions.Clear();

            return positions;
        }
    }
}
