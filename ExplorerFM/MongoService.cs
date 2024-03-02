using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExplorerFM.Datas;
using ExplorerFM.Datas.Dtos;
using MongoDB.Driver;

namespace ExplorerFM
{
    internal class MongoService
    {
        private readonly IMongoCollection<StaffDto> _staffCollection;
        private readonly IMongoCollection<ConfederationDto> _confederationsCollection;
        private readonly IMongoCollection<CountryDto> _countriesCollection;
        private readonly IMongoCollection<ClubDto> _clubsCollection;

        public MongoService(string connectionString, string database)
        {
            var db = new MongoClient(connectionString).GetDatabase(database);

            _staffCollection = db.GetCollection<StaffDto>("staff");
            _confederationsCollection = db.GetCollection<ConfederationDto>("confederations");
            _countriesCollection = db.GetCollection<CountryDto>("countries");
            _clubsCollection = db.GetCollection<ClubDto>("clubs");
        }

        public IReadOnlyList<Club> GetClubs(IReadOnlyDictionary<int, Country> countries)
        {
            var filter = Builders<ClubDto>.Filter.Empty;

            var dtos = _clubsCollection.Find(filter).ToList();

            var clubs = new List<Club>(dtos.Count);
            foreach (var dto in dtos)
            {
                var club = new Club
                {
                    DislikedStaffIds = dto.DislikedStaff?.ToList() ?? new List<int>(),
                    DivisionId = dto.DivisionID,
                    MatchDay = dto.MatchDay,
                    PreviousDivisionId = dto.DivisionPreviousID,
                    ReserveDivisionId = dto.DivisionReserveID,
                    AverageAttendance = dto.AverageAttendance,
                    AwayShirtBackgroundId = dto.AwayShirtBackgroundID,
                    AwayShirtForegroundId = dto.AwayShirtForegroundID,
                    Bank = dto.Bank,
                    Country = dto.Country != null && countries.TryGetValue(dto.Country.Id, out var country) ? country : null,
                    Facilities = dto.Facilities,
                    HomeShirtBackgroundId = dto.HomeShirtBackgroundID,
                    HomeShirtForegroundId = dto.HomeShirtForegroundID,
                    Id = dto.ID,
                    LastPosition = dto.LastPosition,
                    LikedStaffIds = dto.LikedStaff?.ToList() ?? new List<int>(),
                    LongName = dto.LongName,
                    MaximumAttendance = dto.MaximumAttendance,
                    MinimumAttendance = dto.MinimumAttendance,
                    Name = dto.ShortName,
                    PublicLimitedCompany = dto.PLC,
                    Reputation = dto.Reputation,
                    ReserveStadiumId = dto.StadiumReserveID,
                    RivalClubIds = dto.RivalClubs?.ToList() ?? new List<int>(),
                    StadiumId = dto.StadiumID,
                    StadiumOwner = dto.StadiumOwner,
                    Statut = (ClubStatut)(int)dto.Statut,
                    ThirdShirtBackgroundId = dto.ThirdShirtBackgroundID,
                    ThirdShirtForegroundId = dto.ThirdShirtForegroundID
                };

                clubs.Add(club);
            }

            return clubs;
        }

        public IReadOnlyList<Country> GetCountries(IReadOnlyDictionary<int, Confederation> confederations)
        {
            var filter = Builders<CountryDto>.Filter.Empty;

            var dtos = _countriesCollection.Find(filter).ToList();

            var countries = new List<Country>(dtos.Count);
            foreach (var dto in dtos)
            {
                var country = new Country
                {
                    Code = dto.Name3,
                    Confederation = dto.ConfederationId.HasValue && confederations.TryGetValue(dto.ConfederationId.Value, out var confederation) ? confederation : null,
                    Id = dto.ID,
                    IsEU = false, // TODO
                    LongName = dto.Name,
                    Name = dto.NameShort
                };

                countries.Add(country);
            }

            return countries;
        }

        public IReadOnlyList<Confederation> GetConfederations()
        {
            var filter = Builders<ConfederationDto>.Filter.Empty;

            var dtos = _confederationsCollection.Find(filter).ToList();

            var confederations = new List<Confederation>(dtos.Count);
            foreach (var dto in dtos)
            {
                var confederation = new Confederation
                {
                    Code = dto.Name3,
                    FedCode = dto.FedSigle,
                    FedName = dto.FedName,
                    Id = dto.ID,
                    Name = dto.Name,
                    PeopleName = dto.PeopleName,
                    Strength = dto.Strength / (decimal)100
                };

                confederations.Add(confederation);
            }

            return confederations;
        }

