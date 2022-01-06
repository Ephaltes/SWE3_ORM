using System.Data;
using System.Diagnostics.CodeAnalysis;
using Npgsql;
using ORM.PostgresSQL.Interface;
using ORM.PostgresSQL.Model;

namespace ORM.PostgresSQL
{
    /// <summary>
    /// Excluded from code coverage because it is a wrapper for NpgsqlConnection.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PostgresDb : IDatabaseWrapper
    {
        private readonly string _connectionString;

        public PostgresDb(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("connectionString invalid");

            _connectionString = connectionString;
        }
        /// <inheritdoc />
        public List<string> ListTables()
        {
            List<string> tableNames = new List<string>();
            DataTable result = Query(PostgresSqlProvider.LoadTableNamesQuery());

            if (result.Rows.Count <= 0)
                return tableNames;

            foreach (DataRow row in result.Rows)
                tableNames.Add(row["tablename"].ToString());

            return tableNames;
        }
        /// <inheritdoc />
        public DataTable Select(string tableName, int? indexStart, int? maxResults, List<string>? returnFields,
            CustomExpression? filter)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));

            return Query(PostgresSqlProvider.SelectQuery(tableName, indexStart, maxResults, returnFields, filter,
                null));
        }
        /// <inheritdoc />
        public DataTable Insert(string tableName, Dictionary<string, object> keyValuePairs)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (keyValuePairs == null || keyValuePairs.Count < 1)
                throw new ArgumentNullException(nameof(keyValuePairs));

            #region key value

            string keys = "";
            string values = "";
            int added = 0;

            foreach (KeyValuePair<string, object> curr in keyValuePairs)
            {
                if (string.IsNullOrEmpty(curr.Key)) continue;

                if (added > 0)
                {
                    keys += ",";
                    values += ",";
                }

                keys += curr.Key;

                if (curr.Value != null)
                    values += curr.Value switch
                    {
                        DateTime time => "'" + PostgresSqlProvider.DbTimestamp(time) + "'",
                        TimeSpan span => "'" + span + "'",
                        int or long or decimal => curr.Value.ToString(),
                        Enum => (int)curr.Value,
                        _ => $"'{curr.Value}'"
                    };
                else
                    values += "null";

                added++;
            }

            #endregion


            return Query(PostgresSqlProvider.InsertQuery(tableName, keys, values));
        }
        /// <inheritdoc />
        public DataTable Update(string tableName, Dictionary<string, object> keyValuePairs, CustomExpression? filter)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (keyValuePairs == null || keyValuePairs.Count < 1)
                throw new ArgumentNullException(nameof(keyValuePairs));

            #region key value

            string keyValueClause = "";
            int added = 0;

            foreach (KeyValuePair<string, object> curr in keyValuePairs)
            {
                if (string.IsNullOrEmpty(curr.Key)) continue;

                if (added > 0) keyValueClause += ",";

                if (curr.Value != null)
                    keyValueClause += curr.Value switch
                    {
                        DateTime time => curr.Key + "='" + PostgresSqlProvider.DbTimestamp(time) + "'",
                        TimeSpan span => curr.Key + "='" + span + "'",
                        int or long or decimal => curr.Key + "=" + curr.Value,
                        Enum => $"{curr.Key} = {(int)curr.Value}",
                        _ => $"{curr.Key} = '{curr.Value}'"
                    };
                else
                    keyValueClause += curr.Key + "= null";

                added++;
            }

            #endregion


            return Query(PostgresSqlProvider.UpdateQuery(tableName, keyValueClause, filter));
        }
        /// <inheritdoc />
        public void Delete(string tableName, CustomExpression? filter)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            Query(PostgresSqlProvider.DeleteQuery(tableName, filter));
        }
        /// <inheritdoc />
        public DataTable Query(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException(query);

            DataTable result = new DataTable();

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                        result = ds.Tables[0];

                    conn.Close();
                }

                return result;
            }
            catch (Exception e)
            {
                e.Data.Add("Query", query);

                throw;
            }
        }
    }
}