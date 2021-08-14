using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class MainWindow : Window
    {
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

        private void PlayersView_MouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e)
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

            foreach (var criteriaSetChild in CriteriaSetPanel.Children.OfType<Border>())
            {
                var criterionSets = new List<Criterion>();

                var criteriaPanel = criteriaSetChild.Find<StackPanel>("CriteriaPanel");
                foreach (var criterionPanel in criteriaPanel.Children.OfType<StackPanel>())
                {
                    var criterion = ExtractCriterion(criterionPanel);
                    if (criterion != null)
                        criterionSets.Add(criterion);
                }

                if (criterionSets.Count > 0)
                    criteriaSets.Add(new CriteriaSet(false, criterionSets.ToArray()));
            }

            return new CriteriaSet(true, criteriaSets.ToArray());
        }

        private Criterion ExtractCriterion(StackPanel criterionPanel)
        {
            var attributeComboBox = criterionPanel.Find<ComboBox>("AttributeComboBox");
            var comparatorComboBox = criterionPanel.Find<ComboBox>("ComparatorComboBox");

            if (attributeComboBox.SelectedIndex < 0 || comparatorComboBox.SelectedIndex < 0)
                return null;

            var isNullCheckBox = criterionPanel.Find<CheckBox>("IsNullCheckBox");
            var includeNullCheckBox = criterionPanel.Find<CheckBox>("IncludeNullCheckBox");
            var criterionValuePanel = criterionPanel.Find<StackPanel>("CriterionValuePanel");

            var attrPropInfo = attributeComboBox.SelectedItem as PropertyInfo;

            return new Criterion(
                attrPropInfo.GetCustomAttribute<FieldAttribute>(),
                attrPropInfo.DeclaringType,
                (Comparator)comparatorComboBox.SelectedItem,
                GetUiElementValue(criterionValuePanel.Children[0]),
                includeNullCheckBox.IsChecked == true,
                isNullCheckBox.IsChecked == true);
        }

        private static object GetUiElementValue(UIElement valuatedElement, UIElement copyTo = null)
        {
            var elementType = valuatedElement.GetType();
            if (elementType == typeof(StackPanel))
            {
                var v1 = ((valuatedElement as StackPanel).Children[0] as ComboBox).SelectedItem;
                var v2 = ((valuatedElement as StackPanel).Children[1] as Xceed.Wpf.Toolkit.LongUpDown).Value;
                if (copyTo != null)
                {
                    ((copyTo as StackPanel).Children[0] as ComboBox).SelectedIndex = ((valuatedElement as StackPanel).Children[0] as ComboBox).SelectedIndex;
                    ((copyTo as StackPanel).Children[1] as Xceed.Wpf.Toolkit.LongUpDown).Value = v2;
                }
                return new[] { v1, v2 };
            }
            else if (elementType == typeof(TextBox))
            {
                var v = (valuatedElement as TextBox).Text;
                if (copyTo != null) (copyTo as TextBox).Text = v;
                return v;
            }
            else if (elementType == typeof(CheckBox))
            {
                var v = (valuatedElement as CheckBox).IsChecked;
                if (copyTo != null) (copyTo as CheckBox).IsChecked = v;
                return v == true;
            }
            else if (elementType == typeof(ComboBox))
            {
                if (copyTo != null) (copyTo as ComboBox).SelectedIndex = (valuatedElement as ComboBox).SelectedIndex;
                return (valuatedElement as ComboBox).SelectedItem;
            }
            else if (elementType == typeof(DatePicker))
            {
                var v = (valuatedElement as DatePicker).SelectedDate;
                if (copyTo != null) (copyTo as DatePicker).SelectedDate = v;
                return v;
            }
            else if (elementType == typeof(Xceed.Wpf.Toolkit.LongUpDown))
            {
                var v = (valuatedElement as Xceed.Wpf.Toolkit.LongUpDown).Value;
                if (copyTo != null) (copyTo as Xceed.Wpf.Toolkit.LongUpDown).Value = v;
                return v;
            }
            else if (elementType == typeof(Xceed.Wpf.Toolkit.DecimalUpDown))
            {
                var v = (valuatedElement as Xceed.Wpf.Toolkit.DecimalUpDown).Value;
                if (copyTo != null) (copyTo as Xceed.Wpf.Toolkit.DecimalUpDown).Value = v;
                return v;
            }
            else if (elementType == typeof(Xceed.Wpf.Toolkit.DoubleUpDown))
            {
                var v = (valuatedElement as Xceed.Wpf.Toolkit.DoubleUpDown).Value;
                if (copyTo != null) (copyTo as Xceed.Wpf.Toolkit.DoubleUpDown).Value = v;
                return v;
            }
            else
                throw new NotSupportedException();
        }

        private void AddCriteriaSetButton_Click(object sender, RoutedEventArgs e)
        {
            AddCriteriaSet(true);
        }

        private void AddCriteriaSet(bool addFirstCriterion)
        {
            var criteriaSetBorder = GetByTemplateKey<Border>("CriteriaPanelTemplate");
            var criteriaPanel = criteriaSetBorder.Find<StackPanel>("CriteriaPanel");

            criteriaSetBorder.Find<Button>("RemoveCriteriaButton").Click += (_1, _2) => RemoveCriteriaBase(CriteriaSetPanel, criteriaSetBorder);
            criteriaSetBorder.Find<Button>("AddCriterionButton").Click += (_1, _2) => AddCriterion(criteriaPanel);
            criteriaSetBorder.Find<Button>("CopyCriteriaButton").Click += (_1, _2) => CopyCriteriaSet(criteriaPanel);

            AddContentGroup(CriteriaSetPanel, criteriaSetBorder, true);

            if (addFirstCriterion)
                AddCriterion(criteriaPanel);
        }

        private void CopyCriteriaSet(StackPanel criteriaSetPanel)
        {
            AddCriteriaSet(false);
            var newCriteriaPanel = (CriteriaSetPanel.Children.OfType<Border>().Last()).Find<StackPanel>("CriteriaPanel");
            foreach (var currentCriterion in criteriaSetPanel.Children.OfType<StackPanel>())
            {
                AddCriterion(newCriteriaPanel);
                CopyCriterion(newCriteriaPanel.Children.OfType<StackPanel>().Last(), currentCriterion);
            }
        }

        private void CopyCriterion(StackPanel newCriterion, StackPanel currentCriterion)
        {
            newCriterion.Find<ComboBox>("AttributeComboBox").SelectedIndex =
                currentCriterion.Find<ComboBox>("AttributeComboBox").SelectedIndex;
            newCriterion.Find<ComboBox>("ComparatorComboBox").SelectedIndex =
                currentCriterion.Find<ComboBox>("ComparatorComboBox").SelectedIndex;
            GetUiElementValue(
                newCriterion.Find<StackPanel>("CriterionValuePanel").Children[0],
                currentCriterion.Find<StackPanel>("CriterionValuePanel").Children[0]);
            newCriterion.Find<CheckBox>("IsNullCheckBox").IsChecked =
                currentCriterion.Find<CheckBox>("IsNullCheckBox").IsChecked;
            newCriterion.Find<CheckBox>("IncludeNullCheckBox").IsChecked =
                currentCriterion.Find<CheckBox>("IncludeNullCheckBox").IsChecked;
        }

        private void AddCriterion(StackPanel criteriaPanel)
        {
            var criterionPanel = GetByTemplateKey<StackPanel>("CriterionPanelTemplate");

            var attributeComboBox = criterionPanel.Find<ComboBox>("AttributeComboBox");
            var comparatorComboBox = criterionPanel.Find<ComboBox>("ComparatorComboBox");
            var removeCriterionButton = criterionPanel.Find<Button>("RemoveCriterionButton");

            var attributeItemsSourceView = new ListCollectionView(_attributeProperties);
            attributeItemsSourceView.GroupDescriptions.Add(
                new PropertyGroupDescription(
                    nameof(PropertyInfo.DeclaringType),
                    new Converters.TypeDisplayConverter()));

            attributeComboBox.ItemsSource = attributeItemsSourceView;
            
            comparatorComboBox.ItemsSource = Enumerable.Empty<string>();

            var dpd = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ComboBox));
            dpd?.AddValueChanged(comparatorComboBox, (_1, _2) =>
            {
                if (comparatorComboBox.HasItems)
                    comparatorComboBox.SelectedIndex = 0;
            });

            attributeComboBox.SelectionChanged += (_1, _2) => AddCriterionValuator(criterionPanel);

            removeCriterionButton.Click += (_1, _2) => RemoveCriteriaBase(criteriaPanel, criterionPanel);

            AddContentGroup(criteriaPanel, criterionPanel, false);

            AddCriterionValuator(criterionPanel);
        }

        private void AddCriterionValuator(StackPanel criterionContentPanel)
        {
            var attributeComboBox = criterionContentPanel.Find<ComboBox>("AttributeComboBox");
            var comparatorCombo = criterionContentPanel.Find<ComboBox>("ComparatorComboBox");
            var criterionValuePanel = criterionContentPanel.Find<StackPanel>("CriterionValuePanel");
            var includeNullCheckBox = criterionContentPanel.Find<CheckBox>("IncludeNullCheckBox");
            var isNullCheckBox = criterionContentPanel.Find<CheckBox>("IsNullCheckBox");

            criterionValuePanel.Children.Clear();
            includeNullCheckBox.Visibility = Visibility.Hidden;
            isNullCheckBox.Visibility = Visibility.Hidden;

            if (attributeComboBox.SelectedIndex < 0)
            {
                comparatorCombo.ItemsSource = Enumerable.Empty<Comparator>();
            }
            else
            {
                var propInfo = attributeComboBox.SelectedItem as PropertyInfo;
                var propType = propInfo.PropertyType;
                comparatorCombo.ItemsSource = propType.GetComparators(false);

                var propAttribute = propInfo.GetCustomAttribute<FieldAttribute>();

                var nullableType = Nullable.GetUnderlyingType(propType);
                if (nullableType != null)
                    propType = nullableType;

                if (typeof(IList).IsAssignableFrom(propType) && propType.IsGenericType)
                    propType = propType.GenericTypeArguments.First();

                var isCustomType = propType == typeof(Datas.Club)
                    || propType == typeof(Datas.Country)
                    || propType == typeof(Datas.Confederation)
                    || !propAttribute.IsSql;

                FrameworkElement childElement;

                if (!propAttribute.IsSql)
                {
                    var childPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };
                    childElement = childPanel;

                    var underType = propType.GenericTypeArguments.First();
                    var childSelectElement = new ComboBox
                    {
                        Width = 100,
                        ItemsSource = underType.IsEnum ? Enum.GetValues(underType) : (IEnumerable)_dataProvider.Attributes
                    };

                    if (!underType.IsEnum)
                        childSelectElement.DisplayMemberPath = nameof(Datas.Attribute.Name);

                    var childValueElement = new Xceed.Wpf.Toolkit.LongUpDown
                    {
                        Margin = new Thickness(5, 0, 0, 0),
                        Width = 50,
                        Minimum = propAttribute.Min,
                        Maximum = propAttribute.Max
                    };
                    childPanel.Children.Add(childSelectElement);
                    childPanel.Children.Add(childValueElement);
                }
                else
                {
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

                    childElement.Width = 150;
                }

                criterionValuePanel.Children.Add(childElement);

                if (nullableType == null && !isCustomType && propType != typeof(string))
                {
                    includeNullCheckBox.Visibility = Visibility.Hidden;
                    isNullCheckBox.Visibility = Visibility.Hidden;
                }
                else
                {
                    includeNullCheckBox.Visibility = Visibility.Visible;
                    isNullCheckBox.Visibility = Visibility.Visible;
                }

                isNullCheckBox.RemoveRoutedEventHandlers(System.Windows.Controls.Primitives.ToggleButton.CheckedEvent);
                isNullCheckBox.RemoveRoutedEventHandlers(System.Windows.Controls.Primitives.ToggleButton.CheckedEvent);
                isNullCheckBox.Checked += (_1, _2) =>
                {
                    childElement.IsEnabled = false;
                    includeNullCheckBox.IsEnabled = false;
                    comparatorCombo.ItemsSource = propType.GetComparators(true);
                };
                isNullCheckBox.Unchecked += (_1, _2) =>
                {
                    childElement.IsEnabled = true;
                    includeNullCheckBox.IsEnabled = true;
                    comparatorCombo.ItemsSource = propType.GetComparators(false);
                };
            }
        }

        private static void RemoveCriteriaBase(Panel containerPanel, UIElement relatedContent)
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
                parentContainer.Find<Button>("RemoveCriteriaButton").RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            }
        }

        private void AddContentGroup(Panel containerPanel, UIElement groupContent, bool isOr)
        {
            if (containerPanel.Children.Count > 0)
            {
                var labelKey = $"{(isOr ? "Or" : "And")}LabelTemplate";
                containerPanel.Children.Add(GetByTemplateKey<Label>(labelKey));
            }

            containerPanel.Children.Add(groupContent);
        }

        private T GetByTemplateKey<T>(string key) where T : UIElement
        {
            return (FindResource(key) as ControlTemplate).LoadContent() as T;
        }
    }
}
