using MongoDB.Bson.Serialization.Attributes;

namespace ExplorerFM.Datas.Dtos
{
    public class StaffClubDto
    {
        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("country")]
        [BsonIgnoreIfNull]
        public CountryClubDto Country { get; set; }

        [BsonElement("divisionId")]
        [BsonIgnoreIfNull]
        public int? DivisionID { get; set; }
    }
}
