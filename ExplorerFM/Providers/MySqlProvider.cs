using System;
using System.Collections.Generic;
using ExplorerFM.Datas;
using ExplorerFM.RuleEngine;
using MySql.Data.MySqlClient;

namespace ExplorerFM.Providers
{
    internal class MySqlProvider : IProvider
    {
        private readonly Func<MySqlConnection> _getConnection;

        internal static string TestConnection(string connectionString)
        {
            string error = null;

            MySqlConnection connection = null;
            MySqlCommand command = null;
            try
            {
                connection = new MySqlConnection(connectionString);
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

        public MySqlProvider(string connectionString)
        {
            _getConnection = () =>
            {
                var connection = new MySqlConnection(connectionString);
                connection.Open();
                return connection;
            };
        }

        public IReadOnlyList<Club> GetClubs(IReadOnlyDictionary<int, Country> countries) => throw new NotImplementedException();
        public IReadOnlyList<Confederation> GetConfederations() => throw new NotImplementedException();
        public IReadOnlyList<Country> GetCountries(IReadOnlyDictionary<int, Confederation> confederations) => throw new NotImplementedException();
        public IReadOnlyList<Player> GetPlayersByClub(int? clubId, IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries) => throw new NotImplementedException();
        public IReadOnlyList<Player> GetPlayersByCountry(int? countryId, bool selectionEligible, IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries) => throw new NotImplementedException();
        public IReadOnlyList<Player> GetPlayersByCriteria(CriteriaSet criteria, IReadOnlyDictionary<int, Club> clubs, IReadOnlyDictionary<int, Country> countries) => throw new NotImplementedException();
    }
}
