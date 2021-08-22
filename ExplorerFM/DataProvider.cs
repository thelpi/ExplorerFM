using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using ExplorerFM.Datas;
using ExplorerFM.Extensions;
using ExplorerFM.RuleEngine;

namespace ExplorerFM
{
    public class DataProvider
    {
        public static readonly Side[] OrderedSides = new Side[] { Side.Left, Side.Center, Side.Right };

        // ignore wing back and free role
        public static readonly Position[] OrderedPositions = new Position[]
        {
            Position.Striker, Position.OffensiveMidfielder, Position.Midfielder, Position.DefensiveMidfielder,
            Position.Defender, Position.Sweeper, Position.GoalKeeper
        };

        private static readonly System.DateTime DefaultContractDate = new System.DateTime(1996, 1, 2, 0, 0, 0);

        private readonly MySqlService _mySqlService;

        private readonly List<Attribute> _attributeDatas;
        private readonly List<Club> _clubDatas;
        private readonly List<Confederation> _confederationDatas;
        private readonly List<Country> _countryDatas;

        public IReadOnlyCollection<Attribute> Attributes => _attributeDatas;
        public IReadOnlyCollection<Club> Clubs => _clubDatas;
        public IReadOnlyCollection<Confederation> Confederations => _confederationDatas;
        public IReadOnlyCollection<Country> Countries => _countryDatas;

        public int MaxTheoreticalRate => 16 * _attributeDatas.Count;

        public DataProvider(string connectionString)
        {
            _mySqlService = new MySqlService(connectionString);
            _attributeDatas = new List<Attribute>();
            _clubDatas = new List<Club>();
            _confederationDatas = new List<Confederation>();
            _countryDatas = new List<Country>();
        }

        public void Initialize()
        {
            BuildAttributesList();
            BuildConfederationsList();
            BuildCountriesList();
            BuildClubsList();
        }

        public List<Player> GetPlayersByClub(int? clubId)
        {
            return _mySqlService.GetDatas(
                $"SELECT * FROM player WHERE ClubContractID {(clubId.HasValue ? $"= {clubId.Value}" : "IS NULL")}",
                GetPlayerFromDataReader);
        }

        public List<Player> GetPlayersByCriteria(CriteriaSet criteria, System.Action<double> reportFunc)
        {
            var count = _mySqlService.GetData(
                $"SELECT COUNT(*) AS players_count FROM player WHERE {criteria}",
                r => r.Get<int>("players_count"));

            return _mySqlService.GetDatas(
                $"SELECT * FROM player WHERE {criteria}",
                GetPlayerFromDataReader,
                i => reportFunc?.Invoke(i / (double)count));
        }

        public static List<PropertyInfo> GetAllAttribute<T>() where T : System.Attribute
        {
            return typeof(Player).GetAttributeProperties<T>()
                .Concat(typeof(Club).GetAttributeProperties<T>())
                .Concat(typeof(Country).GetAttributeProperties<T>())
                .Concat(typeof(Confederation).GetAttributeProperties<T>())
                .ToList();
        }

        private Player GetPlayerFromDataReader(IDataReader r)
        {
            return new Player
            {
                Caps = r.Get<int>("Caps"),
                ClubContract = _clubDatas.Find(_ => _.Id == r.GetNull<int>("ClubContractID")),
                Commonname = r.Get<string>("Commonname"),
                CurrentAbility = r.GetNull<int>("CurrentAbility"),
                CurrentReputation = r.GetNull<int>("CurrentReputation"),
                DateContractEnd = r.GetNull<System.DateTime>("DateContractEnd"),
                DateContractStart = r.GetNull<System.DateTime>("DateContractStart"),
                DislikeClubIds = r.GetIdList("DislikeClubID{0}"),
                FavClubIds = r.GetIdList("FavClubID{0}"),
                DateOfBirth = r.GetNull<System.DateTime>("DateOfBirth"),
                DislikeStaffIds = r.GetIdList("DislikeStaffID{0}"),
                FavStaffIds = r.GetIdList("FavStaffID{0}"),
                Firstname = r.Get<string>("Firstname"),
                HomeReputation = r.GetNull<int>("HomeReputation"),
                Id = r.Get<int>("ID"),
                IntGoals = r.Get<int>("IntGoals"),
                Lastname = r.Get<string>("Lastname"),
                LeftFoot = r.GetNull<int>("LeftFoot"),
                Nationality = _countryDatas.Find(_ => _.Id == r.GetNull<int>("NationID1")),
                PotentialAbility = r.GetNull<int>("PotentialAbility"),
                RightFoot = r.GetNull<int>("RightFoot"),
                SecondNationality = _countryDatas.Find(_ => _.Id == r.GetNull<int>("NationID2")),
                SquadNumber = r.GetNull<int>("SquadNumber"),
                Value = r.GetNull<int>("Value"),
                Wage = r.GetNull<int>("Wage"),
                WorldReputation = r.GetNull<int>("WorldReputation"),
                YearOfBirth = r.GetNull<int>("YearOfBirth"),
                Sides = GetRates("side", r.Get<int>("ID"), _ => (Side)_),
                Positions = GetRates("position", r.Get<int>("ID"), _ => (Position)_),
                Attributes = GetRates("attribute", r.Get<int>("ID"), _ => _attributeDatas.Find(a => a.Id == _))
            };
        }

        private void BuildAttributesList()
        {
            BuildDatasList(
                _attributeDatas,
                new[] { "ID", "name", "type_ID" },
                "attribute",
                r => new Attribute
                {
                    Id = r.Get<int>("ID"),
                    Name = r.Get<string>("name"),
                    Type = (AttributeType)r.Get<int>("type_ID")
                });
        }

