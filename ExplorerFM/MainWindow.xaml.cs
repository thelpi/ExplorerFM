using System;
using System.Collections;
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
    public partial class MainWindow : Window
    {
        private readonly DataProvider _dataProvider;
        private readonly IList _attributeProperties;

        public MainWindow()
        {
            InitializeComponent();

            _dataProvider = new DataProvider(Settings.Default.ConnectionString);

            _attributeProperties = typeof(Datas.Player).GetAttributeProperties()
                .Concat(typeof(Datas.Club).GetAttributeProperties())
                .Concat(typeof(Datas.Country).GetAttributeProperties())
                .Concat(typeof(Datas.Confederation).GetAttributeProperties())
                .ToList();

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
            HideWorkAndDisplay(
                () => _dataProvider.GetPlayersByCriteria(ExtractCriteriaSet()),
                players => PlayersView.ItemsSource = players);
        }

        private CriteriaSet ExtractCriteriaSet()
        {
            return CriteriaSet.Empty;
        }

        private void AddCriteriaSet_Click(object sender, RoutedEventArgs e)
        {
            const double DefaultSize = 25;
            const double DefaultMargin = 5;

            var criteriaSetBorder = new Border
            {
                Margin = new Thickness(DefaultMargin),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };

            var criteriaSetPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(DefaultMargin)
            };

            var removeCriteriaSetButton = new Button
            {
                Content = "X",
                Width = DefaultSize,
                Height = DefaultSize,
                ToolTip = "Removes the criteria set"
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
                Margin = new Thickness(DefaultMargin * 2, 0, 0, 0)
            };

            var addCriterionButton = new Button
            {
                Content = "Add",
                Width = DefaultSize * 2,
                Height = DefaultSize,
                Margin = new Thickness(DefaultMargin, 0, 0, 0),
                ToolTip = "Adds a criterion"
            };

            addCriterionButton.Click += (_x, _y) =>
            {
                var criterionContentPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(DefaultMargin)
                };

                var attributeItemsSourceView = new ListCollectionView(_attributeProperties);
                attributeItemsSourceView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PropertyInfo.DeclaringType)));

                var attributeComboBox = new ComboBox
                {
                    Width = DefaultSize * 6,
                    DisplayMemberPath = nameof(PropertyInfo.Name),
                    ItemsSource = attributeItemsSourceView
                };
                attributeComboBox.GroupStyle.Add(new GroupStyle
                {
                    HeaderTemplate = string.Concat("<TextBlock Text=\"{Binding ", nameof(PropertyInfo.Name), "}\"/>").ToDataTemplate()
                });

                var comparatorCombo = new ComboBox
                {
                    Width = DefaultSize * 2,
                    ItemsSource = Enumerable.Empty<string>(),
                    Margin = new Thickness(DefaultMargin, 0, 0, 0)
                };

                criterionContentPanel.Children.Add(attributeComboBox);
                criterionContentPanel.Children.Add(comparatorCombo);

                void SelectionChangedEvent(object _1, SelectionChangedEventArgs _2)
                {
                    if (criterionContentPanel.Tag != null)
                    {
                        criterionContentPanel.Children.Remove(criterionContentPanel.Tag as UIElement);
                    }

                    if (attributeComboBox.SelectedIndex < 0)
                    {
                        comparatorCombo.ItemsSource = Enumerable.Empty<string>();
                    }
                    else
                    {
                        var propType = (attributeComboBox.SelectedItem as PropertyInfo).PropertyType;
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

                        criterionContentPanel.Children.Insert(2, valuePanel);
                        criterionContentPanel.Tag = valuePanel;
                    }
                }

                attributeComboBox.SelectionChanged += SelectionChangedEvent;

                var removeCriterionButton = new Button
                {
                    Content = "X",
                    Width = DefaultSize,
                    Height = DefaultSize,
                    ToolTip = "Removes the criterion",
                    Margin = new Thickness(DefaultMargin, 0, 0, 0)
                };

                removeCriterionButton.Click += (_w, _z) => criteriaPanel.Children.Remove(criterionContentPanel);

                criterionContentPanel.Children.Add(removeCriterionButton);

                criteriaPanel.Children.Add(criterionContentPanel);

                SelectionChangedEvent(null, null);
            };

            criteriaSetPanel.Children.Add(removeCriteriaSetButton);
            criteriaSetPanel.Children.Add(addCriterionButton);
            criteriaSetPanel.Children.Add(criteriaPanel);

            criteriaSetBorder.Child = criteriaSetPanel;

            if (CriteriaPanel.Children.Count > 0)
            {
                CriteriaPanel.Children.Add(new Label
                {
                    Content = "Or"
                });
            }

            CriteriaPanel.Children.Add(criteriaSetBorder);
        }
    }
}
