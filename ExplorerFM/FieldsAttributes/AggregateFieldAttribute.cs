namespace ExplorerFM.FieldsAttributes
{
    public class AggregateFieldAttribute : FieldAttribute
    {
        public AggregateFieldAttribute(string name, int min, int max)
            : base(name, min, max, false)
        { }
    }
}
