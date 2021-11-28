using System.Collections;
using System.Data;
using ORM.Core.Converter;
using ORM.Core.Models;
using ORM.PostgresSQL.Interface;
using ORM.PostgresSQL.Model;

namespace ORM.Core
{
    public class DbContext
    {
        private readonly ITableConverter _converter;
        private readonly IDatabaseWrapper db;

        public DbContext(IDatabaseWrapper db,
            ITableConverter converter)
        {
            this.db = db;
            _converter = converter;
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

            DataTable result = db.Insert(table.Name, columnValues);

            return _converter.DataTableToObject<T>(result);
        }

        public T Update<T>(T entity) where T : class, new()
        {
            TableModel table = new TableModel(typeof(T));
            List<ColumnModel> columns = table.Columns;
            Dictionary<string, object> columnValues =
                columns.ToDictionary(column => column.ColumnName, column => column.GetValue(entity));

            CustomExpression expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                table.PrimaryKey.GetValue(entity));

            DataTable result = db.Update(table.Name, columnValues, expression);

            return _converter.DataTableToObject<T>(result);
        }

        public T Get<T>(object id) where T : class, new()
        {
            TableModel table = new TableModel(typeof(T));

            CustomExpression expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                id);

            DataTable result = db.Select(table.Name, null, null, null, expression);

            return (T)CreateObject(typeof(T), result);
        }

        internal object? Get(object id, Type type)
        {
            TableModel table = new TableModel(type);

            CustomExpression expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                id);

            DataTable result = db.Select(table.Name, null, null, null, expression);

            return CreateObject(type, result);
        }

        private object? CreateObject(Type type, DataTable mainResult)
        {
            TableModel table = new TableModel(type);
            object? instance = Activator.CreateInstance(type);

            if (instance is null)
                return null;

            foreach (ColumnModel column in table.Columns)
                column.SetValue(instance, column.ConvertToType(mainResult.Rows[0][column.ColumnName]));

            foreach (ColumnModel foreignKeyColumn in table.ForeignKeys)
            {
                if (foreignKeyColumn.IsReferenced)
                {
                    TableModel referencedTable = new TableModel(foreignKeyColumn.Type.GenericTypeArguments.First());
                    
                    CustomExpression expression = new CustomExpression(foreignKeyColumn.ForeignKeyColumnName,
                        CustomOperations.Equals,
                        mainResult.Rows[0][foreignKeyColumn.ParentTable.PrimaryKey.ColumnName]);

                    DataTable dataTable = db.Select(referencedTable.Name, null, null, null, expression);

                    IList list = GetList(foreignKeyColumn.Type, dataTable);

                    foreignKeyColumn.SetValue(instance, list);
                }

                if (foreignKeyColumn.IsManyToMany)
                {
                    int x = 0;
                    foreignKeyColumn.SetValue(instance, null);
                }
                else
                {
                    object? foreignKeyObject = Get(mainResult.Rows[0][foreignKeyColumn.ForeignKeyColumnName],
                        foreignKeyColumn.Type);

                    foreignKeyColumn.SetValue(instance, foreignKeyObject);
                }
            }

            return instance;
        }

        private IList GetList(Type t, DataTable result)
        {
            TableModel table = new TableModel(t.GenericTypeArguments.First()); //Table of Type T
            IList listInstance =(IList) Activator.CreateInstance(t);

            if (listInstance is null)
                return null;

            foreach (DataRow row in result.Rows)
            {
                object? instance = Activator.CreateInstance(t.GenericTypeArguments.First()); //Creates instance of T
                foreach (ColumnModel column in table.Columns)
                    column.SetValue(instance, column.ConvertToType(row[column.ColumnName]));

                foreach (var foreignKeyColumn in table.ForeignKeys)
                {
                    
                }
                
                listInstance.Add(instance);
            }

            return listInstance;
        }
    }
}