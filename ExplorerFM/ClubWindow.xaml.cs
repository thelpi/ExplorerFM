using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ExplorerFM.Datas;

namespace ExplorerFM
{
    /// <summary>
    /// Logique d'interaction pour ClubWindow.xaml
    /// </summary>
    public partial class ClubWindow : Window
    {
        private const string NoClub = "Without club";

        private readonly Club _club;
        private readonly List<Player> _players;
        private readonly DataProvider _dataProvider;

        public ClubWindow(DataProvider dataProvider, Club club, List<Player> players)
        {
            InitializeComponent();

            _dataProvider = dataProvider;
            _club = club;
            _players = players;

            Title = club?.LongName ?? NoClub;
            PlayersView.ItemsSource = _players;
            PositionsComboBox.ItemsSource = System.Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = System.Enum.GetValues(typeof(Side));
        }

        private void PlayersView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var pItem = PlayersView.SelectedItem;
            if (pItem != null)
            {
                Hide();
                new PlayerWindow(pItem as Player).ShowDialog();
                ShowDialog();
            }
        }

        private void PositionsComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SetRatedPlayersListBox();
        }

        private void SidesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SetRatedPlayersListBox();
        }

        private void SetRatedPlayersListBox()
        {
            if (PositionsComboBox.SelectedIndex >= 0
                            && SidesComboBox.SelectedIndex >= 0)
            {
                var position = (Position)PositionsComboBox.SelectedItem;
                var side = (Side)SidesComboBox.SelectedItem;

                var playersRatedList = new List<PlayerRateItemData>();
                foreach (var p in _players)
                {
                    var rate = p.GetPositionSideRate(position, side);
                    playersRatedList.Add(new PlayerRateItemData
                    {
                        AttributeRate = (int)System.Math.Round((p.AttributesTotal * rate) / (decimal)20),
                        Name = p.Fullname,
                        PositionRate = p.Positions[position] ?? 1,
                        SideRate = position == Position.GoalKeeper ? 20 : (p.Sides[side] ?? 1)
                    });
                }
                RatedPlayersListBox.ItemsSource = playersRatedList
                    .OrderByDescending(_ => _.AttributeRate);
            }
        }
    }
}
