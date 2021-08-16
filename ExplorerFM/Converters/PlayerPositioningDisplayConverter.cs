using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ExplorerFM.Datas;

namespace ExplorerFM.Converters
{
    class PlayerPositioningDisplayConverter : IValueConverter
    {
        // ordered
        private static readonly Side[] Sides = new Side[] { Side.Left, Side.Center, Side.Right };

        // ordered
        // ignore wing back and free role
        private static readonly Position[] Positions = new Position[]
        {
            Position.Striker, Position.OffensiveMidfielder, Position.Midfielder, Position.DefensiveMidfielder,
            Position.Defender, Position.Sweeper, Position.GoalKeeper
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var playerValue = value as Player;

            var positioningItems = new List<PositioningItem>();

            foreach (var position in Positions)
            {
                positioningItems.Add(new PositioningItem(position, new Dictionary<Side, int>
                {
                    { Side.Center, playerValue.GetPositionSideRate(position, Side.Center) },
                    { Side.Right, playerValue.GetPositionSideRate(position, Side.Right) },
                    { Side.Left, playerValue.GetPositionSideRate(position, Side.Left) }
                }));
            }

            return positioningItems;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public class PositioningItem
        {
            public Color[] Colors { get; set; }
            public string[] ToolTips { get; set; }
            public Visibility[] Visibilities { get; set; }

            public PositioningItem(Position position, Dictionary<Side, int> rates)
            {
                Colors = new Color[Sides.Length];
                ToolTips = new string[Sides.Length];
                Visibilities = new Visibility[Sides.Length];
                int i = 0;
                foreach (var side in Sides)
                {
                    if ((position == Position.GoalKeeper || position == Position.Sweeper) && side != Side.Center)
                    {
                        Visibilities[i] = Visibility.Hidden;
                    }
                    else
                    {
                        Visibilities[i] = Visibility.Visible;
                        var noteRate = rates[side];
                        Colors[i] = GetColorFromRate(noteRate);
                        ToolTips[i] = $"{position.ToCode()} {side.ToCode()} ({noteRate})";
                    }
                    i++;
                }
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
