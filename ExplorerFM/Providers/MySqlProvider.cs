using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ExplorerFM.Datas;
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
                    FedName = reader.GetString("name")
                });
        }

        public IReadOnlyList<Country> GetCountries(IReadOnlyDictionary<int, Confederation> confederations)
        {
            return GetData(
                "SELECT * FROM countries",
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
                "SELECT * FROM competitions",
                reader => new Competition
                {
                    Acronym = reader.GetString("acronym"),
                    Country = reader.IsDBNull("country_id")
                        ? null
                        : countryDatas[reader.GetInt32("country_id")],
                    Id = reader.GetInt32("id"),
                    LongName = reader.GetString("long_name"),
                    Name = reader.GetString("name")
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
                    Country = reader.IsDBNull("country_id")
                        ? null
                        : countries[reader.GetInt32("country_id")],
                    Reputation = reader.GetInt32("reputation"),
                    Division = reader.IsDBNull("division_id")
                        ? null
                        : competitions[reader.GetInt32("division_id")]
                });
        }

        public IReadOnlyList<Player> GetPlayersByClub(int? clubId, IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries)
        {
            return GetData(
                "SELECT * FROM players " +
                "WHERE club_id = @club_id " +
                "OR (club_id IS NULL AND @club_id IS NULL)",
                reader => ExtractPlayer(reader, countries, clubs),
                ("@club_id", DbType.Int32, clubId));
        }

        public IReadOnlyList<Player> GetPlayersByCountry(int? countryId, bool selectionEligible, IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries)
        {
            return GetData(
                "SELECT * FROM players " +
                "WHERE country_id = @country_id " +
                "OR (country_id IS NULL AND @country_id IS NULL) " +
                "OR secondary_country_id = @country_id",
                reader => ExtractPlayer(reader, countries, clubs),
                ("@country_id", DbType.Int32, countryId));
        }

        public IReadOnlyList<Player> GetPlayersByCriteria(CriteriaSet criteria, IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries)
            => throw new NotImplementedException();

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
            IReadOnlyDictionary<int, Club> clubs)
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
                    .ToDictionary(x => x, x => (int?)reader.GetInt32(_attributesMapper[x.Id])),
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
                Nationality = countries[reader.GetInt32("country_id")],
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
                SecondNationality = reader.IsDBNull("secondary_country_id")
                    ? null
                    : countries[reader.GetInt32("secondary_country_id")],
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
