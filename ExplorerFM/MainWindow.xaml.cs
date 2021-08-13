using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly IDictionary<Type, Func<IEnumerable>> _collectionsProvider;

        public MainWindow()
        {
            InitializeComponent();

            _dataProvider = new DataProvider(Settings.Default.ConnectionString);

            _attributeProperties = typeof(Datas.Player).GetAttributeProperties()
                .Concat(typeof(Datas.Club).GetAttributeProperties())
                .Concat(typeof(Datas.Country).GetAttributeProperties())
                .Concat(typeof(Datas.Confederation).GetAttributeProperties())
                .ToList();

            _collectionsProvider = new Dictionary<Type, Func<IEnumerable>>
            {
                { typeof(Datas.Country), () => _dataProvider.Countries },
                { typeof(Datas.Club), () => _dataProvider.Clubs },
                { typeof(Datas.Confederation), () => _dataProvider.Confederations },
            };

            HideWorkAndDisplay<object>(
                () =>
                {
                    _dataProvider.Initialize();
                    return null;
                },
                nullDummy => { });
        }

        private void PlayersView_MouseDoubleClick(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            var pItem = PlayersView.SelectedItem;
            if (pItem != null)
            {
                new PlayerWindow(pItem as Datas.Player).ShowDialog();
            }
        }

        private void HideWorkAndDisplay<T>(
            Func<T> backgroundFunc, Action<T> foregroundFunc)
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

        private void SearchPlayersButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var criteriaSet = ExtractCriteriaSet();
            HideWorkAndDisplay(
                () => _dataProvider.GetPlayersByCriteria(criteriaSet),
                players => PlayersView.ItemsSource = players);
        }

        private CriteriaSet ExtractCriteriaSet()
        {
            var criteriaSets = new List<CriteriaSet>();

            foreach (var criteriaSetChild in CriteriaSetPanel.Children)
            {
                if (criteriaSetChild is Border)
                {
                    var criterionSets = new List<Criterion>();

                    var criteriaBasePanel = (criteriaSetChild as Border).Child as StackPanel;
                    var criteriaPanel = criteriaBasePanel.Children[criteriaBasePanel.Children.Count - 1] as StackPanel;
                    foreach (var criterionPanel in criteriaPanel.Children)
                    {
                        if (criterionPanel is StackPanel)
                        {
                            /*var criterion = Criterion.New("", true);
                            // type
                            // symbol
                            // stackpanel
                            //      value
                            //      null
                            criterionSets.Add(criterion);*/
                        }
                    }

                    if (criterionSets.Count > 0)
                        criteriaSets.Add(new CriteriaSet(false, criterionSets.ToArray()));
                }
            }

            return new CriteriaSet(true, criteriaSets.ToArray());
        }

        private void AddCriteriaSetButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AddCriteriaSet(true);
        }

        private void AddCriteriaSet(
            bool addFirstCriterion)
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
                CopyCriteriaSet(criteriaSetPanel);
            };

            criteriaSetPanel.Children.Add(removeCriteriaSetButton);
            criteriaSetPanel.Children.Add(copyCriteriaButton);
            criteriaSetPanel.Children.Add(addCriterionButton);
            criteriaSetPanel.Children.Add(criteriaPanel);

            criteriaSetBorder.Child = criteriaSetPanel;

            AddContentGroup(CriteriaSetPanel, criteriaSetBorder, true);

            if (addFirstCriterion)
            {
                AddCriterion(criteriaPanel);
            }
        }

        private void CopyCriteriaSet(
            StackPanel criteriaSetPanel)
        {
            AddCriteriaSet(false);
            var newCriteriaSetPanel = (CriteriaSetPanel.Children[CriteriaSetPanel.Children.Count - 1] as Border).Child as StackPanel;
            var newCriteriaPanel = newCriteriaSetPanel.Children[criteriaSetPanel.Children.Count - 1] as StackPanel;
            var currentCriteriaPanel = criteriaSetPanel.Children[criteriaSetPanel.Children.Count - 1] as StackPanel;
            foreach (var currentCriterionObject in currentCriteriaPanel.Children)
            {
                if (currentCriterionObject is StackPanel)
                {
                    CopyCriterion(newCriteriaPanel, currentCriterionObject);
                }
            }
        }

        private void CopyCriterion(
            StackPanel newCriteriaPanel,
            object currentCriterionObject)
        {
            AddCriterion(newCriteriaPanel);
            var newCriterion = newCriteriaPanel.Children[newCriteriaPanel.Children.Count - 1] as StackPanel;
            var currentCriterion = currentCriterionObject as StackPanel;

            var newAttributeComboBox = newCriterion.Children[0] as ComboBox;
            var currentAttributeComboBox = currentCriterion.Children[0] as ComboBox;
            newAttributeComboBox.SelectedIndex = currentAttributeComboBox.SelectedIndex;

            var newSymbolCheckBox = newCriterion.Children[1] as ComboBox;
            var currentSymbolCheckBox = currentCriterion.Children[1] as ComboBox;
            newSymbolCheckBox.SelectedIndex = currentSymbolCheckBox.SelectedIndex;

            var newValuePanel = newCriterion.Children[2] as StackPanel;
            var currentValuePanel = currentCriterion.Children[2] as StackPanel;

            var newValueInnerElement = newValuePanel.Children[0];
            var currentValueInnerElement = currentValuePanel.Children[0];

            var elementType = currentValueInnerElement.GetType();
            if (elementType == typeof(TextBox))
                (newValueInnerElement as TextBox).Text = (currentValueInnerElement as TextBox).Text;
            else if (elementType == typeof(CheckBox))
                (newValueInnerElement as CheckBox).IsChecked = (currentValueInnerElement as CheckBox).IsChecked;
            else if (elementType == typeof(ComboBox))
                (newValueInnerElement as ComboBox).SelectedIndex = (currentValueInnerElement as ComboBox).SelectedIndex;
            else if (elementType == typeof(DatePicker))
                (newValueInnerElement as DatePicker).SelectedDate = (currentValueInnerElement as DatePicker).SelectedDate;
            else if (elementType == typeof(Xceed.Wpf.Toolkit.LongUpDown))
                (newValueInnerElement as Xceed.Wpf.Toolkit.LongUpDown).Value = (currentValueInnerElement as Xceed.Wpf.Toolkit.LongUpDown).Value;
            else if (elementType == typeof(Xceed.Wpf.Toolkit.DecimalUpDown))
                (newValueInnerElement as Xceed.Wpf.Toolkit.DecimalUpDown).Value = (currentValueInnerElement as Xceed.Wpf.Toolkit.DecimalUpDown).Value;
            else if (elementType == typeof(Xceed.Wpf.Toolkit.DoubleUpDown))
                (newValueInnerElement as Xceed.Wpf.Toolkit.DoubleUpDown).Value = (currentValueInnerElement as Xceed.Wpf.Toolkit.DoubleUpDown).Value;
            else
                throw new NotSupportedException();

            var newValueNullPanel = newValuePanel.Children[1] as DockPanel;
            var currentValueNullPanel = currentValuePanel.Children[1] as DockPanel;

            if (currentValueNullPanel.Children.Count > 0)
            {
                (newValueNullPanel.Children[0] as CheckBox).IsChecked = (currentValueNullPanel.Children[0] as CheckBox).IsChecked;
            }

            var newIncNullPanel = newValuePanel.Children[2] as DockPanel;
            var currentIncNullPanel = currentValuePanel.Children[2] as DockPanel;

            if (currentIncNullPanel.Children.Count > 0)
            {
                (newIncNullPanel.Children[0] as CheckBox).IsChecked = (currentIncNullPanel.Children[0] as CheckBox).IsChecked;
            }
        }

        private void AddCriterion(StackPanel criteriaPanel)
        {
            var criterionContentPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(DefaultMargin)
            };

            var attributeItemsSourceView = new ListCollectionView(_attributeProperties);
            attributeItemsSourceView.GroupDescriptions.Add(
                new PropertyGroupDescription(
                    nameof(PropertyInfo.DeclaringType),
                    new TypeDisplayConverter()));

            var attributeComboBox = new ComboBox
            {
                Width = DefaultSize * 6,
                DisplayMemberPath = nameof(PropertyInfo.Name),
                ItemsSource = attributeItemsSourceView
            };
            attributeComboBox.GroupStyle.Add(new GroupStyle
            {
                // "Name" here references an internal property in the ListCollectionView mechanism
                // It's not the name of the DeclaringType
                HeaderTemplate = string.Concat("<TextBlock Text=\"{Binding Name}\"/>").ToDataTemplate()
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
                AddCriterionValuator(criterionContentPanel, attributeComboBox, comparatorCombo);
            };

            var removeCriterionButton = GetRemovalButton(
                criteriaPanel,
                criterionContentPanel,
                "Removes the criterion",
                new Thickness(DefaultMargin, 0, 0, 0));

            criterionContentPanel.Children.Add(removeCriterionButton);

            AddContentGroup(criteriaPanel, criterionContentPanel, false);

            AddCriterionValuator(criterionContentPanel, attributeComboBox, comparatorCombo);
        }

        private void AddCriterionValuator(
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
                var propInfo = attributeComboBox.SelectedItem as PropertyInfo;
                var propType = propInfo.PropertyType;
                comparatorCombo.ItemsSource = propType.GetComparators(false).Select(_ => _.ToSymbol());

                var propAttribute = propInfo.GetCustomAttribute<FieldAttribute>();

                var valuePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(DefaultMargin, 0, 0, 0)
                };

                var nullableType = Nullable.GetUnderlyingType(propType);
                if (nullableType != null)
                    propType = nullableType;

                if (typeof(IList).IsAssignableFrom(propType) && propType.IsGenericType)
                    propType = propType.GenericTypeArguments.First();

                var isCustomType = propType == typeof(Datas.Club)
                    || propType == typeof(Datas.Country)
                    || propType == typeof(Datas.Confederation);

                FrameworkElement childElement;

                if (propType == typeof(bool))
                    childElement = new CheckBox { Content = "Yes", VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center };
                else if (propType.IsEnum)
                    childElement = new ComboBox { ItemsSource = Enum.GetValues(propType) };
                else if (propType == typeof(DateTime))
                    childElement = new DatePicker();
                else if (propType.Namespace == typeof(Datas.BaseData).Namespace)
                    childElement = new ComboBox { ItemsSource = _collectionsProvider[propType]() };
                else if (propType.IsIntegerType())
                    childElement = new Xceed.Wpf.Toolkit.LongUpDown { Minimum = propAttribute.Min, Maximum = propAttribute.Max };
                else if (propType == typeof(decimal))
                    childElement = new Xceed.Wpf.Toolkit.DecimalUpDown { Minimum = propAttribute.Min, Maximum = propAttribute.Max, CultureInfo = System.Globalization.CultureInfo.InvariantCulture };
                else if (propType == typeof(double) || propType == typeof(float))
                    childElement = new Xceed.Wpf.Toolkit.DoubleUpDown { Minimum = propAttribute.Min, Maximum = propAttribute.Max, CultureInfo = System.Globalization.CultureInfo.InvariantCulture };
                else if (propType == typeof(string))
                    childElement = new TextBox();
                else
                    throw new NotSupportedException();

                childElement.Width = DefaultSize * 6;
                valuePanel.Children.Add(childElement);

                var emptyNullManagement = nullableType == null && !isCustomType;

                var incNullPanel = GetNullManagementPanel(
                    emptyNullManagement,
                    "Inc. N/A",
                    "N/A values are treated as matching the pattern");

                var nullValuePanel = GetNullManagementPanel(
                    emptyNullManagement,
                    "Is N/A",
                    "Checks only if the value is N/A or not",
                    (_1, _2) =>
                    {
                        childElement.IsEnabled = false;
                        incNullPanel.Children[0].IsEnabled = false;
                        comparatorCombo.ItemsSource = propType.GetComparators(true).Select(_ => _.ToSymbol());
                    }, (_1, _2) =>
                    {
                        childElement.IsEnabled = true;
                        incNullPanel.Children[0].IsEnabled = true;
                        comparatorCombo.ItemsSource = propType.GetComparators(false).Select(_ => _.ToSymbol());
                    });

                valuePanel.Children.Add(nullValuePanel);
                valuePanel.Children.Add(incNullPanel);

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

        private static DockPanel GetNullManagementPanel(
            bool emptyPanel,
            string label,
            string toolTip,
            RoutedEventHandler checkedHandler = null,
            RoutedEventHandler uncheckedHandler = null)
        {
            var panel = new DockPanel
            {
                Width = DefaultSize * 3
            };

            if (!emptyPanel)
            {
                var checkBox = new CheckBox
                {
                    Content = label,
                    ToolTip = toolTip,
                    Margin = new Thickness(DefaultMargin, 0, 0, 0),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                checkBox.SetValue(DockPanel.DockProperty, Dock.Left);

                if (uncheckedHandler != null)
                    checkBox.Unchecked += uncheckedHandler;
                if (checkedHandler != null)
                    checkBox.Checked += checkedHandler;

                panel.Children.Add(checkBox);
            }

            return panel;
        }
    }
}
