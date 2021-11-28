using System.Collections;
using System.Data;
using ORM.Cache;
using ORM.Core.Models;
using ORM.PostgresSQL.Interface;
using ORM.PostgresSQL.Model;

namespace ORM.Core
{
    public class DbContext
    {
        private readonly IDatabaseWrapper _db;
        private readonly ICache _cache;

        public DbContext(IDatabaseWrapper db, ICache cache = null)
        {
            _db = db;
            _cache = cache;
        }

        public T Add<T>(T entity) where T : class, new()
        {
            TableModel table = new TableModel(typeof(T));
            List<ColumnModel> columns = table.Columns;
            Dictionary<string, object> columnValues =
                columns.ToDictionary(column => column.ColumnName, column => column.GetValue(entity));

            if (table.ForeignKeys.Count > 0)
                foreach (ColumnModel foreignKey in table.ForeignKeys
                             .Where(x=> x.IsManyToMany == false && x.IsReferenced == false))
                {
                    dynamic? value = foreignKey.GetValue(entity);
                    if (value is not null && value.GetType() == foreignKey.Type)
                        columnValues.Add(foreignKey.ForeignKeyColumnName, value.Id);
                }
            //TODO: add many to many

            if (table.PrimaryKey.IsAutoIncrement)
                columnValues.Remove(table.PrimaryKey.ColumnName);

            DataTable result = _db.Insert(table.Name, columnValues);
            
            return Get<T>(result.Rows[0][table.PrimaryKey.ColumnName]);
        }

        public T Update<T>(T entity) where T : class, new()
        {
            TableModel table = new TableModel(typeof(T));
            List<ColumnModel> columns = table.Columns;
            Dictionary<string, object> columnValues =
                columns.ToDictionary(column => column.ColumnName, column => column.GetValue(entity));

            CustomExpression expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                table.PrimaryKey.GetValue(entity));

            DataTable result = _db.Update(table.Name, columnValues, expression);
            
            return Get<T>(result.Rows[0][table.PrimaryKey.ColumnName]);
        }

        public T Get<T>(object id) where T : class, new()
        {
            TableModel table = new TableModel(typeof(T));

            CustomExpression expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                id);

            DataTable result = _db.Select(table.Name, null, null, null, expression);

            return (T)CreateObject(typeof(T), result.Rows[0]);
        }

        internal object? Get(object id, Type type)
        {
            TableModel table = new TableModel(type);

            CustomExpression expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                id);

            DataTable result = _db.Select(table.Name, null, null, null, expression);

            return CreateObject(type, result.Rows[0]);
        }

        private object? CreateObject(Type type, DataRow row)
        {
            TableModel table = new TableModel(type);
            object? instance = Activator.CreateInstance(type);

            if (instance is null)
                return null;

            foreach (ColumnModel column in table.Columns)
                column.SetValue(instance, column.ConvertToType(row[column.ColumnName]));

            foreach (ColumnModel foreignKeyColumn in table.ForeignKeys)
            {
                if (foreignKeyColumn.IsReferenced)
                {
                    IList list = GetList(foreignKeyColumn, row);

                    foreignKeyColumn.SetValue(instance, list);
                }

                if (foreignKeyColumn.IsManyToMany)
                {
                    int x = 0;
                    foreignKeyColumn.SetValue(instance, null);
                }
                else
                {
                    object foreignKeyValue = row[foreignKeyColumn.ForeignKeyColumnName];
                    object? foreignKeyObject = Get(foreignKeyValue,foreignKeyColumn.Type);

                    foreignKeyColumn.SetValue(instance, foreignKeyObject);
                }
            }

            return instance;
        }

        private IList GetList(ColumnModel column, DataRow dataRow)
        {
            TableModel referencedTable = new TableModel(column.Type.GenericTypeArguments.First());
            IList list = (IList) Activator.CreateInstance(column.Type);
                    
            CustomExpression expression = new CustomExpression(column.ForeignKeyColumnName,
                CustomOperations.Equals,
                dataRow[column.ParentTable.PrimaryKey.ColumnName]);

            DataTable dataTable = _db.Select(referencedTable.Name, null, null, null, expression);

            foreach (DataRow row in dataTable.Rows)
            {
                list.Add(CreateObject(column.Type.GenericTypeArguments.First(), row));
            }
            
            return list;
        }
        
        public void Delete<T>(object id) where T : class, new()
        {
            TableModel table = new TableModel(typeof(T));

            CustomExpression expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                id);

            _db.Delete(table.Name, expression);
        }
        
    }
}