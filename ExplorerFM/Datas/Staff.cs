using System;
using System.Collections.Generic;
using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public abstract class Staff : BaseData
    {
        [Field("Firstname")]
        public string Firstname { get; set; }
        [Field("Lastname")]
        public string Lastname { get; set; }
        [Field("Commonname")]
        public string Commonname { get; set; }
        [Field("DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }
        [Field("YearOfBirth", 1900, 2000)]
        public int? YearOfBirth { get; set; }
        [Field("NationID1")]
        public Country Nationality { get; set; }
        [Field("NationID2")]
        public Country SecondNationality { get; set; }
        [Field("Caps", 0, 999)]
        public int Caps { get; set; }
        [Field("IntGoals", 0, 999)]
        public int IntGoals { get; set; }
        [Field("ClubContractID")]
        public Club ClubContract { get; set; }
        [Field("DateContractStart")]
        public DateTime? DateContractStart { get; set; }
        [Field("DateContractEnd")]
        public DateTime? DateContractEnd { get; set; }
        [Field("Value", 1)]
        public int? Value { get; set; }
        [Field("Wage", 1)]
        public int? Wage { get; set; }
        [TripleIdField("FavClubID")]
        public List<int> FavClubIds { get; set; }
        [TripleIdField("FavStaffID")]
        public List<int> FavStaffIds { get; set; }
        [TripleIdField("DislikeClubID")]
        public List<int> DislikeClubIds { get; set; }
        [TripleIdField("DislikeStaffID")]
        public List<int> DislikeStaffIds { get; set; }
        [Field("CurrentAbility", 1, 200)]
        public int? CurrentAbility { get; set; }
        [Field("PotentialAbility", -2, 200)]
        public int? PotentialAbility { get; set; }
        [Field("HomeReputation", 1, 200)]
        public int? HomeReputation { get; set; }
        [Field("CurrentReputation", 1, 200)]
        public int? CurrentReputation { get; set; }
        [Field("WorldReputation", 1, 200)]
        public int? WorldReputation { get; set; }

        public string Fullname => string.IsNullOrWhiteSpace(Commonname)
            ? string.Concat(Lastname, ", ", Firstname)
            : Commonname;
        public string ActualDateOfBirth => DateOfBirth.HasValue
            ? DateOfBirth.Value.ToString("yyyy-MM-dd")
            : (YearOfBirth.HasValue
                ? YearOfBirth.Value.ToString()
                : MissingDataString);
    }
}
