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
        private readonly DataProvider _dataProvider;
        public Player SelectedPlayer { get; private set; }

        private bool _initialized;

        private List<PlayerRateUiData> _players = null;

        public BestPlayerFinderWindow(DataProvider dataProvider)
            : this(dataProvider, null, null, null)
        { }

        public BestPlayerFinderWindow(DataProvider dataProvider,
            Position? position, Side? side, bool? usePotential)
        {
            InitializeComponent();
            _dataProvider = dataProvider;
            PositionsComboBox.ItemsSource = Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = Enum.GetValues(typeof(Side));

            var collectionCopy = new List<Country>(dataProvider.Countries.OrderBy(x => x.Name));
            collectionCopy.Insert(0, Country.Empty);
            collectionCopy.Insert(0, Country.Global);
            NationalityComboBox.ItemsSource = collectionCopy;

            if (position.HasValue)
            {
                PositionsComboBox.SelectedItem = position.Value;
                PositionsComboBox.IsEnabled = false;
            }

            if (side.HasValue)
            {
                SidesComboBox.SelectedItem = side.Value;
                SidesComboBox.IsEnabled = false;
            }

            if (usePotential.HasValue)
            {
                PotentialAbilityCheckBox.IsChecked = usePotential;
                PotentialAbilityCheckBox.IsEnabled = false;
            }

            foreach (var columnKvp in GuiExtensions.GetAttributeColumns(true, null))
                PlayersGridView.Columns.Add(columnKvp.Key);

            SearchPlayersAndSetSource();
        }

        private void SearchPlayersAndSetSource()
        {
            _initialized = false;
            if (PositionsComboBox.SelectedIndex >= 0 && (SidesComboBox.SelectedIndex >= 0 || (Position)PositionsComboBox.SelectedItem == Position.GoalKeeper))
            {
                var position = (Position)PositionsComboBox.SelectedItem;
                var side = SidesComboBox.SelectedIndex > -1 ? (Side)SidesComboBox.SelectedItem : Side.Center;
                var potentialAbility = PotentialAbilityCheckBox.IsChecked == true;

                var criteria = new List<CriterionBase>
                {
                    new Criterion(typeof(Player), new[] { nameof(Player.Positions), position.ToString() }, 15, Comparator.GreaterEqual)
                };
                if (position != Position.GoalKeeper)
                {
                    criteria.Add(new Criterion(typeof(Player), new[] { nameof(Player.Sides), side.ToString() }, 15, Comparator.GreaterEqual));
                }

                var task = LoadPlayersProgressBar.HideWorkAndDisplay(
                    () =>
                    {
                        var players = _dataProvider.GetPlayersByCriteria(new CriteriaSet(false, criteria.ToArray()), potentialAbility);
                        return players
                            .Select(p => p.ToRateItemData(
                                position, side, _dataProvider.MaxTheoreticalRate, potentialAbility))
                            .OrderByDescending(p => p.Rate)
                            .ToList();
                    },
                    players =>
                    {
                        _players = players;
                        _initialized = true;
                        ApplyLocalFilters();
                    },
                    PlayersListView.Yield(CriteriaGrid.Children.Cast<UIElement>().ToArray()).ToArray());
            }
            else
            {
                MessageBox.Show("Positioning is mandatory.", "ExplorerFM - Warning");
            }
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

        private void NoClubContractCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private void NoClubContractCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private void EuropeanUnionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private void EuropeanUnionCheckBox_Unchecked(object sender, RoutedEventArgs e)
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
