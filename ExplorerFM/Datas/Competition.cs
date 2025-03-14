using ExplorerFM.FieldsAttributes;
using ExplorerFM.Providers;

namespace ExplorerFM.Datas
{
    public class Competition : BaseData
    {
        [Field]
        public string Name { get; set; }

        [Field]
        public string LongName { get; set; }

        [Field]
        public string Acronym { get; set; }

        // TODO: check the GridView
        [GridView("Competition country", 8, typeof(Converters.CountryDisplayConverter))]
        [SelectorField(nameof(DataProvider.Countries), nameof(Datas.Country.Name))]
        public Country Country { get; set; }

        public static Competition Empty
            => new Competition
            {
                Id = NoDataId,
                Name = "No competition",
                Country = Country.Empty
            };

        public static Competition Global
            => new Competition
            {
                Id = AllDataId,
                Name = "All competitions",
                Country = Country.Empty
            };
    }
}
