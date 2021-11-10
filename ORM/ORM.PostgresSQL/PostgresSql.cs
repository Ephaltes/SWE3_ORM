using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Npgsql;

namespace ORM.PostgresSQL
{
    public class PostgresSql
    {
        private readonly string _connectionString;
        public PostgresSql(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int Insert(string tableName, Dictionary<string, object> values)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string sql =
                    $"INSERT INTO {tableName} ({string.Join(",", values.Keys)}) VALUES ({string.Join(",", values.Values)})";

                return ExecuteNonQuery(sql, connection);
            }
        }

        public int Update(string tableName, Dictionary<string, object> values, string where)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string sql =
                    $"UPDATE {tableName} SET {string.Join(",", values.Select(x => $"{x.Key} = {x.Value}"))} WHERE {where}";

                return ExecuteNonQuery(sql, connection);
            }
        }
        
        public int Delete(string tableName, string where)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string sql = $"DELETE FROM {tableName} WHERE {where}";

                return ExecuteNonQuery(sql, connection);
            }
        }
        
       public List<Dictionary<string, object>> Select(string tableName, string where)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string sql = $"SELECT * FROM {tableName} WHERE {where}";

                return ExecuteReader(sql, connection);
            }
        }
       
       protected List<Dictionary<string, object>> ExecuteReader(string sql, NpgsqlConnection connection)
        {
            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
            {
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        result.Add(row);
                    }
                    return result;
                }
            }
        }
        
        
        protected int ExecuteNonQuery(string sql, NpgsqlConnection connection)
        {
            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
            {
                return command.ExecuteNonQuery();
            }
        }
    }
}