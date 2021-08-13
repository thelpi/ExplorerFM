using System.Linq;

namespace ExplorerFM.RuleEngine
{
    public class Criterion : CriterionBase
    {
        public string Property { get; }
        public Comparator Comparator { get; }
        public object Value { get; }
        public bool NullValueAreTrue { get; }
        public bool IsTripleIdentifier { get; }

        public static Criterion New(string property, Comparator comparator, bool isTripleIdentifier)
        {
            if (comparator != Comparator.Equal && comparator != Comparator.NotEqual)
                throw new System.ArgumentException("This comparator is not intended without value.", nameof(comparator));
            return new Criterion(property, null, comparator, false, isTripleIdentifier);
        }

        public static Criterion New(string property, object value, Comparator comparator, bool nullValueAreTrue, bool isTripleIdentifier)
        {
            if (value.GetType() == typeof(string))
                return new Criterion(property, CheckStringValue(value.ToString(), comparator), comparator, nullValueAreTrue, isTripleIdentifier);
            if (value.GetType().IsClass)
                throw new System.ArgumentException("Value is intended as a struct.", nameof(value));

            return new Criterion(property, value, CheckComparator(comparator), nullValueAreTrue, isTripleIdentifier);
        }

        private Criterion(string property, object value, Comparator comparator, bool nullValueAreTrue, bool isTripleIdentifier)
        {
            Property = property;
            Value = value;
            Comparator = comparator;
            NullValueAreTrue = nullValueAreTrue;
            IsTripleIdentifier = isTripleIdentifier;
        }

        public override string ToString()
        {
            return IsTripleIdentifier
                ? string.Concat("((", string.Join(") OR (", Enumerable.Range(1, 3).Select(i => GetPropertySql(string.Concat(Property, i)))), "))")
                : GetPropertySql(Property);
        }

        private string GetPropertySql(string realPropertyName)
        {
            if (Value == null)
            {
                return $"{realPropertyName} {(Comparator == Comparator.Equal ? SqlIsString : SqlIsNotString)} {SqlNullString}";
            }
            var usedValue = Value;

            if (Value.GetType() == typeof(System.DateTime))
                usedValue = string.Concat("'", ((System.DateTime)usedValue).ToString("yyyy-MM-dd hh:mm:ss"), "'");
            else if (Value.GetType() == typeof(string))
                usedValue = string.Concat("'", usedValue, "'");
            else if (Value.GetType() == typeof(bool))
                usedValue = System.Convert.ToBoolean(usedValue) ? "1" : "0";
            var baseSql = $"{realPropertyName} {Comparator.ToSymbol()} {usedValue}";

            return NullValueAreTrue
                ? $"({realPropertyName} IS NULL OR {baseSql})"
                : baseSql;
        }

        private static Comparator CheckComparator(Comparator comparator)
        {
            if (comparator.IsStringSymbol())
                throw new System.ArgumentException("This comparator is intended to string value.", nameof(comparator));
            return comparator;
        }

        private static string CheckStringValue(string value, Comparator comparator)
        {
            if (value == null)
                throw new System.ArgumentNullException(nameof(value));
            var isStringSymbol = comparator.IsStringSymbol();
            value = value.Replace("'", "\\'");
            if (isStringSymbol) value = value.Replace("%", "\\%");
            return string.Concat(
                isStringSymbol ? "%" : "",
                value,
                isStringSymbol ? "%" : "");
        }
    }
}
