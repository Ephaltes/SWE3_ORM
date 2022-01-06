using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ORM.PostgresSQL.Model;

namespace ORM.PostgresSQL.Test;

public class PostgresSqlProviderBehaviour
{
    [Test]
    public void LoadTablesQuery_Should_ReturnSQL()
    {
        var result = PostgresSqlProvider.LoadTableNamesQuery();
        
        result.Should().Be("SELECT * FROM pg_catalog.pg_tables WHERE schemaname != 'pg_catalog' AND schemaname != 'information_schema'");
    }

    [Test]
    public void InsertQuery_Should_ReturnSQL()
    {
        string tableName = "tableName";
        string keys = "id,name,age,is_active";
        string values = "1,'John',25,true";
        
        var result = PostgresSqlProvider.InsertQuery(tableName,keys,values);
        
        result.Should().Be($"INSERT INTO {tableName} ( {keys} ) VALUES ( {values} ) RETURNING *;");
    }
    
    [Test]
    public void UpdateQuery_Should_ReturnSQL()
    {
        string tableName = "tableName";
        string keyValue = "keyValueClause";
        
        var result = PostgresSqlProvider.UpdateQuery(tableName,keyValue,null);
        
        result.Should().Be($"UPDATE {tableName} SET {keyValue} RETURNING *;");
    }
    
    [Test]
    public void UpdateQuery_WithExpression_Should_ReturnSQL()
    {
        string tableName = "tableName";
        string keyValue = "keyValueClause";
        CustomExpression expression = new CustomExpression("left",CustomOperations.Equals, "right");
        
        
        var result = PostgresSqlProvider.UpdateQuery(tableName,keyValue,expression);
        
        result.Should().Be($"UPDATE {tableName} SET {keyValue} WHERE (left = right) RETURNING *;");
    }

    [Test]
    public void DeleteQuery_Should_ReturnSQL()
    {
        string tableName = "tableName";
        
        var result = PostgresSqlProvider.DeleteQuery(tableName,null);
        
        result.Should().Be($"DELETE FROM {tableName};");
    }
    
    [Test]
    public void DeleteQuery_WithExpression_Should_ReturnSQL()
    {
        string tableName = "tableName";
        CustomExpression expression = new CustomExpression("left",CustomOperations.Equals, "right");
        
        var result = PostgresSqlProvider.DeleteQuery(tableName,expression);
        
        result.Should().Be($"DELETE FROM {tableName} WHERE (left = right);");
    }

    [Test]
    public void SelectQuery_Should_ReturnSQL()
    {
        string tableName = "tableName";
        
        var result = PostgresSqlProvider.SelectQuery(tableName,null,null,null,null,null);
        
        result.Should().Be($"SELECT *  FROM {tableName} ");
        
    }
    
    [Test]
    public void SelectQuery_WithOffsetAndLimit_Should_ReturnSQL()
    {
        string tableName = "tableName";
        int indexStart = 2;
        int maxResults = 10;
        
        var result = PostgresSqlProvider.SelectQuery(tableName,indexStart,maxResults,null,null,null);
        
        result.Should().Be($"SELECT *  FROM {tableName} OFFSET {indexStart} LIMIT {maxResults}");
    }
    
    [Test]
    public void SelectQuery_WithLimit_Should_ReturnSQL()
    {
        string tableName = "tableName";
        int maxResults = 10;
        
        var result = PostgresSqlProvider.SelectQuery(tableName,null,maxResults,null,null,null);
        
        result.Should().Be($"SELECT *  FROM {tableName} LIMIT {maxResults}");
    }
    
    [Test]
    public void SelectQuery_WithReturnFields_Should_ReturnSQL()
    {
        string tableName = "tableName";
        List<string> returnFields = new List<string> {"id","name","age"};
        
        var result = PostgresSqlProvider.SelectQuery(tableName,null,null,returnFields,null,null);
        
        result.Should().Be($"SELECT {string.Join(',',returnFields)} FROM {tableName} ");
        
    }
    
    [Test]
    public void SelectQuery_WithFilter_Should_ReturnSQL()
    {
        string tableName = "tableName";
        CustomExpression filter = new CustomExpression("left",CustomOperations.Equals, "right");
        
        
        var result = PostgresSqlProvider.SelectQuery(tableName,null,null,null,filter,null);
        
        result.Should().Be($"SELECT *  FROM {tableName} WHERE (left = right) ");
        
    }
    
    [Test]
    public void SelectQuery_WithOrder_Should_ReturnSQL()
    {
        string tableName = "tableName";
        DatabaseResultOrder resultOrder = new DatabaseResultOrder("id",CustomOrderDirection.Descending);
        
        
        var result = PostgresSqlProvider.SelectQuery(tableName,null,null,null,null,new DatabaseResultOrder[] {resultOrder});
        
        
        result.Should().Be($"SELECT *  FROM {tableName} ORDER BY id DESC ");
    }
}