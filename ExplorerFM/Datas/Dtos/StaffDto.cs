using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ExplorerFM.Datas.Dtos
{
    public class StaffDto
    {
        [BsonId]
        [BsonElement("id")]
        public int ID { get; set; }

        [BsonElement("firstName")]
        [BsonIgnoreIfNull]
        public string Firstname { get; set; }

        [BsonElement("lastName")]
        [BsonIgnoreIfNull]
        public string Lastname { get; set; }

        [BsonElement("commonName")]
        [BsonIgnoreIfNull]
        public string Commonname { get; set; }

        [BsonElement("dateOfBirth")]
        [BsonIgnoreIfNull]
        public DateTime? DateOfBirth { get; set; }

        [BsonElement("yearOfBirth")]
        [BsonIgnoreIfNull]
        public int? YearOfBirth { get; set; }

        [BsonElement("jobClub")]
        [BsonIgnoreIfNull]
        public StaffTypeDto? JobClub { get; set; }

        [BsonElement("jobCountry")]
        [BsonIgnoreIfNull]
        public StaffTypeDto? JobNation { get; set; }

        [BsonElement("country1")]
        [BsonIgnoreIfNull]
        public StaffCountryDto Nation1 { get; set; }

        [BsonElement("country2")]
        [BsonIgnoreIfNull]
        public StaffCountryDto Nation2 { get; set; }

        [BsonElement("caps")]
        public int Caps { get; set; }

        [BsonElement("intGoals")]
        public int IntGoals { get; set; }

        [BsonElement("club")]
        [BsonIgnoreIfNull]
        public StaffClubDto ClubContract { get; set; }

        [BsonElement("dateContractStart")]
        [BsonIgnoreIfNull]
        public DateTime? DateContractStart { get; set; }

        [BsonElement("dateContractEnd")]
        [BsonIgnoreIfNull]
        public DateTime? DateContractEnd { get; set; }

        [BsonElement("value")]
        [BsonIgnoreIfNull]
        public int? Value { get; set; }

        [BsonElement("wage")]
        [BsonIgnoreIfNull]
        public int? Wage { get; set; }

        [BsonElement("natClubContractId")]
        [BsonIgnoreIfNull]
        public int? NatClubContract { get; set; }

        [BsonElement("favClubs")]
        [BsonIgnoreIfNull]
        public IReadOnlyList<int> FavClubs { get; set; }

        [BsonElement("favStaffs")]
        [BsonIgnoreIfNull]
        public IReadOnlyList<int> FavStaffs { get; set; }

        [BsonElement("dislikeClubs")]
        [BsonIgnoreIfNull]
        public IReadOnlyList<int> DislikeClubs { get; set; }

        [BsonElement("dislikeStaffs")]
        [BsonIgnoreIfNull]
        public IReadOnlyList<int> DislikeStaffs { get; set; }

        [BsonElement("playerPositions")]
        [BsonIgnoreIfNull]
        public PlayerPositionsDto PlayerPositions { get; set; }

        [BsonElement("playerSides")]
        [BsonIgnoreIfNull]
        public PlayerSidesDto PlayerSides { get; set; }

        [BsonElement("playerFeatures")]
        [BsonIgnoreIfNull]
        public PlayerFeaturesDto PlayerFeatures { get; set; }

        [BsonElement("playerAttributes")]
        [BsonIgnoreIfNull]
        public PlayerAttributesDto PlayerAttributes { get; set; }

        [BsonElement("coachFeatures")]
        [BsonIgnoreIfNull]
        public CoachFeaturesDto CoachFeatures { get; set; }

        [BsonElement("coachAttributes")]
        [BsonIgnoreIfNull]
        public CoachAttributesDto CoachAttributes { get; set; }

        [BsonElement("isCoach")]
        public bool IsCoach { get; set; }

        [BsonElement("isPlayer")]
        public bool IsPlayer { get; set; }
    }
}
