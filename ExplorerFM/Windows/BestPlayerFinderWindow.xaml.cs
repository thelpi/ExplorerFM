using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using ExplorerFM.Datas;
using ExplorerFM.Datas.Dtos;
using ExplorerFM.Extensions;
using ExplorerFM.FieldsAttributes;
using ExplorerFM.RuleEngine;
using ExplorerFM.UiDatas;

namespace ExplorerFM.Windows
{
    public partial class BestPlayerFinderWindow : Window
    {
        private const int MaxPlayersTake = 1000;

        private readonly DataProvider _dataProvider;
        public Player SelectedPlayer { get; private set; }

        public BestPlayerFinderWindow(DataProvider dataProvider,
            Position? position = null, Side? side = null, NullRateBehavior? nullRateBehavior = null, bool? usePotential = null)
        {
            InitializeComponent();
            _dataProvider = dataProvider;
            PositionsComboBox.ItemsSource = Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = Enum.GetValues(typeof(Side));
            NullRateBehaviorComboBox.ItemsSource = Enum.GetValues(typeof(NullRateBehavior));
            NationalityComboBox.ItemsSource = dataProvider.Countries.WithNullEntry();

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
            if (PositionsComboBox.SelectedIndex >= 0 && SidesComboBox.SelectedIndex >= 0)
            {
                var maxValue = ValueIntUpDown.Value;
                var maxRep = ReputationIntUpDown.Value;
                var maxAge = AgeDatePicker.SelectedDate;
                var isUe = EuropeanUnionCheckBox.IsChecked == true;
                var noClubContract = NoClubContractCheckBox.IsChecked == true;

                var criteria = new List<CriterionBase>();
                if (noClubContract)
                {
                    criteria.Add(new Criterion
                    {
                        Comparator = Comparator.Equal,
                        FieldName = "club",
                        FieldValue = null,
                        IncludeNullValue = false
                    });
                }
                if (NationalityComboBox.SelectedItem is Country country)
                {
                    criteria.Add(new CriteriaSet(true, new Criterion
                    {
                        FieldName = "country1.id",
                        FieldValue = country.Id,
                        Comparator = Comparator.Equal,
                        IncludeNullValue = false
                    }, new Criterion
                    {
                        FieldName = "country2.id",
                        FieldValue = country.Id,
                        Comparator = Comparator.Equal,
                        IncludeNullValue = false
                    }));
                }
                if (maxAge.HasValue)
                {
                    criteria.Add(new CriteriaSet(true, new Criterion
                    {
                        Comparator = Comparator.GreaterEqual,
                        FieldName = "dateOfBirth",
                        FieldValue = maxAge.Value,
                        IncludeNullValue = false
                    }, new Criterion
                    {
                        Comparator = Comparator.GreaterEqual,
                        FieldName = "yearOfBirth",
                        FieldValue = maxAge.Value.Year,
                        IncludeNullValue = false
                    }));
                }
                if (maxValue.HasValue)
                {
                    criteria.Add(new Criterion
                    {
                        Comparator = Comparator.LowerEqual,
                        FieldName = "value",
                        FieldValue = maxValue.Value,
                        IncludeNullValue = true
                    });
                }
                if (maxRep.HasValue)
                {
                    criteria.Add(new Criterion
                    {
                        Comparator = Comparator.LowerEqual,
                        FieldName = "playerFeatures.currentReputation",
                        FieldValue = maxRep.Value,
                        IncludeNullValue = true
                    });
                }
                if (isUe)
                {
                    criteria.Add(new CriteriaSet(true, new Criterion
                    {
                        Comparator = Comparator.Equal,
                        FieldName = "country1.isEU",
                        FieldValue = true,
                        IncludeNullValue = false
                    }, new Criterion
                    {
                        Comparator = Comparator.Equal,
                        FieldName = "country2.isEU",
                        FieldValue = true,
                        IncludeNullValue = false
                    }));
                }

                var position = (Position)PositionsComboBox.SelectedItem;
                var side = (Side)SidesComboBox.SelectedItem;
                var potentialAbility = PotentialAbilityCheckBox.IsChecked == true;

                LoadPlayersProgressBar.HideWorkAndDisplay(
                    () =>
                    {
                        var players = _dataProvider.GetPlayersByCriteria(
                            new CriteriaSet(false, criteria.ToArray()),
                            progress => Dispatcher.Invoke(() => LoadPlayersProgressBar.Value = progress * 100));
                        return players
                            .Select(p => p.ToRateItemData(
                                position, side, _dataProvider.MaxTheoreticalRate, potentialAbility))
                            .OrderByDescending(p => p.Rate)
                            .Take(MaxPlayersTake);
                    },
                    players => PlayersListView.ItemsSource = players,
                    PlayersListView.Yield(CriteriaGrid.Children.Cast<UIElement>().ToArray()).ToArray());
            }
        }

        private void SelectPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPlayer = ((sender as Button).DataContext as PlayerRateUiData).Player;
            Close();
        }
    }
}
