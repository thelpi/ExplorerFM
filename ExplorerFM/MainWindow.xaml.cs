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
using ExplorerFM.FieldsAttributes;
using ExplorerFM.Properties;
using ExplorerFM.RuleEngine;
using Xceed.Wpf.Toolkit;

namespace ExplorerFM
{
    public partial class MainWindow : Window
    {
        public const string CriteriaPanelName = "CriteriaPanel";
        public const string AttributeComboBoxName = "AttributeComboBox";
        public const string ComparatorComboBoxName = "ComparatorComboBox";
        public const string IsNullCheckBoxName = "IsNullCheckBox";
        public const string IncludeNullCheckBoxName = "IncludeNullCheckBox";
        public const string CriterionValuePanelName = "CriterionValuePanel";
        public const string RemoveCriteriaButtonName = "RemoveCriteriaButton";
        public const string AddCriterionButtonName = "AddCriterionButton";
        public const string CopyCriteriaButtonName = "CopyCriteriaButton";
        public const string RemoveCriterionButtonName = "RemoveCriterionButton";
        public const string ComboValueName = "ComboValue";
        public const string NumericValueName = "NumericValue";

        public const string CriteriaPanelTemplateKey = "CriteriaPanelTemplate";
        public const string OrLabelTemplateKey = "OrLabelTemplate";
        public const string AndLabelTemplateKey = "AndLabelTemplate";
        public const string CriterionPanelTemplateKey = "CriterionPanelTemplate";
        public const string IntegerValuePanelKey = "IntegerValuePanel";
        public const string DecimalValuePanelKey = "DecimalValuePanel";
        public const string StringValuePanelKey = "StringValuePanel";
        public const string DateValuePanelKey = "DateValuePanel";
        public const string SelectorValuePanelKey = "SelectorValuePanel";
        public const string BooleanValuePanelKey = "BooleanValuePanel";
        public const string SelectorIntegerValuePanelKey = "SelectorIntegerValuePanel";

        private readonly DataProvider _dataProvider;
        private readonly IList _attributeProperties;
        private readonly IDictionary<Type, Func<IEnumerable>> _collectionsProvider;

        private bool _descendingSort;

        public MainWindow()
        {
            InitializeComponent();

            _dataProvider = new DataProvider(Settings.Default.ConnectionString);

            _attributeProperties = typeof(Datas.Player).GetAttributeProperties<FieldAttribute>()
                .Concat(typeof(Datas.Club).GetAttributeProperties<FieldAttribute>())
                .Concat(typeof(Datas.Country).GetAttributeProperties<FieldAttribute>())
                .Concat(typeof(Datas.Confederation).GetAttributeProperties<FieldAttribute>())
                .ToList();

            _collectionsProvider = new Dictionary<Type, Func<IEnumerable>>
            {
                { typeof(Datas.Country), () => _dataProvider.Countries },
                { typeof(Datas.Club), () => _dataProvider.Clubs },
                { typeof(Datas.Confederation), () => _dataProvider.Confederations },
            };

            var allColumnFields = typeof(Datas.Player).GetAttributeProperties<GridViewAttribute>()
                .Concat(typeof(Datas.Club).GetAttributeProperties<GridViewAttribute>())
                .Concat(typeof(Datas.Country).GetAttributeProperties<GridViewAttribute>())
                .Concat(typeof(Datas.Confederation).GetAttributeProperties<GridViewAttribute>())
                .ToList();

            foreach (var columnField in allColumnFields)
            {
                var fullPath = columnField.Name;
                if (columnField.DeclaringType == typeof(Datas.Confederation))
                    fullPath = string.Concat(nameof(Datas.Player.Nationality), ".", nameof(Datas.Country.Confederation), ".", fullPath);
                else if (columnField.DeclaringType == typeof(Datas.Country))
                    fullPath = string.Concat(nameof(Datas.Player.Nationality), ".", fullPath);
                else if (columnField.DeclaringType == typeof(Datas.Club))
                    fullPath = string.Concat(nameof(Datas.Player.ClubContract), ".", fullPath);

                var attributes = columnField.GetCustomAttributes<GridViewAttribute>();
                foreach (var attribute in attributes)
                {
                    var columnHeader = new GridViewColumnHeader
                    {
                        Content = attribute.Name,
                    };
                    if (typeof(IComparable).IsAssignableFrom(columnField.PropertyType)
                        || (Nullable.GetUnderlyingType(columnField.PropertyType) != null
                            && typeof(IComparable).IsAssignableFrom(Nullable.GetUnderlyingType(columnField.PropertyType))))
                    {
                        columnHeader.Click += (_1, _2) =>
                        {
                            var source = PlayersView.ItemsSource as IEnumerable<Datas.Player>;
                            if (source != null)
                            {
                                PlayersView.ItemsSource = _descendingSort
                                    ? source.OrderByDescending(_ => _.GetPropertyValue(columnField))
                                    : source.OrderBy(_ => _.GetPropertyValue(columnField));
                                _descendingSort = !_descendingSort;
                            }
                        };
                    }

                    PlayersGrid.Columns.Add(new GridViewColumn
                    {
                        Header = columnHeader,
                        DisplayMemberBinding = new Binding
                        {
                            Path = attribute.NoPath ? null : new PropertyPath(fullPath),
                            Converter = attribute.Converter,
                            ConverterParameter = attribute.ConverterParameter
                        }
                    });
                }
            }

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
            var criteriaSet = ExtractCriteriaSet(CriteriaSetPanel);
            var response = criteriaSet.Criteria.Count == 0
                ? System.Windows.MessageBox.Show("Extracts without criteria ?", "ExplorerFM", MessageBoxButton.YesNo)
                : MessageBoxResult.Yes;
            if (response == MessageBoxResult.Yes)
            {
                HideWorkAndDisplay(
                    () => _dataProvider.GetPlayersByCriteria(criteriaSet),
                    players => PlayersView.ItemsSource = players);
            }
        }

