namespace ORM.Core.FluentApi.Interfaces;

public interface IDefaultQueries
{
    /// <summary>
    /// Adds an Equal statement to the query
    /// </summary>
    /// <param name="field">fieldName to check against</param>
    /// <param name="value">the value to Check</param>
    /// <returns></returns>
    public IAndOrQuery EqualTo(string field, object value);
    /// <summary>
    /// Adds an Greather Than statement to the query
    /// </summary>
    /// <param name="field">fieldName to check against</param>
    /// <param name="value">the value to Check</param>
    /// <returns></returns>
    public IAndOrQuery GreaterThan(string field, object value);
    /// <summary>
    /// Adds an Less Than statement to the query
    /// </summary>
    /// <param name="field">fieldName to check against</param>
    /// <param name="value">the value to Check</param>
    /// <returns></returns>
    public IAndOrQuery LessThan(string field, object value);
    /// <summary>
    /// Adds a Greather Than or Equal To statement to the query
    /// </summary>
    /// <param name="field">fieldName to check against</param>
    /// <param name="value">the value to Check</param>
    /// <returns></returns>
    public IAndOrQuery GreaterThanOrEqualTo(string field, object value);
    /// <summary>
    /// Adds a Less Than or Equal To statement to the query
    /// </summary>
    /// <param name="field">fieldName to check against</param>
    /// <param name="value">the value to Check</param>
    /// <returns></returns>
    public IAndOrQuery LessThanOrEqualTo(string field, object value);
    /// <summary>
    /// Adds a Like statement to the query
    /// </summary>
    /// <param name="field">fieldName to check against</param>
    /// <param name="value">the value to Check</param>
    /// <returns></returns>
    public IAndOrQuery Like(string field,object value);
    /// <summary>
    /// Adds a In statement to the query
    /// </summary>
    /// <param name="field">fieldName to check against</param>
    /// <param name="values">the values to Check</param>
    /// <returns></returns>
    public IAndOrQuery In(string field, object[] values);
    /// <summary>
    /// Negates the following statement
    /// </summary>
    /// <returns></returns>
    public IDefaultQueries Not();
}