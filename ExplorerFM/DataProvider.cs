using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        private readonly MySqlService _mySqlService;

        private readonly Dictionary<int, Attribute> _attributeDatas;
        private readonly Dictionary<int, Club> _clubDatas;
        private readonly Dictionary<int, Confederation> _confederationDatas;
        private readonly Dictionary<int, Country> _countryDatas;
        private readonly Dictionary<int, Player> _playerDatas;

        public IEnumerable<Attribute> Attributes => _attributeDatas.Values;
        public IEnumerable<Club> Clubs => _clubDatas.Values;
        public IEnumerable<Confederation> Confederations => _confederationDatas.Values;
        public IEnumerable<Country> Countries => _countryDatas.Values;

        public int MaxTheoreticalRate => 16 * _attributeDatas.Count;

        public DataProvider(string connectionString)
        {
            _mySqlService = new MySqlService(connectionString);
            _attributeDatas = new Dictionary<int, Attribute>();
            _clubDatas = new Dictionary<int, Club>();
            _confederationDatas = new Dictionary<int, Confederation>();
            _countryDatas = new Dictionary<int, Country>();
            _playerDatas = new Dictionary<int, Player>();
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
            return GetPlayers($"SELECT * FROM player WHERE ClubContractID {(clubId.HasValue ? $"= {clubId.Value}" : "IS NULL")}");
        }

        public List<Player> GetPlayersByCountry(int? countryId)
        {
            var sql = $"SELECT * FROM player WHERE NationID1 = {countryId} OR (Caps = 0 AND NationID2 = {countryId})";
            if (!countryId.HasValue)
                sql = "SELECT * FROM player WHERE NationID1 IS NULL AND NationID2 IS NULL";

            return GetPlayers(sql);
        }

        public List<Player> GetPlayersByCriteria(CriteriaSet criteria, System.Action<double> reportFunc)
        {
            var count = _mySqlService.GetData(
                $"SELECT COUNT(*) AS players_count FROM player WHERE {criteria}",
                r => r.Get<int>("players_count"));

            return GetPlayers($"SELECT * FROM player WHERE {criteria}",
                i => reportFunc?.Invoke(i / (double)count));
        }

        private List<Player> GetPlayers(string sql, System.Action<int> reportFunc = null)
        {
            var players = _mySqlService.GetDatas(
                sql,
                GetPlayerFromDataReader,
                reportFunc);

            while (players.Any(p => !p.Loaded))
                System.Threading.Thread.Sleep(100);

            return players;
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
            var pId = r.Get<int>("ID");
            if (!_playerDatas.ContainsKey(pId))
            {
                var ability = r.GetNull<int>("CurrentAbility");
                var potential = r.GetNull<int>("PotentialAbility");
                if ((!potential.HasValue || (potential >= 0 && potential < 100)) && (ability < 100 || !ability.HasValue))
                    return null;

                var p = new Player
                {
                    Caps = r.Get<int>("Caps"),
                    Commonname = r.Get<string>("Commonname"),
                    CurrentAbility = ability,
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
                    Id = pId,
                    IntGoals = r.Get<int>("IntGoals"),
                    Lastname = r.Get<string>("Lastname"),
                    LeftFoot = r.GetNull<int>("LeftFoot"),
                    PotentialAbility = potential,
                    RightFoot = r.GetNull<int>("RightFoot"),
                    SquadNumber = r.GetNull<int>("SquadNumber"),
                    Value = r.GetNull<int>("Value"),
                    Wage = r.GetNull<int>("Wage"),
                    WorldReputation = r.GetNull<int>("WorldReputation"),
                    YearOfBirth = r.GetNull<int>("YearOfBirth"),
                    ClubContract = GetClub(r.GetNull<int>("ClubContractID")),
                    Nationality = GetCountry(r.GetNull<int>("NationID1")),
                    SecondNationality = GetCountry(r.GetNull<int>("NationID2"))
                };

                Task.Run(() =>
                {
                    p.Sides = GetRates("side", pId, _ => (Side)_);
                    p.Positions = GetRates("position", pId, _ => (Position)_);
                    p.Attributes = GetRates("attribute", pId, _ => _attributeDatas[_]);
                    p.Loaded = true;
                });

                _playerDatas.Add(pId, p);
            }

            return _playerDatas[pId];
        }

        private Club GetClub(int? clubId)
        {
            return clubId.HasValue && _clubDatas.ContainsKey(clubId.Value)
                ? _clubDatas[clubId.Value]
                : null;
        }

        private Country GetCountry(int? countryId)
        {
            return countryId.HasValue && _countryDatas.ContainsKey(countryId.Value)
                ? _countryDatas[countryId.Value]
                : null;
        }

        private Confederation GetConfederation(int? confederationId)
        {
            return confederationId.HasValue && _confederationDatas.ContainsKey(confederationId.Value)
                ? _confederationDatas[confederationId.Value]
                : null;
        }

        private void BuildAttributesList()
        {
            BuildDataList(
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
            BuildDataList(
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
            BuildDataList(
                _countryDatas,
                new[] { "ID", "Name", "NameShort", "Name3", "ContinentID", "is_EU" },
                "country",
                r => new Country
                {
                    Code = r.Get<string>("Name3"),
                    Confederation = GetConfederation(r.GetNull<int>("ContinentID")),
                    Id = r.Get<int>("ID"),
                    IsEU = r.Get<byte>("is_EU") != 0,
                    LongName = r.Get<string>("Name"),
                    Name = r.Get<string>("NameShort")
                });
        }

        private void BuildClubsList()
        {
            BuildDataList(
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
                    Country = GetCountry(r.GetNull<int>("NationID")),
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

        private void BuildDataList<T>(
            Dictionary<int, T> targetList,
            string[] columns,
            string table,
            System.Func<IDataReader, T> transformFunc)
            where T : BaseData
        {
            targetList.Clear();
            var sourceData = _mySqlService.GetDatas(
                $"SELECT {string.Join(", ", columns)} FROM {table}",
                transformFunc);
            foreach (var data in sourceData)
                targetList.Add(data.Id, data);
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
