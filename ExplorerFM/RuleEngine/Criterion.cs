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
            return new Criterion(property, value as object, comparator, false);
        }

        public static Criterion New<T>(string property, T value, Comparator comparator, bool nullValueAreTrue)
            where T : struct
        {
            return new Criterion(property, value as object, comparator, nullValueAreTrue);
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
    }
}
