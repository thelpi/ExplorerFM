using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ExplorerFM.Datas.Dtos
{
    internal class ClubDto
    {
        [BsonId]
        [BsonElement("id")]
        public int ID { get; set; }

        [BsonElement("longName")]
        public string LongName { get; set; }

        [BsonElement("shortName")]
        public string ShortName { get; set; }

        [BsonElement("country")]
        [BsonIgnoreIfNull]
        public CountryClubDto Country { get; set; }

        [BsonElement("divisionId")]
        [BsonIgnoreIfNull]
        public int? DivisionID { get; set; }

        [BsonElement("divisionPreviousId")]
        [BsonIgnoreIfNull]
        public int? DivisionPreviousID { get; set; }

        [BsonElement("lastPosition")]
        [BsonIgnoreIfNull]
        public int? LastPosition { get; set; }

        [BsonElement("divisionReserveId")]
        [BsonIgnoreIfNull]
        public int? DivisionReserveID { get; set; }

        [BsonElement("statut")]
        [BsonRepresentation(BsonType.String)]
        public ClubStatutDto Statut { get; set; }

        [BsonElement("bank")]
        [BsonIgnoreIfNull]
        public int? Bank { get; set; }

        [BsonElement("stadiumId")]
        [BsonIgnoreIfNull]
        public int? StadiumID { get; set; }

        [BsonElement("stadiumOwner")]
        public bool StadiumOwner { get; set; }

        [BsonElement("stadiumReserveId")]
        [BsonIgnoreIfNull]
        public int? StadiumReserveID { get; set; }

        [BsonElement("matchDay")]
        [BsonRepresentation(BsonType.String)]
        [BsonIgnoreIfNull]
        public DayOfWeek? MatchDay { get; set; }

        [BsonElement("averageAttendance")]
        [BsonIgnoreIfNull]
        public int? AverageAttendance { get; set; }

        [BsonElement("minimumAttendance")]
        [BsonIgnoreIfNull]
        public int? MinimumAttendance { get; set; }

        [BsonElement("maximumAttendance")]
        [BsonIgnoreIfNull]
        public int? MaximumAttendance { get; set; }

        [BsonElement("facilities")]
        [BsonIgnoreIfNull]
        public int? Facilities { get; set; }

        [BsonElement("reputation")]
        [BsonIgnoreIfNull]
        public int? Reputation { get; set; }

        [BsonElement("plc")]
        public bool PLC { get; set; }

        [BsonElement("homeShirtForegroundId")]
        [BsonIgnoreIfNull]
        public int? HomeShirtForegroundID { get; set; }

        [BsonElement("homeShirtBackgroundId")]
        [BsonIgnoreIfNull]
        public int? HomeShirtBackgroundID { get; set; }

        [BsonElement("awayShirtForegroundId")]
        [BsonIgnoreIfNull]
        public int? AwayShirtForegroundID { get; set; }

        [BsonElement("awayShirtBackgroundId")]
        [BsonIgnoreIfNull]
        public int? AwayShirtBackgroundID { get; set; }

        [BsonElement("thirdShirtForegroundId")]
        [BsonIgnoreIfNull]
        public int? ThirdShirtForegroundID { get; set; }

        [BsonElement("thirdShirtBackgroundId")]
        [BsonIgnoreIfNull]
        public int? ThirdShirtBackgroundID { get; set; }

        [BsonElement("likedStaff")]
        [BsonIgnoreIfNull]
        public IReadOnlyList<int> LikedStaff { get; set; }

        [BsonElement("dislikedStaff")]
        [BsonIgnoreIfNull]
        public IReadOnlyList<int> DislikedStaff { get; set; }

        [BsonElement("rivalClubs")]
        [BsonIgnoreIfNull]
        public IReadOnlyList<int> RivalClubs { get; set; }
    }

    internal class CountryClubDto
    {
        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("confederationId")]
        [BsonIgnoreIfNull]
        public int? ConfederationId { get; set; }
    }

    internal enum ClubStatutDto
    {
        Professional = 1,
        SemiProfessional,
        Amateur
    }
}
