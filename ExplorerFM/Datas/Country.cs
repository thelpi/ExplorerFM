using ExplorerFM.FieldsAttributes;
using ExplorerFM.Providers;

namespace ExplorerFM.Datas
{
    public class Country : BaseData
    {
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
        [MongoName("isEU")]
        public bool IsEU { get; set; }

        [Field(0, 10000)]
        public int Reputation { get; set; }

        [Field(0, 20)]
        public int LeagueStandard { get; set; }

        public static Country Empty
            => new Country
            {
                Id = NoDataId,
                Name = "No country",
                Confederation = Confederation.Empty
            };

        public static Country Global
            => new Country
            {
                Id = AllDataId,
                Name = "All countries",
                Confederation = Confederation.Empty
            };
    }
}
