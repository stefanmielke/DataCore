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
        public override string GetDropColumnIfExistsQuery(string tableName, string memberName)
        {
            return "SELECT 1";
        }

        /// <summary>
        /// SQLite does NOT support this method.
        /// </summary>
        public override string GetCreateForeignKeyIfNotExistsQuery(string indexName, string tableNameFrom, string columnNameFrom,
            string tableNameTo, string columnNameTo)
        {
            return "SELECT 1";
        }

        /// <summary>
        /// SQLite does NOT support this method.
        /// </summary>
        public override string GetDropForeignKeyIfExistsQuery(string tableName, string indexName)
        {
            return "SELECT 1";
        }

        public override string GetLengthFunctionName()
        {
            return "length";
        }

        public override string GetIsNullFunctionName()
        {
            return "IFNULL";
        }
    }
}
