using System.Data;
using System.Data.SqlClient;

namespace DataCore.Database.SqlServer
{
    public class SqlServerDatabase : IDatabaseDefinition
    {
        public IDbConnection GetNewDbConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public ITranslator GetTranslator()
        {
            return new SqlServerTranslator();
        }
    }
}
