using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public class Country : BaseData
    {
        public const int NoCountryId = -1;
        public const int AllCountryId = -2;

        [Field("Name")]
        public string LongName { get; set; }

        [Field("NameShort")]
        public string Name { get; set; }

        [Field("Name3")]
        public string Code { get; set; }

        [SelectorField("ContinentID", nameof(DataProvider.Confederations), nameof(Datas.Confederation.Name))]
        public Confederation Confederation { get; set; }

        [GridView("E.U.", 3.5)]
        [Field("is_EU")]
        public bool IsEU { get; set; }
    }
}
