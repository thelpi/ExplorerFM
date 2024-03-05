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
            FieldName = ComputeMongoFieldName(propertyMap);
        }

        public Criterion(Type targetType, string property, object fieldValue, Comparator comparator = Comparator.Equal, bool includeNullValue = false)
            : this(targetType, new[] { property }, fieldValue, comparator, includeNullValue)
        { }

        public Type TargetType { get; }
        public Comparator Comparator { get; }
        public object FieldValue { get; }
        public bool IncludeNullValue { get; }
        public string FieldName { get; }

        private string ComputeMongoFieldName(string[] propertyMap)
        {
            var fieldNameParts = new string[propertyMap.Length];

            var currentType = TargetType;
            for (var i = 0; i < propertyMap.Length; i++)
            {
                MemberInfo member;
                if (currentType.IsEnum)
                {
                    member = currentType.GetMember(propertyMap[i])[0];
                }
                else
                {
                    var prop = currentType.GetProperty(propertyMap[i]);
                    currentType = prop.PropertyType;
                    member = prop;
                }
                var attr = member.GetCustomAttribute<MongoNameAttribute>();
                fieldNameParts[i] = attr.Name;
                currentType = attr.ForcedType ?? currentType;
            }

            return string.Join(".", fieldNameParts);
        }
    }
}
