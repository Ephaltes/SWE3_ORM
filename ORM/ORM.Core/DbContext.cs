using System.Collections;
using System.Data;
using ORM.Core.Interfaces;
using ORM.Core.Models;
using ORM.PostgresSQL.Interface;
using ORM.PostgresSQL.Model;

namespace ORM.Core
{
    public class DbContext : IDbContext
    {
        private readonly ICache _cache;
        private readonly IDatabaseWrapper _db;

        public DbContext(IDatabaseWrapper db, ICache cache)
        {
            _db = db;
            _cache = cache;
        }

        /// <inheritdoc />

        public T Add<T>(T entity) where T : class, new()
        {
            if (_cache is not null && !_cache.HasChanged(entity)) return entity;

            TableModel table = new TableModel(typeof(T));
            List<ColumnModel> columns = table.Columns;
            Dictionary<string, object> columnValues =
                columns.ToDictionary(column => column.ColumnName, column => column.GetValue(entity));


            foreach (ColumnModel foreignKey in table.ForeignKeys
                         .Where(x => x.IsManyToMany == false && x.IsReferenced == false))
            {
                dynamic? value = foreignKey.GetValue(entity);
                if (value is not null && value.GetType() == foreignKey.Type)
                    columnValues.Add(foreignKey.ForeignKeyColumnName, value.Id);
            }


            if (table.PrimaryKey.IsAutoIncrement)
                columnValues.Remove(table.PrimaryKey.ColumnName);

            DataTable result = _db.Insert(table.Name, columnValues);

            T insertedEntity = Get<T>(result.Rows[0][table.PrimaryKey.ColumnName]);


            foreach (ColumnModel foreignKey in table.ForeignKeys
                         .Where(x => x.IsManyToMany))
            {
                dynamic? value = foreignKey.GetValue(entity);

                if (value is null || value.Count <= 0 || value.GetType() != foreignKey.Type)
                    continue;

                foreach (dynamic? item in value)
                {
                    _db.Insert(foreignKey.ForeignKeyTableName, new Dictionary<string, object>
                    {
                        { foreignKey.ColumnName, table.PrimaryKey.GetValue(insertedEntity) },
                        { foreignKey.ForeignKeyColumnName, item.Id }
                    });
                    dynamic? updatedReference = Get(item.Id, foreignKey.Type.GenericTypeArguments.First(), null, true);
                    _cache?.Update(updatedReference, item.Id);
                }
            }

            insertedEntity = Get<T>(result.Rows[0][table.PrimaryKey.ColumnName]);
            _cache?.Update(insertedEntity, Convert.ToInt32(result.Rows[0][table.PrimaryKey.ColumnName]));

            return insertedEntity;
        }

        /// <inheritdoc />

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
        /// <inheritdoc />

        public T Get<T>(object id) where T : class, new()
        {
            TableModel table = new TableModel(typeof(T));
            IDictionary<Type, Dictionary<int, object>>? localCache = null;

            CustomExpression expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                id);

            DataTable result = _db.Select(table.Name, null, null, null, expression);

            return (T)CreateObject(typeof(T), result.Rows[0], localCache);
        }

        /// <inheritdoc />

        public IReadOnlyCollection<T> GetAll<T>(CustomExpression expression) where T : class, new()
        {
            IDictionary<Type, Dictionary<int, object>>? localCache = null;

            TableModel table = new TableModel(typeof(T));

            DataTable result = _db.Select(table.Name, null, null, null, expression);

            return result.Rows.Cast<DataRow>().Select(row => (T)CreateObject(typeof(T), row, localCache)).ToList();
        }
        /// <inheritdoc />

        public void Delete<T>(object id) where T : class, new()
        {
            TableModel table = new TableModel(typeof(T));

            CustomExpression expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                id);

