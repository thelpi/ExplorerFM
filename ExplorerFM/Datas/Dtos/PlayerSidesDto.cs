using MongoDB.Bson.Serialization.Attributes;

namespace ExplorerFM.Datas.Dtos
{
    public class PlayerSidesDto
    {
        [BsonElement("right")]
        [BsonIgnoreIfNull]
        public int? SidesRight { get; set; }

        [BsonElement("left")]
        [BsonIgnoreIfNull]
        public int? SidesLeft { get; set; }

        [BsonElement("center")]
        [BsonIgnoreIfNull]
        public int? SidesCenter { get; set; }
    }
}
