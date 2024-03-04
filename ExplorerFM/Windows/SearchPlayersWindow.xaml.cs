using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ExplorerFM.Extensions;
using ExplorerFM.FieldsAttributes;
using ExplorerFM.RuleEngine;
using Xceed.Wpf.Toolkit;

namespace ExplorerFM.Windows
{
    public partial class SearchPlayersWindow : Window
    {
        private readonly string CriteriaPanelName = "CriteriaPanel";
        private readonly string AttributeComboBoxName = "AttributeComboBox";
        private readonly string ComparatorComboBoxName = "ComparatorComboBox";
        private readonly string IsNullCheckBoxName = "IsNullCheckBox";
        private readonly string IncludeNullCheckBoxName = "IncludeNullCheckBox";
        private readonly string CriterionValuePanelName = "CriterionValuePanel";
        private readonly string RemoveCriteriaButtonName = "RemoveCriteriaButton";
        private readonly string AddCriterionButtonName = "AddCriterionButton";
        private readonly string CopyCriteriaButtonName = "CopyCriteriaButton";
        private readonly string RemoveCriterionButtonName = "RemoveCriterionButton";
        private readonly string ComboValueName = "ComboValue";
        private readonly string NumericValueName = "NumericValue";

        private readonly string CriteriaPanelTemplateKey = "CriteriaPanelTemplate";
        private readonly string OrLabelTemplateKey = "OrLabelTemplate";
        private readonly string AndLabelTemplateKey = "AndLabelTemplate";
        private readonly string CriterionPanelTemplateKey = "CriterionPanelTemplate";
        private readonly string IntegerValuePanelKey = "IntegerValuePanel";
        private readonly string DecimalValuePanelKey = "DecimalValuePanel";
        private readonly string StringValuePanelKey = "StringValuePanel";
        private readonly string DateValuePanelKey = "DateValuePanel";
        private readonly string SelectorValuePanelKey = "SelectorValuePanel";
        private readonly string BooleanValuePanelKey = "BooleanValuePanel";
        private readonly string SelectorIntegerValuePanelKey = "SelectorIntegerValuePanel";

        private readonly DataProvider _dataProvider;
        private readonly IList _attributeProperties;

        private bool _descendingSort;

        public SearchPlayersWindow(DataProvider dataProvider)
        {
            InitializeComponent();

            _attributeProperties = DataProvider.GetAllAttribute<FieldAttribute>();

            _dataProvider = dataProvider;

            foreach (var columnKvp in GuiExtensions.GetAttributeColumns(false, HeaderColumnClick))
                PlayersGrid.Columns.Add(columnKvp.Key);
        }

        private void HeaderColumnClick(GridViewAttribute attribute, PropertyInfo property)
        {
            if (PlayersView.ItemsSource is IEnumerable<Datas.Player> source)
            {
                PlayersView.ItemsSource = _descendingSort
                    ? source.OrderByDescending(_ => _.GetSortablePropertyValue(property, attribute))
                    : source.OrderBy(_ => _.GetSortablePropertyValue(property, attribute));
                _descendingSort = !_descendingSort;
            }
        }

        private void PlayersView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pItem = PlayersView.SelectedItem;
            if (pItem != null)
            {
                //Hide();
                //new PlayerWindow(pItem as Datas.Player).ShowDialog();
                //ShowDialog();
            }
        }

        private void SearchPlayersButton_Click(object sender, RoutedEventArgs e)
        {
            var criteriaSet = ExtractCriteriaSet(CriteriaSetPanel);
            var response = criteriaSet.Criteria.Count == 0
                ? System.Windows.MessageBox.Show("Extracts without criteria ?", "ExplorerFM", MessageBoxButton.YesNo)
                : MessageBoxResult.Yes;
            if (response == MessageBoxResult.Yes)
            {
                LoadingProgressBar.HideWorkAndDisplay(
                    () => _dataProvider.GetPlayersByCriteria(criteriaSet),
                    players => PlayersView.ItemsSource = players,
                    PlayersView, CriteriaExpander);
            }
        }

        private void AddCriteriaSetButton_Click(object sender, RoutedEventArgs e)
        {
            AddCriteriaSet(true);
        }

        private CriteriaSet ExtractCriteriaSet(StackPanel criteriaSetPanel)
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

        private Criterion ExtractCriterion(StackPanel criterionPanel)
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

            return new Criterion
            {
                Comparator = (Comparator)comparatorComboBox.SelectedItem,
                FieldName = attrPropInfo.GetCustomAttribute<FieldAttribute>().Name,
                FieldValue = value,
                IncludeNullValue = includeNullCheckBox.IsChecked == true
            };
        }

        private object GetUiElementValue(UIElement valuatedElement, UIElement elementToValuate = null)
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

        private void AddCriteriaSet(bool addFirstCriterion)
        {
            var criteriaSetBorder = this.GetByTemplateKey<Border>(CriteriaPanelTemplateKey);
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

        private void CopyCriterion(StackPanel newCriterion, StackPanel currentCriterion)
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
            var criterionPanel = this.GetByTemplateKey<StackPanel>(CriterionPanelTemplateKey);

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
                    valueElement = this.GetByTemplateKey<FrameworkElement>(SelectorIntegerValuePanelKey);

                    valueElement
                        .Find<ComboBox>(ComboValueName)
                        .WithBindingMaxFromAttribute(propAttribute.Cast<SelectorFieldAttribute>(), _dataProvider);

                    valueElement
                        .Find<IntegerUpDown>(NumericValueName)
                        .WithMinMaxFromAttribute(propAttribute);
                }
                else if (propAttribute.IsAggregate)
                    valueElement = this.GetNumericUpDown<int>(IntegerValuePanelKey, propAttribute);
                else if (propAttribute.IsSelector)
                    valueElement = this
                        .GetByTemplateKey<ComboBox>(SelectorValuePanelKey)
                        .WithBindingMaxFromAttribute(propAttribute.Cast<SelectorFieldAttribute>(), _dataProvider);
                else if (propType == typeof(bool))
                    valueElement = this.GetByTemplateKey<FrameworkElement>(BooleanValuePanelKey);
                else if (propType == typeof(DateTime))
                    valueElement = this.GetByTemplateKey<FrameworkElement>(DateValuePanelKey);
                else if (propType == typeof(int))
                    valueElement = this.GetNumericUpDown<int>(IntegerValuePanelKey, propAttribute);
                else if (propType == typeof(decimal))
                    valueElement = this.GetNumericUpDown<decimal>(DecimalValuePanelKey, propAttribute);
                else if (propType == typeof(string))
                    valueElement = this.GetByTemplateKey<FrameworkElement>(StringValuePanelKey);
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

        private void AddContentGroup(Panel containerPanel, UIElement groupContent, bool isOr)
        {
            if (containerPanel.Children.Count > 0)
            {
                containerPanel.Children.Add(this.GetByTemplateKey<Label>(isOr ? OrLabelTemplateKey : AndLabelTemplateKey));
            }

            containerPanel.Children.Add(groupContent);
        }

        private void RemoveCriteriaBase(Panel containerPanel, UIElement relatedContent)
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
        
        private void ClubAccessMenu_Click(object sender, RoutedEventArgs e)
        {
            var player = (sender as FrameworkElement).DataContext as Datas.Player;

            Hide();
            // player.ClubContract
            new ClubWindow(_dataProvider, false).ShowDialog();
            ShowDialog();
        }
    }
}
