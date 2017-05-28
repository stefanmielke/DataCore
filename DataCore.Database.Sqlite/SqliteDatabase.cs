using System.Data;
using System.Data.SQLite;

namespace DataCore.Database.Sqlite
{
    public class SqliteDatabase : IDatabaseDefinition
    {
        public IDbConnection GetNewDbConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }

        public ITranslator GetTranslator()
        {
            return new SqliteTranslator();
        }
    }
}
