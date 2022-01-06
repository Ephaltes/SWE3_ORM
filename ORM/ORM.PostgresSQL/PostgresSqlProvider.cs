using System.Diagnostics.CodeAnalysis;
using ORM.PostgresSQL.Model;
using Serilog;

namespace ORM.PostgresSQL
{
    public static class PostgresSqlProvider
    {
        internal static string TimestampFormat = "yyyy-MM-dd hh:mm:ss";
        private static readonly ILogger _logger = Log.ForContext(typeof(PostgresSqlProvider));
        /// <summary>
        ///     Query to get Table Names
        /// </summary>
        /// <returns></returns>
        public static string LoadTableNamesQuery()
        {
            return
                "SELECT * FROM pg_catalog.pg_tables WHERE schemaname != 'pg_catalog' AND schemaname != 'information_schema'";
        }

        /// <summary>
        ///     Converts enum into database type
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        internal static string ColumnToCreateString(DatabaseColumnModel col)
        {
            string ret =
                "\"" + col.Name + "\" ";

            if (col.PrimaryKey)
            {
                ret += "SERIAL PRIMARY KEY ";

                return ret;
            }

            switch (col.Type)
            {
                case DatabaseColumnType.Varchar:
                case DatabaseColumnType.Nvarchar:
                    ret += "text ";

                    break;
                case DatabaseColumnType.Int:
                    ret += "integer ";

                    break;
                case DatabaseColumnType.Long:
                    ret += "bigint ";

                    break;
                case DatabaseColumnType.Blob:
                    ret += "bytea ";

                    break;
                case DatabaseColumnType.Double:
                    ret += "double precision ";

                    break;
                case DatabaseColumnType.TimeSpan:
                    ret += "timestamp without time zone ";

                    break;
                case DatabaseColumnType.DateTime:
                    ret += "date ";

                    break;
                default:
                    throw new ArgumentException("Unknown DataType: " + col.Type);
            }

            if (col.Nullable) ret += "NULL ";
            else ret += "NOT NULL ";

            return ret;
        }
        /// <summary>
        ///     Creates the select statement for a table
        /// </summary>
        /// <param name="tableName">table to select from</param>
        /// <param name="indexStart">offset</param>
        /// <param name="maxResults">limit</param>
        /// <param name="returnFields">fields to return</param>
        /// <param name="filter">where clause</param>
        /// <param name="resultOrder">ASC or DESC</param>
        /// <returns></returns>
        public static string SelectQuery(string tableName, int? indexStart, int? maxResults, List<string>? returnFields,
            CustomExpression? filter, DatabaseResultOrder[] resultOrder)
        {
            string query = "";
            string whereClause = "";

            query += "SELECT ";

            // fields 
            if (returnFields is null || returnFields.Count < 1)
            {
                query += "* ";
            }
            else
            {
                int fieldsAdded = 0;
                foreach (string curr in returnFields)
                    if (fieldsAdded == 0)
                    {
                        query += curr;
                        fieldsAdded++;
                    }
                    else
                    {
                        query += $",{curr}";
                        fieldsAdded++;
                    }
            }

            query += " ";

            // table 
            query += "FROM " + tableName + " ";

            // expressions 
            if (filter != null) whereClause = ExpressionToWhereClause(filter);
            if (!string.IsNullOrEmpty(whereClause))
                query += "WHERE " + whereClause + " ";

            // order clause 
            query += BuildOrderByClause(resultOrder);

            // limit 
            if (maxResults > 0)
            {
                if (indexStart != null && indexStart >= 0)
                    query += "OFFSET " + indexStart + " LIMIT " + maxResults;
                else
                    query += "LIMIT " + maxResults;
            }

            _logger.Debug($"Select Query: {query}");
            return query;
        }
        private static string BuildOrderByClause(DatabaseResultOrder[] resultOrder)
        {
            if (resultOrder == null || resultOrder.Length < 0) return null;

            string ret = "ORDER BY ";

            for (int i = 0; i < resultOrder.Length; i++)
            {
                if (i > 0) ret += ", ";
                ret += resultOrder[i].ColumnName + " ";

                ret += resultOrder[i].Direction switch
                {
                    CustomOrderDirection.Ascending => "ASC",
                    CustomOrderDirection.Descending => "DESC",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            ret += " ";

            return ret;
        }
        private static string ExpressionToWhereClause(CustomExpression? filter)
        {
            if (filter == null) return null;

            string clause = "";

            if (filter.LeftSide == null) return null;

            clause += "(";

            if (filter.LeftSide is CustomExpression)
            {
                clause += ExpressionToWhereClause((CustomExpression)filter.LeftSide) + " ";
            }
            else
            {
                if (!(filter.LeftSide is string))
                    throw new ArgumentException("Left term must be of type CustomExpression or String");

                if (filter.Operator != CustomOperations.Contains
                    && filter.Operator != CustomOperations.ContainsNot
                    && filter.Operator != CustomOperations.StartsWith
                    && filter.Operator != CustomOperations.StartsWithNot
                    && filter.Operator != CustomOperations.EndsWith
                    && filter.Operator != CustomOperations.EndsWithNot)
                    //
                    // These operators will add the left term
                    //
                    clause += filter.LeftSide + " ";
            }

            switch (filter.Operator)
            {
                #region Process-By-Operation

                case CustomOperations.And:

                    #region And

                    if (filter.RightSide == null) return null;

                    clause += "AND ";

                    if (filter.RightSide is CustomExpression)
                    {
                        clause += ExpressionToWhereClause((CustomExpression)filter.RightSide);
                    }
                    else
                    {
                        if (filter.RightSide is DateTime || filter.RightSide is DateTime?)
                            clause += "'" + DbTimestamp(Convert.ToDateTime(filter.RightSide)) + "'";
                        else if (filter.RightSide is int || filter.RightSide is long || filter.RightSide is decimal)
                            clause += filter.RightSide.ToString();
                        else
                            clause += filter.RightSide.ToString();
                    }

                    break;

                #endregion

                case CustomOperations.Or:

                    #region Or

                    if (filter.RightSide == null) return null;

                    clause += "OR ";
                    if (filter.RightSide is CustomExpression)
                    {
                        clause += ExpressionToWhereClause((CustomExpression)filter.RightSide);
                    }
                    else
                    {
                        if (filter.RightSide is DateTime || filter.RightSide is DateTime?)
                            clause += "'" + DbTimestamp(Convert.ToDateTime(filter.RightSide)) + "'";
                        else if (filter.RightSide is int || filter.RightSide is long || filter.RightSide is decimal)
                            clause += filter.RightSide.ToString();
                        else
                            clause += filter.RightSide.ToString();
                    }

                    break;

                #endregion

                case CustomOperations.Equals:

                    #region Equals

                    if (filter.RightSide == null) return null;

                    clause += "= ";
                    if (filter.RightSide is CustomExpression)
                    {
                        clause += ExpressionToWhereClause((CustomExpression)filter.RightSide);
                    }
                    else
                    {
                        if (filter.RightSide is DateTime || filter.RightSide is DateTime?)
                            clause += "'" + DbTimestamp(Convert.ToDateTime(filter.RightSide)) + "'";
                        else if (filter.RightSide is int || filter.RightSide is long || filter.RightSide is decimal)
                            clause += filter.RightSide.ToString();
                        else
                            clause += filter.RightSide.ToString();
                    }

                    break;

                #endregion

                case CustomOperations.NotEquals:

                    #region NotEquals

                    if (filter.RightSide == null) return null;

                    clause += "<> ";
                    if (filter.RightSide is CustomExpression)
                    {
                        clause += ExpressionToWhereClause((CustomExpression)filter.RightSide);
                    }
                    else
                    {
                        if (filter.RightSide is DateTime || filter.RightSide is DateTime?)
                            clause += "'" + DbTimestamp(Convert.ToDateTime(filter.RightSide)) + "'";
                        else if (filter.RightSide is int || filter.RightSide is long || filter.RightSide is decimal)
                            clause += filter.RightSide.ToString();
                        else
                            clause += filter.RightSide.ToString();
                    }

                    break;

                #endregion

                case CustomOperations.In:

                    #region In

                    if (filter.RightSide == null) return null;

                    int inAdded = 0;

                    if (!DatabaseHelper.IsList(filter.RightSide)) return null;

                    List<object> inTempList = DatabaseHelper.ObjectToList(filter.RightSide);
                    clause += " IN (";
                    foreach (object currObj in inTempList)
                    {
                        if (currObj == null) continue;

                        if (inAdded > 0) clause += ",";
                        if (currObj is DateTime || currObj is DateTime?)
                            clause += "'" + DbTimestamp(Convert.ToDateTime(currObj)) + "'";
                        else if (currObj is int || currObj is long || currObj is decimal)
                            clause += currObj.ToString();
                        else
                            clause += currObj.ToString();
                        inAdded++;
                    }

                    clause += ")";

                    break;

                #endregion

                case CustomOperations.NotIn:

                    #region NotIn

                    if (filter.RightSide == null) return null;

                    int notInAdded = 0;

                    if (!DatabaseHelper.IsList(filter.RightSide)) return null;

                    List<object> notInTempList = DatabaseHelper.ObjectToList(filter.RightSide);
                    clause += " NOT IN (";
                    foreach (object currObj in notInTempList)
                    {
                        if (currObj == null) continue;

                        if (notInAdded > 0) clause += ",";
                        if (currObj is DateTime || currObj is DateTime?)
                            clause += "'" + DbTimestamp(Convert.ToDateTime(currObj)) + "'";
                        else if (currObj is int || currObj is long || currObj is decimal)
                            clause += currObj.ToString();
                        else
                            clause += currObj.ToString();
                        notInAdded++;
                    }

                    clause += ")";

                    break;

                #endregion

                case CustomOperations.Contains:

                    #region Contains

                    if (filter.RightSide == null) return null;

                    if (filter.RightSide is string)
                        clause += $" ({filter.LeftSide} LIKE '%{filter.RightSide}%')" +
                                  $" OR ( {filter.LeftSide} LIKE '%{filter.RightSide}')" +
                                  $" OR ( {filter.LeftSide} LIKE '{filter.RightSide}%') ";
                    else
                        return null;

                    break;

                #endregion

                case CustomOperations.ContainsNot:

                    #region ContainsNot

                    if (filter.RightSide == null) return null;
                    if (filter.RightSide is string)
                        clause += $" ({filter.LeftSide} NOT LIKE '%{filter.RightSide}%')" +
                                  $" OR ( {filter.LeftSide} NOT LIKE '%{filter.RightSide}')" +
                                  $" OR ( {filter.LeftSide} NOT LIKE '{filter.RightSide}%') ";
                    else
                        return null;

                    break;

                #endregion

                case CustomOperations.StartsWith:

                    #region StartsWith

                    if (filter.RightSide == null) return null;
                    if (filter.RightSide is string)
                        clause +=
                            "(" +
                            filter.LeftSide + " LIKE " + (filter.RightSide + "%") +
                            ")";
                    else
                        return null;

                    break;

                #endregion

                case CustomOperations.StartsWithNot:

                    #region StartsWithNot

                    if (filter.RightSide == null) return null;
                    if (filter.RightSide is string)
                        clause +=
                            "(" +
                            filter.LeftSide + " NOT LIKE " + (filter.RightSide + "%") +
                            ")";
                    else
                        return null;

                    break;

                #endregion

                case CustomOperations.EndsWith:

                    #region EndsWith

                    if (filter.RightSide == null) return null;
                    if (filter.RightSide is string)
                        clause +=
                            "(" +
                            filter.LeftSide + " LIKE " + ("%" + filter.RightSide) +
                            ")";
                    else
                        return null;

                    break;

                #endregion

                case CustomOperations.EndsWithNot:

                    #region EndsWithNot

                    if (filter.RightSide == null) return null;
                    if (filter.RightSide is string)
                        clause +=
                            "(" +
                            filter.LeftSide + " NOT LIKE " + ("%" + filter.RightSide) +
                            ")";
                    else
                        return null;

                    break;

                #endregion

                case CustomOperations.GreaterThan:

                    #region GreaterThan

                    if (filter.RightSide == null) return null;

                    clause += "> ";
                    if (filter.RightSide is CustomExpression)
                    {
                        clause += ExpressionToWhereClause((CustomExpression)filter.RightSide);
                    }
                    else
                    {
                        if (filter.RightSide is DateTime || filter.RightSide is DateTime?)
                            clause += "'" + DbTimestamp(Convert.ToDateTime(filter.RightSide)) + "'";
                        else if (filter.RightSide is int || filter.RightSide is long || filter.RightSide is decimal)
                            clause += filter.RightSide.ToString();
                        else
                            clause += filter.RightSide.ToString();
                    }

                    break;

                #endregion

                case CustomOperations.GreaterThanOrEqualTo:

                    #region GreaterThanOrEqualTo

                    if (filter.RightSide == null) return null;

                    clause += ">= ";
                    if (filter.RightSide is CustomExpression)
                    {
                        clause += ExpressionToWhereClause((CustomExpression)filter.RightSide);
                    }
                    else
                    {
                        if (filter.RightSide is DateTime || filter.RightSide is DateTime?)
                            clause += "'" + DbTimestamp(Convert.ToDateTime(filter.RightSide)) + "'";
                        else if (filter.RightSide is int || filter.RightSide is long || filter.RightSide is decimal)
                            clause += filter.RightSide.ToString();
                        else
                            clause += filter.RightSide.ToString();
                    }

                    break;

                #endregion

                case CustomOperations.LessThan:

                    #region LessThan

                    if (filter.RightSide == null) return null;

                    clause += "< ";
                    if (filter.RightSide is CustomExpression)
                    {
                        clause += ExpressionToWhereClause((CustomExpression)filter.RightSide);
                    }
                    else
                    {
                        if (filter.RightSide is DateTime || filter.RightSide is DateTime?)
                            clause += "'" + DbTimestamp(Convert.ToDateTime(filter.RightSide)) + "'";
                        else if (filter.RightSide is int || filter.RightSide is long || filter.RightSide is decimal)
                            clause += filter.RightSide.ToString();
                        else
                            clause += filter.RightSide.ToString();
                    }

                    break;

                #endregion

                case CustomOperations.LessThanOrEqualTo:

                    #region LessThanOrEqualTo

                    if (filter.RightSide == null) return null;

                    clause += "<= ";
                    if (filter.RightSide is CustomExpression)
                    {
                        clause += ExpressionToWhereClause((CustomExpression)filter.RightSide);
                    }
                    else
                    {
                        if (filter.RightSide is DateTime || filter.RightSide is DateTime?)
                            clause += "'" + DbTimestamp(Convert.ToDateTime(filter.RightSide)) + "'";
                        else if (filter.RightSide is int || filter.RightSide is long || filter.RightSide is decimal)
                            clause += filter.RightSide.ToString();
                        else
                            clause += filter.RightSide.ToString();
                    }

                    break;

                #endregion

                case CustomOperations.IsNull:

                    #region IsNull

                    clause += " IS NULL";

                    break;

                #endregion

                case CustomOperations.IsNotNull:

                    #region IsNotNull

                    clause += " IS NOT NULL";

                    break;

                #endregion

                #endregion
            }

            clause += ")";

            return clause;
        }
        /// <summary>
        ///     Converts datetime to databse datetime format
        /// </summary>
        /// <param name="toDateTime">datetime to convert to databse time</param>
        /// <returns>datetime format for database</returns>
        internal static object DbTimestamp(DateTime toDateTime)
        {
            return toDateTime.ToString(TimestampFormat);
        }
        /// <summary>
        ///     Insert statement for table
        /// </summary>
        /// <param name="tableName">tablename</param>
        /// <param name="keys">keys</param>
        /// <param name="values">values</param>
        /// <returns>insert statement</returns>
        public static string InsertQuery(string tableName, string keys, string values)
        {
            string ret = $"INSERT INTO {tableName} ( {keys} ) VALUES ( {values} ) RETURNING *;";
            _logger.Debug($"insert Query: {ret}");
            return ret;
        }
        /// <summary>
        /// Creates Update Statement
        /// </summary>
        /// <param name="tableName">tablename to update</param>
        /// <param name="keyValueClause">fields to update</param>
        /// <param name="filter">where clause</param>
        /// <returns>update statement</returns>
        public static string UpdateQuery(string tableName, string keyValueClause, CustomExpression? filter)
        {
            string ret;
            
            if(filter is not null)
                ret = $"UPDATE {tableName} SET {keyValueClause} WHERE {ExpressionToWhereClause(filter)} RETURNING *;";
            else
                ret = $"UPDATE {tableName} SET {keyValueClause} RETURNING *;";

            _logger.Debug($"update Query: {ret}");
            return ret;
        }
        /// <summary>
        /// Creates Delete Statement
        /// </summary>
        /// <param name="tableName">table to delete from</param>
        /// <param name="filter">where clause</param>
        /// <returns>delete statement</returns>
        public static string DeleteQuery(string tableName, CustomExpression? filter)
        {
            string ret;
            
            if(filter is not null)
                ret = $"DELETE FROM {tableName} WHERE {ExpressionToWhereClause(filter)};";
            else
                ret = $"DELETE FROM {tableName};";
            
            _logger.Debug($"delete Query: {ret}");
            return ret;
        }
    }
}