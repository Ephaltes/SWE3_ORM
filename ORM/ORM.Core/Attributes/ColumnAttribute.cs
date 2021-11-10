using System;

namespace ORM.Core.Attributes
{
    public class ColumnAttribute : Attribute
    {
        public string Name
        {
            get;
        }
        
        public string DbType
        {
            get;
        }
        
        public ColumnAttribute(string name)
        {
            Name = name;
        }
        
        public ColumnAttribute(string name, string dbType)
        {
            Name = name;
            DbType = dbType;
        }
        
    }
    
}