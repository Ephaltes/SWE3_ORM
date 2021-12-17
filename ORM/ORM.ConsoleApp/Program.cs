using ORM.Cache;
using ORM.Core;
using ORM.PostgresSQL;
using ORM.PostgresSQL.Interface;

namespace ORM.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IDatabaseWrapper db =
                new PostgresDb("Server=127.0.0.1;Port=5432;Database=orm;User Id=orm_user;Password=orm_password;",
                    "orm");

            TrackingCache cache = new TrackingCache();


            DbContext dbContext = new DbContext(db, cache);

            Examples example = new Examples(dbContext);

            Console.WriteLine("Hello World!");
            example.DisplayTables();
            example.InsertObject();
            example.UpdateObject();
            example.ShowEntityWithFk();
            example.ShowEntityWithFkList();
            example.ShowEntityWithManyToManyRelation();
        }
    }
}