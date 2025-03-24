using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExplorerFM.Datas;
using ExplorerFM.Extensions;
using ExplorerFM.RuleEngine;

namespace ExplorerFM.Providers
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

        private readonly IProvider _provider;

        private Dictionary<int, Club> _clubDatas;
        private Dictionary<int, Confederation> _confederationDatas;
        private Dictionary<int, Country> _countryDatas;
        private Dictionary<int, Competition> _competitionDatas;

        public IReadOnlyList<Club> Clubs => _clubDatas.Values.ToList();
        public IReadOnlyList<Confederation> Confederations => _confederationDatas.Values.ToList();
        public IReadOnlyList<Country> Countries => _countryDatas.Values.ToList();
        public IReadOnlyList<Competition> Competitions => _competitionDatas.Values.ToList();
        public IReadOnlyList<Attribute> Attributes => Attribute.PlayerInstances;

        public int MaxTheoreticalRate => 20 * Attribute.PlayerInstances.Count;

        public DataProvider()
        {
            _provider = Properties.Settings.Default.DataProvider == nameof(MongoProvider)
                ? (IProvider)new MongoProvider()
                : new MySqlProvider();

            _clubDatas = new Dictionary<int, Club>();
            _confederationDatas = new Dictionary<int, Confederation>();
            _countryDatas = new Dictionary<int, Country>();
            _competitionDatas = new Dictionary<int, Competition>();
        }

        public void Initialize()
        {
            _confederationDatas = _provider.GetConfederations().ToDictionary(x => x.Id);
            _countryDatas = _provider.GetCountries(_confederationDatas).ToDictionary(x => x.Id);
            _competitionDatas = _provider.GetCompetitions(_countryDatas).ToDictionary(x => x.Id);
            _clubDatas = _provider.GetClubs(_countryDatas, _competitionDatas).ToDictionary(x => x.Id);
        }

        public IReadOnlyList<Player> GetPlayersByClub(int? clubId, bool potentialEnabled)
        {
            // TODO: view should decide the order
            return _provider.GetPlayersByClub(clubId, _clubDatas, _countryDatas, potentialEnabled).OrderBy(p => p.Fullname).ToList();
        }

        public IReadOnlyList<Player> GetPlayersByCountry(int? countryId, bool selectionEligible, bool potentialEnabled)
        {
            // TODO: view should decide the order
            return _provider.GetPlayersByCountry(countryId, selectionEligible, _clubDatas, _countryDatas, potentialEnabled).OrderBy(p => p.Fullname).ToList();
        }

        public IReadOnlyList<Player> GetPlayersByCriteria(CriteriaSet criteria, bool potentialEnabled)
        {
            return _provider.GetPlayersByCriteria(criteria, _clubDatas, _countryDatas, potentialEnabled);
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
