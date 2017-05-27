using System.Data;

namespace DataCore.Database.MySql
{
    public class MySqlDatabase : Database
    {
        public MySqlDatabase(IDbConnection connection) : base(connection, new MySqlTranslator())
        {
        }
    }
}
