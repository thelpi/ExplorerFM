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

        [GridView("Country", 3, typeof(Converters.CountryDisplayConverter))]
        [SelectorField("NationID1", nameof(DataProvider.Countries), nameof(Country.Name))]
        public Country Nationality { get; set; }

        [GridView("2nd Country", 4, typeof(Converters.CountryDisplayConverter))]
        [SelectorField("NationID2", nameof(DataProvider.Countries), nameof(Country.Name))]
        public Country SecondNationality { get; set; }

        [GridView("Caps", 5)]
        [Field("Caps", 0, 999)]
        public int Caps { get; set; }

        [GridView("Int. goals", 6)]
        [Field("IntGoals", 0, 999)]
        public int IntGoals { get; set; }

        [GridView("Club", 7, typeof(Converters.ClubDisplayConverter))]
        [SelectorField("ClubContractID", nameof(DataProvider.Clubs), nameof(Club.Name))]
        public Club ClubContract { get; set; }

        [Field("DateContractStart")]
        public DateTime? DateContractStart { get; set; }

        [GridView("Contract end", 9, typeof(Converters.DateDisplayConverter))]
        [Field("DateContractEnd")]
        public DateTime? DateContractEnd { get; set; }

        [GridView("Value", 10)]
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

        [GridView("Curr. abil.", 11)]
        [Field("CurrentAbility", 1, 200)]
        public int? CurrentAbility { get; set; }

        [GridView("Pot. abil.", 12)]
        [Field("PotentialAbility", -2, 200)]
        public int? PotentialAbility { get; set; }

        [GridView("Home rep.", 13)]
        [Field("HomeReputation", 1, 200)]
        public int? HomeReputation { get; set; }

        [GridView("Current rep.", 14)]
        [Field("CurrentReputation", 1, 200)]
        public int? CurrentReputation { get; set; }

        [GridView("World rep.", 15)]
        [Field("WorldReputation", 1, 200)]
        public int? WorldReputation { get; set; }

        [GridView("Name", 1)]
        public string Fullname => string.IsNullOrWhiteSpace(Commonname)
            ? string.Concat(Lastname, ", ", Firstname)
            : Commonname;

        [GridView("Date of birth", 2, typeof(Converters.DateDisplayConverter))]
        public DateTime? ActualDateOfBirth => DateOfBirth.HasValue
            ? DateOfBirth.Value
            : (YearOfBirth.HasValue
                ? new DateTime(YearOfBirth.Value, 7, 1)
                : default(DateTime?));

        public int GetFixedPotentialAbility()
        {
            var pot = PotentialAbility.GetValueOrDefault(100);
            pot = pot == -1 ? 140 : (pot == -2 ? 180 : pot);
            return CurrentAbility.HasValue && pot < CurrentAbility ? CurrentAbility.Value : pot;
        }
    }
}