        private void BuildConfederationsList()
        {
            BuildDatasList(
                _confederationDatas,
                new[] { "ID", "Name3", "Name", "PeopleName", "FedName", "FedSigle", "Strength" },
                "confederation",
                r => new Confederation
                {
                    Id = r.Get<int>("ID"),
                    Name = r.Get<string>("Name"),
                    Code = r.Get<string>("Name3"),
                    FedCode = r.Get<string>("FedSigle"),
                    FedName = r.Get<string>("FedName"),
                    PeopleName = r.Get<string>("PeopleName"),
                    Strength = r.Get<decimal>("Strength")
                });
        }

        private void BuildCountriesList()
        {
            BuildDatasList(
                _countryDatas,
                new[] { "ID", "Name", "NameShort", "Name3", "ContinentID", "is_EU" },
                "country",
                r => new Country
                {
                    Code = r.Get<string>("Name3"),
                    Confederation = _confederationDatas.Find(_ => _.Id == r.GetNull<int>("ContinentID")),
                    Id = r.Get<int>("ID"),
                    IsEU = r.Get<byte>("is_EU") != 0,
                    LongName = r.Get<string>("Name"),
                    Name = r.Get<string>("NameShort")
                });
        }

        private void BuildClubsList()
        {
            BuildDatasList(
                _clubDatas,
                new[] { "ID", "LongName", "ShortName", "StadiumOwner", "PLC", "NationID",
                    "DivisionID", "DivisionPreviousID", "LastPosition", "DivisionReserveID",
                    "StadiumID", "StadiumReserveID", "MatchDay",
                    "AverageAttendance", "MinimumAttendance", "MaximumAttendance",
                    "Facilities", "Reputation", "Statut", "Bank",
                    "HomeShirtForegroundID", "HomeShirtBackgroundID",
                    "AwayShirtForegroundID", "AwayShirtBackgroundID",
                    "ThirdShirtForegroundID", "ThirdShirtBackgroundID",
                    "LikedStaffID1", "LikedStaffID2", "LikedStaffID3",
                    "DislikedStaffID1", "DislikedStaffID2", "DislikedStaffID3",
                    "RivalClubsID1", "RivalClubsID2", "RivalClubsID3" },
                "club",
                r => new Club
                {
                    Country = _countryDatas.Find(_ => _.Id == r.GetNull<int>("NationID")),
                    PublicLimitedCompany = r.Get<byte>("PLC") != 0,
                    AverageAttendance = r.GetNull<int>("AverageAttendance"),
                    AwayShirtBackgroundId = r.GetNull<int>("AwayShirtBackgroundID"),
                    AwayShirtForegroundId = r.GetNull<int>("AwayShirtForegroundID"),
                    Bank = r.GetNull<int>("Bank"),
                    DivisionId = r.GetNull<int>("DivisionID"),
                    Facilities = r.GetNull<int>("Facilities"),
                    HomeShirtBackgroundId = r.GetNull<int>("HomeShirtBackgroundID"),
                    HomeShirtForegroundId = r.GetNull<int>("HomeShirtForegroundID"),
                    Id = r.Get<int>("ID"),
                    LastPosition = r.GetNull<int>("LastPosition"),
                    MatchDay = r.GetNull<int>("MatchDay").ToEnum<System.DayOfWeek>(),
                    MaximumAttendance = r.GetNull<int>("MaximumAttendance"),
                    MinimumAttendance = r.GetNull<int>("MinimumAttendance"),
                    LongName = r.Get<string>("LongName"),
                    PreviousDivisionId = r.GetNull<int>("DivisionPreviousID"),
                    Reputation = r.GetNull<int>("Reputation"),
                    ReserveDivisionId = r.GetNull<int>("DivisionReserveID"),
                    ReserveStadiumId = r.GetNull<int>("StadiumReserveID"),
                    Name = r.Get<string>("ShortName"),
                    StadiumId = r.GetNull<int>("StadiumID"),
                    StadiumOwner = r.Get<byte>("StadiumOwner") != 0,
                    Statut = (ClubStatut)r.Get<int>("Statut"),
                    ThirdShirtBackgroundId = r.GetNull<int>("ThirdShirtBackgroundID"),
                    ThirdShirtForegroundId = r.GetNull<int>("ThirdShirtForegroundID"),
                    DislikedStaffIds = r.GetIdList("DislikedStaffID{0}"),
                    RivalClubIds = r.GetIdList("RivalClubsID{0}"),
                    LikedStaffIds = r.GetIdList("LikedStaffID{0}")
                });
        }

        private void BuildDatasList<T>(
            List<T> targetList,
            string[] columns,
            string table,
            System.Func<IDataReader, T> transformFunc)
        {
            targetList.Clear();
            targetList.AddRange(_mySqlService.GetDatas(
                $"SELECT {string.Join(", ", columns)} FROM {table}",
                transformFunc));
        }

        private Dictionary<T, int?> GetRates<T>(
            string prefix,
            int playerId,
            System.Func<int, T> findFunc)
        {
            return _mySqlService
                .GetDatas(
                    $"SELECT {prefix}_ID, rate FROM player_{prefix} WHERE player_ID = {playerId}",
                    r => new KeyValuePair<T, int?>(
                        findFunc(r.Get<int>($"{prefix}_ID")),
                        r.GetNull<int>("rate")))
                .ToDictionary(_ => _.Key, _ => _.Value);
        }
    }
}
