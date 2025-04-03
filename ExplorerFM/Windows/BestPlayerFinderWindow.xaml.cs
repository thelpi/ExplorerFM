using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ExplorerFM.Datas;
using ExplorerFM.Extensions;
using ExplorerFM.Providers;
using ExplorerFM.RuleEngine;
using ExplorerFM.UiDatas;
using MongoDB.Driver.Linq;

namespace ExplorerFM.Windows
{
    public partial class BestPlayerFinderWindow : Window
    {
        private bool _initialized;
        private List<PlayerRateUiData> _players = null;
        private readonly List<SelectablePositioning<Side>> _alternativeSides;
        private readonly List<SelectablePositioning<Position>> _alternativePositions;
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

            _alternativePositions = Enum.GetValues(typeof(Position))
                .Cast<Position>()
                .Select(x => new SelectablePositioning<Position>(x, x == position, x != position))
                .ToList();
            _alternativeSides = Enum.GetValues(typeof(Side))
                .Cast<Side>()
                .Select(x => new SelectablePositioning<Side>(x, x == side, x != side))
                .ToList();

            AltPositionsList.ItemsSource = _alternativePositions;
            AltSidesList.ItemsSource = _alternativeSides;

            PositionsComboBox.ItemsSource = Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = Enum.GetValues(typeof(Side));

            NationalityComboBox.SetCountriesSource(dataProvider.Countries);
            ClubCountryComboBox.SetCountriesSource(dataProvider.Countries);

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

            ValueIntUpDown.ItemsSource = new List<ValueThreshold>
            {
                new ValueThreshold("None (any)"),
                new ValueThreshold(10000000),
                new ValueThreshold(5000000),
                new ValueThreshold(1000000),
                new ValueThreshold(500000),
                new ValueThreshold(100000),
                new ValueThreshold(50000),
                new ValueThreshold(10000),
                new ValueThreshold(10000, true),
                new ValueThreshold(100000, true),
                new ValueThreshold(1000000, true),
                new ValueThreshold(10000000, true),
            };
            ValueIntUpDown.SelectedIndex = 0;
            ValueIntUpDown.SelectionChanged += ValueIntUpDown_SelectionChanged;

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
            
            foreach (var pos in _alternativePositions)
            {
                pos.Selected = false;
                pos.SetSelectable(PositionsComboBox.SelectedIndex < 0 || (Position)PositionsComboBox.SelectedItem != pos.Value);
            }

            SearchPlayers();
        }

        private void SidesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var side in _alternativeSides)
            {
                side.Selected = false;
                side.SetSelectable(SidesComboBox.SelectedIndex < 0 || (Side)SidesComboBox.SelectedItem != side.Value);
            }

            SearchPlayers();
        }

        private void PotentialAbilityCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            SearchPlayers();
        }

        private void ApplyLocalFilters()
        {
            if (!_initialized) return;

            var players = _players.AsEnumerable();

            var countryId = NationalityComboBox.SelectedItem is Country country ? country.Id : BaseData.AllDataId;
            if (countryId != BaseData.AllDataId)
            {
                players = players.Where(x => x.Player.Nationality.Id == countryId || x.Player.SecondNationality?.Id == countryId);
            }

            if (EuropeanUnionCheckBox.IsChecked == true)
            {
                players = players.Where(x => x.Player.Nationality.IsEU || x.Player.SecondNationality?.IsEU == true);
            }

            var clubCountryId = ClubCountryComboBox.SelectedItem is Country clubCountry ? clubCountry.Id : BaseData.AllDataId;
            if (clubCountryId != BaseData.AllDataId)
            {
                players = clubCountryId == BaseData.NoDataId
                    ? players.Where(x => x.Player.ClubContract?.Country == null)
                    : players.Where(x => x.Player.ClubContract?.Country?.Id == clubCountryId);
            }

            var maxValueItem = (ValueThreshold)ValueIntUpDown.SelectedItem;
            if (!maxValueItem.IsAny)
            {
                players = maxValueItem.IsGreater
                    ? players.Where(x => x.Player.Value >= maxValueItem.Value)
                    : players.Where(x => x.Player.Value < maxValueItem.Value);
            }

            players = players.Where(x => x.Player.WorldReputation <= ReputationIntUpDown.HigherValue);
            players = players.Where(x => x.Player.WorldReputation >= ReputationIntUpDown.LowerValue);

            players = players.Where(x => x.Player.GetAge() <= (int)Math.Floor(AgeSlider.HigherValue));
            players = players.Where(x => x.Player.GetAge() >= (int)Math.Floor(AgeSlider.LowerValue));

            foreach (var pos in _alternativePositions.Where(x => x.Selected && x.Selectable))
            {
                players = players.Where(x => x.Player.Positions[pos.Value] >= Player.PositioningTolerance);
            }

            foreach (var side in _alternativeSides.Where(x => x.Selected && x.Selectable))
            {
                players = players.Where(x => x.Player.Sides[side.Value] >= Player.PositioningTolerance);
            }

            PlayersListView.ItemsSource = players;
        }

        private void NationalityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private void EuropeanUnionCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private void ReputationIntUpDown_ValueChanged(object sender, RoutedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private void AgeSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private void ClubCountryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private void ValueIntUpDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyLocalFilters();
        }

        private class SelectablePositioning<T> : INotifyPropertyChanged
        {
            private bool _selected;
            private bool _selectable;

            public T Value { get; }

            public bool Selected
            {
                get
                {
                    return _selected;
                }
                set
                {
                    _selected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selected)));
                }
            }

            public bool Selectable
            {
                get
                {
                    return _selectable;
                }
                private set
                {
                    _selectable = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selectable)));
                }
            }

            public SelectablePositioning(T value, bool selected, bool selectable)
            {
                Value = value;
                Selected = selected;
                Selectable = selectable;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public void SetSelectable(bool selectable)
            {
                if (Selectable != selectable)
                {
                    Selectable = selectable;
                    if (!selectable && !Selected)
                    {
                        Selected = true;
                    }
                }
            }
        }

        private readonly struct ValueThreshold
        {
            public bool IsGreater { get; }

            public int Value { get; }

            public string Content { get; }

            public bool IsAny { get; }

            public ValueThreshold(int value, bool isGreater = false)
            {
                IsAny = false;
                IsGreater = isGreater;
                Value = value;
                Content = $"{(IsGreater ? ">=" : "<")} {(Value >= 1000000 ? $"{Value / 1000000} M£" : (Value >= 1000 ? $"{Value / 1000} K£" : $"{Value} £"))}";
            }

            public ValueThreshold(string content)
            {
                IsAny = true;
                IsGreater = false;
                Value = 0;
                Content = content;
            }
        }
    }
}
