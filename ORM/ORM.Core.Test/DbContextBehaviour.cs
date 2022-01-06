using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using ORM.Core.Interfaces;
using ORM.Core.Test.Entities;
using ORM.PostgresSQL.Interface;
using Serilog;

namespace ORM.Core.Test;

public class DbContextBehaviour
{
    private IDbContext _dbContext;
    private ICache _cache;
    private IDatabaseWrapper _databaseWrapper;
    private ILogger _logger;
    [SetUp]
    public void Setup()
    {
        _databaseWrapper = A.Fake<IDatabaseWrapper>();
        _cache = A.Fake<ICache>();
        _logger = A.Fake<ILogger>();
        _dbContext = new DbContext(_databaseWrapper, _cache,_logger);
    }

    [Test]
    public void DbContext_Should_DeleteItemFromClass()
    {
        A.CallTo(() => _databaseWrapper.Delete(null,null)).WithAnyArguments().DoesNothing();
        _dbContext.Delete<Classes>(5);
        
        
        A.CallTo(() => _databaseWrapper.Delete(null,null)).WithAnyArguments().MustHaveHappened();
    }
    
    [Test]
    public void DbContext_Should_GetAllPerson()
    {
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Id", typeof(int));
        dataTable.Columns.Add("Name",typeof(string));
        dataTable.Columns.Add("Firstname",typeof(string));
        dataTable.Columns.Add("Gender",typeof(Gender));
        dataTable.Columns.Add("BirthDate",typeof(DateTime));
        DataRow row = dataTable.NewRow();
        row.BeginEdit();
        row["Id"] = 1;
        row["Name"] = "Test";
        row["Firstname"] = "Markus";
        row["Gender"] = Gender.Female;
        row["BirthDate"] = DateTime.Now;
        row.EndEdit();
        
        dataTable.Rows.Add(row);
        
        A.CallTo(() => _databaseWrapper.Select(null,null,null,null,null))
            .WithAnyArguments().Returns(dataTable);
        A.CallTo( () => _cache.Get(null, 0)).WithAnyArguments().Returns(new Persons());

        var result = _dbContext.GetAll<Persons>(null);

        result.Should().NotBeEmpty();
    }
    
    [Test]
    public void DbContext_Should_GetPerson()
    {
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Id", typeof(int));
        dataTable.Columns.Add("Name",typeof(string));
        dataTable.Columns.Add("Firstname",typeof(string));
        dataTable.Columns.Add("Gender",typeof(Gender));
        dataTable.Columns.Add("BirthDate",typeof(DateTime));
        DataRow row = dataTable.NewRow();
        row.BeginEdit();
        row["Id"] = 1;
        row["Name"] = "Test";
        row["Firstname"] = "Markus";
        row["Gender"] = Gender.Female;
        row["BirthDate"] = DateTime.Now;
        row.EndEdit();
        
        dataTable.Rows.Add(row);
        
        A.CallTo(() => _databaseWrapper.Select(null,null,null,null,null))
            .WithAnyArguments().Returns(dataTable);
        A.CallTo( () => _cache.Contains(null, 0)).WithAnyArguments().Returns(false);

        var result = _dbContext.Get<Persons>(null);

        result.Should().NotBeNull();
    }
    
    [Test]
    public void DbContext_Should_UpdatePerson()
    {
        var person = new Persons();
        
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Id", typeof(int));
        dataTable.Columns.Add("Name",typeof(string));
        dataTable.Columns.Add("Firstname",typeof(string));
        dataTable.Columns.Add("Gender",typeof(Gender));
        dataTable.Columns.Add("BirthDate",typeof(DateTime));
        DataRow row = dataTable.NewRow();
        row.BeginEdit();
        row["Id"] = 1;
        row["Name"] = "Test";
        row["Firstname"] = "Markus";
        row["Gender"] = Gender.Female;
        row["BirthDate"] = DateTime.Now;
        row.EndEdit();
        
        dataTable.Rows.Add(row);
        
        A.CallTo(() => _databaseWrapper.Select(null,null,null,null,null))
            .WithAnyArguments().Returns(dataTable);
        A.CallTo(() => _databaseWrapper.Update(null,null,null))
            .WithAnyArguments().Returns(dataTable);
        A.CallTo( () => _cache.Contains(null, 0)).WithAnyArguments().Returns(false);

        var result = _dbContext.Update(person);

        result.Should().NotBeNull();
    }
    
    [Test]
    public void DbContext_Should_AddPerson()
    {
        var person = new Persons();
        
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Id", typeof(int));
        dataTable.Columns.Add("Name",typeof(string));
        dataTable.Columns.Add("Firstname",typeof(string));
        dataTable.Columns.Add("Gender",typeof(Gender));
        dataTable.Columns.Add("BirthDate",typeof(DateTime));
        DataRow row = dataTable.NewRow();
        row.BeginEdit();
        row["Id"] = 1;
        row["Name"] = "Test";
        row["Firstname"] = "Markus";
        row["Gender"] = Gender.Female;
        row["BirthDate"] = DateTime.Now;
        row.EndEdit();
        
        dataTable.Rows.Add(row);
        
        A.CallTo(() => _databaseWrapper.Select(null,null,null,null,null))
            .WithAnyArguments().Returns(dataTable);
        A.CallTo(() => _databaseWrapper.Insert("string",new Dictionary<string, object>()))
            .WithAnyArguments().Returns(dataTable);
        A.CallTo(() => _cache.Update(null, 0)).WithAnyArguments().DoesNothing();
        A.CallTo(() => _cache.HasChanged(null)).WithAnyArguments().Returns(true);

        var result = _dbContext.Add(person);

        result.Should().NotBeNull();
    }
}