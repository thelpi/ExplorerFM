using MongoDB.Bson.Serialization.Attributes;

namespace ExplorerFM.Datas.Dtos
{
    public class CoachAttributesDto
    {
        [BsonElement("adaptability")]
        [BsonIgnoreIfNull]
        public int? Adaptability { get; set; }

        [BsonElement("ambition")]
        [BsonIgnoreIfNull]
        public int? Ambition { get; set; }

        [BsonElement("determination")]
        [BsonIgnoreIfNull]
        public int? Determination { get; set; }

        [BsonElement("loyalty")]
        [BsonIgnoreIfNull]
        public int? Loyalty { get; set; }

        [BsonElement("pressure")]
        [BsonIgnoreIfNull]
        public int? Pressure { get; set; }

        [BsonElement("professionalism")]
        [BsonIgnoreIfNull]
        public int? Professionalism { get; set; }

        [BsonElement("sportsmanship")]
        [BsonIgnoreIfNull]
        public int? Sportsmanship { get; set; }

        [BsonElement("temperament")]
        [BsonIgnoreIfNull]
        public int? Temperament { get; set; }

        [BsonElement("attacking")]
        [BsonIgnoreIfNull]
        public int? Attacking { get; set; }

        [BsonElement("freeRoles")]
        [BsonIgnoreIfNull]
        public int? FreeRoles { get; set; }

        [BsonElement("marking")]
        [BsonIgnoreIfNull]
        public int? Marking { get; set; }

        [BsonElement("offside")]
        [BsonIgnoreIfNull]
        public int? Offside { get; set; }

        [BsonElement("pressing")]
        [BsonIgnoreIfNull]
        public int? Pressing { get; set; }

        [BsonElement("business")]
        [BsonIgnoreIfNull]
        public int? Business { get; set; }

        [BsonElement("coaching")]
        [BsonIgnoreIfNull]
        public int? Coaching { get; set; }

        [BsonElement("coachingGoals")]
        [BsonIgnoreIfNull]
        public int? CoachingGoals { get; set; }

        [BsonElement("coachingTechnique")]
        [BsonIgnoreIfNull]
        public int? CoachingTechnique { get; set; }

        [BsonElement("directness")]
        [BsonIgnoreIfNull]
        public int? Directness { get; set; }

        [BsonElement("discipline")]
        [BsonIgnoreIfNull]
        public int? Discipline { get; set; }

        [BsonElement("interferences")]
        [BsonIgnoreIfNull]
        public int? Interferences { get; set; }

        [BsonElement("judgingAbility")]
        [BsonIgnoreIfNull]
        public int? JudgingAbility { get; set; }

        [BsonElement("judgingPotential")]
        [BsonIgnoreIfNull]
        public int? JudgingPotential { get; set; }

        [BsonElement("manHandling")]
        [BsonIgnoreIfNull]
        public int? ManHandling { get; set; }

        [BsonElement("motivating")]
        [BsonIgnoreIfNull]
        public int? Motivating { get; set; }

        [BsonElement("patience")]
        [BsonIgnoreIfNull]
        public int? Patience { get; set; }

        [BsonElement("physiotherapy")]
        [BsonIgnoreIfNull]
        public int? Physiotherapy { get; set; }

        [BsonElement("ressources")]
        [BsonIgnoreIfNull]
        public int? Ressources { get; set; }

        [BsonElement("tactics")]
        [BsonIgnoreIfNull]
        public int? Tactics { get; set; }

        [BsonElement("youngsters")]
        [BsonIgnoreIfNull]
        public int? Youngsters { get; set; }
    }
}
