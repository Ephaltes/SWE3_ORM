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
}