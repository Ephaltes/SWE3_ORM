using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using ORM.Core.Converter;
using ORM.Core.Models;
using ORM.PostgresSQL.Interface;
using ORM.PostgresSQL.Model;

namespace ORM.Core
{
    public class DbContext
    {
        private readonly IDatabaseWrapper db;

        public DbContext(IDatabaseWrapper db)
        {
            this.db = db;
        }

        public T Add<T>(T entity) where T : class, new()
        {
            TableModel table = new TableModel(typeof(T));
            List<ColumnModel> columns = table.Columns;
            Dictionary<string, object> columnValues =
                columns.ToDictionary(column => column.ColumnName, column => column.GetValue(entity));

            if (table.PrimaryKey.IsAutoIncrement)
                columnValues.Remove(table.PrimaryKey.ColumnName);

            var result = db.Insert(table.Name, columnValues);

            PostgresTableConverter converter = new PostgresTableConverter();
            return converter.DataTableToObject<T>(result);
        }
        
        public T Update<T>(T entity) where T : class, new()
        {
            TableModel table = new TableModel(typeof(T));
            List<ColumnModel> columns = table.Columns;
            Dictionary<string, object> columnValues =
                columns.ToDictionary(column => column.ColumnName, column => column.GetValue(entity));

            CustomExpression expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                table.PrimaryKey.GetValue(entity));
            
            var result = db.Update(table.Name, columnValues, expression);

            PostgresTableConverter converter = new PostgresTableConverter();
            return converter.DataTableToObject<T>(result);
        }
        
        
    }
}