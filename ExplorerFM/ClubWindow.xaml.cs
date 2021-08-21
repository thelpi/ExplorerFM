using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExplorerFM.Datas;

namespace ExplorerFM
{
    /// <summary>
    /// Logique d'interaction pour ClubWindow.xaml
    /// </summary>
    public partial class ClubWindow : Window
    {
        private const string PlayerPositionTemplateKey = "PlayerPositionTemplate";
        private const string PlayerNameTemplateKey = "PlayerNameTemplate";
        private const string NoClub = "Without club";
        
        private readonly List<Player> _players;
        private readonly DataProvider _dataProvider;

        public ClubWindow(DataProvider dataProvider, Club club, List<Player> players)
        {
            InitializeComponent();

            _dataProvider = dataProvider;
            _players = players;

            Title = club?.LongName ?? NoClub;
            PlayersView.ItemsSource = _players;
            PositionsComboBox.ItemsSource = Enum.GetValues(typeof(Position));
            SidesComboBox.ItemsSource = Enum.GetValues(typeof(Side));
            TacticsComboBox.ItemsSource = Tactic.Tactics;
        }

        private void PlayersView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var pItem = PlayersView.SelectedItem;
            if (pItem != null)
            {
                Hide();
                new PlayerWindow(pItem as Player).ShowDialog();
                ShowDialog();
            }
        }

        private void PositionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetRatedPlayersListBox();
        }

        private void SidesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetRatedPlayersListBox();
        }

        private void SetRatedPlayersListBox()
        {
            if (PositionsComboBox.SelectedIndex >= 0
                && SidesComboBox.SelectedIndex >= 0)
            {
                RatedPlayersListBox.ItemsSource = GetRatedPlayers(
                    (Position)PositionsComboBox.SelectedItem,
                    (Side)SidesComboBox.SelectedItem);
            }
        }

        private void TacticsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TacticsComboBox.SelectedItem != null)
            {
                // TODO
                TacticInfoLabel.Content = "Total value: xxx";
            }
        }

        private List<PlayerRateItemData> GetRatedPlayers(Position position, Side side)
        {
            return _players
                .Select(p => p.ToRateItemData(position, side, _dataProvider.Attributes.Count))
                .OrderByDescending(p => p.Rate)
                .ToList();
        }
    }
}
