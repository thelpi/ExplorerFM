using System;
using System.Linq;

namespace ExplorerFM.RuleEngine
{
    public class Criterion : CriterionBase
    {
        public string FieldName { get; }
        public Comparator Comparator { get; }
        public object FieldValue { get; }
        public bool IncludeNullValue { get; }
        public bool FieldIsTripleIdentifier { get; }

        public Criterion(
            FieldAttribute fieldAttribute,
            Type targetedType,
            Comparator comparator,
            object fieldValue,
            bool isNullComparison,
            bool includeNullValue)
        {
            if (typeof(Datas.BaseData).IsAssignableFrom(fieldValue.GetType()))
                fieldValue = (fieldValue as Datas.BaseData).Id;

            if (isNullComparison)
            {
                if (comparator != Comparator.Equal && comparator != Comparator.NotEqual)
                    throw new ArgumentException("This comparator is not intended without value.", nameof(comparator));
                fieldValue = null;
                includeNullValue = false;
            }
            else if (fieldValue.GetType() == typeof(string))
            {
                ProcessValueAsString(comparator, ref fieldValue);
            }
            else if (comparator.IsStringSymbol())
                throw new ArgumentException("This comparator is intended for string value only.", nameof(comparator));


            if (fieldAttribute.IsSql)
            {
                FieldName = NestedQueries.ContainsKey(targetedType)
                    ? string.Format(NestedQueries[targetedType], fieldAttribute.Name)
                    : fieldAttribute.Name;
            }
            else if (fieldAttribute.Name == nameof(Datas.Player.AttributesSum))
                FieldName = string.Format(AttributesSumSql, includeNullValue ? 10 : 0);
            else
            {
                var valueComponents = fieldValue as object[];
                var valueItem = valueComponents[0];
                var valueItemTargetedType = valueItem.GetType();
                var valueItemValue = valueItemTargetedType.IsClass
                    ? (valueItem as Datas.Attribute).Id
                    : (int)valueItem;
                fieldValue = valueComponents[1];
                FieldName = string.Format(NestedQueries[valueItemTargetedType], valueItemValue);
            }

            FieldValue = fieldValue;
            Comparator = comparator;
            IncludeNullValue = includeNullValue;
            FieldIsTripleIdentifier = fieldAttribute.IsTripleIdentifier;
        }

        private static void ProcessValueAsString(Comparator comparator, ref object fieldValue)
        {
            var stringValue = fieldValue.ToString().Replace("'", "\\'");
            if (comparator.IsStringSymbol()) stringValue = stringValue.Replace("%", "\\%");
            fieldValue = string.Concat(
                comparator.IsStringSymbol() ? "%" : "",
                stringValue,
                comparator.IsStringSymbol() ? "%" : "");
        }

        public override string ToString()
        {
            return FieldIsTripleIdentifier
                ? string.Concat("((", string.Join($") {SqlOr} (", Enumerable.Range(1, 3).Select(i => GetPropertySql(string.Concat(FieldName, i)))), "))")
                : GetPropertySql(FieldName);
        }

        private string GetPropertySql(string realPropertyName)
        {
            if (FieldValue == null)
            {
                return $"{realPropertyName} {(Comparator == Comparator.Equal ? SqlIsString : SqlIsNotString)} {SqlNullString}";
            }
            var usedValue = FieldValue;

            if (FieldValue.GetType() == typeof(DateTime))
                usedValue = string.Concat("'", ((DateTime)usedValue).ToString("yyyy-MM-dd hh:mm:ss"), "'");
            else if (FieldValue.GetType() == typeof(string))
                usedValue = string.Concat("'", usedValue, "'");
            else if (FieldValue.GetType() == typeof(bool))
                usedValue = Convert.ToBoolean(usedValue) ? "1" : "0";
            else if (FieldValue.GetType() == typeof(decimal))
                usedValue = ((decimal)usedValue).ToString(System.Globalization.CultureInfo.InvariantCulture);
            var baseSql = $"{realPropertyName} {Comparator.ToSymbol()} {usedValue}";

            return IncludeNullValue
                ? $"({realPropertyName} IS NULL OR {baseSql})"
                : baseSql;
        }
    }
}
