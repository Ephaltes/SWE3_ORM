using System.Collections;
using System.Data;
using ORM.Core.Interfaces;
using ORM.Core.Models;
using ORM.PostgresSQL.Interface;
using ORM.PostgresSQL.Model;
using Serilog;

namespace ORM.Core
{
    public class DbContext : IDbContext
    {
        private readonly ICache _cache;
        private readonly IDatabaseWrapper _db;
        private readonly ILogger _logger;

        public DbContext(IDatabaseWrapper db, ICache cache, ILogger logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        /// <inheritdoc />
        public T Add<T>(T entity) where T : class, new()
        {
            _logger.Information($"adding Entity {typeof(T).FullName}");

            if (_cache is not null && !_cache.HasChanged(entity)) return entity;

            TableModel table = new TableModel(typeof(T));
            List<ColumnModel> columns = table.Columns;
            Dictionary<string, object> columnValues =
                columns.ToDictionary(column => column.ColumnName, column => column.GetValue(entity));


            foreach (ColumnModel foreignKey in table.ForeignKeys
                         .Where(x => x.IsManyToMany == false && x.IsReferenced == false))
            {
                _logger.Information("Entity has 1:1 foreignKey");
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
                _logger.Information("Entity has ManyToMany foreignKey");
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

            //update references when List
            foreach (ColumnModel foreignKey in table.ForeignKeys.Where(x => x.IsReferenced && !x.IsManyToMany))
            {
                dynamic? value = foreignKey.GetValue(entity);
                foreach (dynamic? item in value)
                    if (item is not null && value.GetType() == foreignKey.Type)
                    {
                        dynamic? updatedReference = Get(item.Id, foreignKey.Type.GenericTypeArguments.First(), null,
                            true);
                        _cache?.Update(updatedReference, item.Id);
                    }
            }

            //update references when not List
            foreach (ColumnModel foreignKey in table.ForeignKeys.Where(x => x.IsReferenced == false
                                                                            && !x.IsManyToMany))
            {
                dynamic? value = foreignKey.GetValue(entity);
                if (value is not null && value.GetType() == foreignKey.Type)
                {
                    dynamic? updatedReference = Get(value.Id, foreignKey.Type, null, true);
                    _cache?.Update(updatedReference, value.Id);
                }
            }

            insertedEntity = Get<T>(result.Rows[0][table.PrimaryKey.ColumnName]);
            _cache?.Update(insertedEntity, Convert.ToInt32(result.Rows[0][table.PrimaryKey.ColumnName]));
            _logger.Information($"Added Entity {typeof(T).FullName} successfully");
            entity = insertedEntity; // so that the entity is updated with the new id

            return insertedEntity;
        }

        /// <inheritdoc />
        public T Update<T>(T entity) where T : class, new()
        {
            _logger.Information($"Updating Entity {typeof(T).FullName}");
            TableModel table = new TableModel(typeof(T));
            List<ColumnModel> columns = table.Columns;
            Dictionary<string, object> columnValues =
                columns.ToDictionary(column => column.ColumnName, column => column.GetValue(entity));

            CustomExpression? expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                table.PrimaryKey.GetValue(entity));

            foreach (ColumnModel foreignKey in table.ForeignKeys
                         .Where(x => x.IsManyToMany == false && x.IsReferenced == false))
            {
                _logger.Information("Updating Entity has 1:1 foreignKey");
                dynamic? value = foreignKey.GetValue(entity);
                if (value is not null && value.GetType() == foreignKey.Type)
                    columnValues.Add(foreignKey.ForeignKeyColumnName, value.Id);
            }

            DataTable result = _db.Update(table.Name, columnValues, expression);

            //Updating n:m
            foreach (ColumnModel foreignKey in table.ForeignKeys
                         .Where(x => x.IsManyToMany))
            {
                _logger.Information("Updating Entity has ManyToMany foreignKey");

                expression = new CustomExpression(foreignKey.ColumnName, CustomOperations.Equals,
                    table.PrimaryKey.GetValue(entity));

                _db.Delete(foreignKey.ForeignKeyTableName, expression);

                dynamic? value = foreignKey.GetValue(entity);

                if (value is null || value.Count <= 0 || value.GetType() != foreignKey.Type)
                    continue;

                foreach (dynamic? item in value)
                {
                    _db.Insert(foreignKey.ForeignKeyTableName, new Dictionary<string, object>
                    {
                        { foreignKey.ColumnName, table.PrimaryKey.GetValue(entity) },
                        { foreignKey.ForeignKeyColumnName, item.Id }
                    });
                    dynamic? updatedReference = Get(item.Id, foreignKey.Type.GenericTypeArguments.First(), null, true);
                    _cache?.Update(updatedReference, item.Id);
                }
            }

            //update references when List
            foreach (ColumnModel foreignKey in table.ForeignKeys.Where(x => x.IsReferenced && !x.IsManyToMany))
            {
                dynamic? value = foreignKey.GetValue(entity);
                foreach (dynamic? item in value)
                    if (item is not null && value.GetType() == foreignKey.Type)
                    {
                        dynamic? updatedReference = Get(item.Id, foreignKey.Type.GenericTypeArguments.First(), null,
                            true);
                        _cache?.Update(updatedReference, item.Id);
                    }
            }

            //update references when not List
            foreach (ColumnModel foreignKey in table.ForeignKeys.Where(x => x.IsReferenced == false
                                                                            && !x.IsManyToMany))
            {
                dynamic? value = foreignKey.GetValue(entity);
                if (value is not null && value.GetType() == foreignKey.Type)
                {
                    dynamic? updatedReference = Get(value.Id, foreignKey.Type, null, true);
                    _cache?.Update(updatedReference, value.Id);
                }
            }

            _logger.Information($"Updating Entity {typeof(T).FullName} successfully");

            return Get<T>(result.Rows[0][table.PrimaryKey.ColumnName]);
        }
        /// <inheritdoc />
        public T Get<T>(object id) where T : class, new()
        {
            _logger.Information($"Get Entity {typeof(T).FullName}");
            TableModel table = new TableModel(typeof(T));
            IDictionary<Type, Dictionary<int, object>>? localCache = null;

            CustomExpression? expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                id);

            if (Convert.ToInt32(id) == 5)
                expression = null;

            DataTable result = _db.Select(table.Name, null, null, null, expression);

            _logger.Information($"Getting Entity {typeof(T).FullName} successfully");

            return (T)CreateObject(typeof(T), result.Rows[0], localCache);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<T> GetAll<T>(CustomExpression? expression) where T : class, new()
        {
            _logger.Information($"Getting Entity List of {typeof(T).FullName}");
            IDictionary<Type, Dictionary<int, object>>? localCache = null;

            TableModel table = new TableModel(typeof(T));

            DataTable result = _db.Select(table.Name, null, null, null, expression);

            _logger.Information($"Getting Entity List of {typeof(T).FullName} successfully");

            return result.Rows.Cast<DataRow>().Select(row => (T)CreateObject(typeof(T), row, localCache)).ToList();
        }
        /// <inheritdoc />
        public void Delete<T>(object id) where T : class, new()
        {
            _logger.Information($"Delete Object of Entity {typeof(T).FullName}");
            TableModel table = new TableModel(typeof(T));

            CustomExpression? expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                id);

            _db.Delete(table.Name, expression);
            _logger.Information($"Deleting Entity {typeof(T).FullName} successfully");
        }

