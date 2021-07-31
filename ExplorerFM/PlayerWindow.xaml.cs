using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ExplorerFM.Datas;

namespace ExplorerFM
{
    /// <summary>
    /// Logique d'interaction pour PlayerWindow.xaml
    /// </summary>
    public partial class PlayerWindow : Window
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

        private readonly Player _player;

        public PlayerWindow(Player player)
        {
            InitializeComponent();
            _player = player;
            FillFieldPositionsGrid();
        }

        private void FillFieldPositionsGrid()
        {
            for (int i = 0; i < Positions.Length; i++)
            {
                for (int j = 0; j < Sides.Length; j++)
                {
                    if ((Positions[i] == Position.GoalKeeper || Positions[i] == Position.Sweeper) && Sides[j] != Side.Center)
                        continue;

                    var noteRate = _player.GetPositionSideRate(Positions[i], Sides[j]);
                    var rateColor = GetColorFromRate(noteRate);

                    var ellipse = new Rectangle
                    {
                        Height = 45,
                        Width = 75,
                        Fill = new RadialGradientBrush(
                            new GradientStopCollection(
                                new List<GradientStop>
                                {
                                    new GradientStop(rateColor, 0.33),
                                    new GradientStop(Colors.Green, 1)
                                })),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        ToolTip = $"{Positions[i].ToCode()} {Sides[j].ToCode()} ({noteRate})"
                    };
                    ellipse.SetValue(Grid.RowProperty, i);
                    ellipse.SetValue(Grid.ColumnProperty, j);
                    FieldPositionsGrid.Children.Add(ellipse);
                }
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
