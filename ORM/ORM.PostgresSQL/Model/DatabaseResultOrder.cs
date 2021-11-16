using System;

namespace ORM.PostgresSQL.Model
{
    public class DatabaseResultOrder
    {
        public string ColumnName { get; set; } = null;
        public CustomOrderDirection Direction { get; set; } = CustomOrderDirection.Ascending;

        public DatabaseResultOrder()
        {
        }

        public DatabaseResultOrder(string columnName, CustomOrderDirection direction)
        {
            if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException(nameof(columnName));
            ColumnName = columnName;
            Direction = direction;
        }

    }
}