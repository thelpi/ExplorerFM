using System;

namespace ExplorerFM.FieldsAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MongoNameAttribute : Attribute
    {
        public string Name { get; }
        public Type ForcedType { get; }

        public MongoNameAttribute(string name, Type forcedType = null)
        {
            Name = name;
            ForcedType = forcedType;
        }
    }
}
