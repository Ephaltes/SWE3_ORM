using System.Collections.Generic;
using ORM.Core.Attributes;

namespace ORM.Core.Test.Entities
{
    public class Courses
    {
        [PrimaryKey]
        public int Id { get; set; }
        public bool Active { get; set; }
        public string Name { get; set; }
        [ForeignKey("teachersid")]
        public Teachers Teacher { get; set; }
        [ManyToMany("r_courses_students", "coursesid", "studentsid")]
        public List<Students> Students { get; set; }
    }
}