using System.Diagnostics.CodeAnalysis;
using ORM.Cache;
using ORM.ConsoleApp.Entities;
using ORM.Core;
using ORM.Core.FluentApi;
using ORM.Core.Models;
using ORM.PostgresSQL;
using ORM.PostgresSQL.Interface;
using Serilog;

namespace ORM.ConsoleApp
{
    /// <summary>
    ///     Class with examples to show how the orm is working
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Examples
    {
        private readonly IDatabaseWrapper _databaseWrapper;
        private readonly DbContext _dbContext;
        private readonly ILogger _logger;
        public Examples()
        {
            _databaseWrapper =
                new PostgresDb("Server=127.0.0.1;Port=5432;Database=orm;User Id=orm_user;Password=orm_password;");

            _logger = new LoggerConfiguration().WriteTo.Debug().MinimumLevel.Debug().CreateLogger();
            Log.Logger = _logger;
            
            TrackingCache cache = new TrackingCache(_logger);
            
            _dbContext = new DbContext(_databaseWrapper, cache,_logger);
        }
        public void DisplayTables()
        {
            Console.WriteLine("Displaying Tables");
            TableModel table = new TableModel(typeof(Students));
            Console.WriteLine(table.Name);
            Console.WriteLine();
        }

        public void InsertObject()
        {
            Console.WriteLine("InsertObject");
            Teachers t = new Teachers
            {
                Firstname = "Lisi",
                Name = "Mouse",
                Gender = Gender.Female,
                BirthDate = new DateTime(1970, 8, 18),
                HireDate = new DateTime(2015, 6, 20),
                Salary = 50000
            };


            Teachers t1 = _dbContext.Add(t);

            Console.WriteLine($"Id: {t1.Id}, Salary: {t1.Salary}, Firstname: {t1.Firstname}, Name: {t1.Name}");
            Console.WriteLine();
        }

        public void UpdateObject()
        {
            Console.WriteLine("UpdateObject");
            Teachers t = _dbContext.Get<Teachers>(1);
            t.Salary = 99000;

            Teachers t1 = _dbContext.Update(t);

            Console.WriteLine($"Id: {t1.Id}, Salary: {t1.Salary}, Firstname: {t1.Firstname}, Name: {t1.Name}");
            Console.WriteLine();
        }

        public void ShowEntityWithFk()
        {
            Console.WriteLine("ShowEntityWithFk");
            Teachers teacher = _dbContext.Get<Teachers>(1);

            Classes classes = new Classes
            {
                Name = "Math",
                Teacher = teacher
            };

            _dbContext.Add(classes);

            Classes classes1 = _dbContext.Get<Classes>(1);

            Console.WriteLine($"Id: {classes1.Id}, Name: {classes1.Name}, Teacher: {classes1.Teacher.Firstname}");
            Console.WriteLine();
        }

        public void ShowEntityWithFkList()
        {
            Console.WriteLine("Show 1:n");
            
            Teachers teacher = _dbContext.Get<Teachers>(1);

            Classes newClass = new Classes
            {
                Name = "German",
                Teacher = teacher
            };

            _dbContext.Add(newClass);

            Console.WriteLine($"Teacher {teacher.Firstname}");

            foreach (Classes classes in teacher.Classes)
                Console.WriteLine("Classes: " + classes.Name);
            
            Console.WriteLine();
        }

        public void ShowEntityWithManyToManyRelation()
        {
            Console.WriteLine("Show M:N Relation");
            Courses course = new Courses
            {
                Name = "English",
                Teacher = _dbContext.Get<Teachers>(1)
            };

            Students student = new Students
            {
                Name = "Elisabeth",
                Firstname = "The Sleeping Panda",
                Grade = 1
            };
            student = _dbContext.Add(student);

            course.Students.Add(student);

            student = new Students
            {
                Name = "Samuel",
                Firstname = "The Flying Fish",
                Grade = 1
            };

            student = _dbContext.Add(student);

            course.Students.Add(student);

            course = _dbContext.Add(course);
            course = _dbContext.Add(course);
            course = _dbContext.Get<Courses>(course.Id);

            Console.WriteLine($"Course : {course.Name} has following Students:");

            foreach (Students students in course.Students)
                Console.WriteLine($"Student: {students.Firstname} {students.Name}");
            
            Console.WriteLine();
        }
        public void ShowCaching()
        {
            Console.WriteLine("Show Caching");
            
            Students student1 = _dbContext.Get<Students>(1);
            Students student2 = _dbContext.Get<Students>(1);

            DbContext dbcontext = new DbContext(_databaseWrapper, null,_logger);
            Students student3 = dbcontext.Get<Students>(1);
            Students student4 = dbcontext.Get<Students>(1);


            Console.WriteLine($"With Cache: {student1 == student2}");
            Console.WriteLine($"Without Cache: {student3 == student4}");
            
            Console.WriteLine();
        }

        public void ShowQuery()
        {
            Console.WriteLine("Show FluentApi");
            
            
            IReadOnlyCollection<Students> x = FluentApi.Get<Students>().Like("name", "li").Execute(_dbContext);

            foreach (Students students in x)
                Console.WriteLine($"Id: {students.Id}, Firstname: {students.Firstname}, Name: {students.Name}");
            
            
            Console.WriteLine("Get All Students");
            x = FluentApi.Get<Students>().Execute(_dbContext);

            foreach (Students students in x)
                Console.WriteLine($"Id: {students.Id}, Firstname: {students.Firstname}, Name: {students.Name}");
            
            Console.WriteLine();
        }
        
        public void UpdateNToMObject()
        {
            Console.WriteLine("Update M:N");
            
            Students student = _dbContext.Get<Students>(1);
            
            Console.WriteLine($"Student: {student.Firstname} {student.Name}");
            Console.WriteLine($"Class {student.Class?.Name}");
            foreach (Courses studentCourse in student.Courses)
            {
                Console.WriteLine($"Course: {studentCourse.Name}");
            }

            Console.WriteLine("Now Deleting Course");
            student.Courses.Remove(student.Courses.FirstOrDefault());

            student = _dbContext.Update(student);
            
            Console.WriteLine($"Course Count {student.Courses.Count}");

            Console.WriteLine("Now adding Course");
            var course = _dbContext.Get<Courses>(1);
            
            student.Courses.Add(course);

            student = _dbContext.Update(student);
            
            Console.WriteLine($"Course Count {student.Courses.Count}");

            Console.WriteLine();
        }

        public void Test()
        {
            Console.WriteLine("Custom Test");
            var result = FluentApi.Get<Students>().Execute(_dbContext);
            
            Console.WriteLine();
        }
    }
}