using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml;
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

            var properties = GetAttributeProperties<Datas.Player>()
                .Concat(GetAttributeProperties<Datas.Club>())
                .Concat(GetAttributeProperties<Datas.Country>())
                .Concat(GetAttributeProperties<Datas.Confederation>())
                .ToList();

            var lcv = new ListCollectionView(properties);
            lcv.GroupDescriptions.Add(new PropertyGroupDescription(nameof(System.Reflection.PropertyInfo.DeclaringType)));

            var propertyCombo = new ComboBox
            {
                Width = 100,
                DisplayMemberPath = "Name",
                ItemsSource = lcv
            };
            propertyCombo.GroupStyle.Add(
                new GroupStyle
                {
                    HeaderTemplate = LoadXaml("<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBlock Text=\"{Binding Name}\"/></DataTemplate>") as DataTemplate
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
                    comparatorCombo.ItemsSource = ComparatorByType((propertyCombo.SelectedItem as PropertyInfo).PropertyType).Select(_ => _.ToSymbol());
            };

            delButton.Click += (_1, _2) => CriteriaPanel.Children.Remove(panel);

            panel.Children.Add(propertyCombo);
            panel.Children.Add(comparatorCombo);
            panel.Children.Add(delButton);

            CriteriaPanel.Children.Add(panel);
        }

        private static List<PropertyInfo> GetAttributeProperties<T>()
        {
            return typeof(T).GetProperties().Where(p => p.GetCustomAttributes(typeof(FieldAttribute), true).Length > 0).ToList();
        }

        private CriteriaSet ExtractCriteriaSet()
        {
            return CriteriaSet.Empty;
        }

        private object LoadXaml(string xamlContent)
        {
            return XamlReader.Load(XmlReader.Create(new StringReader(xamlContent)));
        }

        private IEnumerable<Comparator> ComparatorByType(Type t)
        {
            var comparators = new List<Comparator>
            {
                Comparator.Equal,
                Comparator.NotEqual,
                Comparator.Greater,
                Comparator.GreaterEqual,
                Comparator.Lower,
                Comparator.LowerEqual
            };

            if (t == typeof(string))
            {
                comparators.Add(Comparator.Contain);
                comparators.Add(Comparator.NotContain);
            }
            else if (t == typeof(bool) || (!typeof(IComparable).IsAssignableFrom(t)
                && !(t.IsGenericType && typeof(IComparable).IsAssignableFrom(t.GenericTypeArguments[0]))))
            {
                comparators.Remove(Comparator.Greater);
                comparators.Remove(Comparator.GreaterEqual);
                comparators.Remove(Comparator.Lower);
                comparators.Remove(Comparator.LowerEqual);
            }

            return comparators;
        }
    }
}
