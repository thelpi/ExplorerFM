using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public abstract class BaseData
    {
        [MongoName("_id")]
        public int Id { get; set; }
    }
}
