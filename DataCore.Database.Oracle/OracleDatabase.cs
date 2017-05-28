using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace DataCore.Database.Oracle
{
    public class OracleDatabase : IDatabaseDefinition
    {
        public IDbConnection GetNewDbConnection(string connectionString)
        {
            return new OracleConnection(connectionString);
        }

        public ITranslator GetTranslator()
        {
            return new OracleTranslator();
        }
    }
}
