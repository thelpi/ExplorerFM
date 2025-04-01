using System.Windows;
using ExplorerFM.Extensions;
using ExplorerFM.Providers;

namespace ExplorerFM.Windows
{
    public partial class IntroWindow : Window
    {
        private readonly DataProvider _dataProvider;

        public IntroWindow()
        {
            InitializeComponent();

            _dataProvider = new DataProvider();
            
            DatasLoadingProgressBar.HideWorkAndDisplay<object>(
                () =>
                {
                    _dataProvider.Initialize();
                    return null;
                },
                o => { },
                ClubExplorerButton, PlayersSearchButton, BestPlayersFinderButton);
        }

        private void ClubExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            var window = new ClubWindow(_dataProvider);
            window.ShowDialog();
            ShowDialog();
        }

        private void PlayersSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            var window = new SearchPlayersWindow(_dataProvider);
            window.ShowDialog();
            ShowDialog();
        }

        private void BestPlayersFinderButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            var window = new BestPlayerFinderWindow(_dataProvider);
            window.ShowDialog();
            ShowDialog();
        }
    }
}
