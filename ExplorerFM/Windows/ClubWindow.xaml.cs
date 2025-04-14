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

namespace ExplorerFM.Windows;

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

        CountryClubComboBox.SetCountriesSource(dataProvider.Countries, includeGenerics: false);
        CountryClubComboBox.SelectedIndex = -1;
        ClubComboBox.Visibility = Visibility.Hidden;
    }

    private void PlayersView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var pItem = PlayersView.SelectedItem;
        if (pItem is not null)
        {
            MessageBox.Show("Soon!");
            //Hide();
            //new PlayerWindow(pItem as Player).ShowDialog();
            //ShowDialog();
        }
    }

    private void PositionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SetRatedPlayersListBox();
        SidesComboBox.Visibility = PositionsComboBox.SelectedIndex >= 0
            && (Position)PositionsComboBox.SelectedItem == Position.GoalKeeper
            ? Visibility.Hidden
            : Visibility.Visible;
    }

    private void SidesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SetRatedPlayersListBox();
    }

    private void SetRatedPlayersListBox()
    {
        if (PositionsComboBox.SelectedIndex < 0) return;

        var position = (Position)PositionsComboBox.SelectedItem;
        var side = Side.Center;
        if (position != Position.GoalKeeper)
        {
            if (SidesComboBox.SelectedIndex < 0) return;
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

    private void TacticsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TacticsComboBox.SelectedItem is not null)
        {
            SetTacticLineUp();
        }
    }

    private void SetTacticLineUp()
    {
        if (TacticsComboBox.SelectedItem is not Tactic tactic) return;

        TacticPlayersGrid.Children.Clear();

        var lineUp = tactic
            .GetBestLineUp([.. _players], _dataProvider.MaxTheoreticalRate, UsePotentialAbility);

        foreach (var posGroup in lineUp.GroupBy(_ => (_.Item1, _.Item2)))
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

                AddLineUpUiComponent(PlayerPositionTemplateKey, rowIndex, colIndex, posPlayer.Item3);

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
        ReloadPlayersUsingCurrentCriteriaSelection();
    }

    private void CountryClubComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CountryClubComboBox.SelectedItem is not Country country) return;

        _isSourceChange = true;

        var clubsList = _dataProvider.Clubs
            .Where(c => c.Country?.Id == country.Id)
            .OrderByDescending(x => x.Division?.Reputation)
            .ThenBy(x => x.Name)
            .ToList();
        clubsList.Insert(0, Club.Global);

        var clubsView = new ListCollectionView(clubsList);
        clubsView.GroupDescriptions.Add(new PropertyGroupDescription($"{nameof(Club.Division)}.{nameof(Competition.Name)}"));

        ClubComboBox.ItemsSource = clubsView;
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
        ReloadPlayersUsingCurrentCriteriaSelection();
    }

    private void NullRateBehaviorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ClearForms();
    }

    private Dictionary<int, PlayerRateUiData>.ValueCollection GetTopTenRatedPlayers()
    {
        if (_players is null) return null;

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
            if (finalList.Count == 10) break;
            finalList.TryAdd(p.Player.Id, p);
        }

        return finalList.Values;
    }

    private void PlayerLineUpButton_Click(object sender, MouseButtonEventArgs e)
    {
        if ((sender as FrameworkElement).DataContext is not PlayerRateUiData p) return;

        Hide();
        var win = new BestPlayerFinderWindow(_dataProvider,
            p.Position,
            p.Side,
            PotentialAbilityCheckBox.IsChecked == true);
        win.ShowDialog();

        var player = win.SelectedPlayer;
        if (player is not null && !_players.Any(x => x.Id == player.Id))
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

    private void ReloadPlayersUsingCurrentCriteriaSelection()
    {
        if (_isSourceChange) return;

        if (ClubComboBox.SelectedItem is not Club club) return;

        if (CountryClubComboBox.SelectedItem is not Country country) return;

        var potentialEnabled = UsePotentialAbility;

        LoadPlayersProgressBar.HideWorkAndDisplay(
            () => club.Id == BaseData.AllDataId
                ? _dataProvider.GetPlayersByCountry(country.Id, true, potentialEnabled)
                : _dataProvider.GetPlayersByClub(club.Id, potentialEnabled),
            p =>
            {
                _players = new ObservableCollection<Player>(p);
                Title = club.Id == BaseData.AllDataId ? country.Name : club.Name;
                PlayersView.ItemsSource = _players;
                ClearForms();
            },
            MainGrid.Children.Cast<UIElement>().ToArray());
    }
}
