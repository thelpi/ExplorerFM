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
        }

        private void InitializeButton_Click(object sender, RoutedEventArgs e)
        {
            _dataProvider.Initialize();
            AttributesComboBox.ItemsSource = _dataProvider.Attributes;
            ClubsComboBox.ItemsSource = _dataProvider.Clubs;
            ConfederationsComboBox.ItemsSource = _dataProvider.Confederations;
            CountriesComboBox.ItemsSource = _dataProvider.Countries;
            PlayersListBox.ItemsSource = _dataProvider.GetPlayersByCriteria(withoutClub: true);
        }
    }
}
