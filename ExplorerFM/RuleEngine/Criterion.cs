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
                        if (i == mapParts.Length - 1 && mapParts[i].Contains(":"))
                        {
                            var enumType = Type.GetType($"{typeof(BaseData).Namespace}.{mapParts[i].Split(':')[0]}");
                            var enumValue = enumType.GetMember(mapParts[i].Split(':')[1])[0];
                            mongoParts[i] = enumValue.GetCustomAttribute<MongoNameAttribute>().Name;
                        }
                        else
                        {
                            var property = currentType.GetProperty(mapParts[i]);
                            mongoParts[i] = property.GetCustomAttribute<MongoNameAttribute>().Name;
                            currentType = property.PropertyType;
                        }
                    }

                    fieldName = string.Join(".", mongoParts);
                }
                return fieldName;
            }
        }
    }
}
