using System.Diagnostics.CodeAnalysis;
using ORM.Core.Attributes;

namespace ORM.ConsoleApp.Entities
{
    [ExcludeFromCodeCoverage]
    public class Students : Persons
    {
        [ForeignKey("classesid")]
        public Classes Class { get; set; }
        public int Grade { get; set; }

        [ManyToMany("r_students_courses", "studentsid", "coursesid")]
        public List<Courses> Courses { get; set; } = new List<Courses>();
    }
}