namespace DataCore.Database.Sqlite
{
    public class SqliteTranslator : Translator
    {
        public override string GetTableName(string tableName)
        {
            return tableName;
        }

        /// <summary>
        /// SQLite does NOT support this method.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public override string GetDropColumnIfExistsQuery(string tableName, string memberName)
        {
            return "SELECT 1";
        }
    }
}
