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
            : this(name, false, long.MinValue, long.MaxValue)
        { }

        public FieldAttribute(string name, bool isTripleIdentifier)
            : this(name, isTripleIdentifier, isTripleIdentifier ? 0 : default(long), long.MaxValue)
        { }

        public FieldAttribute(string name, long min)
            : this(name, false, min, long.MaxValue)
        { }

        public FieldAttribute(string name, long min, long max)
            : this(name, false, min, max)
        { }

        private FieldAttribute(string name, bool isTripleIdentifier, long min, long max)
        {
            Name = name;
            IsTripleIdentifier = isTripleIdentifier;
            Min = min;
            Max = max;
        }
    }
}
