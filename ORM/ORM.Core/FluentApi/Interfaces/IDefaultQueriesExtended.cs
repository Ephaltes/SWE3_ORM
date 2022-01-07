namespace ORM.Core.FluentApi.Interfaces;

public interface IDefaultQueriesExtended<T> : IDefaultQueries<T>
{
    /// <summary>
    /// Executes the query.
    /// </summary>
    /// <param name="dbContext">the dbcontext</param>
    /// <typeparam name="T">Entity to be executed against</typeparam>
    /// <returns>returns a list of objects T</returns>
    public IReadOnlyCollection<T> Execute(IDbContext dbContext);
}