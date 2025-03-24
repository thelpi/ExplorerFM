using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ExplorerFM.Datas;
using ExplorerFM.Extensions;
using ExplorerFM.Properties;
using ExplorerFM.RuleEngine;
using MySql.Data.MySqlClient;

namespace ExplorerFM.Providers
{
    internal class MySqlProvider : IProvider
    {
        private readonly Func<MySqlConnection> _getConnection =
            () => new MySqlConnection(Settings.Default.MySqlConnectionString);

        private readonly IReadOnlyDictionary<int, string> _attributesMapper = new Dictionary<int, string>
        {
            { 01, "acceleration" },
            { 02, "agility" },
            { 03, "balance" },
            { 04, "injury_proneness" },
            { 05, "jumping" },
            { 06, "natural_fitness" },
            { 07, "pace" },
            { 08, "stamina" },
            { 09, "strength" },
            { 10, "handling" },
            { 11, "one_on_ones" },
            { 12, "reflexes" },
            { 13, "corners" },
            { 14, "set_pieces" },
            { 15, "throw_ins" },
            { 16, "crossing" },
            { 17, "dribbling" },
            { 18, "finishing" },
            { 19, "heading" },
            { 20, "long_shots" },
            { 21, "marking" },
            { 22, "off_the_ball" },
            { 23, "passing" },
            { 24, "penalties" },
            { 25, "positioning" },
            { 26, "tackling" },
            { 27, "technique" },
            { 28, "crossing" },
            { 29, "adaptability" },
            { 30, "aggression" },
            { 31, "ambition" },
            { 32, "anticipation" },
            { 33, "bravery" },
            { 34, "consistency" },
            { 35, "decisions" },
            { 36, "determination" },
            { 37, "dirtiness" },
            { 38, "flair" },
            { 39, "important_matches" },
            { 40, "influence" },
            { 41, "loyalty" },
            { 42, "pressure" },
            { 43, "professionalism" },
            { 44, "sportsmanship" },
            { 45, "teamwork" },
            { 46, "temperament" },
            { 47, "versatility" },
            { 48, "work_rate" },
        };

        internal static string TestConnection()
        {
            string error = null;

            MySqlConnection connection = null;
            MySqlCommand command = null;
            try
            {
                connection = new MySqlConnection(Settings.Default.MySqlConnectionString);
                connection.Open();
                command = connection.CreateCommand();
                command.CommandText = "SELECT 1 FROM players LIMIT 0, 1";
                command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                command?.Dispose();
                connection?.Dispose();
            }

            return error;
        }

        public IReadOnlyList<Confederation> GetConfederations()
        {
            return GetData(
                "SELECT * FROM confederations",
                reader => new Confederation
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("continent_name"),
                    FedCode = reader.GetString("acronym"),
                    FedName = reader.GetString("name"),
                    Strength = reader.GetInt32("strength")
                });
        }

        public IReadOnlyList<Country> GetCountries(IReadOnlyDictionary<int, Confederation> confederations)
        {
            return GetData(
                "SELECT * FROM nations",
                reader => new Country
                {
                    Confederation = reader.IsDBNull("confederation_id")
                        ? null
                        : confederations[reader.GetInt32("confederation_id")],
                    Id = reader.GetInt32("id"),
                    IsEU = reader.GetBoolean("is_eu"),
                    Name = reader.GetString("name"),
                    Code = reader.GetString("acronym"),
                    LeagueStandard = reader.GetInt32("league_standard"),
                    Reputation = reader.GetInt32("reputation")
                });
        }

        public IReadOnlyList<Competition> GetCompetitions(Dictionary<int, Country> countryDatas)
        {
            return GetData(
                "SELECT * FROM club_competitions",
                reader => new Competition
                {
                    Acronym = reader.GetString("acronym"),
                    Country = reader.IsDBNull("nation_id")
                        ? null
                        : countryDatas[reader.GetInt32("nation_id")],
                    Id = reader.GetInt32("id"),
                    LongName = reader.GetString("long_name"),
                    Name = reader.GetString("name"),
                    Reputation = reader.GetInt32("reputation")
                });
        }

