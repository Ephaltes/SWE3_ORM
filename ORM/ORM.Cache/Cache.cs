using System;
using System.Collections.Generic;
using System.Reflection;
using ORM.Core.Interfaces;

namespace ORM.Cache
{
    /// <summary>
    /// Implements a simple in memory cache
    /// </summary>
    public class Cache : ICache
    {
        protected readonly Dictionary<Type, Dictionary<int, object>> _cache =
            new Dictionary<Type, Dictionary<int, object>>();

        /// <summary>
        /// Adds an object with an Id to the cache
        /// </summary>
        /// <param name="entity">an object to be added</param>
        /// <param name="id">the id of the object</param>
        public virtual void Add(object entity, int id)
        {
            Type type = entity.GetType();
            if (!_cache.ContainsKey(type))
                _cache.Add(type, new Dictionary<int, object>());
            _cache[type][id] = entity;
        }
        /// <summary>
        /// Updates an object in the cache
        /// </summary>
        /// <param name="entity">object to be replaced with current one</param>
        /// <param name="id">id of the object to be replaced</param>
        public virtual void Update(object entity, int id)
        {
            Add(entity, id);
        }
        /// <summary>
        /// Removes an object from the cache
        /// </summary>
        /// <param name="entity">removes the object from the cache, object needs to have an Id property</param>
        public virtual void Remove(object entity)
        {
            Type type = entity.GetType();
            int id = GetId(entity);
            if (_cache.ContainsKey(type))
                _cache[type].Remove(id);
        }
        /// <summary>
        /// removes an object from the cache by type and id
        /// </summary>
        /// <param name="type">type of the object to be removed</param>
        /// <param name="id">id of the object to be removed, Id and type has to match to be removed</param>
        public virtual void Remove(Type type, int id)
        {
            if (_cache.ContainsKey(type))
                _cache[type].Remove(id);
        }

        /// <summary>
        /// re,pves all objects with the type from the cache
        /// </summary>
        /// <param name="type">type to be removed</param>
        public virtual void Remove(Type type)
        {
            _cache.Remove(type);
        }

        /// <summary>
        /// Gets an object from the cache by Type and Id
        /// </summary>
        /// <param name="type">type of the object to get</param>
        /// <param name="id">id of the object with type</param>
        /// <returns>An object of type with Id</returns>
        public virtual object? Get(Type type, int id)
        {
            if (!_cache.ContainsKey(type))
                return null;

            return _cache[type].ContainsKey(id) ? _cache[type][id] : null;
        }

        /// <summary>
        /// Gets a list of objects from the cache by Type
        /// </summary>
        /// <param name="type">type to get</param>
        /// <returns>returns a list of objects from type</returns>
        public virtual IEnumerable<object> GetAll(Type type)
        {
            if (_cache.ContainsKey(type))
                return _cache[type].Values;

            return new List<object>();
        }
        /// <summary>
        /// Check if a type with id is in the cache
        /// </summary>
        /// <param name="type">type to check</param>
        /// <param name="id">id to check</param>
        /// <returns>whether the type with id is in the cache or not</returns>

        public virtual bool Contains(Type type, int id)
        {
            return _cache.ContainsKey(type) && _cache[type].ContainsKey(id);
        }

        /// <summary>
        /// Check if a type is in the cache
        /// </summary>
        /// <param name="type">type to check</param>
        /// <returns>whether the type is in the cache or not</returns>
        public virtual bool Contains(Type type)
        {
            return _cache.ContainsKey(type);
        }
        /// <summary>
        /// Checks if an object in the cache has changed since it was added
        /// </summary>
        /// <param name="entity">object to check</param>
        /// <returns></returns>
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