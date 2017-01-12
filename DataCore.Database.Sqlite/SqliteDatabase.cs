using System.Data;

namespace DataCore.Database.Sqlite
{
    public class SqliteDatabase : Database
    {
        public SqliteDatabase(IDbConnection connection)
            : base(connection, new SqliteTranslator())
        {
        }
    }
}
