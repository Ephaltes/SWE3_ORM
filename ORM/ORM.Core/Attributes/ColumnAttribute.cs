using System;

namespace ORM.Core.Attributes
{
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// Name of the Column in the Db
        /// </summary>
        public string Name
        {
            get;
        }
        /// <summary>
        /// The DataType in the Db
        /// </summary>
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