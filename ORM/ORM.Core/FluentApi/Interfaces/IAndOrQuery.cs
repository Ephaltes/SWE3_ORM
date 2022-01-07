using ORM.PostgresSQL.Model;

namespace ORM.Core.FluentApi.Interfaces;

public interface IAndOrQuery<T>
{
    /// <summary>
    /// Adds an AND clause to the query.
    /// </summary>
    /// <returns></returns>
    public IDefaultQueries<T> And();
    /// <summary>
    /// Adds an OR clause to the query.
    /// </summary>
    /// <returns></returns>
    public IDefaultQueries<T> Or();
    /// <summary>
    /// Executes the query.
    /// </summary>
    /// <param name="dbContext">the dbcontext</param>
    /// <typeparam name="T">Entity to be executed against</typeparam>
    /// <returns>returns a list of objects T</returns>
    public IReadOnlyCollection<T> Execute(IDbContext dbContext);
}