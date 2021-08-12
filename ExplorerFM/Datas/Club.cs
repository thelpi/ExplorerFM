using System;
using System.Collections.Generic;

namespace ExplorerFM.Datas
{
    public class Club : BaseData
    {
        [Field("LongName")]
        public string Name { get; set; }
        [Field("ShortName")]
        public string ShortName { get; set; }
        [Field("StadiumOwner")]
        public bool StadiumOwner { get; set; }
        [Field("PLC")]
        public bool PublicLimitedCompany { get; set; }
        [Field("NationID")]
        public Country Country { get; set; }
        [Field("DivisionID")]
        public int? DivisionId { get; set; }
        [Field("DivisionPreviousID")]
        public int? PreviousDivisionId { get; set; }
        [Field("DivisionReserveID")]
        public int? ReserveDivisionId { get; set; }
        [Field("LastPosition")]
        public int? LastPosition { get; set; }
        [Field("StadiumID")]
        public int? StadiumId { get; set; }
        [Field("StadiumReserveID")]
        public int? ReserveStadiumId { get; set; }
        [Field("Statut")]
        public ClubStatut Statut { get; set; }
        [Field("Bank")]
        public int? Bank { get; set; }
        [Field("MatchDay")]
        public DayOfWeek? MatchDay { get; set; }
        [Field("AverageAttendance")]
        public int? AverageAttendance { get; set; }
        [Field("MinimumAttendance")]
        public int? MinimumAttendance { get; set; }
        [Field("MaximumAttendance")]
        public int? MaximumAttendance { get; set; }
        [Field("Facilities")]
        public int? Facilities { get; set; }
        [Field("Reputation")]
        public int? Reputation { get; set; }
        [Field("HomeShirtForegroundID")]
        public int? HomeShirtForegroundId { get; set; }
        [Field("HomeShirtBackgroundID")]
        public int? HomeShirtBackgroundId { get; set; }
        [Field("AwayShirtForegroundID")]
        public int? AwayShirtForegroundId { get; set; }
        [Field("AwayShirtBackgroundID")]
        public int? AwayShirtBackgroundId { get; set; }
        [Field("ThirdShirtForegroundID")]
        public int? ThirdShirtForegroundId { get; set; }
        [Field("ThirdShirtBackgroundID")]
        public int? ThirdShirtBackgroundId { get; set; }
        [Field("LikedStaffID", true)]
        public List<int> LikedStaffIds { get; set; }
        [Field("DislikedStaffID", true)]
        public List<int> DislikedStaffIds { get; set; }
        [Field("RivalClubsID", true)]
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

        public override string ToString()
        {
            return ShortName;
        }
    }
}
