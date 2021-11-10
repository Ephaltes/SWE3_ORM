using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using ORM.Core.Attributes;
using ColumnAttribute = ORM.Core.Attributes.ColumnAttribute;
using ForeignKeyAttribute = ORM.Core.Attributes.ForeignKeyAttribute;

namespace ORM.Core.Models
{
    public class ColumnModel
    {
        public TableModel ParentTable { get; set; }
        public string Name { get; set; }
        public bool PrimaryKey { get; set; }
        public Type Type { get; set; }
        public bool IsNullable { get; set; }
        public bool Ignore { get; set; }
        public bool IsForeignKey { get; set; }
        
        public string ForeignKeyTableName { get; set; }
        
        public string ForeignKeyColumnName { get; set; }
        
        public bool IsReferenced { get; set; }

        public bool IsManyToMany { get; set; }
        
        public string DbType { get; set; }
        
        public ColumnModel()
        {
        }

        public ColumnModel(PropertyInfo property,TableModel parentTable)
        {
            ReadCustomAttributes(property);
            ParentTable = parentTable;
        }

        private void ReadCustomAttributes(PropertyInfo property)
        {
            IEnumerable<Attribute> attributes = property.GetCustomAttributes();
            foreach (Attribute attribute in attributes)
                switch (attribute)
                {
                    case ColumnAttribute columnAttribute:
                        Name = columnAttribute.Name;
                        DbType = columnAttribute.DbType;
                        break;
                    case ForeignKeyAttribute foreignKeyAttribute:
                        IsForeignKey = true;
                        ForeignKeyColumnName = foreignKeyAttribute.RemoteColumnName;
                        if(typeof(IEnumerable).IsAssignableFrom(Type))
                            IsReferenced = true;
                        break;
                    case ManyToManyAttribute manyToManyAttribute:
                        IsManyToMany = true;
                        IsForeignKey = true;
                        ForeignKeyColumnName = manyToManyAttribute.RemoteColumnName;
                        ForeignKeyTableName = manyToManyAttribute.RemoteTableName;
                        Name = manyToManyAttribute.LocalColumnName;
                        break;
                    case NotMappedAttribute :
                        Ignore = true;
                        break;
                    case NotNullAttribute :
                        IsNullable = false;
                        break;
                    case PrimaryKeyAttribute :
                        PrimaryKey = true;
                        break;
                }
        }
    }
}