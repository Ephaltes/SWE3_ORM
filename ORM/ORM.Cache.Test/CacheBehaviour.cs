using System;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Serilog;

namespace ORM.Cache.Test;

public class CacheBehaviour
{
    private Cache _cache;
    private ILogger _logger;
    [SetUp]
    public void Setup()
    {
        _logger = A.Fake<ILogger>();
        _cache = new Cache(_logger);
    }

    [Test]
    public void Add_Should_AddObjectWithId()
    {
        int test = 5;
        Type expectedType = test.GetType();
        _cache.Add(test,3);

        _cache.GetAll(expectedType).Count().Should().Be(1);
    }
    
    [Test]
    public void Update_Should_UpdateExistingObject()
    {
        int test = 5;
        Type expectedType = test.GetType();
        _cache.Add(test,3);

        test = 3;
        _cache.Update(test,3);

        _cache.Get(expectedType,3).Should().Be(3);
    }
    
    [Test]
    public void Remove_Should_DeleteByObject()
    {
        object test = new { Id = 3 };
        Type expectedType = test.GetType();
        _cache.Add(test,3);

        _cache.Remove(test);

        _cache.GetAll(expectedType).Count().Should().Be(0);
    }
    
    [Test]
    public void Remove_Should_DeleteTypeById()
    {
        int test = 5;
        Type expectedType = test.GetType();
        _cache.Add(test,3);

        _cache.Remove(expectedType,3);

        _cache.GetAll(expectedType).Count().Should().Be(0);
    }
    
    [Test]
    public void Remove_Should_DeleteType()
    {
        int test = 5;
        string test2 = "test";
        Type expectedType = test.GetType();
        _cache.Add(test,3);
        _cache.Add(test2,2);

        _cache.Remove(expectedType);

        _cache.GetAll(expectedType).Count().Should().Be(0);
        _cache.GetAll(test2.GetType()).Count().Should().Be(1);
    }
    
    [Test]
    public void HasChanged_Should_ReturnTrue()
    {
        int test = 5;
        Type expectedType = test.GetType();

        _cache.HasChanged(expectedType).Should().BeTrue();
    }
    
    [Test]
    public void ContainsWithId_Should_ReturnTrue()
    {
        int test = 5;
        Type expectedType = test.GetType();
        _cache.Add(test,3);

        _cache.Contains(expectedType,3).Should().BeTrue();
    }
    
    [Test]
    public void Contains_Should_ReturnTrue()
    {
        int test = 5;
        Type expectedType = test.GetType();
        _cache.Add(test,3);

        _cache.Contains(expectedType).Should().BeTrue();
    }
}