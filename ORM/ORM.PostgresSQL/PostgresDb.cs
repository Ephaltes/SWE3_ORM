using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;
using ORM.Core.Interfaces;

namespace ORM.PostgresSQL
{
    public class PostgresDb : IDatabaseWrapper
    {
        private readonly string _connectionString;
        public PostgresDb(string connectionString)
        {
            _connectionString = connectionString;
        }

    }
}