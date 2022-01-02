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
                case "bigserial": 
                case "bigint": 
                    return DatabaseColumnType.Long;

                case "boolean":
                case "integer": 
                case "int": 
                case "serial": 
                    return DatabaseColumnType.Int;

                case "double": 
                case "double precision":
                case "float": 
                    return DatabaseColumnType.Double;

                case "date": 
                    return DatabaseColumnType.DateTime;

                case "timestamp without timezone": 
                case "timestamp without time zone": 
                case "time without timezone": 
                case "time without time zone": 
                    return DatabaseColumnType.TimeSpan;

                case "text": 
                case "varchar": 
                    return DatabaseColumnType.Varchar;

                case "blob":
                case "bytea":
                    return DatabaseColumnType.Blob;

                default:
                    throw new ArgumentException("Unknown DataType: " + toString);
            }
        }
    }
}