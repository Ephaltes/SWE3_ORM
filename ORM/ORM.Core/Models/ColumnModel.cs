using System.Collections;
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
        public string PropertyName { get; set; }
        
        public string ColumnName { get; set; }
        public bool PrimaryKey { get; set; }
        public Type Type { get; set; }
        public bool IsNullable { get; set; }
        public bool Ignore { get; set; }
        public bool IsForeignKey { get; set; }

        public string ForeignKeyTableName { get; set; }

        public string ForeignKeyColumnName { get; set; }

        //List<object>
        public bool IsReferenced { get; set; }
        
        public bool IsManyToMany { get; set; }

        public string DbType { get; set; }

        public bool IsAutoIncrement { get; set; } = true;

        public ColumnModel()
        {
        }

        public ColumnModel(PropertyInfo property, TableModel parentTable)
        {
            ReadCustomAttributes(property);
            ParentTable = parentTable;
        }

        private void ReadCustomAttributes(PropertyInfo property)
        {
            PropertyName = property.Name;
            ColumnName = PropertyName;
            Type = property.PropertyType;
            
            IEnumerable<Attribute> attributes = property.GetCustomAttributes();
            foreach (Attribute attribute in attributes)
                switch (attribute)
                {
                    case ColumnAttribute columnAttribute:
                        ColumnName = columnAttribute.Name;
                        DbType = columnAttribute.DbType;

                        break;
                    case ForeignKeyAttribute foreignKeyAttribute:
                        IsForeignKey = true;
                        ForeignKeyColumnName = foreignKeyAttribute.RemoteColumnName;
                        if (typeof(IEnumerable).IsAssignableFrom(Type))
                            IsReferenced = true;

                        break;
                    case ManyToManyAttribute manyToManyAttribute:
                        IsManyToMany = true;
                        IsForeignKey = true;
                        ForeignKeyColumnName = manyToManyAttribute.RemoteColumnName;
                        ForeignKeyTableName = manyToManyAttribute.RemoteTableName;
                        ColumnName = manyToManyAttribute.LocalColumnName;

                        break;
                    case NotMappedAttribute:
                        Ignore = true;

                        break;
                    case NotNullAttribute:
                        IsNullable = false;

                        break;
                    case PrimaryKeyAttribute primaryKeyAttribute:
                        PrimaryKey = true;
                        IsAutoIncrement = primaryKeyAttribute.AutoIncrement;
                        break;
                }
        }

        public object? GetValue(object obj)
        {
            PropertyInfo? propertyInfo = obj.GetType().GetProperty(PropertyName);

            return propertyInfo?.GetValue(obj);
        }
        
        public void SetValue(object obj, object value)
        {
            PropertyInfo? propertyInfo = obj.GetType().GetProperty(PropertyName);

            propertyInfo?.SetValue(obj, value);
        }
        public object ConvertToType(object value)
        {
            if(Type == typeof(bool)) return Convert.ToBoolean(value);
            if(Type == typeof(int))  return Convert.ToInt32(value); 
            if(Type == typeof(long))  return Convert.ToInt64(value);
            if (Type == typeof(double)) return Convert.ToDouble(value);

            return Type.IsEnum ? Enum.ToObject(Type, value) : value;
        }
    }
}