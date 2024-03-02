using MongoDB.Bson.Serialization.Attributes;

namespace ExplorerFM.Datas.Dtos
{
    public class CountryDto
    {
        [BsonId]
        [BsonElement("id")]
        public int ID { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("nameShort")]
        public string NameShort { get; set; }

        [BsonElement("name3")]
        public string Name3 { get; set; }

        [BsonElement("confederationId")]
        [BsonIgnoreIfNull]
        public int? ConfederationId { get; set; }
    }
}
