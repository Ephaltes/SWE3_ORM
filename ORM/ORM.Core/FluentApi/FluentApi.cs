using ORM.Core.FluentApi.Interfaces;
using ORM.PostgresSQL.Model;

namespace ORM.Core.FluentApi;

public class FluentApi : IAndOrQuery, IDefaultQueriesExtended
{
    public readonly CustomExpression? CustomExpression = new CustomExpression();
    private bool _isNot;
    private CustomExpression _tempExpression = new CustomExpression("1",CustomOperations.Equals,"1");
    
    private FluentApi()
    {
        CustomExpression.LeftSide = "1";
        CustomExpression.RightSide = "1";
        CustomExpression.Operator = CustomOperations.Equals;
        CustomExpression.PrependAnd(_tempExpression);
    }
    /// <inheritdoc />
    public IDefaultQueries And()
    {
        _tempExpression = new CustomExpression("1",CustomOperations.Equals,"1");
        CustomExpression.PrependAnd(_tempExpression);

        return this;
    }
    /// <inheritdoc />
    public IDefaultQueries Or()
    {
        _tempExpression = new CustomExpression("1",CustomOperations.Equals,"1");
        CustomExpression.PrependOr(_tempExpression);

        return this;
    }
    /// <inheritdoc />
    public IReadOnlyCollection<T> Execute<T>(IDbContext dbContext) where T : class, new()
    {
        return dbContext.GetAll<T>(CustomExpression);
    }
    /// <inheritdoc />
    public IAndOrQuery EqualTo(string field, object value)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.NotEquals : CustomOperations.Equals;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = value;

        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IAndOrQuery GreaterThan(string field, object value)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.LessThanOrEqualTo : CustomOperations.GreaterThan;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = value;
        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IAndOrQuery LessThan(string field, object value)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.GreaterThanOrEqualTo : CustomOperations.LessThan;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = value;
        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IAndOrQuery GreaterThanOrEqualTo(string field, object value)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.LessThan : CustomOperations.GreaterThanOrEqualTo;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = value;
        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IAndOrQuery LessThanOrEqualTo(string field, object value)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.GreaterThan : CustomOperations.LessThanOrEqualTo;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = value;
        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IAndOrQuery Like(string field, object value)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.ContainsNot : CustomOperations.Contains;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = value;
        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IAndOrQuery In(string field, object[] values)
    {
        _tempExpression.Operator = _isNot ? CustomOperations.NotIn : CustomOperations.In;
        _tempExpression.LeftSide = field;
        _tempExpression.RightSide = values;
        _isNot = false;

        return this;
    }
    /// <inheritdoc />
    public IDefaultQueries Not()
    {
        _isNot = true;

        return this;
    }
    /// <summary>
    /// EntryPoint for the FluentApi
    /// </summary>
    /// <returns></returns>
    public static IDefaultQueriesExtended Get()
    {
        return new FluentApi();
    }
}