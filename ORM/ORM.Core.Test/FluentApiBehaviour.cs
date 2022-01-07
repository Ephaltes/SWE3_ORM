using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using ORM.Core.Test.Entities;
using ORM.PostgresSQL;
using ORM.PostgresSQL.Interface;
using ORM.PostgresSQL.Model;

namespace ORM.Core.Test;

public class FluentApiBehaviour
{
    private IDbContext _dbContext;
    [SetUp]
    public void Setup()
    {
        _dbContext = A.Fake<IDbContext>();
    }

    [Test]
    public void FluentApi_Should_ReturnInExpression()
    {
        string fieldName = "fieldName";
        object [] fieldValue = { "fieldValue" };
        
        var result =(FluentApi.FluentApi<Students>) FluentApi.FluentApi<Students>.Get().In(fieldName, fieldValue);
        
        ((CustomExpression)result.CustomExpression.LeftSide).LeftSide.Should().Be(fieldName);
        ((CustomExpression)result.CustomExpression.LeftSide).RightSide.Should().Be(fieldValue);
        ((CustomExpression)result.CustomExpression.LeftSide).Operator.Should().Be(CustomOperations.In);
    }
    
    [Test]
    public void FluentApi_Should_ReturnEqualExpression()
    {
        string fieldName = "fieldName";
        object  fieldValue = "fieldValue" ;

        var result =(FluentApi.FluentApi<Students>) FluentApi.FluentApi<Students>.Get().EqualTo(fieldName, fieldValue);
        
        ((CustomExpression)result.CustomExpression.LeftSide).LeftSide.Should().Be(fieldName);
        ((CustomExpression)result.CustomExpression.LeftSide).RightSide.Should().Be(fieldValue);
        ((CustomExpression)result.CustomExpression.LeftSide).Operator.Should().Be(CustomOperations.Equals);
    }
    
    [Test]
    public void FluentApi_Should_ReturnGreaterThanExpression()
    {
        string fieldName = "fieldName";
        object  fieldValue = "fieldValue" ;

        var result =(FluentApi.FluentApi<Students>) FluentApi.FluentApi<Students>.Get().GreaterThan(fieldName, fieldValue);
        
        ((CustomExpression)result.CustomExpression.LeftSide).LeftSide.Should().Be(fieldName);
        ((CustomExpression)result.CustomExpression.LeftSide).RightSide.Should().Be(fieldValue);
        ((CustomExpression)result.CustomExpression.LeftSide).Operator.Should().Be(CustomOperations.GreaterThan);
    }
    
    [Test]
    public void FluentApi_Should_ReturnLessThanExpression()
    {
        string fieldName = "fieldName";
        object  fieldValue = "fieldValue" ;

        var result =(FluentApi.FluentApi<Students>) FluentApi.FluentApi<Students>.Get().LessThan(fieldName, fieldValue);
        
        ((CustomExpression)result.CustomExpression.LeftSide).LeftSide.Should().Be(fieldName);
        ((CustomExpression)result.CustomExpression.LeftSide).RightSide.Should().Be(fieldValue);
        ((CustomExpression)result.CustomExpression.LeftSide).Operator.Should().Be(CustomOperations.LessThan);
    }
    
    [Test]
    public void FluentApi_Should_ReturnGreaterThanOrEqualExpression()
    {
        string fieldName = "fieldName";
        object  fieldValue = "fieldValue" ;

        var result =(FluentApi.FluentApi<Students>) FluentApi.FluentApi<Students>.Get().GreaterThanOrEqualTo(fieldName, fieldValue);
        
        ((CustomExpression)result.CustomExpression.LeftSide).LeftSide.Should().Be(fieldName);
        ((CustomExpression)result.CustomExpression.LeftSide).RightSide.Should().Be(fieldValue);
        ((CustomExpression)result.CustomExpression.LeftSide).Operator.Should().Be(CustomOperations.GreaterThanOrEqualTo);
    }
    
