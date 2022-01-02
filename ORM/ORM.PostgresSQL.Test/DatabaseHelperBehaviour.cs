using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualBasic;
using NUnit.Framework;
using ORM.PostgresSQL.Interface;

namespace ORM.PostgresSQL.Test;

public class DatabaseHelperBehaviour
{

    [Test]
    public void IsList_Should_ReturnTrue()
    {

        var result = DatabaseHelper.IsList(new List<object>());

        result.Should().BeTrue();
    }
    
    [Test]
    public void IsList_Should_ReturnFalse()
    {

        var result = DatabaseHelper.IsList(5);

        result.Should().BeFalse();
    }
    
    [Test]
    public void ObjectToList_Should_ReturnList()
    {

        var result = DatabaseHelper.ObjectToList(new Collection());

        result.Should().BeOfType<List<object>>();
    }
    
    [Test]
    [TestCase("bigint", 3)]
    [TestCase("boolean", 2)]
    [TestCase("double", 4)]
    [TestCase("date", 5)]
    [TestCase("timestamp without timezone", 6)]
    [TestCase("bytea", 7)]
    [TestCase("text", 0)]
    [Parallelizable(ParallelScope.All)]
    public void DataTypeFromString_Should_Return_RightDatatype(string input, int expectedEnum)
    {

        var result =Convert.ToInt32( DatabaseHelper.DataTypeFromString(input));

        result.Should().Be(expectedEnum);
    }

}