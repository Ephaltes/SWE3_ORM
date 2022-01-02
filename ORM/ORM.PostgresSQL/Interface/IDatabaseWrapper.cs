using System.Data;
using ORM.PostgresSQL.Model;

namespace ORM.PostgresSQL.Interface
{
    public interface IDatabaseWrapper
    {
        public List<string> ListTables();
      
        public DataTable Select(string tableName, int? indexStart, int? maxResults, List<string>? returnFields,
            CustomExpression filter);

        public DataTable Insert(string tableName, Dictionary<string, object> keyValuePairs);

        public DataTable Update(string tableName, Dictionary<string, object> keyValuePairs, CustomExpression filter);

        public void Delete(string tableName, CustomExpression filter);
        public DataTable Query(string query);

    }
}