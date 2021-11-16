using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using ORM.PostgresSQL.Model;

namespace ORM.PostgresSQL.Interface
{
    public interface IDatabaseWrapper
    {
        public List<string> ListTables();
        public bool TableExists(string tableName);
        public List<DatabaseColumnModel> DescribeTable(string tableName);

        public Dictionary<string, List<DatabaseColumnModel>> DescribeDatabase();

        public void CreateTable(string tableName, List<DatabaseColumnModel> columns);

        public void DeleteTable(string tableName);

        public string GetPrimaryKeyColumn(string tableName);

        public List<string> GetColumnNames(string tableName);

        public DataTable Select(string tableName, int? indexStart, int? maxResults, List<string> returnFields,
            CustomExpression filter);

        public DataTable Select(string tableName, int? indexStart, int? maxResults, List<string> returnFields,
            CustomExpression filter, DatabaseResultOrder[] resultOrder);

        public DataTable Insert(string tableName, Dictionary<string, object> keyValuePairs);
        public void Insert(string tableName, List<Dictionary<string, object>> keyValuePairList);

        public DataTable Update(string tableName, Dictionary<string, object> keyValuePairs, CustomExpression filter);

        public void Delete(string tableName, CustomExpression filter);

        public DataTable Query(string query);

        public bool Exists(string tableName, CustomExpression filter);

        public long Count(string tableName, CustomExpression filter);

        public decimal Sum(string tableName, string fieldName, CustomExpression filter);
    }
}