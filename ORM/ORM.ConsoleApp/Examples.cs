using ORM.ConsoleApp.Entities;
using ORM.Core;
using ORM.Core.Models;

namespace ORM.ConsoleApp
{
    public class Examples
    {
        private readonly DbContext _dbContext;
        public Examples(DbContext dbContext)
        {
            _dbContext = dbContext;
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

            var classes1 = _dbContext.Get<Classes>(1);
            
            Console.WriteLine($"Id: {classes1.Id}, Name: {classes1.Name}, Teacher: {classes1.Teacher.Firstname}");

        }

        public void ShowEntityWithFkList()
        {
        }

        public void ShowEntityWithManyToManyRelation()
        {
        }

        public void ShowLazyList()
        {
        }

        public void ShowCaching()
        {
        }

        public void ShowQuery()
        {
        }
    }
}