    [Test]
    public void FluentApi_Should_ReturnLessThanOrEqualExpression()
    {
        string fieldName = "fieldName";
        object  fieldValue = "fieldValue" ;

        var result =(FluentApi.FluentApi<Students>) FluentApi.FluentApi<Students>.Get().LessThanOrEqualTo(fieldName, fieldValue);
        
        ((CustomExpression)result.CustomExpression.LeftSide).LeftSide.Should().Be(fieldName);
        ((CustomExpression)result.CustomExpression.LeftSide).RightSide.Should().Be(fieldValue);
        ((CustomExpression)result.CustomExpression.LeftSide).Operator.Should().Be(CustomOperations.LessThanOrEqualTo);
    }
    
    [Test]
    public void FluentApi_Should_ReturnLikeExpression()
    {
        string fieldName = "fieldName";
        object  fieldValue = "fieldValue" ;

        var result =(FluentApi.FluentApi<Students>) FluentApi.FluentApi<Students>.Get().Like(fieldName, fieldValue);
        
        ((CustomExpression)result.CustomExpression.LeftSide).LeftSide.Should().Be(fieldName);
        ((CustomExpression)result.CustomExpression.LeftSide).RightSide.Should().Be(fieldValue);
        ((CustomExpression)result.CustomExpression.LeftSide).Operator.Should().Be(CustomOperations.Contains);
    }
    
    [Test]
    public void FluentApi_Should_ReturnNotEqualExpression()
    {
        string fieldName = "fieldName";
        object  fieldValue = "fieldValue" ;

        var result =(FluentApi.FluentApi<Students>) FluentApi.FluentApi<Students>.Get().Not().EqualTo(fieldName, fieldValue);
        
        ((CustomExpression)result.CustomExpression.LeftSide).LeftSide.Should().Be(fieldName);
        ((CustomExpression)result.CustomExpression.LeftSide).RightSide.Should().Be(fieldValue);
        ((CustomExpression)result.CustomExpression.LeftSide).Operator.Should().Be(CustomOperations.NotEquals);
    }
    
    [Test]
    public void FluentApi_Should_ReturnEqualAndEqualExpression()
    {
        string fieldName = "fieldName";
        object  fieldValue = "fieldValue" ;
        string fieldName2 = "fieldName2";
        object  fieldValue2 = "fieldValue2" ;

        var result =(FluentApi.FluentApi<Students>) FluentApi.FluentApi<Students>.Get().EqualTo(fieldName, fieldValue).And()
            .EqualTo(fieldName2, fieldValue2);
        
        ((CustomExpression)result.CustomExpression.LeftSide).LeftSide.Should().Be(fieldName2);
        ((CustomExpression)result.CustomExpression.LeftSide).RightSide.Should().Be(fieldValue2);
        ((CustomExpression)result.CustomExpression.LeftSide).Operator.Should().Be(CustomOperations.Equals);
    }
    
    [Test]
    public void FluentApi_Should_ReturnEqualOrEqualExpression()
    {
        string fieldName = "fieldName";
        object  fieldValue = "fieldValue" ;
        string fieldName2 = "fieldName2";
        object  fieldValue2 = "fieldValue2" ;

        var result =(FluentApi.FluentApi<Students>) FluentApi.FluentApi<Students>.Get().EqualTo(fieldName, fieldValue).Or()
            .EqualTo(fieldName2, fieldValue2);
        
        ((CustomExpression)result.CustomExpression.LeftSide).LeftSide.Should().Be(fieldName2);
        ((CustomExpression)result.CustomExpression.LeftSide).RightSide.Should().Be(fieldValue2);
        ((CustomExpression)result.CustomExpression.LeftSide).Operator.Should().Be(CustomOperations.Equals);
        result.CustomExpression.Operator.Should().Be(CustomOperations.Or);
    }
}