        private static CriteriaSet ExtractCriteriaSet(StackPanel criteriaSetPanel)
        {
            var criteriaSets = new List<CriteriaSet>();

            foreach (var criteriaSetChild in criteriaSetPanel.Children.OfType<Border>())
            {
                var criterionSets = new List<Criterion>();

                var criteriaPanel = criteriaSetChild.Find<StackPanel>(CriteriaPanelName);
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

        private static Criterion ExtractCriterion(StackPanel criterionPanel)
        {
            var attributeComboBox = criterionPanel.Find<ComboBox>(AttributeComboBoxName);
            var comparatorComboBox = criterionPanel.Find<ComboBox>(ComparatorComboBoxName);

            if (attributeComboBox.SelectedIndex < 0 || comparatorComboBox.SelectedIndex < 0)
                return null;

            var isNullCheckBox = criterionPanel.Find<CheckBox>(IsNullCheckBoxName);
            var includeNullCheckBox = criterionPanel.Find<CheckBox>(IncludeNullCheckBoxName);
            var criterionValuePanel = criterionPanel.Find<StackPanel>(CriterionValuePanelName);

            var value = GetUiElementValue(criterionValuePanel.Children[0]);
            if (isNullCheckBox.IsChecked != true && value.IsNullOrContainsNull())
                return null;

            var attrPropInfo = attributeComboBox.SelectedItem as PropertyInfo;

            return new Criterion(
                attrPropInfo.GetCustomAttribute<FieldAttribute>(),
                attrPropInfo.DeclaringType,
                (Comparator)comparatorComboBox.SelectedItem,
                value,
                isNullCheckBox.IsChecked == true,
                includeNullCheckBox.IsChecked == true);
        }

        private static object GetUiElementValue(UIElement valuatedElement, UIElement elementToValuate = null)
        {
            var elementType = valuatedElement.GetType();
            if (elementType == typeof(StackPanel))
            {
                var valuatedCombo = (valuatedElement as StackPanel).Find<ComboBox>(ComboValueName);
                var valuatedIntValue = (valuatedElement as StackPanel).Find<IntegerUpDown>(NumericValueName).Value;
                if (elementToValuate != null)
                {
                    var copyPanel = elementToValuate as StackPanel;
                    copyPanel.Find<ComboBox>(ComboValueName).SelectedIndex = valuatedCombo.SelectedIndex;
                    copyPanel.Find<IntegerUpDown>(NumericValueName).Value = valuatedIntValue;
                }
                return new[] { valuatedCombo.SelectedItem, valuatedIntValue };
            }
            else if (elementType == typeof(TextBox))
            {
                var v = (valuatedElement as TextBox).Text;
                if (elementToValuate != null) (elementToValuate as TextBox).Text = v;
                return v;
            }
            else if (elementType == typeof(CheckBox))
            {
                var v = (valuatedElement as CheckBox).IsChecked;
                if (elementToValuate != null) (elementToValuate as CheckBox).IsChecked = v;
                return v == true;
            }
            else if (elementType == typeof(ComboBox))
            {
                var comboBox = valuatedElement as ComboBox;
                if (elementToValuate != null) (elementToValuate as ComboBox).SelectedIndex = comboBox.SelectedIndex;
                return comboBox.SelectedItem;
            }
            else if (elementType == typeof(DatePicker))
            {
                var v = (valuatedElement as DatePicker).SelectedDate;
                if (elementToValuate != null) (elementToValuate as DatePicker).SelectedDate = v;
                return v;
            }
            else if (elementType == typeof(IntegerUpDown))
            {
                var v = (valuatedElement as IntegerUpDown).Value;
                if (elementToValuate != null) (elementToValuate as IntegerUpDown).Value = v;
                return v;
            }
            else if (elementType == typeof(DecimalUpDown))
            {
                var v = (valuatedElement as DecimalUpDown).Value;
                if (elementToValuate != null) (elementToValuate as DecimalUpDown).Value = v;
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
            var criteriaSetBorder = GetByTemplateKey<Border>(CriteriaPanelTemplateKey);
            var criteriaPanel = criteriaSetBorder.Find<StackPanel>(CriteriaPanelName);

            criteriaSetBorder.Find<Button>(RemoveCriteriaButtonName).Click +=
                (_1, _2) => RemoveCriteriaBase(CriteriaSetPanel, criteriaSetBorder);
            criteriaSetBorder.Find<Button>(AddCriterionButtonName).Click +=
                (_1, _2) => AddCriterion(criteriaPanel);
            criteriaSetBorder.Find<Button>(CopyCriteriaButtonName).Click +=
                (_1, _2) => CopyCriteriaSet(criteriaPanel);

            AddContentGroup(CriteriaSetPanel, criteriaSetBorder, true);

            if (addFirstCriterion)
                AddCriterion(criteriaPanel);
        }

        private void CopyCriteriaSet(StackPanel criteriaSetPanel)
        {
            AddCriteriaSet(false);
            var newCriteriaPanel = (CriteriaSetPanel.Children.OfType<Border>().Last()).Find<StackPanel>(CriteriaPanelName);
            foreach (var currentCriterion in criteriaSetPanel.Children.OfType<StackPanel>())
            {
                AddCriterion(newCriteriaPanel);
                CopyCriterion(newCriteriaPanel.Children.OfType<StackPanel>().Last(), currentCriterion);
            }
        }

        private static void CopyCriterion(StackPanel newCriterion, StackPanel currentCriterion)
        {
            newCriterion.Find<ComboBox>(AttributeComboBoxName).SelectedIndex =
                currentCriterion.Find<ComboBox>(AttributeComboBoxName).SelectedIndex;
            GetUiElementValue(
                currentCriterion.Find<StackPanel>(CriterionValuePanelName).Children[0],
                newCriterion.Find<StackPanel>(CriterionValuePanelName).Children[0]);
            newCriterion.Find<CheckBox>(IsNullCheckBoxName).IsChecked =
                currentCriterion.Find<CheckBox>(IsNullCheckBoxName).IsChecked;
            newCriterion.Find<ComboBox>(ComparatorComboBoxName).SelectedIndex =
                currentCriterion.Find<ComboBox>(ComparatorComboBoxName).SelectedIndex;
            newCriterion.Find<CheckBox>(IncludeNullCheckBoxName).IsChecked =
                currentCriterion.Find<CheckBox>(IncludeNullCheckBoxName).IsChecked;
        }

        private void AddCriterion(StackPanel criteriaPanel)
        {
            var criterionPanel = GetByTemplateKey<StackPanel>(CriterionPanelTemplateKey);

            var attributeComboBox = criterionPanel.Find<ComboBox>(AttributeComboBoxName);
            var comparatorComboBox = criterionPanel.Find<ComboBox>(ComparatorComboBoxName);
            var removeCriterionButton = criterionPanel.Find<Button>(RemoveCriterionButtonName);

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

            attributeComboBox.SelectionChanged +=
                (_1, _2) => AddCriterionValuator(criterionPanel);

            removeCriterionButton.Click +=
                (_1, _2) => RemoveCriteriaBase(criteriaPanel, criterionPanel);

            AddContentGroup(criteriaPanel, criterionPanel, false);

            AddCriterionValuator(criterionPanel);
        }

        private void AddCriterionValuator(StackPanel criterionContentPanel)
        {
            var attributeComboBox = criterionContentPanel.Find<ComboBox>(AttributeComboBoxName);
            var comparatorCombo = criterionContentPanel.Find<ComboBox>(ComparatorComboBoxName);
            var criterionValuePanel = criterionContentPanel.Find<StackPanel>(CriterionValuePanelName);
            var includeNullCheckBox = criterionContentPanel.Find<CheckBox>(IncludeNullCheckBoxName);
            var isNullCheckBox = criterionContentPanel.Find<CheckBox>(IsNullCheckBoxName);

            criterionValuePanel.Children.Clear();

            if (attributeComboBox.SelectedIndex < 0)
            {
                comparatorCombo.ItemsSource = Enumerable.Empty<Comparator>();
            }
            else
            {
                var propInfo = attributeComboBox.SelectedItem as PropertyInfo;

                var propAttribute = propInfo.GetCustomAttribute<FieldAttribute>();

                var propType = propInfo.PropertyType.GetUnderlyingNotNullType();

                var comparators = propType.GetComparators(propAttribute);

                comparatorCombo.ItemsSource = comparators;

                FrameworkElement valueElement;

                if (propAttribute.IsNestedSelector)
                {
                    valueElement = GetByTemplateKey<FrameworkElement>(SelectorIntegerValuePanelKey);

                    var valueComboBox = valueElement.Find<ComboBox>(ComboValueName);
                    SetComboBoxBindingFromAttribute(valueComboBox, propAttribute.Cast<SelectorFieldAttribute>());

                    WithMinMaxFromAttribute(valueElement.Find<IntegerUpDown>(NumericValueName), propAttribute);
                }
                else if (propAttribute.IsAggregate)
                    valueElement = GetNumericUpDown<int>(IntegerValuePanelKey, propAttribute);
                else if (propAttribute.IsSelector)
                    valueElement = SetComboBoxBindingFromAttribute(GetByTemplateKey<ComboBox>(SelectorValuePanelKey), propAttribute.Cast<SelectorFieldAttribute>());
                else if (propType == typeof(bool))
                    valueElement = GetByTemplateKey<FrameworkElement>(BooleanValuePanelKey);
                else if (propType == typeof(DateTime))
                    valueElement = GetByTemplateKey<FrameworkElement>(DateValuePanelKey);
                else if (propType == typeof(int))
                    valueElement = GetNumericUpDown<int>(IntegerValuePanelKey, propAttribute);
                else if (propType == typeof(decimal))
                    valueElement = GetNumericUpDown<decimal>(DecimalValuePanelKey, propAttribute);
                else if (propType == typeof(string))
                    valueElement = GetByTemplateKey<FrameworkElement>(StringValuePanelKey);
                else
                    throw new NotSupportedException();

                criterionValuePanel.Children.Add(valueElement);

                isNullCheckBox.RemoveRoutedEventHandlers(System.Windows.Controls.Primitives.ToggleButton.CheckedEvent);
                isNullCheckBox.RemoveRoutedEventHandlers(System.Windows.Controls.Primitives.ToggleButton.CheckedEvent);
                isNullCheckBox.Checked += (_1, _2) =>
                {
                    valueElement.IsEnabled = false;
                    includeNullCheckBox.IsEnabled = false;
                    comparatorCombo.ItemsSource = Comparator.Equal.Yield(Comparator.NotEqual);
                };
                isNullCheckBox.Unchecked += (_1, _2) =>
                {
                    valueElement.IsEnabled = true;
                    includeNullCheckBox.IsEnabled = true;
                    comparatorCombo.ItemsSource = comparators;
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
                parentContainer.Find<Button>(RemoveCriteriaButtonName)
                    .RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            }
        }

        private void AddContentGroup(Panel containerPanel, UIElement groupContent, bool isOr)
        {
            if (containerPanel.Children.Count > 0)
            {
                containerPanel.Children.Add(GetByTemplateKey<Label>(isOr ? OrLabelTemplateKey : AndLabelTemplateKey));
            }

            containerPanel.Children.Add(groupContent);
        }

        private T GetByTemplateKey<T>(string key) where T : UIElement
        {
            return (FindResource(key) as ControlTemplate).LoadContent() as T;
        }

        private CommonNumericUpDown<T> GetNumericUpDown<T>(string key, FieldAttribute attribute)
            where T : struct, IFormattable, IComparable<T>
        {
            return WithMinMaxFromAttribute(GetByTemplateKey<CommonNumericUpDown<T>>(key), attribute);
        }

        private static CommonNumericUpDown<T> WithMinMaxFromAttribute<T>(CommonNumericUpDown<T> element, FieldAttribute attribute)
            where T : struct, IFormattable, IComparable<T>
        {
            element.Minimum = (T)Convert.ChangeType(attribute.Min, typeof(T));
            element.Maximum = (T)Convert.ChangeType(attribute.Max, typeof(T));
            return element;
        }

        private ComboBox SetComboBoxBindingFromAttribute(ComboBox valueComboBox, SelectorFieldAttribute realAttribute)
        {
            valueComboBox.ItemsSource = realAttribute.GetValuesFunc(_dataProvider);
            if (realAttribute.HasDisplayPropertyName)
                valueComboBox.DisplayMemberPath = realAttribute.DisplayPropertyName;
            return valueComboBox;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
