using ORM.Core;

namespace ORM.ConsoleApp
{
    public class Database : DbContextOrm
    {
        public DbSetOrm<Product> Product { get; set; }

        public void hello()
        {
        }
    }
}