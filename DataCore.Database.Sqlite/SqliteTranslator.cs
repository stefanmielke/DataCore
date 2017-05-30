using System;
using System.IO;

namespace DataCore.Database.Sqlite
{
    public class SqliteTranslator : Translator
    {
        public override string GetDatabaseExistsQuery(string name)
        {
            return "SELECT " + (File.Exists(name) ? "1" : "0");
        }

        public override string GetTableExistsQuery(string tableName)
        {
            return string.Concat("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='", tableName, "'");
        }

        /// <summary>
        /// SQLite does NOT support this method.
        /// </summary>
        public override string GetColumnExistsQuery(string tableName, string columnName)
        {
            throw new NotSupportedException("SQLite does NOT support this method.");
        }

        public override string GetIndexExistsQuery(string indexName, string tableName)
        {
            return string.Concat("SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND tbl_name='", tableName,
                "' AND name='", indexName, "'");
        }

        /// <summary>
        /// SQLite does NOT support this method.
        /// </summary>
        public override string GetForeignKeyExistsQuery(string indexName, string tableName)
        {
            throw new NotSupportedException("SQLite does NOT support this method.");
        }

        protected override string GetSelectTableName(string tableName)
        {
            return tableName;
        }

        public override string GetCreateDatabaseQuery(string name)
        {
            File.WriteAllBytes(name, new byte[0]);

            return "SELECT 1";
        }

        public override string GetCreateDatabaseIfNotExistsQuery(string name)
        {
            if (!File.Exists(name))
                File.WriteAllBytes(name, new byte[0]);

            return "SELECT 1";
        }

        public override string GetDropDatabaseQuery(string name)
        {
            File.Delete(name);

            return "SELECT 1";
        }

        public override string GetDropDatabaseIfExistsQuery(string name)
        {
            if (File.Exists(name))
                File.Delete(name);

            return "SELECT 1";
        }

        /// <summary>
        /// SQLite does NOT support this method.
        /// </summary>
        public override string GetDropColumnQuery(string tableName, string memberName)
        {
            throw new NotSupportedException("SQLite does NOT support this method.");
        }

        /// <summary>
        /// SQLite does NOT support this method.
        /// </summary>
        public override string GetDropColumnIfExistsQuery(string tableName, string memberName)
        {
            throw new NotSupportedException("SQLite does NOT support this method.");
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
        public override string GetCreateForeignKeyQuery(string indexName, string tableNameFrom, string columnNameFrom,
            string tableNameTo, string columnNameTo)
        {
            return "SELECT 1";
        }

        /// <summary>
        /// SQLite does NOT support this method.
        /// </summary>
        public override string GetDropForeignKeyQuery(string tableName, string indexName)
        {
            throw new NotSupportedException("SQLite does NOT support this method.");
        }

        /// <summary>
        /// SQLite does NOT support this method.
        /// </summary>
        public override string GetDropForeignKeyIfExistsQuery(string tableName, string indexName)
        {
            throw new NotSupportedException("SQLite does NOT support this method.");
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
