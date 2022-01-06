using System.Diagnostics.CodeAnalysis;
using ORM.Core.Attributes;

namespace ORM.ConsoleApp.Entities
{
    [ExcludeFromCodeCoverage]
    public class Courses
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool Active { get; set; }
        public string Name { get; set; }
        [ForeignKey("teachersid")]
        public Teachers Teacher { get; set; }

        [ManyToMany("r_students_courses", "coursesid", "studentsid")]
        public List<Students> Students { get; set; } = new List<Students>();
    }
}