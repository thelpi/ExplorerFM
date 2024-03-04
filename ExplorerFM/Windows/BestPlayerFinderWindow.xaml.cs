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
            collectionCopy.Insert(0, new Country { Id = Country.NoCountryId, Name = "No country" });
            collectionCopy.Insert(0, new Country { Id = Country.AllCountryId, Name = "All country" });
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

                var noClubCriterion = new Criterion
                {
                    Comparator = Comparator.Equal,
                    FieldName = "club",
                    FieldValue = null,
                    IncludeNullValue = false
                };

                var criteria = new List<CriterionBase>(5);
                if (noClubContract)
                {
                    criteria.Add(noClubCriterion);
                }
                if (NationalityComboBox.SelectedItem is Country country && country.Id != Country.AllCountryId)
                {
                    if (country.Id == Country.NoCountryId)
                    {
                        criteria.Add(new CriteriaSet(false, new Criterion
                        {
                            FieldName = "country1",
                            FieldValue = null,
                            Comparator = Comparator.Equal
                        }, new Criterion
                        {
                            FieldName = "country2",
                            FieldValue = null,
                            Comparator = Comparator.Equal
                        }));
                    }
                    else
                    {
                        criteria.Add(new CriteriaSet(true, new Criterion
                        {
                            FieldName = "country1._id",
                            FieldValue = country.Id,
                            Comparator = Comparator.Equal,
                            IncludeNullValue = false
                        }, new Criterion
                        {
                            FieldName = "country2._id",
                            FieldValue = country.Id,
                            Comparator = Comparator.Equal,
                            IncludeNullValue = false
                        }));
                    }
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
                    var standardCriterion = new Criterion
                    {
                        Comparator = Comparator.LowerEqual,
                        FieldName = "value",
                        FieldValue = maxValue.Value,
                        IncludeNullValue = true
                    };
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
                var side = SidesComboBox.SelectedIndex > -1 ? (Side)SidesComboBox.SelectedItem : Side.Center;
                var potentialAbility = PotentialAbilityCheckBox.IsChecked == true;

                if (position != Position.GoalKeeper)
                {
                    criteria.Add(new Criterion
                    {
                        Comparator = Comparator.GreaterEqual,
                        FieldName = $"playerSides.{(side == Side.Right ? "right" : (side == Side.Center ? "center" : "left"))}",
                        FieldValue = 15
                    });
                }

                var posName = "";
                switch (position)
                {
                    case Position.Defender: posName = "defender"; break;
                    case Position.Sweeper: posName = "sweeper"; break;
                    case Position.GoalKeeper: posName = "goalKeeper"; break;
                    case Position.DefensiveMidfielder: posName = "defMidfielder"; break;
                    case Position.Midfielder: posName = "midfielder"; break;
                    case Position.OffensiveMidfielder: posName = "offMidfielder"; break;
                    case Position.WingBack: posName = "wingBack"; break;
                    case Position.Striker: posName = "forward"; break;
                    case Position.FreeRole: posName = "freeRole"; break;
                }

                criteria.Add(new Criterion
                {
                    Comparator = Comparator.GreaterEqual,
                    FieldName = $"playerPositions.{posName}",
                    FieldValue = 15
                });

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
    }
}
