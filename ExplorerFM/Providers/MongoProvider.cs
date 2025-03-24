using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ExplorerFM.Datas;
using ExplorerFM.Datas.Dtos;
using ExplorerFM.Datas.Mappers;
using ExplorerFM.Properties;
using ExplorerFM.RuleEngine;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ExplorerFM.Providers
{
    internal class MongoProvider : IProvider
    {
        private readonly IMongoCollection<StaffDto> _staffCollection;
        private readonly IMongoCollection<ConfederationDto> _confederationsCollection;
        private readonly IMongoCollection<CountryDto> _countriesCollection;
        private readonly IMongoCollection<ClubDto> _clubsCollection;

        private const string _staffCollectionName = "staff";
        private const string _confederationsCollectionName = "confederations";
        private const string _countriesCollectionName = "countries";
        private const string _clubsCollectionName = "clubs";

        internal static string TestConnection()
        {
            string error = null;

            try
            {
                new MongoClient(Settings.Default.MongoConnectionString)
                    .GetDatabase(Settings.Default.MongoDatabase)
                    .GetCollection<StaffDto>(_staffCollectionName)
                    .Find(Builders<StaffDto>.Filter.Eq(x => x.ID, 0));
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return error;
        }

        public MongoProvider()
        {
            var db = new MongoClient(Settings.Default.MongoConnectionString)
                .GetDatabase(Settings.Default.MongoDatabase);

            _staffCollection = db.GetCollection<StaffDto>(_staffCollectionName);
            _confederationsCollection = db.GetCollection<ConfederationDto>(_confederationsCollectionName);
            _countriesCollection = db.GetCollection<CountryDto>(_countriesCollectionName);
            _clubsCollection = db.GetCollection<ClubDto>(_clubsCollectionName);
        }

        public IReadOnlyList<Club> GetClubs(IReadOnlyDictionary<int, Country> countries,
            IReadOnlyDictionary<int, Competition> competitions)
        {
            return _clubsCollection
                .Find(Builders<ClubDto>.Filter.Empty)
                .ToEnumerable()
                .Select(dto => dto.ToClub(countries))
                .ToList();
        }

        public IReadOnlyList<Country> GetCountries(IReadOnlyDictionary<int, Confederation> confederations)
        {
            return _countriesCollection
                .Find(Builders<CountryDto>.Filter.Empty)
                .ToEnumerable()
                .Select(dto => dto.ToCountry(confederations))
                .ToList();
        }

        public IReadOnlyList<Confederation> GetConfederations()
        {
            return _confederationsCollection
                .Find(Builders<ConfederationDto>.Filter.Empty)
                .ToEnumerable()
                .Select(dto => dto.ToConfederation())
                .ToList();
        }

        public IReadOnlyList<Competition> GetCompetitions(Dictionary<int, Country> countryDatas)
            => new List<Competition>();

        public IReadOnlyList<Player> GetPlayersByCriteria(CriteriaSet criteria,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries,
            bool potentialEnabled)
        {
            var filter = TransformCriteriaSet(criteria);

            return GetPlayersByFilter(filter, clubs, countries);
        }

        public IReadOnlyList<Player> GetPlayersByClub(int? clubId,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries,
            bool potentialEnabled)
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
            bool selectionEligible,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries,
            bool potentialEnabled)
        {
            var filter = FilterDefinition<StaffDto>.Empty;

            if (countryId.HasValue)
            {
                if (!selectionEligible)
                {
                    filter &= Builders<StaffDto>.Filter.Or(
                        Builders<StaffDto>.Filter.Eq(x => x.Nation1.Id, countryId.Value),
                        Builders<StaffDto>.Filter.Eq(x => x.Nation2.Id, countryId.Value));
                }
                else
                {
                    filter &= Builders<StaffDto>.Filter.Or(
                        Builders<StaffDto>.Filter.Eq(x => x.Nation1.Id, countryId.Value),
                        Builders<StaffDto>.Filter.And(
                            Builders<StaffDto>.Filter.Eq(x => x.Nation2.Id, countryId.Value),
                            Builders<StaffDto>.Filter.Eq(x => x.Caps, 0)));
                }
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

            return _staffCollection
                .Find(filter)
                .ToEnumerable()
                .Select(dto => dto.ToPlayer(clubs, countries))
                .ToList();
        }

        private FilterDefinition<StaffDto> TransformCriterion(Criterion criterion)
        {
            var filter = Builders<StaffDto>.Filter.Empty;
            switch (criterion.Comparator)
            {
                case Comparator.Equal:
                    filter &= Builders<StaffDto>.Filter.Eq(ComputeMongoFieldName(criterion), criterion.FieldValue);
                    break;
                case Comparator.NotEqual:
                    filter &= Builders<StaffDto>.Filter.Not(Builders<StaffDto>.Filter.Eq(ComputeMongoFieldName(criterion), criterion.FieldValue));
                    break;
                case Comparator.GreaterEqual:
                    filter &= Builders<StaffDto>.Filter.Gte(ComputeMongoFieldName(criterion), criterion.FieldValue);
                    break;
                case Comparator.Greater:
                    filter &= Builders<StaffDto>.Filter.Gt(ComputeMongoFieldName(criterion), criterion.FieldValue);
                    break;
                case Comparator.LowerEqual:
                    filter &= Builders<StaffDto>.Filter.Lte(ComputeMongoFieldName(criterion), criterion.FieldValue);
                    break;
                case Comparator.Lower:
                    filter &= Builders<StaffDto>.Filter.Lt(ComputeMongoFieldName(criterion), criterion.FieldValue);
                    break;
                case Comparator.Like:
                    filter &= Builders<StaffDto>.Filter.Regex(ComputeMongoFieldName(criterion), new BsonRegularExpression(new Regex(criterion.FieldValue.ToString(), RegexOptions.IgnoreCase)));
                    break;
                case Comparator.NotLike:
                    filter &= Builders<StaffDto>.Filter.Not(Builders<StaffDto>.Filter.Regex(ComputeMongoFieldName(criterion), new BsonRegularExpression(new Regex(criterion.FieldValue.ToString(), RegexOptions.IgnoreCase))));
                    break;
            }

            if (criterion.IncludeNullValue)
                filter |= Builders<StaffDto>.Filter.Not(Builders<StaffDto>.Filter.Exists(ComputeMongoFieldName(criterion)));

            return filter;
        }

        private FilterDefinition<StaffDto> TransformCriteriaSet(CriteriaSet criteriaSet)
        {
            var filter = FilterDefinition<StaffDto>.Empty;

            if (criteriaSet.Criteria?.Count > 0)
            {
                if (criteriaSet.Or)
                {
                    filter &= Builders<StaffDto>.Filter.Or(
                        criteriaSet.Criteria
                            .Select(x =>
                                x is CriteriaSet set ? TransformCriteriaSet(set) : TransformCriterion(x as Criterion))
                            .ToArray());
                }
                else
                {
                    filter &= Builders<StaffDto>.Filter.And(
                        criteriaSet.Criteria
                            .Select(x =>
                                x is CriteriaSet set ? TransformCriteriaSet(set) : TransformCriterion(x as Criterion))
                            .ToArray());
                }
            }

            return filter;
        }

        private string ComputeMongoFieldName(Criterion criterion)
        {
            var fieldNameParts = new string[criterion.PropertyMap.Length];

            var currentType = criterion.TargetType;
            for (var i = 0; i < criterion.PropertyMap.Length; i++)
            {
                MemberInfo member;
                if (currentType.IsEnum)
                {
                    member = currentType.GetMember(criterion.PropertyMap[i])[0];
                }
                else
                {
                    var prop = currentType.GetProperty(criterion.PropertyMap[i]);
                    currentType = prop.PropertyType;
                    member = prop;
                }
                var (mongoName, mongoForcedType) = _propertiesMongoMap[member.Name];
                fieldNameParts[i] = mongoName;
                currentType = mongoForcedType ?? currentType;
            }

            return string.Join(".", fieldNameParts);
        }

        private static readonly Dictionary<string, (string, Type)> _propertiesMongoMap =
            new Dictionary<string, (string, Type)>
            {
                { nameof(BaseData.Id), ("_id", null) },
                { nameof(Country.IsEU), ("isEU", null) },
                { nameof(Player.Sides), ("playerSides", typeof(Side)) },
                { nameof(Player.Positions), ("playerPositions", typeof(Position)) },
                { nameof(Position.FreeRole), ("freeRole", null) },
                { nameof(Position.WingBack), ("wingBack", null) },
                { nameof(Position.OffensiveMidfielder), ("offMidfielder", null) },
                { nameof(Position.Midfielder), ("midfielder", null) },
                { nameof(Position.DefensiveMidfielder), ("defMidfielder", null) },
                { nameof(Position.DefensiveMidfielder), ("defender", null) },
                { nameof(Position.Sweeper), ("sweeper", null) },
                { nameof(Position.GoalKeeper), ("goalKeeper", null) },
                { nameof(Position.Striker), ("forward", null) },
                { nameof(Side.Right), ("right", null) },
                { nameof(Side.Left), ("left", null) },
                { nameof(Side.Center), ("center", null) },
                { nameof(Staff.Value), ("value", null) },
                { nameof(Staff.ClubContract), ("club", null) },
                { nameof(Staff.SecondNationality), ("country2", null) },
                { nameof(Staff.Nationality), ("country1", null) },
                { nameof(Staff.YearOfBirth), ("yearOfBirth", null) },
                { nameof(Staff.DateOfBirth), ("dateOfBirth", null) },
                { nameof(Staff.CurrentReputation), ("playerFeatures.currentReputation", null) }
            };
    }
}
