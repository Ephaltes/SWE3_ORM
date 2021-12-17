namespace ORM.Core.FluentApi.Interfaces;

public interface IDefaultQueries
{
    public IAndOrQuery EqualTo(string field, object value);
    public IAndOrQuery GreaterThan(string field, object value);
    public IAndOrQuery LessThan(string field, object value);
    public IAndOrQuery GreaterThanOrEqualTo(string field, object value);
    public IAndOrQuery LessThanOrEqualTo(string field, object value);
    public IAndOrQuery Like(string field,object value);
    public IAndOrQuery In(string field, object[] values);
    public IDefaultQueries Not();
}