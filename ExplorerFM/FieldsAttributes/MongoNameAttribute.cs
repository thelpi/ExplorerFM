using System;

namespace ExplorerFM.FieldsAttributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MongoNameAttribute : Attribute
    {
        public string Name { get; }

        public MongoNameAttribute(string name)
        {
            Name = name;
        }
    }
}
