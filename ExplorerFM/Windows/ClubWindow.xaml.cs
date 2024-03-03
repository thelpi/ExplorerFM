using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExplorerFM.Datas;
using ExplorerFM.Extensions;
using ExplorerFM.UiDatas;

namespace ExplorerFM.Windows
{
    /// <summary>
    /// Logique d'interaction pour ClubWindow.xaml
    /// </summary>
    public partial class ClubWindow : Window
    {
        private const string PlayerPositionTemplateKey = "PlayerPositionTemplate";
        private const string NoClub = "Without club";
        private const string NoCountry = "Without country";

        private bool _isSourceChange;
        private ObservableCollection<Player> _players;
        private readonly DataProvider _dataProvider;
        private readonly bool _isCountry;

        private bool UsePotentialAbility => PotentialAbilityCheckBox.IsChecked == true;
        private NullRateBehavior NullRateBehavior => NullRateBehaviorComboBox.SelectedIndex == -1
            ? NullRateBehavior.Minimal
            : (NullRateBehavior)NullRateBehaviorComboBox.SelectedItem;

        public ClubWindow(DataProvider dataProvider)
            : this(dataProvider, false, null, null)
        { }

        public ClubWindow(DataProvider dataProvider, bool isCountry, Club club, Country country)
        {
            InitializeComponent();

            _isCountry = isCountry;
            _dataProvider = dataProvider;
            PositionsComboBox.ItemsSource = Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = Enum.GetValues(typeof(Side));
            TacticsComboBox.ItemsSource = Tactic.Tactics;
            CountryClubComboBox.ItemsSource = _dataProvider.Countries;
            CountryClubComboBox.SelectedItem = club?.Country ?? country;
            if (isCountry)
            {
                ClubComboBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                ClubComboBox.ItemsSource = _dataProvider.Clubs.Where(c => c.Country?.Id == club?.Country?.Id);
                ClubComboBox.SelectedItem = club;
            }
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
                // top 10 best players for the position/side
                RatedPlayersListView.ItemsSource = _players
                    .Select(p =>
                        p.ToRateItemData(
                            (Position)PositionsComboBox.SelectedItem,
                            (Side)SidesComboBox.SelectedItem,
                            _dataProvider.MaxTheoreticalRate,
                            UsePotentialAbility,
                            NullRateBehavior))
                    .OrderByDescending(p => p.Rate)
                    .Take(10);
            }
        }

