using System;
using System.Collections.Generic;
using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public abstract class Staff : BaseData
    {
        private const int AverageAbility = 100;
        private const int GoodPotentialAbility = 140;
        private const int VeryGoodPotentialAbility = 180;
        private static readonly DateTime IgnorePotential = new DateTime(1973, 7, 1);

        [Field]
        public string Firstname { get; set; }

        [Field]
        public string Lastname { get; set; }

        [Field]
        public string Commonname { get; set; }

        [Field]
        public DateTime? DateOfBirth { get; set; }

        [Field(1900, 2000)]
        public int? YearOfBirth { get; set; }

        [GridView("Country", 3, typeof(Converters.CountryDisplayConverter))]
        [SelectorField(nameof(DataProvider.Countries), nameof(Country.Name))]
        public Country Nationality { get; set; }

        [GridView("2nd Country", 4, typeof(Converters.CountryDisplayConverter))]
        [SelectorField(nameof(DataProvider.Countries), nameof(Country.Name))]
        public Country SecondNationality { get; set; }

        [GridView("Caps", 5)]
        [Field(0, 999)]
        public int Caps { get; set; }

        [GridView("Int. goals", 6)]
        [Field(0, 999)]
        public int IntGoals { get; set; }

        [GridView("Club", 7, typeof(Converters.ClubDisplayConverter))]
        [SelectorField(nameof(DataProvider.Clubs), nameof(Club.Name))]
        public Club ClubContract { get; set; }

        [Field]
        public DateTime? DateContractStart { get; set; }

        [GridView("Contract end", 9, typeof(Converters.DateDisplayConverter))]
        [Field]
        public DateTime? DateContractEnd { get; set; }

        [GridView("Value", 10)]
        [Field(1)]
        public int? Value { get; set; }
        
        [Field(1)]
        public int? Wage { get; set; }

        [TripleIdField]
        public List<int> FavClubIds { get; set; }

        [TripleIdField]
        public List<int> FavStaffIds { get; set; }

        [TripleIdField]
        public List<int> DislikeClubIds { get; set; }

        [TripleIdField]
        public List<int> DislikeStaffIds { get; set; }

        [GridView("Curr. abil.", 11)]
        [Field(1, 200)]
        public int? CurrentAbility { get; set; }

        [GridView("Pot. abil.", 12)]
        [Field(-2, 200)]
        public int? PotentialAbility { get; set; }

        [GridView("Home rep.", 13)]
        [Field(1, 200)]
        public int? HomeReputation { get; set; }

        [GridView("Current rep.", 14)]
        [Field(1, 200)]
        public int? CurrentReputation { get; set; }

        [GridView("World rep.", 15)]
        [Field(1, 200)]
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
            if (ActualDateOfBirth.HasValue && ActualDateOfBirth.Value < IgnorePotential)
                return CurrentAbility ?? AverageAbility;

            var actualPotential = PotentialAbility ?? AverageAbility;
            if (PotentialAbility == -1)
                actualPotential = GoodPotentialAbility;
            else if (PotentialAbility == -2)
                actualPotential = VeryGoodPotentialAbility;

            return CurrentAbility.HasValue && actualPotential < CurrentAbility
                ? CurrentAbility.Value
                : actualPotential;
        }
    }
}
