using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using ExplorerFM.Datas;

namespace ExplorerFM
{
    /// <summary>
    /// Logique d'interaction pour ClubWindow.xaml
    /// </summary>
    public partial class ClubWindow : Window
    {
        private const string PlayerPositionTemplateKey = "PlayerPositionTemplate";
        private const string PlayerNameTemplateKey = "PlayerNameTemplate";
        private const string NoClub = "Without club";
        
        private readonly List<Player> _players;
        private readonly DataProvider _dataProvider;

        public ClubWindow(DataProvider dataProvider, Club club, List<Player> players)
        {
            InitializeComponent();

            _dataProvider = dataProvider;
            _players = players;

            Title = club?.LongName ?? NoClub;
            PlayersView.ItemsSource = _players;
            PositionsComboBox.ItemsSource = Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = Enum.GetValues(typeof(Side));
            TacticsComboBox.ItemsSource = Tactic.Tactics;
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

        private void PositionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetRatedPlayersListBox();
        }

        private void SidesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetRatedPlayersListBox();
        }

        private void SetRatedPlayersListBox()
        {
            if (PositionsComboBox.SelectedIndex >= 0
                && SidesComboBox.SelectedIndex >= 0)
            {
                RatedPlayersListBox.ItemsSource = GetOrderedRatedPlayers(
                    (Position)PositionsComboBox.SelectedItem,
                    (Side)SidesComboBox.SelectedItem);
            }
        }

        private void TacticsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TacticsComboBox.SelectedItem != null)
            {
                TacticPlayersGrid.Children.Clear();

                var squad = (TacticsComboBox.SelectedItem as Tactic)
                    .GetBestSquad(_players, _dataProvider.Attributes.Count);

                foreach (var posGroup in squad.GroupBy(_ => new Tuple<Position, Side>(_.Item1, _.Item2)))
                {
                    var groupPlayerCount = posGroup.Count();
                    var currPosIndex = 0;
                    foreach (var posPlayer in posGroup)
                    {
                        var rowIndex = Array.IndexOf(Extensions.OrderedPositions,
                            posPlayer.Item1);
                        var colIndex = Array.IndexOf(Extensions.OrderedSides,
                            posPlayer.Item2) * 2; // 0,1,2 => 0,2,4

                        if (colIndex == 2)
                        {
                            if (groupPlayerCount == 3)
                                colIndex = currPosIndex == 0 ? 1 : (currPosIndex == 1 ? 2 : 3);
                            else if (groupPlayerCount == 2)
                                colIndex = currPosIndex == 0 ? 1 : 3;
                        }

                        AddSquadUiComponent(PlayerPositionTemplateKey,
                            rowIndex, colIndex, posPlayer.Item3);
                        AddSquadUiComponent(PlayerNameTemplateKey,
                            rowIndex, colIndex, posPlayer.Item3);

                        currPosIndex++;
                    }
                }

                TacticInfoLabel.Content = $"Total value: {squad.Sum(_ => _.Item3.Rate)}";
            }
        }

        private void AddSquadUiComponent(string key, int row, int column, PlayerRateItemData playerData)
        {
            var element = this.GetByTemplateKey<FrameworkElement>(key);
            element.DataContext = playerData;
            element.SetValue(Grid.ColumnProperty, column);
            element.SetValue(Grid.RowProperty, row);

            TacticPlayersGrid.Children.Add(element);
        }

        private List<PlayerRateItemData> GetOrderedRatedPlayers(Position position, Side side)
        {
            return _players
                .Select(p => p.ToRateItemData(position, side, _dataProvider.Attributes.Count))
                .OrderByDescending(p => p.Rate)
                .ToList();
        }
    }
}
