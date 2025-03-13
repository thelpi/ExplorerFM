using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public abstract class BaseData
    {
        public const int NoDataId = -1;
        public const int AllDataId = -2;

        [MongoName("_id")]
        public int Id { get; set; }
    }
}
