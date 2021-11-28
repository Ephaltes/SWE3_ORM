using System;
using NUnit.Framework;
using ORM.Core.Converter;
using ORM.Core.Test.Entities;
using ORM.PostgresSQL;
using ORM.PostgresSQL.Interface;

namespace ORM.Core.Test
{
    public class Tests
    {
        private DbContext _dbContext;
        [SetUp]
        public void Setup()
        {
            IDatabaseWrapper db =
                new PostgresDb("Server=127.0.0.1;Port=5432;Database=orm;User Id=orm_user;Password=orm_password;",
                    "orm");

            ITableConverter converter = new PostgresTableConverter();
            
            _dbContext = new DbContext(db,converter);
        }

        [Test]
        public void Test1()
        {
         
        }
        
        [Test]
        public void Test2()
        {
          
        }
    }
}