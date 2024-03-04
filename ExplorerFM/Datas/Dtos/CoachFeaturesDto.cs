using MongoDB.Bson.Serialization.Attributes;

namespace ExplorerFM.Datas.Dtos
{
    public class CoachFeaturesDto
    {
        [BsonElement("specialPotential")]
        [BsonIgnoreIfNull]
        public int? SpecialPotential { get; set; }

        [BsonElement("currentAbility")]
        [BsonIgnoreIfNull]
        public int? ChCurrentAbility { get; set; }

        [BsonElement("potentialAbility")]
        [BsonIgnoreIfNull]
        public int? ChPotentialAbility { get; set; }

        [BsonElement("homeReputation")]
        [BsonIgnoreIfNull]
        public int? ChHomeReputation { get; set; }

        [BsonElement("currentReputation")]
        [BsonIgnoreIfNull]
        public int? ChCurrentReputation { get; set; }

        [BsonElement("worldReputation")]
        [BsonIgnoreIfNull]
        public int? ChWorldReputation { get; set; }
    }
}
