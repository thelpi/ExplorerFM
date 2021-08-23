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
            NationalityComboBox.ItemsSource = dataProvider.Countries;

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
