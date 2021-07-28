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

        public List<T> GetDatas<T>(string sql, Func<IDataReader, T> transformFunc)
        {
            var results = new List<T>();

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
    }
}
