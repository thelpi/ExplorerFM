using System;

namespace ExplorerFM.FieldsAttributes
{
    public class FieldAttribute : Attribute
    {
        public string Name { get; }
        public int Min { get; }
        public int Max { get; }
        public bool IsSql { get; }

        public bool IsAggregate => GetType() == typeof(AggregateFieldAttribute);
        public bool IsTripleIdentifier => GetType() == typeof(TripleIdFieldAttribute);

        public FieldAttribute(string name)
            : this(name, int.MinValue, int.MaxValue, true)
        { }

        public FieldAttribute(string name, int min)
            : this(name, min, int.MaxValue, true)
        { }

        public FieldAttribute(string name, int min, int max)
            : this(name, min, max, true)
        { }

        public FieldAttribute(string name, int min, int max, bool isSql)
        {
            Name = name;
            Min = min;
            Max = max;
            IsSql = isSql;
        }
    }
}
