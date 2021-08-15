using System;
using System.Collections;

namespace ExplorerFM.FieldsAttributes
{
    public class NestedSelectorFieldAttribute : FieldAttribute
    {
        public Func<DataProvider, IEnumerable> GetValuesFunc { get; }
        public string DisplayPropertyName { get; }

        public bool HasDisplayPropertyName => !string.IsNullOrWhiteSpace(DisplayPropertyName);

        public NestedSelectorFieldAttribute(string name, int min, int max, Type enumType)
            : base(name, min, max)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Expected: enum type.", nameof(enumType));
            GetValuesFunc = _ => Enum.GetValues(enumType);
        }

        public NestedSelectorFieldAttribute(string name, int min, int max, string dataProviderPropertyName, string displayPropertyName)
            : base(name, min, max)
        {
            DisplayPropertyName = displayPropertyName;
            GetValuesFunc = _ => typeof(DataProvider).GetProperty(dataProviderPropertyName).GetValue(_) as IEnumerable;
        }
    }
}
