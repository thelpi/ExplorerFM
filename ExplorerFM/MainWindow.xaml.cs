using System;
using System.Threading.Tasks;
using System.Windows;
using ExplorerFM.Properties;

namespace ExplorerFM
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DataProvider _dataProvider;

        public MainWindow()
        {
            InitializeComponent();

            _dataProvider = new DataProvider(Settings.Default.ConnectionString);

            HideWorkAndDisplay<object>(
                () =>
                {
                    _dataProvider.Initialize();
                    return null;
                },
                nullDummy =>
                {
                    AttributesComboBox.ItemsSource = _dataProvider.Attributes;
                    ClubsComboBox.ItemsSource = _dataProvider.Clubs;
                    ConfederationsComboBox.ItemsSource = _dataProvider.Confederations;
                    CountriesComboBox.ItemsSource = _dataProvider.Countries;
                });
        }

        private void PlayersListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pItem = PlayersListBox.SelectedItem;
            if (pItem != null)
            {
                new PlayerWindow(pItem as Datas.Player).ShowDialog();
            }
        }

        private void HideWorkAndDisplay<T>(Func<T> backgroundFunc, Action<T> foregroundFunc)
        {
            MainContentPanel.Visibility = Visibility.Collapsed;
            LoadingProgressBar.Visibility = Visibility.Visible;
            Task.Run(() =>
            {
                var result = backgroundFunc();
                Dispatcher.Invoke(() =>
                {
                    foregroundFunc(result);
                    MainContentPanel.Visibility = Visibility.Visible;
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                });
            });
        }
    }
}
