using System;
using System.Collections.Generic;

namespace ExplorerFM.Datas
{
    public class Club : BaseData
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public bool StadiumOwner { get; set; }
        public bool PublicLimitedCompany { get; set; }
        public Country Country { get; set; }
        public int? DivisionId { get; set; }
        public int? PreviousDivisionId { get; set; }
        public int? ReserveDivisionId { get; set; }
        public int? LastPosition { get; set; }
        public int? StadiumId { get; set; }
        public int? ReserveStadiumId { get; set; }
        public ClubStatut Statut { get; set; }
        public int? Bank { get; set; }
        public DayOfWeek? MatchDay { get; set; }
        public int? AverageAttendance { get; set; }
        public int? MinimumAttendance { get; set; }
        public int? MaximumAttendance { get; set; }
        public int? Facilities { get; set; }
        public int? Reputation { get; set; }
        public int? HomeShirtForegroundId { get; set; }
        public int? HomeShirtBackgroundId { get; set; }
        public int? AwayShirtForegroundId { get; set; }
        public int? AwayShirtBackgroundId { get; set; }
        public int? ThirdShirtForegroundId { get; set; }
        public int? ThirdShirtBackgroundId { get; set; }
        public List<int> LikedStaffIds { get; set; }
        public List<int> DislikedStaffIds { get; set; }
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
