using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExplorerFM.Datas;
using ExplorerFM.Extensions;
using ExplorerFM.RuleEngine;

namespace ExplorerFM
{
    public class DataProvider
    {
        public static readonly Side[] OrderedSides = new Side[]
        {
            Side.Left,
            Side.Center,
            Side.Right
        };

        // ignore wing back and free role
        public static readonly Position[] OrderedPositions = new Position[]
        {
            Position.Striker,
            Position.OffensiveMidfielder,
            Position.Midfielder,
            Position.DefensiveMidfielder,
            Position.Defender,
            Position.Sweeper,
            Position.GoalKeeper
        };

        private readonly MongoService _mongoService;

        private Dictionary<int, Club> _clubDatas;
        private Dictionary<int, Confederation> _confederationDatas;
        private Dictionary<int, Country> _countryDatas;

        public IReadOnlyList<Club> Clubs => _clubDatas.Values.ToList();
        public IReadOnlyList<Confederation> Confederations => _confederationDatas.Values.ToList();
        public IReadOnlyList<Country> Countries => _countryDatas.Values.ToList();
        public IReadOnlyList<Attribute> Attributes => Attribute.PlayerInstances;

        public int MaxTheoreticalRate => 20 * Attribute.PlayerInstances.Count;

        public DataProvider(string mongoConnectionString, string mongoDatabase)
        {
            _mongoService = new MongoService(mongoConnectionString, mongoDatabase);

            _clubDatas = new Dictionary<int, Club>();
            _confederationDatas = new Dictionary<int, Confederation>();
            _countryDatas = new Dictionary<int, Country>();
        }

        public void Initialize()
        {
            _confederationDatas = _mongoService.GetConfederations().ToDictionary(x => x.Id);
            _countryDatas = _mongoService.GetCountries(_confederationDatas).ToDictionary(x => x.Id);
            _clubDatas = _mongoService.GetClubs(_countryDatas).ToDictionary(x => x.Id);
        }

        public IReadOnlyList<Player> GetPlayersByClub(int? clubId)
        {
            return _mongoService.GetPlayersByClub(clubId, _clubDatas, _countryDatas);
        }

        public IReadOnlyList<Player> GetPlayersByCountry(int? countryId, bool selectionEligible)
        {
            return _mongoService.GetPlayersByCountry(countryId, selectionEligible, _clubDatas, _countryDatas);
        }

        public IReadOnlyList<Player> GetPlayersByCriteria(CriteriaSet criteria)
        {
            return _mongoService.GetPlayersByCriteria(criteria, _clubDatas, _countryDatas);
        }

        public static List<PropertyInfo> GetAllAttribute<T>() where T : System.Attribute
        {
            return typeof(Player).GetAttributeProperties<T>()
                .Concat(typeof(Club).GetAttributeProperties<T>())
                .Concat(typeof(Country).GetAttributeProperties<T>())
                .Concat(typeof(Confederation).GetAttributeProperties<T>())
                .ToList();
        }
    }
}
