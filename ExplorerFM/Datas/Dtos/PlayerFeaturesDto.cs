using MongoDB.Bson.Serialization.Attributes;

namespace ExplorerFM.Datas.Dtos
{
    public class PlayerFeaturesDto
    {
        [BsonElement("specialPotential")]
        [BsonIgnoreIfNull]
        public int? SpecialPotential { get; set; }

        [BsonElement("currentAbility")]
        [BsonIgnoreIfNull]
        public int? CurrentAbility { get; set; }

        [BsonElement("potentialAbility")]
        [BsonIgnoreIfNull]
        public int? PotentialAbility { get; set; }

        [BsonElement("homeReputation")]
        [BsonIgnoreIfNull]
        public int? HomeReputation { get; set; }

        [BsonElement("currentReputation")]
        [BsonIgnoreIfNull]
        public int? CurrentReputation { get; set; }

        [BsonElement("worldReputation")]
        [BsonIgnoreIfNull]
        public int? WorldReputation { get; set; }

        [BsonElement("squadNumber")]
        [BsonIgnoreIfNull]
        public int? SquadNumber { get; set; }

        [BsonElement("leftFoot")]
        [BsonIgnoreIfNull]
        public int? LeftFoot { get; set; }

        [BsonElement("rightFoot")]
        [BsonIgnoreIfNull]
        public int? RightFoot { get; set; }
    }
}
