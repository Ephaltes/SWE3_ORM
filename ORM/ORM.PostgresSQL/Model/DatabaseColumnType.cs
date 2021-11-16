using System.Runtime.Serialization;

namespace ORM.PostgresSQL.Model
{
    public enum DatabaseColumnType
    {
        Varchar,
        Nvarchar,
        Int,
        Long,
        Double,
        DateTime,
        TimeSpan,
        Blob

    }
}