using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Linq.Expressions;
using Npgsql;
using ORM.PostgresSQL.Interface;
using ORM.PostgresSQL.Model;

namespace ORM.PostgresSQL
{
    public class PostgresDb : IDatabaseWrapper
    {
        private readonly string _connectionString;
        private readonly string _databaseName;
        private string _countColumnName = "__count__";
        private string _sumColumnName = "__sum__";
        public PostgresDb(string connectionString, string databaseName)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("connectionString invalid");
            if (string.IsNullOrWhiteSpace(databaseName)) throw new ArgumentException("databaseName invalid");
            
            _connectionString = connectionString;
            _databaseName = databaseName;
        }

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
        public bool TableExists(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));

            return ListTables().Contains(tableName);
        }
        public List<DatabaseColumnModel> DescribeTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));

            List<DatabaseColumnModel> columns = new List<DatabaseColumnModel>();
            DataTable result = Query(PostgresSqlProvider.LoadTableColumnsQuery(_databaseName, tableName));
            if (result != null && result.Rows.Count > 0)
                foreach (DataRow currColumn in result.Rows)
                {
                    DatabaseColumnModel tempColumn = new DatabaseColumnModel();

                    tempColumn.Name = currColumn["COLUMN_NAME"].ToString();

                    tempColumn.Type = DatabaseHelper.DataTypeFromString(currColumn["DATA_TYPE"].ToString());

                    if (currColumn.Table.Columns.Contains("IS_NULLABLE"))
                    {
                        if (string.Compare(currColumn["IS_NULLABLE"].ToString(), "YES") == 0)
                            tempColumn.Nullable = true;
                        else tempColumn.Nullable = false;
                    }
                    else if (currColumn.Table.Columns.Contains("IS_NOT_NULLABLE"))
                    {
                        tempColumn.Nullable = !Convert.ToBoolean(currColumn["IS_NOT_NULLABLE"]);
                    }

                    if (currColumn["IS_PRIMARY_KEY"] != null
                        && currColumn["IS_PRIMARY_KEY"] != DBNull.Value
                        && !string.IsNullOrEmpty(currColumn["IS_PRIMARY_KEY"].ToString()))
                        if (currColumn["IS_PRIMARY_KEY"].ToString().ToLower().Equals("yes"))
                            tempColumn.PrimaryKey = true;

                    if (!columns.Exists(c => c.Name.Equals(tempColumn.Name)))
                        columns.Add(tempColumn);
                }

            return columns;
        }
        public Dictionary<string, List<DatabaseColumnModel>> DescribeDatabase()
        {
            Dictionary<string, List<DatabaseColumnModel>> ret = new Dictionary<string, List<DatabaseColumnModel>>();
            List<string> tableNames = ListTables();

            if (tableNames.Count <= 0)
                return ret;

            foreach (string tableName in tableNames)
                ret.Add(tableName, DescribeTable(tableName));

            return ret;
        }
        public void CreateTable(string tableName, List<DatabaseColumnModel> columns)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (columns == null || columns.Count < 1) throw new ArgumentNullException(nameof(columns)); 
            Query(PostgresSqlProvider.CreateTableQuery(tableName, columns)); 
        }
        public void DeleteTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            Query(PostgresSqlProvider.DropTableQuery(tableName));
        }
        public string GetPrimaryKeyColumn(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));

            List<DatabaseColumnModel> details = DescribeTable(tableName);
            if (details != null && details.Count > 0)
            {
                foreach (DatabaseColumnModel c in details)
                {
                    if (c.PrimaryKey) return c.Name;
                }
            }

            return null;
        }
        public List<string> GetColumnNames(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));

            List<DatabaseColumnModel> details = DescribeTable(tableName);
            List<string> columnNames = new List<string>();

            if (details != null && details.Count > 0)
            {
                foreach (DatabaseColumnModel c in details)
                {
                    columnNames.Add(c.Name);
                }
            }

            return columnNames;
        }
        public DataTable Select(string tableName, int? indexStart, int? maxResults, List<string> returnFields,
            CustomExpression filter)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            return Query(PostgresSqlProvider.SelectQuery(tableName, indexStart, maxResults, returnFields, filter, null));
        }
        public DataTable Select(string tableName, int? indexStart, int? maxResults, List<string> returnFields,
            CustomExpression filter,
            DatabaseResultOrder[] resultOrder)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            return Query(PostgresSqlProvider.SelectQuery(tableName, indexStart, maxResults, returnFields, filter, resultOrder));
        }
        public DataTable Insert(string tableName, Dictionary<string, object> keyValuePairs)
        {
           if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (keyValuePairs == null || keyValuePairs.Count < 1) throw new ArgumentNullException(nameof(keyValuePairs));
             
            #region Build-Key-Value-Pairs

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
                {
                    values += curr.Value switch
                    {
                        DateTime time => "'" + PostgresSqlProvider.DbTimestamp(time) + "'",
                        TimeSpan span => "'" + span + "'",
                        int or long or decimal => curr.Value.ToString(),
                        Enum => (int)curr.Value,
                        _ => $"'{curr.Value}'"
                    };
                }
                else
                {
                    values += "null";
                }

                added++;
            }

            #endregion

            #region Build-INSERT-Query-and-Submit

            return Query(PostgresSqlProvider.InsertQuery(tableName, keys, values)); 

            #endregion 
        }
        public void Insert(string tableName, List<Dictionary<string, object>> keyValuePairList)
        {
           if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (keyValuePairList == null || keyValuePairList.Count < 1) throw new ArgumentNullException(nameof(keyValuePairList));

            #region Validate-Inputs

            Dictionary<string, object> reference = keyValuePairList[0];

            if (keyValuePairList.Count > 1)
            {
                foreach (Dictionary<string, object> dict in keyValuePairList)
                {
                    if (!(reference.Count == dict.Count) || !(reference.Keys.SequenceEqual(dict.Keys)))
                    {
                        throw new ArgumentException("All supplied dictionaries must contain exactly the same keys.");
                    }
                }
            }

            #endregion

            #region Build-Keys

            string keys = "";
            int keysAdded = 0;
            foreach (KeyValuePair<string, object> curr in reference)
            {
                if (keysAdded > 0) keys += ",";
                keys += curr.Key;
                keysAdded++;
            }

            #endregion

            #region Build-Values

            List<string> values = new List<string>();

            foreach (Dictionary<string, object> currDict in keyValuePairList)
            {
                string vals = "";
                int valsAdded = 0;

                foreach (KeyValuePair<string, object> currKvp in currDict)
                {
                    if (valsAdded > 0) vals += ",";

                    if (currKvp.Value != null)
                    {
                        vals += currKvp.Value switch
                        {
                            DateTime time => "'" + PostgresSqlProvider.DbTimestamp(time) + "'",
                            TimeSpan span => "'" + span + "'",
                            int or long or decimal => currKvp.Value.ToString(),
                            Enum => (int)currKvp.Value,
                            _ => $"'{currKvp.Value}'"
                        };
                    }
                    else
                    {
                        vals += "null";
                    }

                    valsAdded++;
                }

                values.Add(vals);
            }

            #endregion

            #region Build-INSERT-Query-and-Submit

            Query(PostgresSqlProvider.InsertMultipleQuery(tableName, keys, values));

            #endregion
        }
       
        public DataTable Update(string tableName, Dictionary<string, object> keyValuePairs, CustomExpression filter)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (keyValuePairs == null || keyValuePairs.Count < 1) throw new ArgumentNullException(nameof(keyValuePairs));
             
            #region Build-Key-Value-Clause

            string keyValueClause = ""; 
            int added = 0;

            foreach (KeyValuePair<string, object> curr in keyValuePairs)
            {
                if (string.IsNullOrEmpty(curr.Key)) continue;

                if (added > 0) keyValueClause += ",";
                 
                if (curr.Value != null)
                {
                    keyValueClause += curr.Value switch
                    {
                        DateTime time => (curr.Key) + "='" + PostgresSqlProvider.DbTimestamp(time) + "'",
                        TimeSpan span => (curr.Key) + "='" + span + "'",
                        int or long or decimal => (curr.Key) + "=" + curr.Value,
                        Enum => $"{curr.Key} = {(int)curr.Value}" ,
                        _ => $"{curr.Key} = '{curr.Value}'"
                    };
                }
                else
                {
                    keyValueClause += (curr.Key) + "= null";
                } 

                added++;
            }

            #endregion

            #region Build-UPDATE-Query-and-Submit

            return Query(PostgresSqlProvider.UpdateQuery(tableName, keyValueClause, filter));

            #endregion
        }
        public void Delete(string tableName, CustomExpression filter)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            Query(PostgresSqlProvider.DeleteQuery(tableName, filter));
        }
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
                    {
                        result = ds.Tables[0];
                    }

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
        public bool Exists(string tableName, CustomExpression filter)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            DataTable result = Query(PostgresSqlProvider.ExistsQuery(tableName, filter));
            if (result != null && result.Rows.Count > 0) return true;
            return false;
        }
        public long Count(string tableName, CustomExpression filter)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            DataTable result = Query(PostgresSqlProvider.CountQuery(tableName, _countColumnName, filter));
            if (result != null
                && result.Rows.Count > 0
                && result.Rows[0].Table.Columns.Contains(_countColumnName)
                && result.Rows[0][_countColumnName] != null
                && result.Rows[0][_countColumnName] != DBNull.Value)
            {
                return Convert.ToInt64(result.Rows[0][_countColumnName]);
            }
            return 0;
        }
        public decimal Sum(string tableName, string fieldName, CustomExpression filter)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            DataTable result = Query(PostgresSqlProvider.SumQuery(tableName, fieldName, _sumColumnName, filter));
            if (result != null
                && result.Rows.Count > 0
                && result.Rows[0].Table.Columns.Contains(_sumColumnName)
                && result.Rows[0][_sumColumnName] != null
                && result.Rows[0][_sumColumnName] != DBNull.Value)
            {
                return Convert.ToDecimal(result.Rows[0][_sumColumnName]);
            }
            return 0m;
        } 
    }
}