namespace ExplorerFM.RuleEngine
{
    public class Criterion : CriterionBase
    {
        public string Property { get; }
        public Comparator Comparator { get; }
        public object Value { get; }
        public bool NullValueAreTrue { get; }

        public static Criterion New(string property, bool equalNull)
        {
            return new Criterion(property, null, equalNull ? Comparator.Equal : Comparator.NotEqual, false);
        }

        public static Criterion New<T>(string property, T value, Comparator comparator)
            where T : struct
        {
            return new Criterion(property, value as object, CheckComparator(comparator), false);
        }

        public static Criterion New<T>(string property, T value, Comparator comparator, bool nullValueAreTrue)
            where T : struct
        {
            return new Criterion(property, value as object, CheckComparator(comparator), nullValueAreTrue);
        }

        public static Criterion New(string property, string value, Comparator comparator)
        {
            return new Criterion(property, CheckStringValue(value, comparator), comparator, false);
        }

        public static Criterion New(string property, string value, Comparator comparator, bool nullValueAreTrue)
        {
            return new Criterion(property, CheckStringValue(value, comparator), comparator, nullValueAreTrue);
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
            var baseSql = $"{Property} {Comparator.ToSymbol()} {Value}";
            return NullValueAreTrue ? $"({Property} IS NULL OR {baseSql})" : baseSql;
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
            return string.Concat(
                "'",
                comparator.IsStringSymbol() ? "%" : "",
                value.Replace("'", "\'"),
                comparator.IsStringSymbol() ? "%" : "",
                "'");
        }
    }
}
