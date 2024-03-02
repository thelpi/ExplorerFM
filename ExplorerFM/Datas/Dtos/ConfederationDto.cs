using MongoDB.Bson.Serialization.Attributes;

namespace ExplorerFM.Datas.Dtos
{
    internal class ConfederationDto
    {
        [BsonId]
        [BsonElement("id")]
        public int ID { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("name3")]
        public string Name3 { get; set; }

        [BsonElement("peopleName")]
        public string PeopleName { get; set; }

        [BsonElement("fedName")]
        public string FedName { get; set; }

        [BsonElement("fedSigle")]
        public string FedSigle { get; set; }

        [BsonElement("strength")]
        public int Strength { get; set; }
    }
}
