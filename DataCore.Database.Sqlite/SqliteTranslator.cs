namespace DataCore.Database.Sqlite
{
    public class SqliteTranslator : Translator
    {
        public override string GetTableName(string tableName)
        {
            return tableName;
        }
    }
}
