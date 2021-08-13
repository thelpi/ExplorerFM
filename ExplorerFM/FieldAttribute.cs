using System;

namespace ExplorerFM
{
    public class FieldAttribute : Attribute
    {
        public string Name { get; }
        public bool IsTripleIdentifier { get; }
        public long Min { get; }
        public long Max { get; }
        public bool IsSql { get; }

        public FieldAttribute(string name)
            : this(name, false, long.MinValue, long.MaxValue, true)
        { }

        public FieldAttribute(string name, bool isTripleIdentifier)
            : this(name, isTripleIdentifier, isTripleIdentifier ? 0 : default(long), long.MaxValue, true)
        { }

        public FieldAttribute(string name, long min)
            : this(name, false, min, long.MaxValue, true)
        { }

        public FieldAttribute(string name, long min, long max)
            : this(name, false, min, max, true)
        { }

        public FieldAttribute(string name, long min, long max, bool isSql)
            : this(name, false, min, max, isSql)
        { }

        private FieldAttribute(string name, bool isTripleIdentifier, long min, long max, bool isSql)
        {
            Name = name;
            IsTripleIdentifier = isTripleIdentifier;
            Min = min;
            Max = max;
            IsSql = isSql;
        }
    }
}
