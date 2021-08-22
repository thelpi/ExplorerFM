using System.Windows;
using ExplorerFM.Extensions;
using ExplorerFM.Properties;

namespace ExplorerFM.Windows
{
    public partial class IntroWindow : Window
    {
        private readonly DataProvider _dataProvider;

        public IntroWindow()
        {
            InitializeComponent();

            _dataProvider = new DataProvider(Settings.Default.ConnectionString);
            
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
            new ClubWindow(_dataProvider, null).ShowDialog();
            ShowDialog();
        }

        private void PlayersSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            new MainWindow(_dataProvider).ShowDialog();
            ShowDialog();
        }

        private void BestPlayersFinderButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }
    }
}
