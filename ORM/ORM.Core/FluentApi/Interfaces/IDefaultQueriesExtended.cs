namespace ORM.Core.FluentApi.Interfaces;

public interface IDefaultQueriesExtended : IDefaultQueries
{
    /// <summary>
    /// Executes the query.
    /// </summary>
    /// <param name="dbContext">the dbcontext</param>
    /// <typeparam name="T">Entity to be executed against</typeparam>
    /// <returns>returns a list of objects T</returns>
    public IReadOnlyCollection<T> Execute<T>(IDbContext dbContext) where T : class, new();
}