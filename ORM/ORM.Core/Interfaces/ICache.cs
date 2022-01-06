namespace ORM.Core.Interfaces
{
    /// <summary>
    /// Simple Caching for Objects
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Adds an object with an Id to the cache
        /// </summary>
        /// <param name="entity">an object to be added</param>
        /// <param name="id">the id of the object</param>
        public void Add(object entity, int id);
        /// <summary>
        /// Updates an object in the cache
        /// </summary>
        /// <param name="entity">object to be replaced with current one</param>
        /// <param name="id">id of the object to be replaced</param>

        public void Update(object entity, int id);
        /// <summary>
        /// Removes an object from the cache
        /// </summary>
        /// <param name="entity">removes the object from the cache, object needs to have an Id property</param>

        public void Remove(object entity);
        /// <summary>
        /// removes an object from the cache by type and id
        /// </summary>
        /// <param name="type">type of the object to be removed</param>
        /// <param name="id">id of the object to be removed, Id and type has to match to be removed</param>
        public void Remove(Type type, int id);
        /// <summary>
        /// re,pves all objects with the type from the cache
        /// </summary>
        /// <param name="type">type to be removed</param>
        public void Remove(Type type);
        /// <summary>
        /// Gets an object from the cache by Type and Id
        /// </summary>
        /// <param name="type">type of the object to get</param>
        /// <param name="id">id of the object with type</param>
        /// <returns>An object of type with Id</returns>
        public object? Get(Type type, int id);
        /// <summary>
        /// Gets a list of objects from the cache by Type
        /// </summary>
        /// <param name="type">type to get</param>
        /// <returns>returns a list of objects from type</returns>
        public IEnumerable<object> GetAll(Type type);
        /// <summary>
        /// Check if a type with id is in the cache
        /// </summary>
        /// <param name="type">type to check</param>
        /// <param name="id">id to check</param>
        /// <returns>whether the type with id is in the cache or not</returns>

        public bool Contains(Type type, int id);
        /// <summary>
        /// Check if a type is in the cache
        /// </summary>
        /// <param name="type">type to check</param>
        /// <returns>whether the type is in the cache or not</returns>
        public bool Contains(Type type);
        /// <summary>
        /// Checks if an object in the cache has changed since it was added
        /// </summary>
        /// <param name="entity">object to check</param>
        /// <returns></returns>
        public bool HasChanged(object entity);
    }
}