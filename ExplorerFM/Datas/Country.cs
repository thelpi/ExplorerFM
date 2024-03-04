using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public class Country : BaseData
    {
        public const int NoCountryId = -1;
        public const int AllCountryId = -2;

        [Field]
        public string LongName { get; set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public string Code { get; set; }

        [SelectorField(nameof(DataProvider.Confederations), nameof(Datas.Confederation.Name))]
        public Confederation Confederation { get; set; }

        [GridView("E.U.", 3.5)]
        [Field]
        public bool IsEU { get; set; }
    }
}
