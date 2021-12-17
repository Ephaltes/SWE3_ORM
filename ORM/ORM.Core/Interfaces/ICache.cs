namespace ORM.Core.Interfaces
{
    /// <summary>
    /// Simple Caching for Objects
    /// </summary>
    public interface ICache
    {
        //TODO rework caching to work with objects instead of types
        public void Add(object entity, int id);

        public void Remove(object entity);

        public void Remove(Type type, int id);

        public void Remove(Type type);

        public object? Get(Type type, int id);

        public IEnumerable<object> GetAll(Type type);

        public bool Contains(Type type, int id);

        public bool Contains(Type type);
        
        public bool HasChanged(object entity);
    }
}