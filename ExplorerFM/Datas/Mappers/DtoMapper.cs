using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExplorerFM.Datas.Dtos;

namespace ExplorerFM.Datas.Mappers
{
    internal static class DtoMapper
    {
        private static readonly Dictionary<Attribute, PropertyInfo> _propertiesMapper =
            typeof(PlayerAttributesDto).GetProperties().ToDictionary(x => Attribute.GetPlayerAttributeBy(x.Name), x => x);

        internal static Player ToPlayer(this StaffDto dto, IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries)
        {
            return new Player
            {
                DateContractEnd = dto.DateContractEnd,
                DateContractStart = dto.DateContractStart,
                DateOfBirth = dto.DateOfBirth,
                DislikeClubIds = dto.DislikeClubs.EnsureList(),
                DislikeStaffIds = dto.DislikeStaffs.EnsureList(),
                Caps = dto.Caps,
                ClubContract = dto.ClubContract.ToData(x => x.Id, clubs),
                Commonname = dto.Commonname,
                CurrentAbility = dto.PlayerFeatures.CurrentAbility,
                CurrentReputation = dto.PlayerFeatures.CurrentReputation,
                FavClubIds = dto.FavClubs.EnsureList(),
                FavStaffIds = dto.FavStaffs.EnsureList(),
                Firstname = dto.Firstname,
                HomeReputation = dto.PlayerFeatures.HomeReputation,
                Id = dto.ID,
                IntGoals = dto.IntGoals,
                Lastname = dto.Lastname,
                LeftFoot = dto.PlayerFeatures.LeftFoot,
                Loaded = true,
                Nationality = dto.Nation1.ToData(x => x.Id, countries),
                PotentialAbility = dto.PlayerFeatures.SpecialPotential ?? dto.PlayerFeatures.PotentialAbility,
                RightFoot = dto.PlayerFeatures.RightFoot,
                SecondNationality = dto.Nation2.ToData(x => x.Id, countries),
                SquadNumber = dto.PlayerFeatures.SquadNumber,
                Value = dto.Value,
                Wage = dto.Wage,
                WorldReputation = dto.PlayerFeatures.WorldReputation,
                YearOfBirth = dto.YearOfBirth,
                Sides = new Dictionary<Side, int?>
                {
                    { Side.Left, dto.PlayerSides.SidesLeft },
                    { Side.Right, dto.PlayerSides.SidesRight },
                    { Side.Center, dto.PlayerSides.SidesCenter },
                },
                Attributes = _propertiesMapper.ToDictionary(x => x.Key, x => (int?)x.Value.GetValue(dto.PlayerAttributes)),
                Positions = new Dictionary<Position, int?>
                {
                    { Position.Striker, dto.PlayerPositions.PosForward },
                    { Position.OffensiveMidfielder, dto.PlayerPositions.PosOffMil },
                    { Position.Midfielder, dto.PlayerPositions.PosMil },
                    { Position.Defender, dto.PlayerPositions.PosDefender },
                    { Position.DefensiveMidfielder, dto.PlayerPositions.PosDelMil },
                    { Position.FreeRole, dto.PlayerPositions.PosFreeRole },
                    { Position.GoalKeeper, dto.PlayerPositions.PosGoalKeeper },
                    { Position.Sweeper, dto.PlayerPositions.PosSweeper },
                    { Position.WingBack, dto.PlayerPositions.PosWing }
                }
            };
        }

        internal static Club ToClub(this ClubDto dto, IReadOnlyDictionary<int, Country> countries)
        {
            return new Club
            {
                DislikedStaffIds = dto.DislikedStaff.EnsureList(),
                DivisionId = dto.DivisionID,
                MatchDay = dto.MatchDay,
                PreviousDivisionId = dto.DivisionPreviousID,
                ReserveDivisionId = dto.DivisionReserveID,
                AverageAttendance = dto.AverageAttendance,
                AwayShirtBackgroundId = dto.AwayShirtBackgroundID,
                AwayShirtForegroundId = dto.AwayShirtForegroundID,
                Bank = dto.Bank,
                Country = dto.Country.ToData(x => x.Id, countries),
                Facilities = dto.Facilities,
                HomeShirtBackgroundId = dto.HomeShirtBackgroundID,
                HomeShirtForegroundId = dto.HomeShirtForegroundID,
                Id = dto.ID,
                LastPosition = dto.LastPosition,
                LikedStaffIds = dto.LikedStaff.EnsureList(),
                LongName = dto.LongName,
                MaximumAttendance = dto.MaximumAttendance,
                MinimumAttendance = dto.MinimumAttendance,
                Name = dto.ShortName,
                PublicLimitedCompany = dto.PLC,
                Reputation = dto.Reputation,
                ReserveStadiumId = dto.StadiumReserveID,
                RivalClubIds = dto.RivalClubs.EnsureList(),
                StadiumId = dto.StadiumID,
                StadiumOwner = dto.StadiumOwner,
                Statut = (ClubStatut)(int)dto.Statut,
                ThirdShirtBackgroundId = dto.ThirdShirtBackgroundID,
                ThirdShirtForegroundId = dto.ThirdShirtForegroundID
            };
        }

        internal static Country ToCountry(this CountryDto dto, IReadOnlyDictionary<int, Confederation> confederations)
        {
            return new Country
            {
                Code = dto.Name3,
                Confederation = dto.ToData(x => x.ConfederationId, confederations),
                Id = dto.ID,
                IsEU = dto.IsEU,
                LongName = dto.Name,
                Name = dto.NameShort
            };
        }

        internal static Confederation ToConfederation(this ConfederationDto dto)
        {
            return new Confederation
            {
                Code = dto.Name3,
                FedCode = dto.FedSigle,
                FedName = dto.FedName,
                Id = dto.ID,
                Name = dto.Name,
                PeopleName = dto.PeopleName,
                Strength = dto.Strength
            };
        }

        private static List<int> EnsureList(this IEnumerable<int> baseList)
        {
            return baseList?.ToList() ?? new List<int>();
        }

        private static T ToData<T, TDto>(this TDto dto, Func<TDto, int?> getId, IReadOnlyDictionary<int, T> datas) where T : class
        {
            return dto != null && getId(dto).HasValue && datas.TryGetValue(getId(dto).Value, out var data) ? data : null;
        }
    }
}
