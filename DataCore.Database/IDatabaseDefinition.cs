using System.Data;

namespace DataCore.Database
{
    public interface IDatabaseDefinition
    {
        IDbConnection GetNewDbConnection(string connectionString);
        ITranslator GetTranslator();
    }
}
