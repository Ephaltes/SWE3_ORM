using ORM.PostgresSQL.Model;

namespace ORM.Core.FluentApi.Interfaces;

public interface IAndOrQuery
{
    public IDefaultQueries And();
    public IDefaultQueries Or();
    public IReadOnlyCollection<T> Execute<T>(IDbContext dbContext) where T : class, new();
}