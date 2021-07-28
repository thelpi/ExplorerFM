namespace ExplorerFM.Datas
{
    public class Country : BaseData
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Code { get; set; }
        public Confederation Confederation { get; set; }
        public bool IsEU { get; set; }
    }
}
