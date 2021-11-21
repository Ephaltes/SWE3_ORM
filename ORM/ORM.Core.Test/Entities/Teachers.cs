using System;
using System.Collections.Generic;
using ORM.Core.Attributes;

namespace ORM.Core.Test.Entities
{
    public class Teachers : Persons
    {
        public DateTime HireDate { get; set; }
        public int Salary { get; set; }

        [ForeignKey("teachersid")]
        public List<Classes> Classes { get; set; }
        [ForeignKey("teachersid")]
        public List<Courses> Courses { get; set; }
    }
}