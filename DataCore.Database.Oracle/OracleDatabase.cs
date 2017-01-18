using System.Data;

namespace DataCore.Database.Oracle
{
    public class OracleDatabase : Database
    {
        public OracleDatabase(IDbConnection connection)
            : base(connection, new OracleTranslator())
        {
        }
    }
}