        internal object? Get(object? id, Type type, IDictionary<Type, Dictionary<int, object>>? localCache = null,
            bool forceUpdate = false)
        {
            if (id is null or DBNull)
                return null;

            _logger.Information($"Get Entity {type.FullName} with id {id} force update {forceUpdate}");

            TableModel table = new TableModel(type);

            object? retVal = SearchCaches(type, Convert.ToInt32(id), localCache);

            if (retVal is not null && !forceUpdate)
                return retVal;

            CustomExpression? expression = new CustomExpression(table.PrimaryKey.ColumnName, CustomOperations.Equals,
                id);

            DataTable result = _db.Select(table.Name, null, null, null, expression);

            _logger.Information($"Get Entity {type.FullName} with id {id} force update {forceUpdate} successfully");

            return CreateObject(type, result.Rows[0], localCache);
        }

        private object? CreateObject(Type type, DataRow row, IDictionary<Type, Dictionary<int, object>>? localCache)
        {
            TableModel table = new TableModel(type);
            int id = Convert.ToInt32(row[table.PrimaryKey.ColumnName]);

            _logger.Information($"Create Object for Entity {type.FullName} with id {id}");

            object? instance = SearchCaches(type, id, localCache);

            if (instance is null)
            {
                _logger.Information($"Entity {type.FullName} with id {id} not found in cache");
                instance = Activator.CreateInstance(type);

                localCache ??= new Dictionary<Type, Dictionary<int, object>>();

                if (!localCache.ContainsKey(type))
                    localCache.Add(type, new Dictionary<int, object>());

                localCache[type][id] = instance;
            }

            foreach (ColumnModel column in table.Columns)
                column.SetValue(instance, column.ConvertToType(row[column.ColumnName]));

            foreach (ColumnModel foreignKeyColumn in table.ForeignKeys)
            {
                if (foreignKeyColumn.IsReferenced)
                {
                    _logger.Information($"Entity {type.FullName} with id {id} has 1:n foreignKey");

                    // 1 : n foreign key
                    IList list = GetList(foreignKeyColumn, row, localCache);

                    foreignKeyColumn.SetValue(instance, list);

                    continue;
                }

                if (foreignKeyColumn.IsManyToMany)
                {
                    _logger.Information($"Entity {type.FullName} with id {id} has m:n foreignKey");

                    IList list = GetList(foreignKeyColumn, row, localCache);
                    foreignKeyColumn.SetValue(instance, list);

                    continue;
                }

                //1 : 1 foreign Key
                _logger.Information($"Entity {type.FullName} with id {id} has 1:1 foreignkey");

                object foreignKeyValue = row[foreignKeyColumn.ForeignKeyColumnName];
                object? foreignKeyObject = Get(foreignKeyValue, foreignKeyColumn.Type, localCache);

                foreignKeyColumn.SetValue(instance, foreignKeyObject);
            }

            if (_cache is not null)
            {
                _logger.Information($"Entity {type.FullName} with id {id} added to cache");
                _cache.Add(instance, id);
            }


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
                CustomExpression? expression = new CustomExpression(column.ColumnName,
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
                CustomExpression? expression = new CustomExpression(column.ForeignKeyColumnName,
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