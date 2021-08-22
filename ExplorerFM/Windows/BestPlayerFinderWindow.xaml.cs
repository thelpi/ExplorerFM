using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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
        }

        private void PositionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchPlayersAndSetSource();
        }

        private void SidesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchPlayersAndSetSource();
        }

        private void NullRateBehaviorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchPlayersAndSetSource();
        }

        private void PotentialAbilityCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SearchPlayersAndSetSource();
        }

        private void NoClubContractCheckBox_Click(object sender, RoutedEventArgs e)
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

                var criteriaSet = RuleEngine.CriteriaSet.Empty;
                if (NoClubContractCheckBox.IsChecked == true)
                {
                    var clubContractProp = typeof(Player).GetProperty(nameof(Player.ClubContract));
                    criteriaSet = new RuleEngine.CriteriaSet(false,
                        new RuleEngine.Criterion(
                            clubContractProp.GetCustomAttribute<FieldAttribute>(),
                            clubContractProp.DeclaringType,
                            RuleEngine.Comparator.Equal,
                            null, true, false));
                }

                LoadPlayersProgressBar.HideWorkAndDisplay(
                    () =>
                    {
                        var players = _dataProvider.GetPlayersByCriteria(
                            criteriaSet,
                            progress => Dispatcher.Invoke(() => LoadPlayersProgressBar.Value = progress * 100));
                        return players
                            .Select(p => p.ToRateItemData(
                                position, side, _dataProvider.MaxTheoreticalRate, potentialAbility))
                            .OrderByDescending(p => p.Rate)
                            .Take(MaxPlayersTake);
                    },
                    players => PlayersListView.ItemsSource = players,
                    PositionsComboBox, SidesComboBox, NullRateBehaviorComboBox,
                    PotentialAbilityCheckBox, NoClubContractCheckBox, PlayersListView);
            }
        }
    }
}
