using System;

namespace ExplorerFM.FieldsAttributes
{
    public class FieldAttribute : Attribute
    {
        public int Min { get; }
        public int Max { get; }

        public bool IsAggregate => GetType() == typeof(AggregateFieldAttribute);
        public bool IsTripleIdentifier => GetType() == typeof(TripleIdFieldAttribute);
        public bool IsNestedSelector => GetType() == typeof(NestedSelectorFieldAttribute);
        public bool IsSelector => typeof(SelectorFieldAttribute).IsAssignableFrom(GetType());

        public FieldAttribute()
            : this(int.MinValue, int.MaxValue)
        { }

        public FieldAttribute(int min)
            : this(min, int.MaxValue)
        { }

        public FieldAttribute(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public T Cast<T>() where T : FieldAttribute
        {
            return this as T;
        }
    }
}
