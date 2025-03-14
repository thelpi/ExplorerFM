using System;

namespace ExplorerFM.RuleEngine
{
    public class Criterion : CriterionBase
    {
        public Criterion(Type targetType, string[] propertyMap, object fieldValue, Comparator comparator = Comparator.Equal, bool includeNullValue = false)
        {
            if ((fieldValue == null || fieldValue is bool)
                && comparator != Comparator.Equal
                && comparator != Comparator.NotEqual)
            {
                throw new ArgumentException("Comparator is invalid for this kind of value.", nameof(comparator));
            }

            if (!(fieldValue is string)
                && (comparator == Comparator.Like
                || comparator == Comparator.NotLike))
            {
                throw new ArgumentException("Value should be a string with this comparator.", nameof(fieldValue));
            }

            TargetType = targetType;
            Comparator = comparator;
            FieldValue = fieldValue;
            IncludeNullValue = includeNullValue;
            PropertyMap = propertyMap;
        }

        public Criterion(Type targetType, string property, object fieldValue, Comparator comparator = Comparator.Equal, bool includeNullValue = false)
            : this(targetType, new[] { property }, fieldValue, comparator, includeNullValue)
        { }

        public Type TargetType { get; }
        public Comparator Comparator { get; }
        public object FieldValue { get; }
        public bool IncludeNullValue { get; }
        public string[] PropertyMap { get; }
    }
}
