using System;
using ORM.Core.Attributes;

namespace ORM.Core.Test.Entities
{
    public class Persons
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Firstname { get; set; }
        public Gender Gender { get; set; }
        public DateTime BirthDate { get; set; }
    }
}