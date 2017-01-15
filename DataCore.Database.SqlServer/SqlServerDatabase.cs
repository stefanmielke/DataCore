using System.Data;

namespace DataCore.Database.SqlServer
{
    public class SqlServerDatabase : Database
    {
        public SqlServerDatabase(IDbConnection connection)
            : base(connection, new SqlServerTranslator())
        {
        }
    }
}
