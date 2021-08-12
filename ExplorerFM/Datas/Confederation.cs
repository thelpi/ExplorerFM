namespace ExplorerFM.Datas
{
    public class Confederation : BaseData
    {
        [Field("Name")]
        public string Name { get; set; }
        [Field("Name3")]
        public string Code { get; set; }
        [Field("PeopleName")]
        public string PeopleName { get; set; }
        [Field("FedName")]
        public string FedName { get; set; }
        [Field("FedSigle")]
        public string FedCode { get; set; }
        [Field("Strength")]
        public decimal Strength { get; set; }
    }
}
