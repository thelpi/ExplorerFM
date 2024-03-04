using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public class Confederation : BaseData
    {
        [Field]
        public string Name { get; set; }
        
        [Field]
        public string Code { get; set; }

        [Field]
        public string PeopleName { get; set; }

        [Field]
        public string FedName { get; set; }

        [Field]
        public string FedCode { get; set; }

        [Field(0, 100)]
        public int Strength { get; set; }
    }
}
