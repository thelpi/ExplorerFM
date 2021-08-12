using System;

namespace ExplorerFM
{
    public class FieldAttribute : Attribute
    {
        public string Name { get; }
        public bool IsTriplet { get; }

        public FieldAttribute(string name)
        {
            Name = name;
        }

        public FieldAttribute(string name, bool isTriplet)
        {
            Name = name;
            IsTriplet = isTriplet;
        }
    }
}
