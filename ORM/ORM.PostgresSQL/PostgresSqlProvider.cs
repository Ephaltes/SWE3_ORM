using System;
using System.Collections.Generic;
using ORM.PostgresSQL.Model;

namespace ORM.PostgresSQL
{
    public static class PostgresSqlProvider
    {
        internal static string TimestampFormat = "yyyy-MM-dd hh:mm:ss";
        public static string LoadTableNamesQuery()
        {
            return
                "SELECT * FROM pg_catalog.pg_tables WHERE schemaname != 'pg_catalog' AND schemaname != 'information_schema'";
        }
        public static string LoadTableColumnsQuery(string databaseName, string tableName)
        {
            return
                "SELECT " +
                "  cols.COLUMN_NAME AS COLUMN_NAME, " +
                "  cols.IS_NULLABLE AS IS_NULLABLE, " +
                "  cols.DATA_TYPE AS DATA_TYPE, " +
                "  cols.CHARACTER_MAXIMUM_LENGTH AS CHARACTER_MAXIMUM_LENGTH, " +
                "  CASE " +
                "    WHEN cons.COLUMN_NAME IS NULL THEN 'NO' ELSE 'YES' " +
                "  END AS IS_PRIMARY_KEY " +
                "FROM test.INFORMATION_SCHEMA.COLUMNS cols " +
                "LEFT JOIN " + databaseName +
                ".INFORMATION_SCHEMA.KEY_COLUMN_USAGE cons ON cols.COLUMN_NAME = cons.COLUMN_NAME " +
                "WHERE cols.TABLE_NAME = '" + tableName + "';";
        }
        public static string CreateTableQuery(string tableName, List<DatabaseColumnModel> columns)
        {
            string query =
                "CREATE TABLE " + tableName + " " +
                "(";

            int added = 0;
            foreach (DatabaseColumnModel curr in columns)
            {
                if (added > 0) query += ", ";
                query += ColumnToCreateString(curr);
                added++;
            }

            query +=
                ") " +
                "WITH " +
                "(" +
                "  OIDS = FALSE" +
                ")";

            return query;
        }

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
        public static string DropTableQuery(string tableName)
        {
            string query = "DROP TABLE IF EXISTS " + tableName + " ";

            return query;
        }
        public static string SelectQuery(string tableName, int? indexStart, int? maxResults, List<string> returnFields,
            CustomExpression filter, DatabaseResultOrder[] resultOrder)
        {
            string query = "";
            string whereClause = "";

            // SELECT 
            query += "SELECT ";

            // fields 
            if (returnFields == null || returnFields.Count < 1)
            {
                query += "* ";
            }
            else
            {
                int fieldsAdded = 0;
                foreach (string curr in returnFields)
                    if (fieldsAdded == 0)
                    {
                        query += "\"" + curr + "\"";
                        fieldsAdded++;
                    }
                    else
                    {
                        query += ",\"" + curr + "\"";
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
        private static string ExpressionToWhereClause(CustomExpression filter)
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
                        clause +=
                            "(" +
                            (filter.LeftSide) + " LIKE " + ("%" + filter.RightSide) +
                            "OR " + (filter.LeftSide) + " LIKE " + "%" + filter.RightSide +
                            "%" + "OR " + (filter.LeftSide) + " LIKE " + filter.RightSide +
                            "%" + ")";
                    else
                        return null;

                    break;

                #endregion

                case CustomOperations.ContainsNot:

                    #region ContainsNot

                    if (filter.RightSide == null) return null;
                    if (filter.RightSide is string)
                        clause +=
                            "(" +
                            (filter.LeftSide) + " NOT LIKE " + ("%" + filter.RightSide) +
                            "OR " + (filter.LeftSide) + " NOT LIKE " + "%" +
                            filter.RightSide + "%" + "OR " + (filter.LeftSide) +
                            " NOT LIKE " + filter.RightSide + "%" + ")";
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
                            filter.LeftSide.ToString() + " LIKE " + (filter.RightSide + "%") +
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
                            filter.LeftSide.ToString() + " NOT LIKE " + (filter.RightSide + "%") +
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
                            filter.LeftSide.ToString() + " LIKE " + ("%" + filter.RightSide) +
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
                            filter.LeftSide.ToString() + " NOT LIKE " + ("%" + filter.RightSide) +
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
        internal static object DbTimestamp(DateTime toDateTime)
        {
            return toDateTime.ToString(TimestampFormat);
        }
        public static string InsertQuery(string tableName, string keys, string values)
        {
            string ret =
                "INSERT INTO " + tableName + " " +
                "(" + keys + ") " +
                "VALUES " +
                "(" + values + ") " +
                "RETURNING *;"; 
            return ret;
        }
        public static string InsertMultipleQuery(string tableName, string keys, List<string> values)
        {
            string ret =
                "BEGIN TRANSACTION;" +
                "  INSERT INTO " + tableName + " " +
                "  (" + keys + ") " +
                "  VALUES ";

            int added = 0;
            foreach (string value in values)
            {
                if (added > 0) ret += ",";
                ret += "  (" + value + ")";
                added++;
            }

            ret +=
                ";  COMMIT; ";

            return ret;
        }
        public static string UpdateQuery(string tableName, string keyValueClause, CustomExpression filter)
        {
            string ret =
                "UPDATE " + tableName + " SET " +
                keyValueClause + " ";

            if (filter != null) ret += "WHERE " + ExpressionToWhereClause(filter) + " ";
            ret += "RETURNING *";

            return ret;
        }
        public static string DeleteQuery(string tableName, CustomExpression filter)
        {
            string ret =
                "DELETE FROM " + tableName + " ";

            if (filter != null) ret += "WHERE " + ExpressionToWhereClause(filter) + " ";

            return ret;
        }
        public static string ExistsQuery(string tableName, CustomExpression filter)
        {
            string query = "";
            string whereClause = "";
             
            // select 
            query =
                "SELECT * " +
                "FROM " + tableName + " ";
             
            // expressions 
            if (filter != null) whereClause = ExpressionToWhereClause(filter);
            if (!string.IsNullOrEmpty(whereClause))
            {
                query += "WHERE " + whereClause + " ";
            }

            query += "LIMIT 1";
            return query;
        }
        public static string CountQuery(string tableName, string countColumnName, CustomExpression filter)
        {
            string query = "";
            string whereClause = "";
             
            // select 
            query =
                "SELECT COUNT(*) AS " + countColumnName + " " +
                "FROM " + tableName + " ";
             
            // expressions 
            if (filter != null) whereClause = ExpressionToWhereClause(filter);
            if (!string.IsNullOrEmpty(whereClause))
            {
                query += "WHERE " + whereClause + " ";
            }

            return query;
        }
        public static string SumQuery(string tableName, string fieldName, string sumColumnName, CustomExpression filter)
        {
            string whereClause = "";
             
            // select 
            string query =
                "SELECT SUM(" + fieldName + ") AS " + sumColumnName + " " +
                "FROM " + tableName + " ";
             
            // expressions 
            if (filter != null) whereClause = ExpressionToWhereClause(filter);
            if (!String.IsNullOrEmpty(whereClause))
            {
                query += "WHERE " + whereClause + " ";
            }

            return query;
        }
    }
}