        public IReadOnlyList<Club> GetClubs(IReadOnlyDictionary<int, Country> countries,
            IReadOnlyDictionary<int, Competition> competitions)
        {
            return GetData(
                "SELECT * FROM clubs",
                reader => new Club
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    LongName = reader.GetString("long_name"),
                    Country = reader.IsDBNull("nation_id")
                        ? null
                        : countries[reader.GetInt32("nation_id")],
                    Reputation = reader.GetInt32("reputation"),
                    Division = reader.IsDBNull("division_id")
                        ? null
                        : competitions[reader.GetInt32("division_id")],
                    Bank = reader.GetInt32("bank"),
                    Facilities = reader.GetInt32("facilities")
                    // todo : preferences
                });
        }

        public IReadOnlyList<Player> GetPlayersByClub(int? clubId,
            IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries,
            bool potentialEnabled)
        {
            return GetData(
                "SELECT * FROM players " +
                "WHERE club_id = @club_id " +
                "OR (club_id IS NULL AND @club_id IS NULL)",
                reader => ExtractPlayer(reader, countries, clubs, potentialEnabled),
                ("@club_id", DbType.Int32, clubId));
        }

        public IReadOnlyList<Player> GetPlayersByCountry(int? countryId,
            bool selectionEligible, IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries,
            bool potentialEnabled)
        {
            return GetData(
                "SELECT * FROM players " +
                "WHERE nation_id = @nation_id " +
                "OR (nation_id IS NULL AND @nation_id IS NULL) " +
                "OR secondary_nation_id = @nation_id",
                reader => ExtractPlayer(reader, countries, clubs, potentialEnabled),
                ("@nation_id", DbType.Int32, countryId));
        }

        public IReadOnlyList<Player> GetPlayersByCriteria(CriteriaSet criteria,
            IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries,
            bool potentialEnabled)
        {
            return GetData(
                $"SELECT * FROM players WHERE {TransformCriteriaSet(criteria)}",
                reader => ExtractPlayer(reader, countries, clubs, potentialEnabled));
        }

        private string TransformCriteriaSet(CriteriaSet criteriaSet)
        {
            var sqlBuilder = new StringBuilder();

            if (criteriaSet.Criteria?.Count > 0)
            {
                var first = true;
                foreach (var c in criteriaSet.Criteria)
                {
                    if (!first)
                        sqlBuilder.Append(criteriaSet.Or ? " OR " : " AND ");
                    first = false;
                    sqlBuilder.Append($"({(c is CriteriaSet set ? TransformCriteriaSet(set) : TransformCriterion(c as Criterion))})");
                }
            }

            var sql = sqlBuilder.ToString();

            return string.IsNullOrEmpty(sql) ? "(1=1)" : sql;
        }

        private string TransformCriterion(Criterion criterion)
        {
            var sqlBuilder = new StringBuilder();

            var value = criterion.FieldValue ?? DBNull.Value;
            if (criterion.FieldValue is DateTime)
            {
                value = $"'{Convert.ToDateTime(criterion.FieldValue):yyyy/MM/dd}'";
            }
            else if (value is bool valueBool)
            {
                value = $"{(valueBool ? 1 : 0)}";
            }
            else if (value != DBNull.Value && !value.IsNumeric())
            {
                value = criterion.Comparator == Comparator.Like || criterion.Comparator == Comparator.NotLike
                    ? $"%'{MySqlHelper.EscapeString(value.ToString())}%'"
                    : $"'{MySqlHelper.EscapeString(value.ToString())}'";
            }

            if (criterion.IncludeNullValue)
            {
                sqlBuilder.Append("(");
            }

            var field = _propertiesSqlMap[string.Join(".", criterion.PropertyMap)];

            switch (criterion.Comparator)
            {
                case Comparator.Equal:
                    sqlBuilder.Append(value == DBNull.Value
                        ? $"{field} IS NULL"
                        : $"{field} = {value}");
                    break;
                case Comparator.NotEqual:
                    sqlBuilder.Append(value == DBNull.Value
                        ? $"{field} IS NOT NULL"
                        : $"{field} != {value}");
                    break;
                case Comparator.LowerEqual:
                    sqlBuilder.Append($"{field} <= {value}");
                    break;
                case Comparator.Lower:
                    sqlBuilder.Append($"{field} < {value}");
                    break;
                case Comparator.GreaterEqual:
                    sqlBuilder.Append($"{field} >= {value}");
                    break;
                case Comparator.Greater:
                    sqlBuilder.Append($"{field} > {value}");
                    break;
                case Comparator.Like:
                    sqlBuilder.Append($"{field} LIKE {value}");
                    break;
                case Comparator.NotLike:
                    sqlBuilder.Append($"{field} NOT LIKE {value}");
                    break;
            }

            if (criterion.IncludeNullValue)
            {
                sqlBuilder.Append($" OR {field} IS NULL)");
            }

            return sqlBuilder.ToString();
        }

        private static readonly Dictionary<string, string> _propertiesSqlMap =
            new Dictionary<string, string>
            {
                { nameof(BaseData.Id), "id" },
                { $"{nameof(Player.Positions)}.{nameof(Position.GoalKeeper)}", "pos_goalkeeper" },
                { $"{nameof(Player.Positions)}.{nameof(Position.Sweeper)}", "pos_sweeper" },
                { $"{nameof(Player.Positions)}.{nameof(Position.Defender)}", "pos_defender" },
                { $"{nameof(Player.Positions)}.{nameof(Position.DefensiveMidfielder)}", "pos_defensive_midfielder" },
                { $"{nameof(Player.Positions)}.{nameof(Position.Midfielder)}", "pos_midfielder" },
                { $"{nameof(Player.Positions)}.{nameof(Position.OffensiveMidfielder)}", "pos_attacking_midfielder" },
                { $"{nameof(Player.Positions)}.{nameof(Position.Striker)}", "pos_forward" },
                { $"{nameof(Player.Positions)}.{nameof(Position.WingBack)}", "pos_wingback" },
                { $"{nameof(Player.Positions)}.{nameof(Position.FreeRole)}", "pos_free_role" },
                { $"{nameof(Player.Sides)}.{nameof(Side.Right)}", "side_right" },
                { $"{nameof(Player.Sides)}.{nameof(Side.Left)}", "side_left" },
                { $"{nameof(Player.Sides)}.{nameof(Side.Center)}", "side_center" },
                { nameof(Staff.Value), "value" },
                { $"{nameof(Staff.Nationality)}.{nameof(Country.IsEU)}", "(SELECT is_eu FROM nations AS c WHERE c.id = nation_id)" },
                { $"{nameof(Staff.SecondNationality)}.{nameof(Country.IsEU)}", "IFNULL((SELECT is_eu FROM nations AS c WHERE c.id = secondary_nation_id), 0)" },
                { nameof(Staff.ClubContract), "club_id" },
                { nameof(Staff.SecondNationality), "secondary_nation_id" },
                { nameof(Staff.Nationality), "nation_id" },
                { nameof(Staff.YearOfBirth), "YEAR(date_of_birth)" },
                { nameof(Staff.DateOfBirth), "date_of_birth" },
                { nameof(Staff.CurrentReputation), "current_reputation" }
            };

        private List<T> GetData<T>(
            string sqlQuery,
            Func<MySqlDataReader, T> builder,
            params (string parameterName, DbType dbType, object value)[] parameters)
        {
            var data = new List<T>();
            using (var connection = _getConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    if (parameters?.Length > 0)
                    {
                        foreach (var (parameterName, dbType, value) in parameters)
                        {
                            command.SetParameter(parameterName, dbType, value);
                        }
                    }
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(builder(reader));
                        }
                    }
                }
            }

            return data;
        }

        private Player ExtractPlayer(MySqlDataReader reader,
            IReadOnlyDictionary<int, Country> countries,
            IReadOnlyDictionary<int, Club> clubs,
            bool potentialEnabled)
        {
            return new Player
            {
                DateContractEnd = reader.IsDBNull("contract_expiration")
                    ? null
                    : (DateTime?)reader.GetDateTime("contract_expiration"),
                Caps = reader.GetInt32("caps"),
                ClubContract = reader.IsDBNull("club_id")
                    ? null
                    : clubs[reader.GetInt32("club_id")],
                Commonname = reader.IsDBNull("common_name")
                    ? null
                    : reader.GetString("common_name"),
                CurrentAbility = reader.GetInt32("ability"),
                CurrentReputation = reader.GetInt32("current_reputation"),
                Attributes = Datas.Attribute.PlayerInstances
                    .ToDictionary(x => x, x =>
                    {
                        var columnName = _attributesMapper[x.Id];
                        if (potentialEnabled && reader.Exists($"{columnName}_potential"))
                        {
                            columnName = $"{columnName}_potential";
                        }

                        return (int?)reader.GetInt32(columnName);
                    }),
                DateOfBirth = reader.GetDateTime("date_of_birth"),
                Firstname = reader.IsDBNull("first_name")
                    ? null
                    : reader.GetString("first_name"),
                HomeReputation = reader.GetInt32("home_reputation"),
                Id = reader.GetInt32("id"),
                IntGoals = reader.GetInt32("international_goals"),
                Lastname = reader.IsDBNull("last_name")
                    ? null
                    : reader.GetString("last_name"),
                LeftFoot = reader.GetInt32("left_foot"),
                Loaded = true,
                Nationality = countries[reader.GetInt32("nation_id")],
                Positions = new Dictionary<Position, int?>
                {
                    { Position.GoalKeeper, reader.GetInt32("pos_goalkeeper") },
                    { Position.Defender, reader.GetInt32("pos_defender") },
                    { Position.DefensiveMidfielder, reader.GetInt32("pos_defensive_midfielder") },
                    { Position.Sweeper, reader.GetInt32("pos_sweeper") },
                    { Position.Midfielder, reader.GetInt32("pos_midfielder") },
                    { Position.OffensiveMidfielder, reader.GetInt32("pos_attacking_midfielder") },
                    { Position.Striker, reader.GetInt32("pos_forward") },
                    { Position.FreeRole, reader.GetInt32("pos_free_role") },
                    { Position.WingBack, reader.GetInt32("pos_wingback") }
                },
                PotentialAbility = reader.GetInt32("potential_ability"),
                RightFoot = reader.GetInt32("right_foot"),
                SecondNationality = reader.IsDBNull("secondary_nation_id")
                    ? null
                    : countries[reader.GetInt32("secondary_nation_id")],
                Sides = new Dictionary<Side, int?>
                {
                    { Side.Center, reader.GetInt32("side_center") },
                    { Side.Left, reader.GetInt32("side_left") },
                    { Side.Right, reader.GetInt32("side_right") }
                },
                Value = reader.GetInt32("value"),
                Wage = reader.GetInt32("wage"),
                WorldReputation = reader.GetInt32("world_reputation"),
                YearOfBirth = reader.GetDateTime("date_of_birth").Year
            };
        }
    }
}
