using System;
using System.Collections.Generic;
using System.Reflection;
using ORM.Core.Interfaces;

namespace ORM.Cache
{
    // Implements a simple in-memory cache for entities.
    public class Cache : ICache
    {
        protected readonly Dictionary<Type, Dictionary<int, object>> _cache =
            new Dictionary<Type, Dictionary<int, object>>();

        public virtual void Add(object entity, int id)
        {
            Type type = entity.GetType();
            if (!_cache.ContainsKey(type))
                _cache.Add(type, new Dictionary<int, object>());
            _cache[type][id] = entity;
        }

        public virtual void Remove(object entity)
        {
            Type type = entity.GetType();
            int id = GetId(entity);
            if (_cache.ContainsKey(type))
                _cache[type].Remove(id);
        }

        public virtual void Remove(Type type, int id)
        {
            if (_cache.ContainsKey(type))
                _cache[type].Remove(id);
        }

        public virtual void Remove(Type type)
        {
            _cache.Remove(type);
        }

        public  virtual object? Get(Type type, int id)
        {
            if (!_cache.ContainsKey(type))
                return null;
            
            return _cache[type].ContainsKey(id) ? _cache[type][id] : null;
        }

        public virtual IEnumerable<object> GetAll(Type type)
        {
            if (_cache.ContainsKey(type))
                return _cache[type].Values;

            return new List<object>();
        }

        public virtual bool Contains(Type type, int id)
        {
            return _cache.ContainsKey(type) && _cache[type].ContainsKey(id);
        }

        public virtual bool Contains(Type type)
        {
            return _cache.ContainsKey(type);
        }
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