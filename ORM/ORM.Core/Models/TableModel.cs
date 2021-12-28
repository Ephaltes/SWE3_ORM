using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace ORM.Core.Models
{
    public class TableModel
    {
        public List<ColumnModel> Columns { get; set; }
        public List<ColumnModel> ForeignKeys { get; set; }
        public ColumnModel PrimaryKey => GetPrimaryKey();


        public string Name { get; set; }

        public TableModel(Type type)
        {
            Columns = new List<ColumnModel>();
            ForeignKeys = new List<ColumnModel>();
            
            foreach (PropertyInfo property in type.GetProperties())
            {
                ColumnModel column = new ColumnModel(property, this);

                if (column.IsForeignKey)
                    ForeignKeys.Add(column);
                else
                    Columns.Add(column);
            }

            Name = type.GetCustomAttribute<TableAttribute>() != null
                ? type.GetCustomAttribute<TableAttribute>()?.Name
                : type.Name;
        }
        private ColumnModel GetPrimaryKey()
        {
            return Columns.Single(x => x.PrimaryKey);
        }
    }
}