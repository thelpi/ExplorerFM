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
            ChangeWindow<ClubWindow>();
        }

        private void PlayersSearchButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeWindow<SearchPlayersWindow>();
        }

        private void BestPlayersFinderButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeWindow<BestPlayerFinderWindow>();
        }

        private void CountryExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            var window = new ClubWindow(_dataProvider, true, null, null);
            window.ShowDialog();
            ShowDialog();
        }

        private void ChangeWindow<T>() where T : Window
        {
            Hide();
            var window = typeof(T)
                .GetConstructor(new[] { typeof(DataProvider) })
                .Invoke(new[] { _dataProvider }) as T;
            window.ShowDialog();
            ShowDialog();
        }
    }
}
