using System.Diagnostics.CodeAnalysis;
using ORM.Core.Attributes;

namespace ORM.ConsoleApp.Entities
{
    [ExcludeFromCodeCoverage]
    public class Teachers : Persons
    {
        public DateTime HireDate { get; set; }
        public int Salary { get; set; }

        [ForeignKey("teachersid")] 
        public List<Classes> Classes { get; set; } = new List<Classes>();

        [ForeignKey("teachersid")] 
        public List<Courses> Courses { get; set; } = new List<Courses>();
    }
}