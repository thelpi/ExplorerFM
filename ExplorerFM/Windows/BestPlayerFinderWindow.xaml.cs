using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using ExplorerFM.Datas;
using ExplorerFM.Extensions;
using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Windows
{
    public partial class BestPlayerFinderWindow : Window
    {
        private const int MaxPlayersTake = 50;

        private readonly DataProvider _dataProvider;

        public BestPlayerFinderWindow(DataProvider dataProvider)
        {
            InitializeComponent();
            _dataProvider = dataProvider;
            PositionsComboBox.ItemsSource = Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = Enum.GetValues(typeof(Side));
            NullRateBehaviorComboBox.ItemsSource = Enum.GetValues(typeof(NullRateBehavior));
            NullRateBehaviorComboBox.SelectedItem = NullRateBehavior.Minimal;
            NationalityComboBox.ItemsSource = dataProvider.Countries.WithNullEntry();

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
                var position = (Position)PositionsComboBox.SelectedItem;
                var side = (Side)SidesComboBox.SelectedItem;
                var potentialAbility = PotentialAbilityCheckBox.IsChecked == true;
                var country = NationalityComboBox.SelectedItem as Country;
                var maxValue = ValueIntUpDown.Value;
                var maxRep = ReputationIntUpDown.Value;
                var maxAge = AgeDatePicker.SelectedDate;

                var criteria = new List<RuleEngine.Criterion>();
                if (NoClubContractCheckBox.IsChecked == true)
                {
                    var clubContractProp = typeof(Player).GetProperty(nameof(Player.ClubContract));
                    criteria.Add(
                        new RuleEngine.Criterion(
                            clubContractProp.GetCustomAttribute<FieldAttribute>(),
                            clubContractProp.DeclaringType,
                            RuleEngine.Comparator.Equal,
                            null, true, false));
                }
                if (country != null)
                {
                    var countryProp = typeof(Player).GetProperty(nameof(Player.Nationality));
                    criteria.Add(
                        new RuleEngine.Criterion(
                            countryProp.GetCustomAttribute<FieldAttribute>(),
                            countryProp.DeclaringType,
                            RuleEngine.Comparator.Equal,
                            country, false, false));
                }
                if (maxAge.HasValue)
                {
                    var dobProp = typeof(Player).GetProperty(nameof(Player.DateOfBirth));
                    criteria.Add(
                        new RuleEngine.Criterion(
                            dobProp.GetCustomAttribute<FieldAttribute>(),
                            dobProp.DeclaringType,
                            RuleEngine.Comparator.GreaterEqual,
                            maxAge.Value, false, true));
                }
                if (maxValue.HasValue)
                {
                    var valueProp = typeof(Player).GetProperty(nameof(Player.Value));
                    criteria.Add(
                        new RuleEngine.Criterion(
                            valueProp.GetCustomAttribute<FieldAttribute>(),
                            valueProp.DeclaringType,
                            RuleEngine.Comparator.LowerEqual,
                            maxValue.Value, false, true));
                }
                if (maxRep.HasValue)
                {
                    var repProp = typeof(Player).GetProperty(nameof(Player.CurrentReputation));
                    criteria.Add(
                        new RuleEngine.Criterion(
                            repProp.GetCustomAttribute<FieldAttribute>(),
                            repProp.DeclaringType,
                            RuleEngine.Comparator.LowerEqual,
                            maxRep.Value, false, true));
                }

                LoadPlayersProgressBar.HideWorkAndDisplay(
                    () =>
                    {
                        var players = _dataProvider.GetPlayersByCriteria(
                            new RuleEngine.CriteriaSet(false, criteria.ToArray()),
                            progress => Dispatcher.Invoke(() => LoadPlayersProgressBar.Value = progress * 100));
                        return players
                            .Select(p => p.ToRateItemData(
                                position, side, _dataProvider.MaxTheoreticalRate, potentialAbility))
                            .OrderByDescending(p => p.Rate)
                            .Take(MaxPlayersTake);
                    },
                    players => PlayersListView.ItemsSource = players,
                    (PlayersListView as UIElement).Yield(CriteriaGrid.Children.Cast<UIElement>().ToArray()).ToArray());
            }
        }
    }
}
