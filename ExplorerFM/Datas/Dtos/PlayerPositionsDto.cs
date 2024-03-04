using MongoDB.Bson.Serialization.Attributes;

namespace ExplorerFM.Datas.Dtos
{
    public class PlayerPositionsDto
    {
        [BsonElement("goalKeeper")]
        [BsonIgnoreIfNull]
        public int? PosGoalKeeper { get; set; }

        [BsonElement("sweeper")]
        [BsonIgnoreIfNull]
        public int? PosSweeper { get; set; }

        [BsonElement("defender")]
        [BsonIgnoreIfNull]
        public int? PosDefender { get; set; }

        [BsonElement("defMidfielder")]
        [BsonIgnoreIfNull]
        public int? PosDelMil { get; set; }

        [BsonElement("midfielder")]
        [BsonIgnoreIfNull]
        public int? PosMil { get; set; }

        [BsonElement("offMidfielder")]
        [BsonIgnoreIfNull]
        public int? PosOffMil { get; set; }

        [BsonElement("forward")]
        [BsonIgnoreIfNull]
        public int? PosForward { get; set; }

        [BsonElement("wingBack")]
        [BsonIgnoreIfNull]
        public int? PosWing { get; set; }

        [BsonElement("freeRole")]
        [BsonIgnoreIfNull]
        public int? PosFreeRole { get; set; }
    }
}
