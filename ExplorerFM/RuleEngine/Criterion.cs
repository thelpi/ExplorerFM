namespace ExplorerFM.RuleEngine
{
    public class Criterion : CriterionBase
    {
        public string Property { get; }
        public Comparator Comparator { get; }
        public object Value { get; }
        public bool NullValueAreTrue { get; }

        public static Criterion New(string property, Comparator comparator)
        {
            if (comparator != Comparator.Equal && comparator != Comparator.NotEqual)
                throw new System.ArgumentException("This comparator is not intended without value.", nameof(comparator));
            return new Criterion(property, null, comparator, false);
        }

        public static Criterion New(string property, object value, Comparator comparator, bool nullValueAreTrue)
        {
            if (value.GetType() == typeof(string))
                return new Criterion(property, CheckStringValue(value.ToString(), comparator), comparator, nullValueAreTrue);
            if (value.GetType().IsClass)
                throw new System.ArgumentException("Value is intended as a struct.", nameof(value));

            return new Criterion(property, value, CheckComparator(comparator), nullValueAreTrue);
        }

        private Criterion(string property, object value, Comparator comparator, bool nullValueAreTrue)
        {
            Property = property;
            Value = value;
            Comparator = comparator;
            NullValueAreTrue = nullValueAreTrue;
        }

        public override string ToString()
        {
            if (Value == null)
            {
                return $"{Property} {(Comparator == Comparator.Equal ? SqlIsString : SqlIsNotString)} {SqlNullString}";
            }
            var usedValue = Value;
            
            if (Value.GetType() == typeof(System.DateTime))
                usedValue = string.Concat("'", ((System.DateTime)usedValue).ToString("yyyy-MM-dd hh:mm:ss"), "'");
            else if (Value.GetType() == typeof(string))
                usedValue = string.Concat("'", usedValue, "'");
            else if (Value.GetType() == typeof(bool))
                usedValue = System.Convert.ToBoolean(usedValue) ? "1" : "0";
            var baseSql = $"{Property} {Comparator.ToSymbol()} {usedValue}";

            return NullValueAreTrue
                ? $"({Property} IS NULL OR {baseSql})"
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
