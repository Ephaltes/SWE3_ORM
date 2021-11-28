using System;
using System.Collections.Generic;
using System.Reflection;

namespace ORM.Cache
{
    // Implements a simple in-memory cache for entities.
    public class Cache : ICache
    {
        private readonly Dictionary<Type, Dictionary<int, object>> _cache =
            new Dictionary<Type, Dictionary<int, object>>();

        public void Add(object entity)
        {
            Type type = entity.GetType();
            int id = GetId(entity);
            if (!_cache.ContainsKey(type))
                _cache.Add(type, new Dictionary<int, object>());
            _cache[type].Add(id, entity);
        }

        public void Remove(object entity)
        {
            Type type = entity.GetType();
            int id = GetId(entity);
            if (_cache.ContainsKey(type))
                _cache[type].Remove(id);
        }

        public void Remove(Type type, int id)
        {
            if (_cache.ContainsKey(type))
                _cache[type].Remove(id);
        }

        public void Remove(Type type)
        {
            _cache.Remove(type);
        }

        public object Get(Type type, int id)
        {
            return _cache.ContainsKey(type) ? _cache[type][id] : null;
        }

        public IEnumerable<object> GetAll(Type type)
        {
            if (_cache.ContainsKey(type))
                return _cache[type].Values;

            return new List<object>();
        }

        public bool Contains(Type type, int id)
        {
            return _cache.ContainsKey(type) && _cache[type].ContainsKey(id);
        }

        public bool Contains(Type type)
        {
            return _cache.ContainsKey(type);
        }

        private int GetId(object entity)
        {
            PropertyInfo idProperty = entity.GetType().GetProperty("Id");

            return (int)idProperty.GetValue(entity);
        }
    }
}