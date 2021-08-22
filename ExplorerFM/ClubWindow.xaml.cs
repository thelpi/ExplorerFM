using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private bool _isSourceChange;
        private List<Player> _players;
        private readonly DataProvider _dataProvider;

        private bool UsePotentialAbility => PotentialAbilityCheckBox.IsChecked == true;
        private NullRateBehavior NullRateBehavior => NullRateBehaviorComboBox.SelectedIndex == -1
            ? NullRateBehavior.Minimal
            : (NullRateBehavior)NullRateBehaviorComboBox.SelectedItem;

        public ClubWindow(DataProvider dataProvider, Club club)
        {
            InitializeComponent();

            _dataProvider = dataProvider;
            PositionsComboBox.ItemsSource = Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = Enum.GetValues(typeof(Side));
            TacticsComboBox.ItemsSource = Tactic.Tactics;
            CountryClubComboBox.ItemsSource = _dataProvider.Countries;
            CountryClubComboBox.SelectedItem = club?.Country;
            ClubComboBox.ItemsSource = _dataProvider.Clubs.Where(c => c.Country?.Id == club?.Country?.Id);
            ClubComboBox.SelectedItem = club;
            NullRateBehaviorComboBox.ItemsSource = Enum.GetValues(typeof(NullRateBehavior));
            NullRateBehaviorComboBox.SelectedItem = NullRateBehavior.Minimal;
        }

        private void PlayersView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var pItem = PlayersView.SelectedItem;
            if (pItem != null)
            {
                //Hide();
                //new PlayerWindow(pItem as Player).ShowDialog();
                //ShowDialog();
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
                RatedPlayersListView.ItemsSource = GetPositioningTopTenPlayers(
                    (Position)PositionsComboBox.SelectedItem,
                    (Side)SidesComboBox.SelectedItem);
            }
        }

        private void TacticsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TacticsComboBox.SelectedItem != null)
            {
                TacticPlayersGrid.Children.Clear();

                var lineUp = (TacticsComboBox.SelectedItem as Tactic)
                    .GetBestLineUp(_players, _dataProvider.MaxTheoreticalRate, UsePotentialAbility, NullRateBehavior);

                foreach (var posGroup in lineUp.GroupBy(_ => new Tuple<Position, Side>(_.Item1, _.Item2)))
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

                        AddLineUpUiComponent(PlayerPositionTemplateKey,
                            rowIndex, colIndex, posPlayer.Item3);
                        AddLineUpUiComponent(PlayerNameTemplateKey,
                            rowIndex, colIndex, posPlayer.Item3);

                        currPosIndex++;
                    }
                }

                TacticInfoLabel.Content = $"Total value: {lineUp.Sum(_ => _.Item3.Rate)}";
            }
        }

        private void AddLineUpUiComponent(string key, int row, int column, PlayerRateItemData playerData)
        {
            var element = this.GetByTemplateKey<FrameworkElement>(key);
            element.DataContext = playerData;
            element.SetValue(Grid.ColumnProperty, column);
            element.SetValue(Grid.RowProperty, row);

            TacticPlayersGrid.Children.Add(element);
        }

        private IEnumerable<PlayerRateItemData> GetPositioningTopTenPlayers(Position position, Side side)
        {
            return _players
                .Select(p => p.ToRateItemData(position, side, _dataProvider.MaxTheoreticalRate, UsePotentialAbility, NullRateBehavior))
                .OrderByDescending(p => p.Rate)
                .Take(10);
        }

        private void ClubComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSourceChange)
                return;

            var club = ClubComboBox.SelectedItem as Club;

            LoadPlayersProgressBar.HideWorkAndDisplay(
                () => _dataProvider.GetPlayersByClub(club?.Id),
                p =>
                {
                    _players = p;
                    Title = club?.LongName ?? NoClub;
                    PlayersView.ItemsSource = _players;
                    ClearForms();
                },
                MainGrid.Children.Cast<UIElement>().ToArray());
        }

        private void CountryClubComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _isSourceChange = true;
            ClubComboBox.ItemsSource = _dataProvider.Clubs.Where(c =>
                c.Country?.Id == (CountryClubComboBox.SelectedItem as Country)?.Id);
            _isSourceChange = false;
        }

        private void ClearForms()
        {
            PositionsComboBox.SelectedIndex = -1;
            SidesComboBox.SelectedIndex = -1;
            TacticsComboBox.SelectedIndex = -1;
            TacticInfoLabel.Content = null;
            TacticPlayersGrid.Children.Clear();
            RatedPlayersListView.ItemsSource = null;
        }

        private void PotentialAbilityCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ClearForms();
        }

        private void NullRateBehaviorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearForms();
        }
    }
}
