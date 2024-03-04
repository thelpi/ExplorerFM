using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public enum Side
    {
        [MongoName("right")]
        Right = 1,
        [MongoName("left")]
        Left,
        [MongoName("center")]
        Center
    }
}
