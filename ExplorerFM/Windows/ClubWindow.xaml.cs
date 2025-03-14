using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ExplorerFM.Datas;
using ExplorerFM.Extensions;
using ExplorerFM.Providers;
using ExplorerFM.UiDatas;

namespace ExplorerFM.Windows
{
    /// <summary>
    /// Logique d'interaction pour ClubWindow.xaml
    /// </summary>
    public partial class ClubWindow : Window
    {
        private const string PlayerPositionTemplateKey = "PlayerPositionTemplate";

        private bool _isSourceChange;
        private ObservableCollection<Player> _players;
        private readonly DataProvider _dataProvider;

        private bool UsePotentialAbility => PotentialAbilityCheckBox.IsChecked == true;

        public ClubWindow(DataProvider dataProvider)
        {
            InitializeComponent();

            _dataProvider = dataProvider;
            PositionsComboBox.ItemsSource = Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = Enum.GetValues(typeof(Side));
            TacticsComboBox.ItemsSource = Tactic.Tactics;

            // TODO: sort confederation by strength when available
            var countriesCopy = new List<Country>(dataProvider.Countries.OrderBy(x => x.Confederation?.Name).ThenBy(x => x.Name));
            countriesCopy.Insert(0, Country.Empty);
            countriesCopy.Insert(0, Country.Global);
            
            var countriesView = new ListCollectionView(countriesCopy);
            countriesView.GroupDescriptions.Add(new PropertyGroupDescription($"{nameof(Country.Confederation)}.{nameof(Confederation.FedCode)}"));

            CountryClubComboBox.ItemsSource = countriesView;
            CountryClubComboBox.SelectedIndex = -1;
            ClubComboBox.Visibility = Visibility.Hidden;
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
            if (PositionsComboBox.SelectedIndex >= 0 && (Position)PositionsComboBox.SelectedItem == Position.GoalKeeper)
                SidesComboBox.Visibility = Visibility.Hidden;
            else
                SidesComboBox.Visibility = Visibility.Visible;
        }

        private void SidesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetRatedPlayersListBox();
        }

        private void SetRatedPlayersListBox()
        {
            if (PositionsComboBox.SelectedIndex >= 0)
            {
                var position = (Position)PositionsComboBox.SelectedItem;
                var side = Side.Center;
                if (position != Position.GoalKeeper)
                {
                    if (SidesComboBox.SelectedIndex < 0)
                        return;
                    side = (Side)SidesComboBox.SelectedItem;
                }

                // top 10 best players for the position/side
                RatedPlayersListView.ItemsSource = _players
                    .Select(p =>
                        p.ToRateItemData(
                            position,
                            side,
                            _dataProvider.MaxTheoreticalRate,
                            UsePotentialAbility))
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
                .GetBestLineUp(_players.ToList(), _dataProvider.MaxTheoreticalRate, UsePotentialAbility);

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
            if (_isSourceChange || ClubComboBox.SelectedIndex == -1)
                return;

            var club = ClubComboBox.SelectedItem as Club;
            var country = CountryClubComboBox.SelectedItem as Country;

            LoadPlayersProgressBar.HideWorkAndDisplay(
                () => club.Id == BaseData.AllDataId
                    ? (country.Id == BaseData.AllDataId
                        ? _dataProvider.GetPlayersByCriteria(new RuleEngine.CriteriaSet(false))
                        : _dataProvider.GetPlayersByCountry(country.Id == BaseData.NoDataId ? default(int?) : country.Id, true))
                    : _dataProvider.GetPlayersByClub(club.Id == BaseData.NoDataId ? default(int?) : club.Id),
                p =>
                {
                    _players = new ObservableCollection<Player>(p);
                    Title = club.Id == BaseData.AllDataId ? country.Name : club.Name;
                    PlayersView.ItemsSource = _players;
                    ClearForms();
                },
                MainGrid.Children.Cast<UIElement>().ToArray());
        }

        private void CountryClubComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var country = CountryClubComboBox.SelectedItem as Country;
            if (country == null)
                return;

            _isSourceChange = true;

            List<Club> clubsList;
            string groupProperty = null;
            if (country.Id == BaseData.AllDataId)
            {
                clubsList = new List<Club>
                {
                    Club.Global,
                    Club.Empty
                };
            }
            else if (country.Id == BaseData.NoDataId)
            {
                clubsList = new List<Club>(_dataProvider.Clubs.Where(c => c.Country == null).OrderBy(x => x.Name));
                clubsList.Insert(0, Club.Global);
            }
            else
            {
                // TODO: sort division by reputation when available
                clubsList = new List<Club>(_dataProvider.Clubs.Where(c => c.Country?.Id == country.Id).OrderByDescending(x => x.Division?.Acronym).ThenBy(x => x.Name));
                clubsList.Insert(0, Club.Empty);
                groupProperty = $"{nameof(Club.Division)}.{nameof(Competition.Name)}";
            }

            var countriesView = new ListCollectionView(clubsList);
            if (groupProperty != null)
                countriesView.GroupDescriptions.Add(new PropertyGroupDescription(groupProperty));

            ClubComboBox.ItemsSource = countriesView;
            ClubComboBox.Visibility = Visibility.Visible;
            ClubComboBox.SelectedIndex = -1;

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
                            p.ToRateItemData(position, side, _dataProvider.MaxTheoreticalRate, UsePotentialAbility));
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
