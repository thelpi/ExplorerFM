using System;

namespace ExplorerFM.FieldsAttributes
{
    public class NestedSelectorFieldAttribute : SelectorFieldAttribute
    {
        public NestedSelectorFieldAttribute(string name, int min, int max, Type enumType)
            : base(name, min, max, enumType)
        { }

        public NestedSelectorFieldAttribute(string name, int min, int max, string dataProviderPropertyName, string displayPropertyName)
            : base(name, min, max, dataProviderPropertyName, displayPropertyName)
        { }
    }
}
