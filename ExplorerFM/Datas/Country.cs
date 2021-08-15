using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public class Country : BaseData
    {
        [Field("Name")]
        public string LongName { get; set; }
        [Field("NameShort")]
        public string Name { get; set; }
        [Field("Name3")]
        public string Code { get; set; }
        [Field("ContinentID")]
        public Confederation Confederation { get; set; }
        [Field("is_EU")]
        public bool IsEU { get; set; }
    }
}
