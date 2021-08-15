namespace ExplorerFM.FieldsAttributes
{
    public class TripleIdFieldAttribute : FieldAttribute
    {
        public TripleIdFieldAttribute(string name)
            : base(name, 0, int.MaxValue)
        { }
    }
}
