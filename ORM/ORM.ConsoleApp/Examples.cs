using ORM.ConsoleApp.Entities;
using ORM.Core.Models;

namespace ORM.ConsoleApp
{
    public static class Examples
    {
        public static void DisplayTables()
        {
            TableModel table = new TableModel(typeof(Students));
        }
    }
}