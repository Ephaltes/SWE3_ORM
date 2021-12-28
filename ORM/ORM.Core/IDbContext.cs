using ORM.PostgresSQL.Model;

namespace ORM.Core;

public interface IDbContext
{
    T Add<T>(T entity) where T : class, new();
    T Update<T>(T entity) where T : class, new();
    T Get<T>(object id) where T : class, new();
    IReadOnlyCollection<T> GetAll<T>(CustomExpression expression) where T : class, new();
    void Delete<T>(object id) where T : class, new();
}