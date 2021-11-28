using System;
using ORM.Core;
using ORM.PostgresSQL;
using ORM.PostgresSQL.Interface;

namespace ORM.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            IDatabaseWrapper db =
                new PostgresDb("Server=127.0.0.1;Port=5432;Database=orm;User Id=orm_user;Password=orm_password;",
                    "orm");

            
            var dbContext = new DbContext(db);
            
            Examples example = new Examples(dbContext);
            
            Console.WriteLine("Hello World!");
            example.DisplayTables();
            example.InsertObject();
            example.UpdateObject();
            example.ShowEntityWithFk();
        }
    }
}