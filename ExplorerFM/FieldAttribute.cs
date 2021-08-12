using System;

namespace ExplorerFM
{
    public class FieldAttribute : Attribute
    {
        public string Name { get; }
        public bool IsTripleIdentifier { get; }
        public long Min { get; }
        public long Max { get; }

        public FieldAttribute(string name)
        {
            Name = name;
        }

        public FieldAttribute(string name, bool isTripleIdentifier)
        {
            Name = name;
            IsTripleIdentifier = isTripleIdentifier;
            Min = isTripleIdentifier
                ? 0
                : default(long);
        }

        public FieldAttribute(string name, long min)
        {
            Min = min;
        }

        public FieldAttribute(string name, long min, long max)
        {
            Min = min;
            Max = max;
        }
    }
}
