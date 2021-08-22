using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace ExplorerFM
{
    public class MySqlService
    {
        private readonly string _connectionString;

        public MySqlService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<T> GetDatas<T>(string sql, Func<IDataReader, T> transformFunc, Action<int> countReportFunc = null)
        {
            var results = new List<T>();

            int i = 0;
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(transformFunc(reader));
                            countReportFunc?.Invoke(i);
                            i++;
                        }
                    }
                }
            }

            return results;
        }

        public T GetData<T>(string sql, Func<IDataReader, T> transformFunc)
        {
            return GetDatas(sql, transformFunc).FirstOrDefault();
        }

        public T GetValue<T>(string sql, T defaultValue = default(T))
        {
            var results = new List<T>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    var data = command.ExecuteScalar();
                    return data == DBNull.Value || data == null
                        ? defaultValue
                        : (T)Convert.ChangeType(data, typeof(T));
                }
            }
        }

        public static string TestConnection(string connectionString)
        {
            string error = null;

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT id FROM player LIMIT 0, 1";
                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                                error = "The database exists but no data has been found.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return error;
        }
    }
}
