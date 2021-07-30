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
                    var noteRate = _player.GetPositionSideRate(Positions[i], Sides[j]);
                    var ellipse = new Ellipse
                    {
                        Height = 25,
                        Width = 25,
                        Fill = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, GetColorByte(noteRate), GetColorByte(noteRate))),
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

        private byte GetColorByte(int redPercent)
        {
            return (byte)(byte.MaxValue - (redPercent * (byte.MaxValue / (decimal)20)));
        }
    }
}
