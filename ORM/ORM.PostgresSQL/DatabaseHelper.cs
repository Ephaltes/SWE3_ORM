using System.Collections;

namespace ORM.PostgresSQL
{
    public class DatabaseHelper
    {
        /// <summary>
        ///     Checks whether the object is an IEnumerable
        /// </summary>
        /// <param name="o">object to check</param>
        /// <returns>true if it is a list else false</returns>
        public static bool IsList(object o)
        {
            if (o == null) return false;

            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        /// <summary>
        ///     Converts an object into a List
        /// </summary>
        /// <param name="obj">object to convert to list</param>
        /// <returns>List of object</returns>
        public static List<object> ObjectToList(object obj)
        {
            if (obj == null) return null;

            List<object> ret = new List<object>();
            IEnumerator? enumerator = ((IEnumerable)obj).GetEnumerator();
            while (enumerator.MoveNext())
                ret.Add(enumerator.Current);

            return ret;
        }
    }
}