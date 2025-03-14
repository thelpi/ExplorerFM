using System;
using System.Reflection;
using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.RuleEngine
{
    public class Criterion : CriterionBase
    {
        public Criterion(Type targetType, string[] propertyMap, object fieldValue, Comparator comparator = Comparator.Equal, bool includeNullValue = false)
        {
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
