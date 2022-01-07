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
            TableModel table = new TableModel(typeof(Students));
        }

        public void InsertObject()
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


            Teachers t1 = _dbContext.Add(t);

            Console.WriteLine($"Id: {t1.Id}, Salary: {t1.Salary}, Firstname: {t1.Firstname}, Name: {t1.Name}");
        }

        public void UpdateObject()
        {
            Teachers t = new Teachers
            {
                Firstname = "Lisi",
                Id = 1,
                Name = "Mouse",
                Gender = Gender.Female,
                BirthDate = new DateTime(1970, 8, 18),
                HireDate = new DateTime(2015, 6, 20),
                Salary = 70000
            };

            Teachers t1 = _dbContext.Update(t);

            Console.WriteLine($"Id: {t1.Id}, Salary: {t1.Salary}, Firstname: {t1.Firstname}, Name: {t1.Name}");
        }

        public void ShowEntityWithFk()
        {
            Teachers teacher = _dbContext.Get<Teachers>(1);

            Classes classes = new Classes
            {
                Name = "Math",
                Teacher = teacher
            };

            _dbContext.Add(classes);

            Classes classes1 = _dbContext.Get<Classes>(1);

            Console.WriteLine($"Id: {classes1.Id}, Name: {classes1.Name}, Teacher: {classes1.Teacher.Firstname}");
        }

        public void ShowEntityWithFkList()
        {
            Teachers teacher = _dbContext.Get<Teachers>(1);

            Console.WriteLine($"Teacher {teacher.Firstname}");

            foreach (Classes classes in teacher.Classes)
                Console.WriteLine("Classes: " + classes.Name);
        }

        public void ShowEntityWithManyToManyRelation()
        {
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
        }
        public void ShowCaching()
        {
            Students student1 = _dbContext.Get<Students>(1);
            Students student2 = _dbContext.Get<Students>(1);

            DbContext dbcontext = new DbContext(_databaseWrapper, null,_logger);
            Students student3 = dbcontext.Get<Students>(1);
            Students student4 = dbcontext.Get<Students>(1);


            Console.WriteLine($"With Cache: {student1 == student2}");
            Console.WriteLine($"Without Cache: {student3 == student4}");
        }

        public void ShowQuery()
        {
            IReadOnlyCollection<Students> x = FluentApi<Students>.Get().Like("name", "li").Execute(_dbContext);

            foreach (Students students in x)
                Console.WriteLine($"Id: {students.Id}, Firstname: {students.Firstname}, Name: {students.Name}");
        }
        
        public void UpdateNToMObject()
        {
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
            
            foreach (Courses studentCourse in student.Courses)
            {
                Console.WriteLine($"Course: {studentCourse.Name}");
            }

            Console.WriteLine("Now adding Course");
            var course = _dbContext.Get<Courses>(1);
            Classes classes = _dbContext.Get<Classes>(1);
            
            student.Courses.Add(course);
            student.Class = classes;

            student = _dbContext.Update(student);
            
            foreach (Courses studentCourse in student.Courses)
            {
                Console.WriteLine($"Course: {studentCourse.Name}");
            }
            Console.WriteLine($"Class {student.Class.Name}");

        }

        public void Test()
        {
            var result = FluentApi<Students>.Get().Execute(_dbContext);
        }
    }
}