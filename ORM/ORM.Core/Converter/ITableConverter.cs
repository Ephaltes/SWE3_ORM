using System.Data;

namespace ORM.Core.Converter;

public interface ITableConverter
{
    public List<T> DataTableToObjectList<T>(DataTable table) where T : class, new();

    public T DataTableToObject<T>(DataTable table) where T : class, new();
}