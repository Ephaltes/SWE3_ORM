using System;
using NUnit.Framework;
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
            _dbContext = new DbContext(db);
        }

        [Test]
        public void Test1()
        {
            Teachers t = new Teachers
            {
                Firstname = "Lisi",
                Name = "Mouse",
                Gender = Gender.Female,
                BirthDate = new DateTime(1970, 8, 18),
                HireDate = new DateTime(2015, 6, 20),
                Salary = 50000
            };


            var t1 = _dbContext.Add(t);
        }
        
        [Test]
        public void Test2()
        {
            Teachers t = new Teachers
            {
                Firstname = "Lisi",
                Id = 4,
                Name = "Mouse",
                Gender = Gender.Female,
                BirthDate = new DateTime(1970, 8, 18),
                HireDate = new DateTime(2015, 6, 20),
                Salary = 70000
            };
            
            var t1 = _dbContext.Update(t);
        }
    }
}