            _db.Delete(table.Name, expression);
        }

        internal object? Get(object? id, Type type, IDictionary<Type, Dictionary<int, object>>? localCache = null,
            bool forceUpdate = false)
        {
            if (id is null or DBNull)
                return null;

            TableModel table = new TableModel(type);

            object? retVal = SearchCaches(type, Convert.ToInt32(id), localCache);

            if (retVal is not null && !forceUpdate)
                return retVal;

            CustomExpression expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                id);

            DataTable result = _db.Select(table.Name, null, null, null, expression);

            return CreateObject(type, result.Rows[0], localCache);
        }

        private object? CreateObject(Type type, DataRow row, IDictionary<Type, Dictionary<int, object>>? localCache)
        {
            TableModel table = new TableModel(type);
            object? instance = SearchCaches(type, Convert.ToInt32(row[table.PrimaryKey.ColumnName]), localCache);

            if (instance is null)
            {
                instance = Activator.CreateInstance(type);

                localCache ??= new Dictionary<Type, Dictionary<int, object>>();

                if (!localCache.ContainsKey(type))
                    localCache.Add(type, new Dictionary<int, object>());

                int id = Convert.ToInt32(row[table.PrimaryKey.ColumnName]);
                localCache[type][id] = instance;
            }

            foreach (ColumnModel column in table.Columns)
                column.SetValue(instance, column.ConvertToType(row[column.ColumnName]));

            foreach (ColumnModel foreignKeyColumn in table.ForeignKeys)
            {
                if (foreignKeyColumn.IsReferenced)
                {
                    // 1 : n foreign key
                    IList list = GetList(foreignKeyColumn, row, localCache);

                    foreignKeyColumn.SetValue(instance, list);

                    continue;
                }

                if (foreignKeyColumn.IsManyToMany)
                {
                    IList list = GetList(foreignKeyColumn, row, localCache);
                    foreignKeyColumn.SetValue(instance, list);

                    continue;
                }

                //1 : 1 foreign Key
                object foreignKeyValue = row[foreignKeyColumn.ForeignKeyColumnName];
                object? foreignKeyObject = Get(foreignKeyValue, foreignKeyColumn.Type, localCache);

                foreignKeyColumn.SetValue(instance, foreignKeyObject);
            }

            if (_cache is not null)
                _cache.Add(instance, Convert.ToInt32(row[table.PrimaryKey.ColumnName]));

            return instance;
        }
        private object? SearchCaches(Type type, int id, IDictionary<Type, Dictionary<int, object>>? localCache)
        {
            if (_cache is not null && _cache.Contains(type, id))
                return _cache.Get(type, id);

            if (localCache is not null && localCache.ContainsKey(type) && localCache[type].ContainsKey(id))
                return localCache[type][id];

            return null;
        }


        private IList GetList(ColumnModel column, DataRow dataRow,
            IDictionary<Type, Dictionary<int, object>>? localCache)
        {
            Type tableType = column.Type.GenericTypeArguments.First();
            TableModel referencedTable = new TableModel(tableType);
            IList list = (IList)Activator.CreateInstance(column.Type);

            if (column.IsManyToMany)
            {
                CustomExpression expression = new CustomExpression(column.ColumnName,
                    CustomOperations.Equals,
                    dataRow[column.ParentTable.PrimaryKey.ColumnName]);

                DataTable result = _db.Select(column.ForeignKeyTableName, null, null, null, expression);

                foreach (DataRow row in result.Rows)
                {
                    object? instance = Get(row[column.ForeignKeyColumnName], tableType, localCache);
                    list.Add(instance);
                }
            }
            else
            {
                CustomExpression expression = new CustomExpression(column.ForeignKeyColumnName,
                    CustomOperations.Equals,
                    dataRow[column.ParentTable.PrimaryKey.ColumnName]);

                DataTable dataTable = _db.Select(referencedTable.Name, null, null, null, expression);

                foreach (DataRow row in dataTable.Rows)
                    list.Add(CreateObject(column.Type.GenericTypeArguments.First(), row, localCache));
            }

            return list;
        }

    }
}