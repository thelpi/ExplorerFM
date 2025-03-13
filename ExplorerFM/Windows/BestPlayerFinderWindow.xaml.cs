using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ExplorerFM.Datas;
using ExplorerFM.Extensions;
using ExplorerFM.RuleEngine;
using ExplorerFM.UiDatas;

namespace ExplorerFM.Windows
{
    public partial class BestPlayerFinderWindow : Window
    {
        private const int MaxPlayersTake = 1000;

        private readonly DataProvider _dataProvider;
        public Player SelectedPlayer { get; private set; }

        public BestPlayerFinderWindow(DataProvider dataProvider)
            : this(dataProvider, null, null, null, null)
        { }

        public BestPlayerFinderWindow(DataProvider dataProvider,
            Position? position, Side? side, NullRateBehavior? nullRateBehavior, bool? usePotential)
        {
            InitializeComponent();
            _dataProvider = dataProvider;
            PositionsComboBox.ItemsSource = Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = Enum.GetValues(typeof(Side));
            NullRateBehaviorComboBox.ItemsSource = Enum.GetValues(typeof(NullRateBehavior));

            var collectionCopy = new List<Country>(dataProvider.Countries);
            collectionCopy.Insert(0, Country.Empty);
            collectionCopy.Insert(0, Country.Global);
            NationalityComboBox.ItemsSource = collectionCopy;

            if (nullRateBehavior.HasValue)
            {
                NullRateBehaviorComboBox.SelectedItem = nullRateBehavior.Value;
                NullRateBehaviorComboBox.IsEnabled = false;
            }
            else
                NullRateBehaviorComboBox.SelectedItem = NullRateBehavior.Minimal;

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
        }

        private void SearchPlayersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchPlayersAndSetSource();
        }

        private void SearchPlayersAndSetSource()
        {
            if (PositionsComboBox.SelectedIndex >= 0 && (SidesComboBox.SelectedIndex >= 0 || (Position)PositionsComboBox.SelectedItem == Position.GoalKeeper))
            {
                var maxValue = ValueIntUpDown.Value;
                var maxRep = ReputationIntUpDown.Value;
                var maxAge = AgeDatePicker.SelectedDate;
                var isUe = EuropeanUnionCheckBox.IsChecked == true;
                var noClubContract = NoClubContractCheckBox.IsChecked == true;

                var noClubCriterion = new Criterion(typeof(Staff), nameof(Staff.ClubContract), null);

                var criteria = new List<CriterionBase>(5);
                if (noClubContract)
                {
                    criteria.Add(noClubCriterion);
                }

                if (NationalityComboBox.SelectedItem is Country country && country.Id != BaseData.AllDataId)
                {
                    if (country.Id == BaseData.NoDataId)
                    {
                        criteria.Add(new CriteriaSet(false,
                            new Criterion(typeof(Staff), nameof(Staff.Nationality), null),
                            new Criterion(typeof(Staff), nameof(Staff.SecondNationality), null)));
                    }
                    else
                    {
                        criteria.Add(new CriteriaSet(true,
                            new Criterion(typeof(Staff), new[] { nameof(Staff.Nationality), nameof(Country.Id) }, country.Id),
                            new Criterion(typeof(Staff), new[] { nameof(Staff.SecondNationality), nameof(Country.Id) }, country.Id)));
                    }
                }

                if (maxAge.HasValue)
                {
                    criteria.Add(new CriteriaSet(true,
                        new Criterion(typeof(Staff), nameof(Staff.DateOfBirth), maxAge.Value, Comparator.GreaterEqual),
                        new Criterion(typeof(Staff), nameof(Staff.YearOfBirth), maxAge.Value.Year, Comparator.GreaterEqual)));
                }

                if (maxValue.HasValue)
                {
                    var standardCriterion = new Criterion(typeof(Staff), nameof(Staff.Value), maxValue.Value, Comparator.LowerEqual, true);
                    if (noClubContract)
                    {
                        criteria.Add(standardCriterion);
                    }
                    else
                    {
                        // some player have value while being free agent
                        criteria.Add(new CriteriaSet(true, standardCriterion, noClubCriterion));
                    }
                }

                if (maxRep.HasValue)
                {
                    criteria.Add(new Criterion(typeof(Staff), nameof(Staff.CurrentReputation), maxRep.Value, Comparator.LowerEqual, true));
                }

                if (isUe)
                {
                    criteria.Add(new CriteriaSet(true,
                        new Criterion(typeof(Staff), new[] { nameof(Staff.Nationality), nameof(Country.IsEU) }, true),
                        new Criterion(typeof(Staff), new[] { nameof(Staff.SecondNationality), nameof(Country.IsEU) }, true)));
                }

                var position = (Position)PositionsComboBox.SelectedItem;
                var side = SidesComboBox.SelectedIndex > -1 ? (Side)SidesComboBox.SelectedItem : Side.Center;
                var potentialAbility = PotentialAbilityCheckBox.IsChecked == true;

                if (position != Position.GoalKeeper)
                {
                    criteria.Add(new Criterion(typeof(Player), new[] { nameof(Player.Sides), side.ToString() }, 15, Comparator.GreaterEqual));
                }

                criteria.Add(new Criterion(typeof(Player), new[] { nameof(Player.Positions), position.ToString() }, 15, Comparator.GreaterEqual));

                var nullRateBehavior = NullRateBehaviorComboBox.SelectedIndex > -1 ? (NullRateBehavior)NullRateBehaviorComboBox.SelectedItem : NullRateBehavior.Minimal;

                LoadPlayersProgressBar.HideWorkAndDisplay(
                    () =>
                    {
                        var players = _dataProvider.GetPlayersByCriteria(new CriteriaSet(false, criteria.ToArray()));
                        return players
                            .Select(p => p.ToRateItemData(
                                position, side, _dataProvider.MaxTheoreticalRate, potentialAbility, nullRateBehavior))
                            .OrderByDescending(p => p.Rate)
                            .Take(MaxPlayersTake);
                    },
                    players => PlayersListView.ItemsSource = players,
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
            if (PositionsComboBox.SelectedIndex >= 0 && (Position)PositionsComboBox.SelectedItem == Position.GoalKeeper)
            {
                SidesComboBox.Visibility = Visibility.Hidden;
            }
            else
            {
                SidesComboBox.Visibility = Visibility.Visible;
            }
        }
    }
}
