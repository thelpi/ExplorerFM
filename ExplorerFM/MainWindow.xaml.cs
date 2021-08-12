using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var properties = typeof(Datas.Player).GetAttributeProperties()
                .Concat(typeof(Datas.Club).GetAttributeProperties())
                .Concat(typeof(Datas.Country).GetAttributeProperties())
                .Concat(typeof(Datas.Confederation).GetAttributeProperties())
                .ToList();

            var lcv = new ListCollectionView(properties);
            lcv.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PropertyInfo.DeclaringType)));

            var propertyCombo = new ComboBox
            {
                Width = 100,
                DisplayMemberPath = "Name",
                ItemsSource = lcv
            };
            propertyCombo.GroupStyle.Add(
                new GroupStyle
                {
                    HeaderTemplate = "<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBlock Text=\"{Binding Name}\"/></DataTemplate>".ToXaml<DataTemplate>()
                });

            var comparatorCombo = new ComboBox
            {
                Width = 50,
                ItemsSource = Enumerable.Empty<string>()
            };

            var delButton = new Button
            {
                Content = "x",
                Width = 25,
                Height = 25
            };

            propertyCombo.SelectionChanged += (_1, _2) =>
            {
                if (propertyCombo.SelectedIndex < 0)
                    comparatorCombo.ItemsSource = Enumerable.Empty<string>();
                else
                    comparatorCombo.ItemsSource = (propertyCombo.SelectedItem as PropertyInfo).PropertyType.GetComparators().Select(_ => _.ToSymbol());
            };

            delButton.Click += (_1, _2) => CriteriaPanel.Children.Remove(panel);

            panel.Children.Add(propertyCombo);
            panel.Children.Add(comparatorCombo);
            panel.Children.Add(delButton);

            CriteriaPanel.Children.Add(panel);
        }

        private CriteriaSet ExtractCriteriaSet()
        {
            return CriteriaSet.Empty;
        }
    }
}
