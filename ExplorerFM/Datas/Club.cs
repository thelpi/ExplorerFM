using System;
using System.Collections.Generic;
using ExplorerFM.Extensions;
using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public class Club : BaseData
    {
        public const int NoClubId = -1;
        public const int AllClubId = -2;

        [Field]
        public string LongName { get; set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public bool StadiumOwner { get; set; }

        [Field]
        public bool PublicLimitedCompany { get; set; }

        [GridView("Club country", 8, typeof(Converters.CountryDisplayConverter))]
        [SelectorField(nameof(DataProvider.Countries), nameof(Datas.Country.Name))]
        public Country Country { get; set; }

        [Field(0)]
        public int? DivisionId { get; set; }

        [Field(0)]
        public int? PreviousDivisionId { get; set; }

        [Field(0)]
        public int? ReserveDivisionId { get; set; }

        [Field(1, 100)]
        public int? LastPosition { get; set; }

        [Field(0)]
        public int? StadiumId { get; set; }

        [Field(0)]
        public int? ReserveStadiumId { get; set; }

        [SelectorField(typeof(ClubStatut))]
        public ClubStatut Statut { get; set; }

        [Field]
        public int? Bank { get; set; }

        [SelectorField(typeof(DayOfWeek))]
        public DayOfWeek? MatchDay { get; set; }

        [Field(1)]
        public int? AverageAttendance { get; set; }

        [Field(1)]
        public int? MinimumAttendance { get; set; }

        [Field(1)]
        public int? MaximumAttendance { get; set; }

        [Field(1, 20)]
        public int? Facilities { get; set; }

        [Field(1, 10000)]
        public int? Reputation { get; set; }

        [Field(0)]
        public int? HomeShirtForegroundId { get; set; }

        [Field(0)]
        public int? HomeShirtBackgroundId { get; set; }

        [Field(0)]
        public int? AwayShirtForegroundId { get; set; }

        [Field(0)]
        public int? AwayShirtBackgroundId { get; set; }

        [Field(0)]
        public int? ThirdShirtForegroundId { get; set; }

        [Field(0)]
        public int? ThirdShirtBackgroundId { get; set; }

        [TripleIdField]
        public List<int> LikedStaffIds { get; set; }

        [TripleIdField]
        public List<int> DislikedStaffIds { get; set; }

        [TripleIdField]
        public List<int> RivalClubIds { get; set; }

        public List<Club> GetRivalClubs<T>(List<Club> clubs)
        {
            return RivalClubIds.GetSubList(clubs);
        }

        public List<Staff> GetDislikedStaffs<T>(List<Staff> staffs)
        {
            return DislikedStaffIds.GetSubList(staffs);
        }

        public List<Staff> GetLikedStaffs<T>(List<Staff> staffs)
        {
            return LikedStaffIds.GetSubList(staffs);
        }
    }
}
