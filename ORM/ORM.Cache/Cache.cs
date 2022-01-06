using System;
using System.Collections.Generic;
using System.Reflection;
using ORM.Core.Interfaces;
using Serilog;

namespace ORM.Cache
{
    /// <summary>
    /// Implements a simple in memory cache
    /// </summary>
    public class Cache : ICache
    {
        protected readonly Dictionary<Type, Dictionary<int, object>> _cache =
            new Dictionary<Type, Dictionary<int, object>>();

        protected readonly ILogger _logger;
        public Cache(ILogger logger)
        {
            _logger = logger;
        }

       /// <inheritdoc />
        public virtual void Add(object entity, int id)
        {
            Type type = entity.GetType();
            _logger.Debug($"Adding/Updating Type {type.FullName} with Id {id} to cache");
            if (!_cache.ContainsKey(type))
                _cache.Add(type, new Dictionary<int, object>());
            _cache[type][id] = entity;
        }
       /// <inheritdoc />
        public virtual void Update(object entity, int id)
        {
            _logger.Debug($"Updating Object with Id {id} to cache");
            Add(entity, id);
        }
       /// <inheritdoc />
        public virtual void Remove(object entity)
        {
            Type type = entity.GetType();
            int id = GetId(entity);
            _logger.Debug($"Removing Type {type.FullName} with Id {id} from cache");
            
            if (_cache.ContainsKey(type))
                _cache[type].Remove(id);
        }
       /// <inheritdoc />
        public virtual void Remove(Type type, int id)
        {
            _logger.Debug($"Removing Type {type.FullName} with Id {id} from cache");
            if (_cache.ContainsKey(type))
                _cache[type].Remove(id);
        }

       /// <inheritdoc />
        public virtual void Remove(Type type)
        {
            _logger.Debug($"Removing Type {type.FullName} from cache");

            _cache.Remove(type);
        }

       /// <inheritdoc />
        public virtual object? Get(Type type, int id)
        {
            _logger.Debug($"Trying to Get Type {type.FullName} with Id {id} from cache");

            
            if (!_cache.ContainsKey(type))
                return null;

            return _cache[type].ContainsKey(id) ? _cache[type][id] : null;
        }

       /// <inheritdoc />
        public virtual IEnumerable<object> GetAll(Type type)
        {
            _logger.Debug($"Get all Type {type.FullName} from cache");

            
            if (_cache.ContainsKey(type))
                return _cache[type].Values;

            return new List<object>();
        }
       /// <inheritdoc />
        public virtual bool Contains(Type type, int id)
        {
            _logger.Debug($"Check if Type {type.FullName} with Id {id} in cache");
            return _cache.ContainsKey(type) && _cache[type].ContainsKey(id);
        }

       /// <inheritdoc />
        public virtual bool Contains(Type type)
        {
            _logger.Debug($"Check if Type {type.FullName} in cache");
            return _cache.ContainsKey(type);
        }
       /// <inheritdoc />
        public virtual bool HasChanged(object entity)
        {
            return true;
        }

        protected virtual int GetId(object entity)
        {
            PropertyInfo idProperty = entity.GetType().GetProperty("Id");

            return (int)idProperty.GetValue(entity);
        }
    }
}