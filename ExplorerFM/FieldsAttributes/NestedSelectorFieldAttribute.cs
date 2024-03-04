using System;

namespace ExplorerFM.FieldsAttributes
{
    public class NestedSelectorFieldAttribute : SelectorFieldAttribute
    {
        public NestedSelectorFieldAttribute(int min, int max, Type enumType)
            : base(min, max, enumType)
        { }

        public NestedSelectorFieldAttribute(int min, int max, string dataProviderPropertyName, string displayPropertyName)
            : base(min, max, dataProviderPropertyName, displayPropertyName)
        { }
    }
}
