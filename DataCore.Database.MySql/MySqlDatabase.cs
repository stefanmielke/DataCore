using System.Data;
using MySql.Data.MySqlClient;

namespace DataCore.Database.MySql
{
    public class MySqlDatabase : IDatabaseDefinition
    {
        public IDbConnection GetNewDbConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public ITranslator GetTranslator()
        {
            return new MySqlTranslator();
        }
    }
}
