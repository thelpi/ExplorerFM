using System;
using System.Linq;
using System.Reflection;
using ExplorerFM.Datas;
using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.RuleEngine
{
    public class Criterion : CriterionBase
    {
        private string fieldName = null;

        public Criterion(Type targetType, string propertyMap, object fieldValue, Comparator comparator = Comparator.Equal, bool includeNullValue = false)
        {
            TargetType = targetType;
            PropertyMap = propertyMap;
            Comparator = comparator;
            FieldValue = fieldValue;
            IncludeNullValue = includeNullValue;
        }

        public Type TargetType { get; }
        public string PropertyMap { get; }
        public Comparator Comparator { get; }
        public object FieldValue { get; }
        public bool IncludeNullValue { get; }

        public string FieldName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    var mapParts = PropertyMap.Split('.');
                    var mongoParts = new string[mapParts.Length];

                    var currentType = TargetType;
                    for (var i = 0; i < mapParts.Length; i++)
                    {
                        MemberInfo member;
                        if (currentType.IsEnum)
                        {
                            member = currentType.GetMember(mapParts[i])[0];
                        }
                        else
                        {
                            var prop = currentType.GetProperty(mapParts[i]);
                            currentType = prop.PropertyType;
                            member = prop;
                        }
                        var attr = member.GetCustomAttribute<MongoNameAttribute>();
                        mongoParts[i] = attr.Name;
                        currentType = attr.ForcedType ?? currentType;
                    }

                    fieldName = string.Join(".", mongoParts);
                }
                return fieldName;
            }
        }
    }
}
