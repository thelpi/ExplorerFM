using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ExplorerFM.Datas;

namespace ExplorerFM
{
    /// <summary>
    /// Logique d'interaction pour ClubWindow.xaml
    /// </summary>
    public partial class ClubWindow : Window
    {
        private readonly Club _club;
        private readonly List<Player> _players;
        private readonly DataProvider _dataProvider;

        public ClubWindow(DataProvider dataProvider, Club club, List<Player> players)
        {
            InitializeComponent();
            _dataProvider = dataProvider;
            _club = club;
            _players = players;
            DataContext = _club;
            PlayersView.ItemsSource = _players;
        }

        private void PlayersView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var pItem = PlayersView.SelectedItem;
            if (pItem != null)
            {
                new PlayerWindow(pItem as Player).ShowDialog();
            }
        }
    }
}
