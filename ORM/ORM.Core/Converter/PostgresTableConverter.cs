using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Npgsql.Replication.PgOutput.Messages;
using ORM.Core.Models;

namespace ORM.Core.Converter
{
    public class PostgresTableConverter
    {
        public List<T> DataTableToObjectList<T>(DataTable table) where T : class, new()
        {
            List<T> ret = new List<T>();

            if (table == null) return null;
            if (table.Rows == null || table.Rows.Count < 1) return ret;

            foreach (DataRow row in table.Rows)
            {
                T obj = DataRowToObject<T>(row);
                if (obj != null) ret.Add(obj);
            }

            return ret;
        }

        public T DataTableToObject<T>(DataTable table) where T : class, new()
        {
            if (table == null || table.Rows == null || table.Rows.Count < 1) return null;
            return DataRowToObject<T>(table.Rows[0]);
        }
        
        protected T DataRowToObject<T>(DataRow row) where T : class, new()
        {
            if (row == null) return null;

            T ret = new T();
            TableModel table = new TableModel(typeof(T));

            foreach (DataColumn dc in row.Table.Columns)
            {
                if (table.Columns.Any(c => c.ColumnName.Equals(dc.ColumnName,StringComparison.InvariantCultureIgnoreCase)))
                {
                    ColumnModel col = table.Columns.Where(c => c.ColumnName.Equals(dc.ColumnName,StringComparison.InvariantCultureIgnoreCase)).First();
                    object val = row[dc.ColumnName];
                    string? propName = table.GetPropertyNameFromColumnName(dc.ColumnName);
                    if (string.IsNullOrEmpty(propName)) throw new ArgumentException("Unable to find property in type '" + typeof(T).Name + "' for column '" + dc.ColumnName + "'.");

                    PropertyInfo property = typeof(T).GetProperty(propName);
                    if (val != null && val != DBNull.Value)
                    {
                        // Remap the object to the property type since some databases misalign
                        // Example: sqlite uses int64 when it should be int32 
                        // Console.WriteLine("| Column data type [" + col.DataType.ToString() + "], property data type [" + property.PropertyType.ToString() + "]");

                        Type propType = property.PropertyType;
                        Type underlyingType = Nullable.GetUnderlyingType(propType);

                        if (underlyingType != null)
                        {
                            if (underlyingType == typeof(bool))
                            {
                                property.SetValue(ret, Convert.ToBoolean(val));
                            }
                            else if (underlyingType.IsEnum)
                            {
                                property.SetValue(ret, (Enum.Parse(underlyingType, val.ToString())));
                            }
                            else if (underlyingType == typeof(DateTimeOffset))
                            {
                                property.SetValue(ret, DateTimeOffset.Parse(val.ToString()));
                            }
                            else
                            {
                                property.SetValue(ret, Convert.ChangeType(val, underlyingType));
                            }
                        }
                        else
                        {
                            if (propType == typeof(bool))
                            {
                                property.SetValue(ret, Convert.ToBoolean(val));
                            }
                            else if (propType.IsEnum)
                            {
                                property.SetValue(ret, (Enum.Parse(propType, val.ToString())));
                            }
                            else if (propType == typeof(DateTime))
                            {
                                property.SetValue(ret, DateTime.Parse(val.ToString()));
                            }
                            else
                            {
                                property.SetValue(ret, Convert.ChangeType(val, propType));
                            }
                        }
                    }
                    else
                    {
                        property.SetValue(ret, null);
                    }
                }
            }

            return ret;
        }
    }
}