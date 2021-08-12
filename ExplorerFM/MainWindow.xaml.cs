using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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
            var sourcePanel = (sender as Button).Tag as Panel;

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

            panel.Children.Add(propertyCombo);
            panel.Children.Add(comparatorCombo);

            void SelectionChangedEvent(object _1, SelectionChangedEventArgs _2)
            {
                if (panel.Tag != null)
                {
                    panel.Children.Remove(panel.Tag as UIElement);
                }

                if (propertyCombo.SelectedIndex < 0)
                {
                    comparatorCombo.ItemsSource = Enumerable.Empty<string>();
                }
                else
                {
                    var propType = (propertyCombo.SelectedItem as PropertyInfo).PropertyType;
                    comparatorCombo.ItemsSource = propType.GetComparators().Select(_ => _.ToSymbol());

                    var valuePanel = new StackPanel { Orientation = Orientation.Horizontal };

                    if (Nullable.GetUnderlyingType(propType) != null)
                    {
                        propType = propType.GenericTypeArguments[0];
                        valuePanel.Children.Add(new CheckBox { Content = "No value?" });
                    }

                    if (propType == typeof(bool))
                    {
                        valuePanel.Children.Add(new CheckBox { Content = "Yes?" });
                    }
                    else if (propType.IsEnum)
                    {
                        valuePanel.Children.Add(new ComboBox { Width = 150, ItemsSource = Enum.GetValues(propType) });
                    }
                    else if (propType == typeof(DateTime))
                    {
                        valuePanel.Children.Add(new DatePicker());
                    }
                    else if (propType == typeof(Datas.Club))
                    {
                        valuePanel.Children.Add(new ComboBox { Width = 150, ItemsSource = _dataProvider.Clubs, DisplayMemberPath = nameof(Datas.Club.ShortName) });
                    }
                    else if (propType == typeof(Datas.Country))
                    {
                        valuePanel.Children.Add(new ComboBox { Width = 150, ItemsSource = _dataProvider.Countries, DisplayMemberPath = nameof(Datas.Country.ShortName) });
                    }
                    else if (propType == typeof(Datas.Confederation))
                    {
                        valuePanel.Children.Add(new ComboBox { Width = 150, ItemsSource = _dataProvider.Confederations, DisplayMemberPath = nameof(Datas.Confederation.Name) });
                    }
                    else
                    {
                        valuePanel.Children.Add(new TextBox { Width = 150 });
                    }

                    panel.Children.Insert(2, valuePanel);
                    panel.Tag = valuePanel;
                }
            }

            propertyCombo.SelectionChanged += SelectionChangedEvent;

            var delButton = new Button
            {
                Content = "x",
                Width = 25,
                Height = 25
            };

            delButton.Click += (_1, _2) => sourcePanel.Children.Remove(panel);

            panel.Children.Add(delButton);

            sourcePanel.Children.Add(panel);

            SelectionChangedEvent(null, null);
        }

        private CriteriaSet ExtractCriteriaSet()
        {
            return CriteriaSet.Empty;
        }

        private void AddCriteriaSet_Click(object sender, RoutedEventArgs e)
        {
            const double DefaultHeight = 25;

            var criteriaSetBorder = new Border
            {
                Margin = new Thickness(5),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };

            var criteriaSetPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5)
            };

            var removeCriteriaSetButton = new Button
            {
                Content = "X",
                Width = 25,
                Height = DefaultHeight,
                ToolTip = "Remove the criteria set"
            };

            removeCriteriaSetButton.Click += (_1, _2) =>
            {
                var indexOfRemovedPanel = CriteriaPanel.Children.IndexOf(criteriaSetBorder);
                CriteriaPanel.Children.Remove(criteriaSetBorder);
                if (indexOfRemovedPanel > 0)
                {
                    CriteriaPanel.Children.RemoveAt(indexOfRemovedPanel - 1);
                }
            };

            var criteriaPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(10, 0, 0, 0)
            };

            var addButton = new Button
            {
                Content = "Add criterion",
                Tag = criteriaPanel,
                Width = 100,
                Height = DefaultHeight,
                Margin = new Thickness(5, 0, 0, 0)
            };

            addButton.Click += AddCriterion_Click;

            criteriaSetPanel.Children.Add(removeCriteriaSetButton);
            criteriaSetPanel.Children.Add(addButton);
            criteriaSetPanel.Children.Add(criteriaPanel);

            criteriaSetBorder.Child = criteriaSetPanel;

            if (CriteriaPanel.Children.Count > 0)
            {
                CriteriaPanel.Children.Add(new Label
                {
                    Content = "OR"
                });
            }

            CriteriaPanel.Children.Add(criteriaSetBorder);
        }
    }
}
