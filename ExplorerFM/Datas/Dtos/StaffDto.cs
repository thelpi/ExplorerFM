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
        [BsonIgnoreIfNull]
        public int? Caps { get; set; }

        [BsonElement("intGoals")]
        [BsonIgnoreIfNull]
        public int? IntGoals { get; set; }

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

    public class PlayerSidesDto
    {
        [BsonElement("right")]
        [BsonIgnoreIfNull]
        public int? SidesRight { get; set; }

        [BsonElement("left")]
        [BsonIgnoreIfNull]
        public int? SidesLeft { get; set; }

        [BsonElement("center")]
        [BsonIgnoreIfNull]
        public int? SidesCenter { get; set; }
    }

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

    public class PlayerAttributesDto
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

        [BsonElement("acceleration")]
        [BsonIgnoreIfNull]
        public int? Acceleration { get; set; }

        [BsonElement("agression")]
        [BsonIgnoreIfNull]
        public int? Agression { get; set; }

        [BsonElement("agility")]
        [BsonIgnoreIfNull]
        public int? Agility { get; set; }

        [BsonElement("anticipation")]
        [BsonIgnoreIfNull]
        public int? Anticipation { get; set; }

        [BsonElement("balance")]
        [BsonIgnoreIfNull]
        public int? Balance { get; set; }

        [BsonElement("bravery")]
        [BsonIgnoreIfNull]
        public int? Bravery { get; set; }

        [BsonElement("consistency")]
        [BsonIgnoreIfNull]
        public int? Consistency { get; set; }

        [BsonElement("corners")]
        [BsonIgnoreIfNull]
        public int? Corners { get; set; }

        [BsonElement("crossing")]
        [BsonIgnoreIfNull]
        public int? Crossing { get; set; }

        [BsonElement("decisions")]
        [BsonIgnoreIfNull]
        public int? Decisions { get; set; }

        [BsonElement("dirtiness")]
        [BsonIgnoreIfNull]
        public int? Dirtiness { get; set; }

        [BsonElement("dribbling")]
        [BsonIgnoreIfNull]
        public int? Dribbling { get; set; }

        [BsonElement("finishing")]
        [BsonIgnoreIfNull]
        public int? Finishing { get; set; }

        [BsonElement("flair")]
        [BsonIgnoreIfNull]
        public int? Flair { get; set; }

        [BsonElement("freeKicks")]
        [BsonIgnoreIfNull]
        public int? FreeKicks { get; set; }

        [BsonElement("handling")]
        [BsonIgnoreIfNull]
        public int? Handling { get; set; }

        [BsonElement("heading")]
        [BsonIgnoreIfNull]
        public int? Heading { get; set; }

        [BsonElement("importantMatchs")]
        [BsonIgnoreIfNull]
        public int? ImportantMatchs { get; set; }

        [BsonElement("injuryProneness")]
        [BsonIgnoreIfNull]
        public int? InjuryProneness { get; set; }

        [BsonElement("jumping")]
        [BsonIgnoreIfNull]
        public int? Jumping { get; set; }

        [BsonElement("leadership")]
        [BsonIgnoreIfNull]
        public int? Leadership { get; set; }

        [BsonElement("longShots")]
        [BsonIgnoreIfNull]
        public int? LongShots { get; set; }

        [BsonElement("marking")]
        [BsonIgnoreIfNull]
        public int? Marking { get; set; }

        [BsonElement("movement")]
        [BsonIgnoreIfNull]
        public int? Movement { get; set; }

        [BsonElement("naturalFitness")]
        [BsonIgnoreIfNull]
        public int? NaturalFitness { get; set; }

        [BsonElement("oneOnOnes")]
        [BsonIgnoreIfNull]
        public int? OneOnOnes { get; set; }

        [BsonElement("pace")]
        [BsonIgnoreIfNull]
        public int? Pace { get; set; }

        [BsonElement("passing")]
        [BsonIgnoreIfNull]
        public int? Passing { get; set; }

        [BsonElement("penalties")]
        [BsonIgnoreIfNull]
        public int? Penalties { get; set; }

        [BsonElement("positioning")]
        [BsonIgnoreIfNull]
        public int? Positioning { get; set; }

        [BsonElement("reflexes")]
        [BsonIgnoreIfNull]
        public int? Reflexes { get; set; }

        [BsonElement("stamina")]
        [BsonIgnoreIfNull]
        public int? Stamina { get; set; }

        [BsonElement("strength")]
        [BsonIgnoreIfNull]
        public int? Strength { get; set; }

        [BsonElement("tackling")]
        [BsonIgnoreIfNull]
        public int? Tackling { get; set; }

        [BsonElement("teamWork")]
        [BsonIgnoreIfNull]
        public int? TeamWork { get; set; }

        [BsonElement("technique")]
        [BsonIgnoreIfNull]
        public int? Technique { get; set; }

        [BsonElement("throwIns")]
        [BsonIgnoreIfNull]
        public int? ThrowIns { get; set; }

        [BsonElement("versatility")]
        [BsonIgnoreIfNull]
        public int? Versatility { get; set; }

        [BsonElement("vision")]
        [BsonIgnoreIfNull]
        public int? Vision { get; set; }

        [BsonElement("workRate")]
        [BsonIgnoreIfNull]
        public int? WorkRate { get; set; }
    }

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

    public enum StaffTypeDto
    {
        Chairman = 1,
        ManagingDirector,
        GeneralManager,
        DirectorOfFootball,
        Manager,
        AssistantManager,
        PlayerCoach,
        Coach,
        Scout,
        Physio,
        Player,
        PlayerManager,
        PlayerAssistantManager
    }

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
