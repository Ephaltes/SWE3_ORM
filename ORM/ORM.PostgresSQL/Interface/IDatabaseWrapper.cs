using System.Data;
using ORM.PostgresSQL.Model;

namespace ORM.PostgresSQL.Interface
{
    public interface IDatabaseWrapper
    {
        /// <summary>
        ///     Lists the tables in the database
        /// </summary>
        /// <returns>List with tables in database</returns>
        public List<string> ListTables();

        /// <summary>
        ///     Selects a Table in the Database
        /// </summary>
        /// <param name="tableName">Name of the table to select from</param>
        /// <param name="indexStart">Offset</param>
        /// <param name="maxResults">Limit</param>
        /// <param name="returnFields">Fields to return, if null all fields will be returned</param>
        /// <param name="filter">Where clause</param>
        /// <returns></returns>
        public DataTable Select(string tableName, int? indexStart, int? maxResults, List<string>? returnFields,
            CustomExpression? filter);

        /// <summary>
        ///     Inserts a row into a table
        /// </summary>
        /// <param name="tableName">Table to insert into</param>
        /// <param name="keyValuePairs">Key Value Pairs to insert</param>
        /// <returns>Inserted object</returns>
        public DataTable Insert(string tableName, Dictionary<string, object> keyValuePairs);

        /// <summary>
        ///     Updates a row in a table
        /// </summary>
        /// <param name="tableName">Table to update in</param>
        /// <param name="keyValuePairs">Key Value Pairs to insert</param>
        /// <param name="filter">Where clause</param>
        /// <returns>Updated object</returns>
        public DataTable Update(string tableName, Dictionary<string, object> keyValuePairs, CustomExpression? filter);

        /// <summary>
        ///     Deletes an item from a table
        /// </summary>
        /// <param name="tableName">table to delete from</param>
        /// <param name="filter">where clause</param>
        public void Delete(string tableName, CustomExpression? filter);
        /// <summary>
        ///     Custom Sql Query
        /// </summary>
        /// <param name="query">Sql Query to be executed</param>
        /// <returns>Whatever your sql query returns</returns>
        public DataTable Query(string query);
    }
}