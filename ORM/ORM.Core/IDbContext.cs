using ORM.PostgresSQL.Model;

namespace ORM.Core;

public interface IDbContext
{
    /// <summary>
    ///     Adds an entity to the database
    /// </summary>
    /// <param name="entity">entity to add to the database</param>
    /// <typeparam name="T">Type of the entity to add</typeparam>
    /// <returns></returns>
    T Add<T>(T entity) where T : class, new();
    /// <summary>
    ///     Updates an entity in the database
    /// </summary>
    /// <param name="entity">entity to update</param>
    /// <typeparam name="T">Type of the entity</typeparam>
    /// <returns></returns>
    T Update<T>(T entity) where T : class, new();
    /// <summary>
    ///     Gets an entity from the database
    /// </summary>
    /// <param name="id">id of the entity to get</param>
    /// <typeparam name="T">type to get</typeparam>
    /// <returns>Returns an object of T or null if not found</returns>
    T Get<T>(object id) where T : class, new();
    /// <summary>
    ///     Returns a list of entities of type T
    /// </summary>
    /// <param name="expression">Filter</param>
    /// <typeparam name="T">Type of list to get</typeparam>
    /// <returns>Returns a list of entities of type T</returns>
    IReadOnlyCollection<T> GetAll<T>(CustomExpression? expression) where T : class, new();
    
    /// <summary>
    ///     Deletes an object in the database
    /// </summary>
    /// <param name="id">id to delete</param>
    /// <typeparam name="T">type of the object to delete</typeparam>
    void Delete<T>(object id) where T : class, new();
}