using System;
using System.Threading.Tasks;
using System.Windows;
using ExplorerFM.Properties;
using ExplorerFM.RuleEngine;

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
                nullDummy => { });
        }

        private void PlayersView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pItem = PlayersView.SelectedItem;
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

        private void SearchPlayersButton_Click(object sender, RoutedEventArgs e)
        {
            var criteria = ExtractCriteriaSet();
            HideWorkAndDisplay(
                () => _dataProvider.GetPlayersByCriteria(criteria),
                players => PlayersView.ItemsSource = players);
        }

        private void AddCriterion_Click(object sender, RoutedEventArgs e)
        {

        }

        private CriteriaSet ExtractCriteriaSet()
        {
            return new CriteriaSet(false,
                Criterion.New("Lastname", "Reid", Comparator.Contain),
                Criterion.New("Firstname", "Steven", Comparator.Contain));
            //return CriteriaSet.Empty;
        }
    }
}
