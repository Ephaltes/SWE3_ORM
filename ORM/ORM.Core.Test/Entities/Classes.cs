using System.Collections.Generic;
using ORM.Core.Attributes;

namespace ORM.Core.Test.Entities
{
    
    public class Classes
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        [ForeignKey("teachersid")]
        public Teachers Teacher { get; set; }
        [ForeignKey("classesid")]
        public List<Students> Students { get; set; }
    }
}