using System;
using System.Collections;

namespace ExplorerFM.FieldsAttributes
{
    public class SelectorFieldAttribute : FieldAttribute
    {
        public Func<DataProvider, IEnumerable> GetValuesFunc { get; }
        public string DisplayPropertyName { get; }
        public Type ValueType { get; }

        public bool HasDisplayPropertyName => !string.IsNullOrWhiteSpace(DisplayPropertyName);

        public SelectorFieldAttribute(string name, Type enumType)
            : this(name, int.MinValue, int.MaxValue, enumType)
        { }

        public SelectorFieldAttribute(string name, int min, int max, Type enumType)
            : base(name, min, max)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Expected: enum type.", nameof(enumType));
            GetValuesFunc = _ => Enum.GetValues(enumType);
            ValueType = enumType;
        }

        public SelectorFieldAttribute(string name, string dataProviderPropertyName, string displayPropertyName)
            : this(name, int.MinValue, int.MaxValue, dataProviderPropertyName, displayPropertyName)
        { }

        public SelectorFieldAttribute(string name, int min, int max, string dataProviderPropertyName, string displayPropertyName)
            : base(name, min, max)
        {
            var propInfo = typeof(DataProvider).GetProperty(dataProviderPropertyName);

            DisplayPropertyName = displayPropertyName;
            GetValuesFunc = _ => propInfo.GetValue(_) as IEnumerable;
            ValueType = propInfo.PropertyType.GenericTypeArguments[0];
        }
    }
}
