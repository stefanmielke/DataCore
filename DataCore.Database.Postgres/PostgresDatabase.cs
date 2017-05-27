using System.Data;

namespace DataCore.Database.Postgres
{
    public class PostgresDatabase : Database
    {
        public PostgresDatabase(IDbConnection connection) : base(connection, new PostgresTranslator())
        {
        }
    }
}
