using System;
using System.Collections.Generic;

namespace ExplorerFM.Datas
{
    public abstract class Staff : BaseData
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Commonname { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? YearOfBirth { get; set; }
        public Country Nationality { get; set; }
        public Country SecondNationality { get; set; }
        public int Caps { get; set; }
        public int IntGoals { get; set; }
        public Club ClubContract { get; set; }
        public DateTime? DateContractStart { get; set; }
        public DateTime? DateContractEnd { get; set; }
        public int? Value { get; set; }
        public int? Wage { get; set; }
        public List<int> FavClubIds { get; set; }
        public List<int> FavStaffIds { get; set; }
        public List<int> DislikeClubIds { get; set; }
        public List<int> DislikeStaffIds { get; set; }
        public int? CurrentAbility { get; set; }
        public int? PotentialAbility { get; set; }
        public int? HomeReputation { get; set; }
        public int? CurrentReputation { get; set; }
        public int? WorldReputation { get; set; }
    }
}
