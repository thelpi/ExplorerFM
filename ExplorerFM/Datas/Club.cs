using System;
using System.Collections.Generic;
using ExplorerFM.FieldsAttributes;

namespace ExplorerFM.Datas
{
    public class Club : BaseData
    {
        [Field("LongName")]
        public string LongName { get; set; }

        [Field("ShortName")]
        public string Name { get; set; }

        [Field("StadiumOwner")]
        public bool StadiumOwner { get; set; }

        [Field("PLC")]
        public bool PublicLimitedCompany { get; set; }

        [GridView("Club country", typeof(Converters.CountryDisplayConverter))]
        [SelectorField("NationID", nameof(DataProvider.Countries), nameof(Datas.Country.Name))]
        public Country Country { get; set; }

        [Field("DivisionID", 0)]
        public int? DivisionId { get; set; }

        [Field("DivisionPreviousID", 0)]
        public int? PreviousDivisionId { get; set; }

        [Field("DivisionReserveID", 0)]
        public int? ReserveDivisionId { get; set; }

        [Field("LastPosition", 1, 100)]
        public int? LastPosition { get; set; }

        [Field("StadiumID", 0)]
        public int? StadiumId { get; set; }

        [Field("StadiumReserveID", 0)]
        public int? ReserveStadiumId { get; set; }

        [SelectorField("Statut", typeof(ClubStatut))]
        public ClubStatut Statut { get; set; }

        [Field("Bank")]
        public int? Bank { get; set; }

        [SelectorField("MatchDay", typeof(DayOfWeek))]
        public DayOfWeek? MatchDay { get; set; }

        [Field("AverageAttendance", 1)]
        public int? AverageAttendance { get; set; }

        [Field("MinimumAttendance", 1)]
        public int? MinimumAttendance { get; set; }

        [Field("MaximumAttendance", 1)]
        public int? MaximumAttendance { get; set; }

        [Field("Facilities", 1, 20)]
        public int? Facilities { get; set; }

        [Field("Reputation", 1, 10000)]
        public int? Reputation { get; set; }

        [Field("HomeShirtForegroundID", 0)]
        public int? HomeShirtForegroundId { get; set; }

        [Field("HomeShirtBackgroundID", 0)]
        public int? HomeShirtBackgroundId { get; set; }

        [Field("AwayShirtForegroundID", 0)]
        public int? AwayShirtForegroundId { get; set; }

        [Field("AwayShirtBackgroundID", 0)]
        public int? AwayShirtBackgroundId { get; set; }

        [Field("ThirdShirtForegroundID", 0)]
        public int? ThirdShirtForegroundId { get; set; }

        [Field("ThirdShirtBackgroundID", 0)]
        public int? ThirdShirtBackgroundId { get; set; }

        [TripleIdField("LikedStaffID")]
        public List<int> LikedStaffIds { get; set; }

        [TripleIdField("DislikedStaffID")]
        public List<int> DislikedStaffIds { get; set; }

        [TripleIdField("RivalClubsID")]
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
