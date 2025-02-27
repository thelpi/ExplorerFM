using System;
using System.Collections.Generic;
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
            { 41, "loyality" },
            { 42, "pressure" },
            { 43, "professionalism" },
            { 44, "sportsmanship" },
            { 45, "teamwork" },
            { 46, "temperament" },
            { 47, "versatility" },
            { 48, "work_rate" },
        };

        private const int MaxRate = 20;
        private const int EnoughRate = 15;
        private const int MinRate = 1;

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
            var sqlQuery = "SELECT DISTINCT confederation AS name " +
                "FROM players " +
                "WHERE confederation IS NOT NULL " +
                "UNION " +
                "SELECT DISTINCT confederation_2 AS name " +
                "FROM players " +
                "WHERE confederation_2 IS NOT NULL";

            return GetDataList(
                    sqlQuery,
                    (reader, i) =>
                        new Confederation
                        {
                            Id = i + 1,
                            Name = reader.GetString("name")
                        })
                .OrderBy(x => x.Name)
                .ToList();
        }

        public IReadOnlyList<Country> GetCountries(IReadOnlyDictionary<int, Confederation> confederations)
        {
            var sqlQuery = "SELECT DISTINCT nation AS name, is_eu, confederation " +
                "FROM players " +
                "WHERE nation IS NOT NULL " +
                "UNION " +
                "SELECT DISTINCT nation_2 AS name, 0 AS is_eu, confederation_2 AS confederation " +
                "FROM players " +
                "WHERE nation_2 IS NOT NULL";

            return GetDataList(
                    sqlQuery,
                    (reader, i) =>
                        new Country
                        {
                            Id = i,
                            Name = reader.GetString("name"),
                            Confederation = confederations.Values.FirstOrDefault(x => x.Name == reader.GetString("confederation")),
                            IsEU = reader.GetBoolean("is_eu")
                        },
                    (country, reader) =>
                    {
                        if (reader.GetBoolean("is_eu"))
                        {
                            country.IsEU = true;
                        }
                    },
                    (dataList, reader) =>
                        dataList.FirstOrDefault(x => x.Name == reader.GetString("name")))
                .OrderBy(x => x.Name)
                .ToList();
        }

        public IReadOnlyList<Club> GetClubs(IReadOnlyDictionary<int, Country> countries)
        {
            var sqlQuery = "SELECT DISTINCT club, club_reputation " +
                "FROM players " +
                "WHERE club IS NOT NULL";

            return GetDataList(
                    sqlQuery,
                    (reader, i) =>
                        new Club
                        {
                            Id = i,
                            Name = reader.GetString("club"),
                            Reputation = reader.GetInt32("club_reputation")
                        })
                .OrderBy(x => x.Name)
                .ToList();
        }
        
        public IReadOnlyList<Player> GetPlayersByClub(int? clubId, IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries)
        {
            var sqlQuery = "SELECT * FROM players " +
                "WHERE club = @club OR (@club IS NULL AND club IS NULL)";

            return GetDataList(
                sqlQuery,
                (reader, _) => BuildPlayer(reader, clubs, countries),
                null,
                null,
                cmd =>
                {
                    var param = cmd.CreateParameter();
                    param.DbType = System.Data.DbType.String;
                    param.ParameterName = "@club";
                    param.Value = clubId.HasValue ? (object)clubs[clubId.Value].Name : DBNull.Value;
                    cmd.Parameters.Add(param);
                });
        }

        public IReadOnlyList<Player> GetPlayersByCountry(int? countryId, bool selectionEligible, IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries)
        {
            // TODO: binder sur la nouvelle conf
            if (!countryId.HasValue)
            {
                return new List<Player>();
            }

            var sqlQuery = "SELECT * FROM players " +
                "WHERE nation = @nation " +
                "OR (@selectionEligible = 0 AND nation_2 = @nation) " +
                "OR (caps = 0 AND nation_2 = @nation)";

            return GetDataList(
                sqlQuery,
                (reader, _) => BuildPlayer(reader, clubs, countries),
                null,
                null,
                cmd =>
                {
                    var param = cmd.CreateParameter();
                    param.DbType = System.Data.DbType.String;
                    param.ParameterName = "@nation";
                    param.Value = countries[countryId.Value].Name;
                    cmd.Parameters.Add(param);
                },
                cmd =>
                {
                    var param2 = cmd.CreateParameter();
                    param2.DbType = System.Data.DbType.Int32;
                    param2.ParameterName = "@selectionEligible";
                    param2.Value = selectionEligible ? 1 : 0;
                    cmd.Parameters.Add(param2);
                });
        }
        
        public IReadOnlyList<Player> GetPlayersByCriteria(CriteriaSet criteria, IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries) => throw new NotImplementedException();

        private List<T> GetDataList<T>(
            string sqlQuery,
            Func<MySqlDataReader, int, T> createInstance,
            Action<T, MySqlDataReader> updateInstance = null,
            Func<List<T>, MySqlDataReader, T> getInstance = null,
            params Action<MySqlCommand>[] parametersBuilder)
            where T : class
        {
            var dataList = new List<T>(100);

            using (var connection = _getConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sqlQuery;

                    if (parametersBuilder?.Length > 0)
                    {
                        foreach (var prm in parametersBuilder)
                        {
                            prm.Invoke(cmd);
                        }
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var dataMatch = getInstance?.Invoke(dataList, reader) ?? null;
                            if (dataMatch != null)
                            {
                                updateInstance?.Invoke(dataMatch, reader);
                            }
                            else
                            {
                                dataList.Add(createInstance(reader, dataList.Count));
                            }
                        }
                    }
                }
            }

            return dataList;
        }

        private Player BuildPlayer(
            MySqlDataReader reader,
            IReadOnlyDictionary<int, Club> clubs,
            IReadOnlyDictionary<int, Country> countries)
        {
            return new Player
            {
                DateContractEnd = reader.IsDBNull(reader.GetOrdinal("contract_expiration"))
                    ? default
                    : reader.GetDateTime("contract_expiration"),
                DateOfBirth = reader.GetDateTime("date_of_birth"),
                Attributes = Datas.Attribute.PlayerInstances
                    .ToDictionary(
                        x => x,
                        x => (int?)reader.GetInt32(_attributesMapper[x.Id])),
                Caps = reader.GetInt32("caps"),
                ClubContract = reader.IsDBNull(reader.GetOrdinal("club"))
                    ? default
                    : clubs.Values.FirstOrDefault(c => c.Name == reader.GetString("club")),
                Commonname = reader.GetString("name"),
                CurrentAbility = reader.GetInt32("ability"),
                CurrentReputation = reader.GetInt32("current_reputation"),
                HomeReputation = reader.GetInt32("home_reputation"),
                Id = reader.GetInt32("id"),
                IntGoals = reader.GetInt32("international_goals"),
                LeftFoot = reader.GetInt32("left_foot"),
                Loaded = true,
                Nationality = countries.Values.FirstOrDefault(c => c.Name == reader.GetString("nation")),
                Positions = Enum
                    .GetValues(typeof(Position))
                    .Cast<Position>()
                    .ToDictionary(
                        x => x,
                        x =>
                        {
                            switch (x)
                            {
                                case Position.Defender:
                                    return reader.GetBoolean("position_d") ? MaxRate : MinRate;
                                case Position.DefensiveMidfielder:
                                    return reader.GetBoolean("position_dm") ? MaxRate : MinRate;
                                case Position.GoalKeeper:
                                    return reader.GetBoolean("position_gk") ? MaxRate : MinRate;
                                case Position.Midfielder:
                                    return reader.GetBoolean("position_m") || reader.GetBoolean("position_am") || reader.GetBoolean("position_dm") ? MaxRate : MinRate;
                                case Position.OffensiveMidfielder:
                                    return reader.GetBoolean("position_am") ? MaxRate : (reader.GetBoolean("position_f") ? EnoughRate : MinRate);
                                case Position.Striker:
                                    return reader.GetBoolean("position_s") || reader.GetBoolean("position_f") ? MaxRate : MinRate;
                                case Position.Sweeper:
                                    return reader.GetBoolean("position_sw") ? MaxRate : MinRate;
                                default:
                                    return (int?)MinRate;
                            }
                        }),
                PotentialAbility = reader.GetInt32("potential_ability"),
                RightFoot = reader.GetInt32("right_foot"),
                SecondNationality = reader.IsDBNull(reader.GetOrdinal("nation_2"))
                    ? default
                    : countries.Values.FirstOrDefault(c => c.Name == reader.GetString("nation_2")),
                Sides = Enum
                    .GetValues(typeof(Side))
                    .Cast<Side>()
                    .ToDictionary(
                        x => x,
                        x => (int?)(x == Side.Right
                            ? (reader.GetBoolean("side_right") ? MaxRate : MinRate)
                            : (x == Side.Center
                                ? (reader.GetBoolean("side_center") ? MaxRate : MinRate)
                                : (reader.GetBoolean("side_left") ? MaxRate : MinRate)))),
                Value = reader.GetInt32("value"),
                Wage = reader.GetInt32("wage"),
                WorldReputation = reader.GetInt32("world_reputation"),
                YearOfBirth = reader.GetDateTime("date_of_birth").Year
            };
        }
    }
}
