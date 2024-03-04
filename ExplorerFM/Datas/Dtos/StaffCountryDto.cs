using MongoDB.Bson.Serialization.Attributes;

namespace ExplorerFM.Datas.Dtos
{
    public class StaffCountryDto
    {
        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("confederationId")]
        [BsonIgnoreIfNull]
        public int? ConfederationId { get; set; }

        [BsonElement("isEU")]
        [BsonIgnoreIfNull]
        public bool IsEU { get; set; }
    }
}
