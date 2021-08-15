using System;

namespace ExplorerFM.FieldsAttributes
{
    public class FieldAttribute : Attribute
    {
        public string Name { get; }
        public bool IsTripleIdentifier { get; }
        public int Min { get; }
        public int Max { get; }
        public bool IsSql { get; }

        public bool IsAggregate => GetType() == typeof(AggregateFieldAttribute);

        public FieldAttribute(string name)
            : this(name, false, int.MinValue, int.MaxValue, true)
        { }

        public FieldAttribute(string name, bool isTripleIdentifier)
            : this(name, isTripleIdentifier, isTripleIdentifier ? 0 : default(int), int.MaxValue, true)
        { }

        public FieldAttribute(string name, int min)
            : this(name, false, min, int.MaxValue, true)
        { }

        public FieldAttribute(string name, int min, int max)
            : this(name, false, min, max, true)
        { }

        public FieldAttribute(string name, int min, int max, bool isSql)
            : this(name, false, min, max, isSql)
        { }

        private FieldAttribute(string name, bool isTripleIdentifier, int min, int max, bool isSql)
        {
            Name = name;
            IsTripleIdentifier = isTripleIdentifier;
            Min = min;
            Max = max;
            IsSql = isSql;
        }
    }
}
