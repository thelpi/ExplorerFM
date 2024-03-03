namespace ExplorerFM.RuleEngine
{
    public class Criterion : CriterionBase
    {
        public string FieldName { get; set; }
        public Comparator Comparator { get; set; }
        public object FieldValue { get; set; }
        public bool IncludeNullValue { get; set; }
    }
}
