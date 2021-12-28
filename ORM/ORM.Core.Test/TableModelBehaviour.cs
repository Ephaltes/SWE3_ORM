using System;
using FluentAssertions;
using NUnit.Framework;
using ORM.Core.Models;
using ORM.Core.Test.Entities;

namespace ORM.Core.Test;

public class TableModelBehaviour
{
    private TableModel _tableModel;
    [SetUp]
    public void Setup()
    {
        _tableModel = new TableModel(typeof(Courses));
    }

    [Test]
    public void GetPrimaryKey_Should_ReturnId()
    {
        _tableModel.PrimaryKey.Should().NotBeNull();
    }
    
    [Test]
    public void SetValue_Should_SetId()
    {
        Courses courses = new Courses();
        _tableModel.PrimaryKey.SetValue(courses,5);
        
        courses.Id.Should().Be(5);
    }
    
    [Test]
    public void ConvertToType_Should_ReturnRightType()
    {
        _tableModel.PrimaryKey.Type = typeof(bool);
        var resultBool = _tableModel.PrimaryKey.ConvertToType(true);
        _tableModel.PrimaryKey.Type = typeof(int);
        var resultInt = _tableModel.PrimaryKey.ConvertToType(5);
        _tableModel.PrimaryKey.Type = typeof(long);
        var resultLong = _tableModel.PrimaryKey.ConvertToType(5366623);
        _tableModel.PrimaryKey.Type = typeof(double);
        var resultDouble = _tableModel.PrimaryKey.ConvertToType(1.23);
        _tableModel.PrimaryKey.Type = typeof(Gender);
        var resultEnum = _tableModel.PrimaryKey.ConvertToType(Gender.Female);

        resultBool.Should().BeOfType<bool>();
        resultInt.Should().BeOfType<int>();
        resultLong.Should().BeOfType<long>();
        resultDouble.Should().BeOfType<double>();
        resultEnum.Should().BeOfType<Gender>();
    }

  
}