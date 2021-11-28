﻿using ORM.Core.Attributes;

namespace ORM.ConsoleApp.Entities
{
    public class Students : Persons
    {
        [ForeignKey("classesid")]
        public Classes Class { get; set; }
        public int Grade { get; set; }
        [ManyToMany("r_courses_students", "studentsid", "coursesid")]
        public List<Courses> Courses { get; set; }
    }
}