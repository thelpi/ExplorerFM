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
        const double DefaultSize = 25;
        const double DefaultMargin = 5;

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
            var criteriaSetBorder = new Border
            {
                Margin = new Thickness(DefaultMargin),
                BorderBrush = Brushes.Gainsboro,
                BorderThickness = new Thickness(0.5)
            };

            var criteriaSetPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(DefaultMargin)
            };

            var removeCriteriaSetButton = GetRemovalButton(CriteriaSetPanel, criteriaSetBorder, "Removes the criteria set");

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
                AddCriterion(criteriaPanel);
            };

            var copyCriteriaButton = new Button
            {
                Content = "Copy",
                Width = DefaultSize * 2,
                Height = DefaultSize,
                Margin = new Thickness(DefaultMargin, 0, 0, 0),
                ToolTip = "Adds a criteria set"
            };

            copyCriteriaButton.Click += (_x, _y) =>
            {
                AddCriteriaSet_Click(null, null);
                //var currentCriteria = CriteriaSetPanel.Children[CriteriaSetPanel.Children.Count - 1];
            };

            criteriaSetPanel.Children.Add(removeCriteriaSetButton);
            criteriaSetPanel.Children.Add(copyCriteriaButton);
            criteriaSetPanel.Children.Add(addCriterionButton);
            criteriaSetPanel.Children.Add(criteriaPanel);

            criteriaSetBorder.Child = criteriaSetPanel;

            AddContentGroup(CriteriaSetPanel, criteriaSetBorder, true);

            AddCriterion(criteriaPanel);
        }

        private void AddCriterion(StackPanel criteriaPanel)
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

            attributeComboBox.SelectionChanged += (_1, _2) =>
            {
                SetCriterionValue(criterionContentPanel, attributeComboBox, comparatorCombo);
            };

            var removeCriterionButton = GetRemovalButton(
                criteriaPanel,
                criterionContentPanel,
                "Removes the criterion",
                new Thickness(DefaultMargin, 0, 0, 0));

            criterionContentPanel.Children.Add(removeCriterionButton);

            AddContentGroup(criteriaPanel, criterionContentPanel, false);

            SetCriterionValue(criterionContentPanel, attributeComboBox, comparatorCombo);
        }

        private void SetCriterionValue(
            StackPanel criterionContentPanel,
            ComboBox attributeComboBox,
            ComboBox comparatorCombo)
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

                var valuePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(DefaultMargin, 0, 0, 0)
                };

                var nullableType = Nullable.GetUnderlyingType(propType);
                if (nullableType != null)
                    propType = nullableType;

                var isCustomType = propType == typeof(Datas.Club)
                    || propType == typeof(Datas.Country)
                    || propType == typeof(Datas.Confederation);

                UIElement childElement;
                if (propType == typeof(bool))
                {
                    childElement = new CheckBox { Width = 150, Content = "Yes", VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center };
                }
                else if (propType.IsEnum)
                {
                    childElement = new ComboBox { Width = 150, ItemsSource = Enum.GetValues(propType) };
                }
                else if (propType == typeof(DateTime))
                {
                    childElement = new DatePicker { Width = 150 };
                }
                else if (propType == typeof(Datas.Club))
                {
                    childElement = new ComboBox { Width = 150, ItemsSource = _dataProvider.Clubs, DisplayMemberPath = nameof(Datas.Club.ShortName) };
                }
                else if (propType == typeof(Datas.Country))
                {
                    childElement = new ComboBox { Width = 150, ItemsSource = _dataProvider.Countries, DisplayMemberPath = nameof(Datas.Country.ShortName) };
                }
                else if (propType == typeof(Datas.Confederation))
                {
                    childElement = new ComboBox { Width = 150, ItemsSource = _dataProvider.Confederations, DisplayMemberPath = nameof(Datas.Confederation.Name) };
                }
                else
                {
                    childElement = new TextBox { Width = 150 };
                }
                valuePanel.Children.Add(childElement);

                var nullValuePanel = new DockPanel
                {
                    Width = DefaultSize * 3
                };
                if (nullableType != null || isCustomType)
                {
                    var nullCheck = new CheckBox
                    {
                        Content = "No value",
                        Margin = new Thickness(DefaultMargin, 0, 0, 0),
                        VerticalContentAlignment = VerticalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    nullCheck.SetValue(DockPanel.DockProperty, Dock.Left);
                    nullCheck.Unchecked += (_3, _4) => childElement.IsEnabled = true;
                    nullCheck.Checked += (_5, _6) => childElement.IsEnabled = false;
                    nullValuePanel.Children.Add(nullCheck);
                }

                valuePanel.Children.Add(nullValuePanel);

                criterionContentPanel.Children.Insert(2, valuePanel);
                criterionContentPanel.Tag = valuePanel;
            }
        }

        private static Button GetRemovalButton(
            Panel containerPanel,
            UIElement relatedContent,
            string toolTip,
            Thickness margin = default(Thickness))
        {
            var removalButton = new Button
            {
                Content = "X",
                Width = DefaultSize,
                Height = DefaultSize,
                ToolTip = toolTip,
                Margin = margin
            };

            removalButton.Click += (_1, _2) =>
            {
                var removedIndex = containerPanel.Children.IndexOf(relatedContent);
                containerPanel.Children.RemoveAt(removedIndex);
                if (removedIndex > 0)
                    containerPanel.Children.RemoveAt(removedIndex - 1);
                else if (removedIndex < containerPanel.Children.Count - 1)
                    containerPanel.Children.RemoveAt(removedIndex);
                if (containerPanel.Children.Count == 0 && containerPanel.Parent is StackPanel)
                {
                    var parentContainer = containerPanel.Parent as StackPanel;
                    (parentContainer.Children[0] as Button).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                }
            };

            return removalButton;
        }

        private static void AddContentGroup(
            Panel containerPanel,
            UIElement groupContent,
            bool isOr)
        {
            if (containerPanel.Children.Count > 0)
            {
                containerPanel.Children.Add(new Label
                {
                    Content = isOr ? "Or" : "And"
                });
            }

            containerPanel.Children.Add(groupContent);
        }
    }
}
