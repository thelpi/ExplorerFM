using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace ExplorerFM.Providers
{
    internal static class SqlHelper
    {
        public static int? GetInt32(this MySqlDataReader reader, string columnName)
        {
            return reader.IsDBNull(columnName) ? null : (int?)reader.GetInt32(columnName);
        }

        public static bool IsDBNull(this MySqlDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName));
        }

        public static void SetParameter(this MySqlCommand command, string parameterName, DbType dbType, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = parameterName;
            p.DbType = dbType;
            p.Value = value ?? DBNull.Value;
            command.Parameters.Add(p);
        }
    }
}
