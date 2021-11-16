using System;
using System.Collections;
using System.Collections.Generic;
using ORM.PostgresSQL.Model;

namespace ORM.PostgresSQL
{
    public class DatabaseHelper
    {
        public static bool IsList(object o)
        {
            if (o == null) return false;

            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static List<object> ObjectToList(object obj)
        {
            if (obj == null) return null;

            List<object> ret = new List<object>();
            IEnumerator? enumerator = ((IEnumerable)obj).GetEnumerator();
            while (enumerator.MoveNext())
                ret.Add(enumerator.Current);

            return ret;
        }
        public static DatabaseColumnType DataTypeFromString(string? toString)
        {
            if (string.IsNullOrEmpty(toString)) throw new ArgumentNullException(nameof(toString));

            toString = toString.ToLower();
            if (toString.Contains("(")) toString = toString.Substring(0, toString.IndexOf("("));

            switch (toString)
            {
                case "bigserial": // pgsql
                case "bigint": // mssql
                    return DatabaseColumnType.Long;

                case "boolean": // pgsql
                case "integer": // pgsql, sqlite
                case "int": // mssql, mysql
                case "serial": // pgsql
                    return DatabaseColumnType.Int;

                case "double": // mysql
                case "double precision": // pgsql
                case "float": // mysql
                    return DatabaseColumnType.Double;

                case "date": // mssql, mysql
                    return DatabaseColumnType.DateTime;

                case "timestamp without timezone": // pgsql
                case "timestamp without time zone": // pgsql
                case "time without timezone": // pgsql
                case "time without time zone": // pgsql
                    return DatabaseColumnType.TimeSpan;

                case "text": // mssql, mysql, pgsql, sqlite
                case "varchar": // mssql, mysql, pgsql
                    return DatabaseColumnType.Varchar;

                case "blob":
                case "bytea":
                    return DatabaseColumnType.Blob; // sqlite

                default:
                    throw new ArgumentException("Unknown DataType: " + toString);
            }
        }
    }
}