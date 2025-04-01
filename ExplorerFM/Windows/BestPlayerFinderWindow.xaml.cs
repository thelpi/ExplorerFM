using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ExplorerFM.Datas;
using ExplorerFM.Extensions;
using ExplorerFM.Providers;
using ExplorerFM.RuleEngine;
using ExplorerFM.UiDatas;

namespace ExplorerFM.Windows
{
    public partial class BestPlayerFinderWindow : Window
    {
        private bool _initialized;
        private List<PlayerRateUiData> _players = null;
        private readonly DataProvider _dataProvider;

        public Player SelectedPlayer { get; private set; }

        public BestPlayerFinderWindow(
            DataProvider dataProvider)
            : this(dataProvider, null, null, null, false)
        { }

        public BestPlayerFinderWindow(
            DataProvider dataProvider, Position position, Side side, bool usePotential)
            : this(dataProvider, position, side, usePotential, true)
        { }

        private BestPlayerFinderWindow(
            DataProvider dataProvider, Position? position, Side? side, bool? usePotential, bool forcedValues)
        {
            InitializeComponent();

            _dataProvider = dataProvider;

            PositionsComboBox.ItemsSource = Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = Enum.GetValues(typeof(Side));

            NationalityComboBox.SetCountriesSource(dataProvider.Countries);

            if (position.HasValue)
            {
                PositionsComboBox.SelectedItem = position.Value;
                PositionsComboBox.IsEnabled = !forcedValues;
            }
            PositionsComboBox.SelectionChanged += PositionsComboBox_SelectionChanged;

            if (side.HasValue)
            {
                SidesComboBox.SelectedItem = side.Value;
                SidesComboBox.IsEnabled = !forcedValues;
            }
            SidesComboBox.SelectionChanged += SidesComboBox_SelectionChanged;

            if (usePotential.HasValue)
            {
                PotentialAbilityCheckBox.IsChecked = usePotential;
                PotentialAbilityCheckBox.IsEnabled = !forcedValues;
            }
            PotentialAbilityCheckBox.Checked += PotentialAbilityCheckBox_CheckChanged;
            PotentialAbilityCheckBox.Unchecked += PotentialAbilityCheckBox_CheckChanged;

            if (!forcedValues)
            {
                PlayersGridView.Columns.RemoveAt(0);
            }

            foreach (var columnKvp in GuiExtensions.GetAttributeColumns(true, null))
            {
                PlayersGridView.Columns.Add(columnKvp.Key);
            }

            SearchPlayers();
        }

        private void SearchPlayers()
        {
            if (PositionsComboBox.SelectedIndex < 0) return;
            var position = (Position)PositionsComboBox.SelectedItem;

            if (position != Position.GoalKeeper && SidesComboBox.SelectedIndex < 0) return;
            var side = SidesComboBox.SelectedIndex > -1 ? (Side)SidesComboBox.SelectedItem : Side.Center;
            
            var potentialAbility = PotentialAbilityCheckBox.IsChecked == true;

            _initialized = false;

            var criteria = new List<CriterionBase>
            {
                new Criterion(typeof(Player), new[] { nameof(Player.Positions), position.ToString() }, 15, Comparator.GreaterEqual)
            };

            if (position != Position.GoalKeeper)
            {
                criteria.Add(new Criterion(typeof(Player), new[] { nameof(Player.Sides), side.ToString() }, 15, Comparator.GreaterEqual));
            }

            LoadPlayersProgressBar.HideWorkAndDisplay(
                () => _dataProvider
                    .GetPlayersByCriteria(new CriteriaSet(false, criteria.ToArray()), potentialAbility)
                    .Select(p => p.ToRateItemData(position, side, _dataProvider.MaxTheoreticalRate, potentialAbility))
                    .OrderByDescending(p => p.Rate)
                    .ToList(),
                players =>
                {
                    _players = players;
                    _initialized = true;
                    ApplyLocalFilters();
                },
                PlayersListView.Yield(CriteriaGrid.Children.Cast<UIElement>().ToArray()).ToArray());
        }

        private void SelectPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPlayer = ((sender as Button).DataContext as PlayerRateUiData).Player;
            Close();
        }

        private void PositionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SidesComboBox.Visibility = PositionsComboBox.SelectedIndex >= 0
                && (Position)PositionsComboBox.SelectedItem == Position.GoalKeeper
                ? Visibility.Hidden
                : Visibility.Visible;

            SearchPlayers();
        }

        private void SidesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchPlayers();
        }

        private void PotentialAbilityCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            SearchPlayers();
        }

        private void ApplyLocalFilters()
        {
            if (!_initialized) return;

            var countryId = NationalityComboBox.SelectedItem is Country country ? country.Id : BaseData.AllDataId;
            var noClubContract = NoClubContractCheckBox.IsChecked == true;
            var isUe = EuropeanUnionCheckBox.IsChecked == true;
            var maxAge = (int)Math.Floor(AgeSlider.HigherValue);
            var minAge = (int)Math.Floor(AgeSlider.LowerValue);
            var ageIsSet = AgeSlider.HigherValue != AgeSlider.Maximum || AgeSlider.LowerValue != AgeSlider.Minimum;
            PlayersListView.ItemsSource = _players
                .Where(x => (countryId == BaseData.AllDataId || x.Player.Nationality.Id == countryId || x.Player.SecondNationality?.Id == countryId)
                    && (!noClubContract || x.Player.ClubContract == null)
                    && (!isUe || x.Player.Nationality.IsEU || x.Player.SecondNationality?.IsEU == true)
                    && (!ValueIntUpDown.Value.HasValue || x.Player.Value <= ValueIntUpDown.Value.Value)
                    && (!ReputationIntUpDown.Value.HasValue || x.Player.CurrentReputation <= ReputationIntUpDown.Value.Value)
                    && (!ageIsSet || x.Player.GetAge() <= maxAge)
                    && (!ageIsSet || x.Player.GetAge() >= minAge))
                .ToList();
        }

        private void NationalityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private void NoClubContractCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private void EuropeanUnionCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private void ValueIntUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ApplyLocalFilters();
        }

        private void ReputationIntUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ApplyLocalFilters();
        }

        private void AgeSlider_HigherValueChanged(object sender, RoutedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private void AgeSlider_LowerValueChanged(object sender, RoutedEventArgs e)
        {
            ApplyLocalFilters();
        }
    }
}
