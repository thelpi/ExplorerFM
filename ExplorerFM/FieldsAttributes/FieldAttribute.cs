using System;

namespace ExplorerFM.FieldsAttributes
{
    public class FieldAttribute : Attribute
    {
        public string Name { get; }
        public int Min { get; }
        public int Max { get; }

        public bool IsAggregate => GetType() == typeof(AggregateFieldAttribute);
        public bool IsTripleIdentifier => GetType() == typeof(TripleIdFieldAttribute);
        public bool IsNestedSelector => GetType() == typeof(NestedSelectorFieldAttribute);

        public FieldAttribute(string name)
            : this(name, int.MinValue, int.MaxValue)
        { }

        public FieldAttribute(string name, int min)
            : this(name, min, int.MaxValue)
        { }

        public FieldAttribute(string name, int min, int max)
        {
            Name = name;
            Min = min;
            Max = max;
        }

        public T Cast<T>() where T : FieldAttribute
        {
            return this as T;
        }
    }
}
