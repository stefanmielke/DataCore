using System.Data;
using System.Data.SQLite;

namespace DataCore.Database.Sqlite
{
    public class SqliteDatabase : Database
    {
        public SqliteDatabase(IDbConnection connection)
            : base(connection, new Translator())
        {
        }
    }
}