        public IReadOnlyList<Player> GetPlayersByClub(int? clubId,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries)
        {
            var filter = FilterDefinition<StaffDto>.Empty;

            if (clubId.HasValue)
            {
                filter &= Builders<StaffDto>.Filter.Eq(x => x.ClubContract.Id, clubId.Value);
            }
            else
            {
                filter &= Builders<StaffDto>.Filter.Eq(x => x.ClubContract, null);
            }

            return GetPlayersByFilter(filter, clubs, countries);
        }

        public IReadOnlyList<Player> GetPlayersByCountry(int? countryId,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries)
        {
            var filter = FilterDefinition<StaffDto>.Empty;

            if (countryId.HasValue)
            {
                filter &= Builders<StaffDto>.Filter.Or(
                    Builders<StaffDto>.Filter.Eq(x => x.Nation1.Id, countryId.Value),
                    Builders<StaffDto>.Filter.Eq(x => x.Nation2.Id, countryId.Value));
            }
            else
            {
                filter &= Builders<StaffDto>.Filter.Eq(x => x.Nation1, null);
                filter &= Builders<StaffDto>.Filter.Eq(x => x.Nation2, null);
            }

            return GetPlayersByFilter(filter, clubs, countries);
        }

        private IReadOnlyList<Player> GetPlayersByFilter(FilterDefinition<StaffDto> filter,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries)
        {
            filter &= Builders<StaffDto>.Filter.Eq(x => x.IsPlayer, true);
            filter &= Builders<StaffDto>.Filter.Not(
                Builders<StaffDto>.Filter.Eq(x => x.PlayerAttributes, null));
            filter &= Builders<StaffDto>.Filter.Not(
                Builders<StaffDto>.Filter.Eq(x => x.PlayerFeatures, null));
            filter &= Builders<StaffDto>.Filter.Or(
                Builders<StaffDto>.Filter.Gte(x => x.PlayerFeatures.CurrentAbility, 100),
                Builders<StaffDto>.Filter.Gte(x => x.PlayerFeatures.PotentialAbility, 100),
                Builders<StaffDto>.Filter.Eq(x => x.PlayerFeatures.SpecialPotential, -1),
                Builders<StaffDto>.Filter.Eq(x => x.PlayerFeatures.SpecialPotential, -2));

            var dtos = _staffCollection.Find(filter).ToList();

            var players = new List<Player>(dtos.Count);
            foreach (var dto in dtos)
            {
                var player = new Player
                {
                    DateContractEnd = dto.DateContractEnd,
                    DateContractStart = dto.DateContractStart,
                    DateOfBirth = dto.DateOfBirth,
                    DislikeClubIds = dto.DislikeClubs?.ToList() ?? new List<int>(),
                    DislikeStaffIds = dto.DislikeStaffs?.ToList() ?? new List<int>(),
                    Caps = dto.Caps ?? 0,
                    ClubContract = dto.ClubContract != null && clubs.TryGetValue(dto.ClubContract.Id, out var club) ? club : null,
                    Commonname = dto.Commonname,
                    CurrentAbility = dto.PlayerFeatures.CurrentAbility,
                    CurrentReputation = dto.PlayerFeatures.CurrentReputation,
                    FavClubIds = dto.FavClubs?.ToList() ?? new List<int>(),
                    FavStaffIds = dto.FavStaffs?.ToList() ?? new List<int>(),
                    Firstname = dto.Firstname,
                    HomeReputation = dto.PlayerFeatures.HomeReputation,
                    Id = dto.ID,
                    IntGoals = dto.IntGoals ?? 0,
                    Lastname =dto.Lastname ,
                    LeftFoot = dto.PlayerFeatures.LeftFoot,
                    Loaded = true,
                    Nationality = dto.Nation1 != null && countries.TryGetValue(dto.Nation1.Id, out var country) ? country : null,
                    PotentialAbility = dto.PlayerFeatures.SpecialPotential.HasValue
                        ? dto.PlayerFeatures.SpecialPotential.Value
                        : dto.PlayerFeatures.PotentialAbility,
                    RightFoot = dto.PlayerFeatures.RightFoot,
                    SecondNationality = dto.Nation2 != null && countries.TryGetValue(dto.Nation2.Id, out var country2) ? country2 : null,
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

                players.Add(player);
            }

            return players;
        }

        private static readonly Dictionary<Datas.Attribute, PropertyInfo> _propertiesMapper =
            typeof(PlayerAttributesDto).GetProperties().ToDictionary(x => Datas.Attribute.GetPlayerAttributeBy(x.Name), x => x);

        public static string TestConnection(string connectionString, string database)
        {
            string error = null;

            try
            {
                new MongoClient(connectionString)
                    .GetDatabase(database)
                    .GetCollection<StaffDto>("staff")
                    .Find(Builders<StaffDto>.Filter.Eq(x => x.ID, 0));
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return error;
        }
    }
}
