using System.Runtime.CompilerServices;
using ORM.Core.FluentApi.Interfaces;
using ORM.PostgresSQL.Model;

[assembly:InternalsVisibleTo("ORM.Core.Test")]
namespace ORM.Core.FluentApi;

internal class FluentApi<T> : IAndOrQuery<T>, IDefaultQueriesExtended<T> where T: class ,new()
{
    public readonly CustomExpression? CustomExpression = new CustomExpression();
    private bool _isNot;
    private CustomExpression _tempExpression = new CustomExpression("1",CustomOperations.Equals,"1");
    
    internal FluentApi()
    {
        CustomExpression.LeftSide = "1";
        CustomExpression.RightSide = "1";
        CustomExpression.Operator = CustomOperations.Equals;
        CustomExpression.PrependAnd(_tempExpression);
    }
    /// <inheritdoc />
    public IDefaultQueries<T> And()
    {
        _tempExpression = new CustomExpression("1",CustomOperations.Equals,"1");
        CustomExpression.PrependAnd(_tempExpression);

        return this;
    }
    /// <inheritdoc />
    public IDefaultQueries<T> Or()
    {
        _tempExpression = new CustomExpression("1",CustomOperations.Equals,"1");
        CustomExpression.PrependOr(_tempExpression);

        return this;
    }
    /// <inheritdoc />
    public IReadOnlyCollection<T> Execute(IDbContext dbContext)
    {
        return dbContext.GetAll<T>(CustomExpression);
    }
    /// <inheritdoc />
    public IAndOrQuery<T> EqualTo(string field, object value)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.NotEquals : CustomOperations.Equals;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = value;

        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IAndOrQuery<T> GreaterThan(string field, object value)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.LessThanOrEqualTo : CustomOperations.GreaterThan;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = value;
        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IAndOrQuery<T> LessThan(string field, object value)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.GreaterThanOrEqualTo : CustomOperations.LessThan;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = value;
        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IAndOrQuery<T> GreaterThanOrEqualTo(string field, object value)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.LessThan : CustomOperations.GreaterThanOrEqualTo;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = value;
        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IAndOrQuery<T> LessThanOrEqualTo(string field, object value)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.GreaterThan : CustomOperations.LessThanOrEqualTo;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = value;
        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IAndOrQuery<T> Like(string field, object value)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.ContainsNot : CustomOperations.Contains;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = value;
        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IAndOrQuery<T> In(string field, object[] values)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.NotIn : CustomOperations.In;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = values;
        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IDefaultQueries<T> Not()
    {
        _isNot = true;

        return this;
    }
}

public class FluentApi
{
    /// <summary>
    /// EntryPoint for the FluentApi
    /// </summary>
    /// <returns></returns>
    public static IDefaultQueriesExtended<T> Get<T>() where T : class, new()
    {
        return new FluentApi<T>();
    }
}