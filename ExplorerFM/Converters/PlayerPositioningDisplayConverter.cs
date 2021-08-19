using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using ExplorerFM.Datas;

namespace ExplorerFM.Converters
{
    class PlayerPositioningDisplayConverter : IValueConverter
    {
        // ordered
        public static readonly Side[] Sides = new Side[] { Side.Left, Side.Center, Side.Right };

        // ordered
        // ignore wing back and free role
        public static readonly Position[] Positions = new Position[]
        {
            Position.Striker, Position.OffensiveMidfielder, Position.Midfielder, Position.DefensiveMidfielder,
            Position.Defender, Position.Sweeper, Position.GoalKeeper
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var playerValue = value as Player;

            return Positions
                .Select(p =>
                    new PositioningItemGroup(p, Sides
                        .ToDictionary(s => s, s => playerValue.GetPositionSideRate(p, s))))
                .ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public class PositioningItemGroup
        {
            public IReadOnlyCollection<PositioningItem> Items { get; }

            public PositioningItemGroup(Position position, Dictionary<Side, int> rates)
            {
                Items = rates
                    .Select(s => new PositioningItem(position, s.Key, s.Value))
                    .ToList();
            }
        }

        public class PositioningItem
        {
            public Color Color { get; }
            public string ToolTip { get; }

            public PositioningItem(Position position, Side side, int rate)
            {
                var invisible = (position == Position.GoalKeeper || position == Position.Sweeper) && side != Side.Center;
                Color = invisible ? Colors.Green : GetColorFromRate(rate);
                ToolTip = $"{position.ToCode()} {side.ToCode()} ({rate})";
            }

            private Color GetColorFromRate(int rate)
            {
                var switchStop = 20 / (decimal)3;

                var blue = rate > switchStop
                    ? 0
                    : 255 - ((rate / switchStop) * 255);

                var green = rate <= switchStop
                    ? 255
                    : 255 - (((rate - switchStop) / (switchStop * 2)) * 255);

                return Color.FromArgb(byte.MaxValue, byte.MaxValue, (byte)green, (byte)blue);
            }
        }
    }
}
