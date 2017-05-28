using System.Data;
using Npgsql;

namespace DataCore.Database.Postgres
{
    public class PostgresDatabase : IDatabaseDefinition
    {
        public IDbConnection GetNewDbConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }

        public ITranslator GetTranslator()
        {
            return new PostgresTranslator();
        }
    }
}
