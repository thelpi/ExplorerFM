using System;
using System.Collections.Generic;

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
        [Field("YearOfBirth")]
        public int? YearOfBirth { get; set; }
        [Field("NationID1")]
        public Country Nationality { get; set; }
        [Field("NationID2")]
        public Country SecondNationality { get; set; }
        [Field("Caps")]
        public int Caps { get; set; }
        [Field("IntGoals")]
        public int IntGoals { get; set; }
        [Field("ClubContractID")]
        public Club ClubContract { get; set; }
        [Field("DateContractStart")]
        public DateTime? DateContractStart { get; set; }
        [Field("DateContractEnd")]
        public DateTime? DateContractEnd { get; set; }
        [Field("Value")]
        public int? Value { get; set; }
        [Field("Wage")]
        public int? Wage { get; set; }
        [Field("FavClubID", true)]
        public List<int> FavClubIds { get; set; }
        [Field("FavStaffID", true)]
        public List<int> FavStaffIds { get; set; }
        [Field("DislikeClubID", true)]
        public List<int> DislikeClubIds { get; set; }
        [Field("DislikeStaffID", true)]
        public List<int> DislikeStaffIds { get; set; }
        [Field("CurrentAbility")]
        public int? CurrentAbility { get; set; }
        [Field("PotentialAbility")]
        public int? PotentialAbility { get; set; }
        [Field("HomeReputation")]
        public int? HomeReputation { get; set; }
        [Field("CurrentReputation")]
        public int? CurrentReputation { get; set; }
        [Field("WorldReputation")]
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
