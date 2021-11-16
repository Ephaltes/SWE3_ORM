
using System;
using System.ComponentModel.DataAnnotations;

namespace ORM.PostgresSQL.Model
{
    public class DatabaseColumnModel
    {
        public string Name { get; set; }
        public bool PrimaryKey { get; set; }
        public DatabaseColumnType Type { get; set; } 
        public bool Nullable { get; set; }

        public DatabaseColumnModel()
        {
            
        }
        public DatabaseColumnModel(string name, bool primaryKey, DatabaseColumnType dataType, bool nullable)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (primaryKey && nullable) throw new ArgumentException("Primary key column '" + name + "' cannot be nullable.");

            Name = name;
            PrimaryKey = primaryKey;
            Type = dataType; 
            Nullable = nullable;
        }
    }
}