        private void TacticsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TacticsComboBox.SelectedItem != null)
            {
                SetTacticLineUp();
            }
        }

        private void SetTacticLineUp()
        {
            TacticPlayersGrid.Children.Clear();

            var lineUp = (TacticsComboBox.SelectedItem as Tactic)
                .GetBestLineUp(_players.ToList(), _dataProvider.MaxTheoreticalRate, UsePotentialAbility, NullRateBehavior);

            foreach (var posGroup in lineUp.GroupBy(_ => new Tuple<Position, Side>(_.Item1, _.Item2)))
            {
                var groupPlayerCount = posGroup.Count();
                var currPosIndex = 0;
                foreach (var posPlayer in posGroup)
                {
                    var rowIndex = Array.IndexOf(DataProvider.OrderedPositions,
                        posPlayer.Item1);
                    var colIndex = Array.IndexOf(DataProvider.OrderedSides,
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

                    currPosIndex++;
                }
            }

            TacticInfoLabel.Content = $"Total value: {lineUp.Sum(_ => _.Item3.Rate)}";
        }

        private void AddLineUpUiComponent(string key, int row, int column, PlayerRateUiData playerData)
        {
            var element = this.GetByTemplateKey<FrameworkElement>(key);
            element.DataContext = playerData;
            element.SetValue(Grid.ColumnProperty, column);
            element.SetValue(Grid.RowProperty, row);

            TacticPlayersGrid.Children.Add(element);
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
                    _players = new ObservableCollection<Player>(p);
                    Title = club?.LongName ?? NoClub;
                    PlayersView.ItemsSource = _players;
                    ClearForms();
                },
                MainGrid.Children.Cast<UIElement>().ToArray());
        }

        private void CountryClubComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isCountry)
            {
                if (_isSourceChange)
                    return;

                var country = CountryClubComboBox.SelectedItem as Country;

                LoadPlayersProgressBar.HideWorkAndDisplay(
                    () => _dataProvider.GetPlayersByCountry(country?.Id, true),
                    p =>
                    {
                        _players = new ObservableCollection<Player>(p);
                        Title = country?.LongName ?? NoCountry;
                        PlayersView.ItemsSource = _players;
                        ClearForms();
                    },
                    MainGrid.Children.Cast<UIElement>().ToArray());
            }
            else
            {
                _isSourceChange = true;
                ClubComboBox.ItemsSource = _dataProvider.Clubs.Where(c =>
                    c.Country?.Id == (CountryClubComboBox.SelectedItem as Country)?.Id);
                _isSourceChange = false;
            }
        }

        private void ClearForms()
        {
            PositionsComboBox.SelectedIndex = -1;
            SidesComboBox.SelectedIndex = -1;
            TacticsComboBox.SelectedIndex = -1;
            TacticInfoLabel.Content = null;
            TacticPlayersGrid.Children.Clear();
            RatedPlayersListView.ItemsSource = null;
            TopTenPlayersListView.ItemsSource = GetTopTenRatedPlayers();
        }

        private void PotentialAbilityCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ClearForms();
        }

        private void NullRateBehaviorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearForms();
        }

        private IEnumerable<PlayerRateUiData> GetTopTenRatedPlayers()
        {
            if (_players == null)
                return null;

            var fullList = new List<PlayerRateUiData>(_players.Count * 10);

            foreach (var p in _players)
            {
                var bestSides = p.Sides.Where(x => x.Value == p.Sides.Max(y => y.Value)).Select(x => x.Key);
                var bestPositions = p.Positions.Where(x => x.Value == p.Positions.Max(y => y.Value)).Select(x => x.Key);
                foreach (var position in bestPositions)
                {
                    foreach (var side in bestSides)
                    {
                        fullList.Add(
                            p.ToRateItemData(position, side, _dataProvider.MaxTheoreticalRate, UsePotentialAbility, NullRateBehavior));
                    }
                }
            }

            var finalList = new Dictionary<int, PlayerRateUiData>(fullList.Count);

            foreach (var p in fullList.OrderByDescending(r => r.Rate))
            {
                if (finalList.Count == 10)
                    break;
                if (!finalList.ContainsKey(p.Player.Id))
                    finalList.Add(p.Player.Id, p);
            }

            return finalList.Values;
        }

        private void PlayerLineUpButton_Click(object sender, MouseButtonEventArgs e)
        {
            var p = (sender as FrameworkElement).DataContext as PlayerRateUiData;

            Hide();
            var win = new BestPlayerFinderWindow(_dataProvider,
                p.Position,
                p.Side,
                (NullRateBehavior)NullRateBehaviorComboBox.SelectedItem,
                PotentialAbilityCheckBox.IsChecked);
            win.ShowDialog();

            var player = win.SelectedPlayer;
            if (player != null && !_players.Any(x => x.Id == player.Id))
            {
                var res = MessageBox.Show($"Replace {p.Player.Fullname} from squad ?" +
                        $"\n\nYes: new player will replace the selected player." +
                        $"\nNo: new player will be added to the squad." +
                        $"\nCancel: no player will be added nor removed.",
                    "ExplorerFM",
                    MessageBoxButton.YesNoCancel);
                if (res != MessageBoxResult.Cancel)
                {
                    _players.Add(player);
                    if (res == MessageBoxResult.Yes)
                        _players.Remove(p.Player);
                    SetTacticLineUp();
                    TopTenPlayersListView.ItemsSource = GetTopTenRatedPlayers();
                    SetRatedPlayersListBox();
                }
            }

            ShowDialog();
        }
    }
}
