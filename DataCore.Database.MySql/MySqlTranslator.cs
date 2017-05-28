using System.Data;

namespace DataCore.Database.MySql
{
    public class MySqlTranslator : Translator
    {
        protected override string GetStringForColumn(FieldDefinition field)
        {
            var nullable = field.Nullable ? "NULL" : "NOT NULL";
            var primaryKey = field.IsPrimaryKey ? " PRIMARY KEY" : null;

            var extra = primaryKey ?? nullable;

            return string.Format(GetFormatFor(field), field.Name, GetTextFor(field.Type), field.Size, field.Precision, extra,
                field.IsIdentity ? "AUTO_INCREMENT" : "");
        }

        protected override string GetSelectTableName(string tableName)
        {
            return tableName;
        }

        public override string GetIsNullFunctionName()
        {
            return "coalesce";
        }

        public override string GetLengthFunctionName()
        {
            return "length";
        }

        public override string GetAliasFormat()
        {
            return "\"{0}\"";
        }

        public override string GetDropIndexQuery(string tableName, string indexName)
        {
            return string.Concat("ALTER TABLE ", tableName, " DROP INDEX ", indexName);
        }

        public override string GetDropIndexIfExistsQuery(string tableName, string indexName)
        {
            return
                string.Concat(
                    "IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME = '",
                    tableName,
                    "' AND INDEX_NAME = '", indexName,
                    "') THEN ALTER TABLE ", tableName, " DROP INDEX ", indexName, "; END IF;");
        }

        public override string GetCreateIndexIfNotExistsQuery(string indexName, string tableName, string columns, bool unique)
        {
            return
                string.Concat(
                    "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME = '",
                    tableName,
                    "' AND INDEX_NAME = '", indexName,
                    "') THEN CREATE", unique ? " UNIQUE" : "", " INDEX ", indexName, " ON ", tableName, "(", columns,
                    "); END IF;");
        }

        public override string GetDropForeignKeyQuery(string tableName, string indexName)
        {
            return string.Concat("ALTER TABLE ", tableName, " DROP FOREIGN KEY ", indexName);
        }

        public override string GetDropForeignKeyIfExistsQuery(string tableName, string indexName)
        {
            return string.Concat("ALTER TABLE ", tableName, " DROP FOREIGN KEY ", indexName);
        }

        protected override string GetTextFor(DbType type, bool isCasting = false)
        {
            switch (type)
            {
                case DbType.Boolean:
                    return "BOOLEAN";
                case DbType.Double:
                case DbType.Decimal:
                case DbType.Single:
                case DbType.Currency:
                case DbType.VarNumeric:
                    return "REAL";
                case DbType.Guid:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Object:
                case DbType.Xml:
                    return "VARCHAR";
                case DbType.Time:
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return "DATETIME";
                default:
                    return isCasting ? "SIGNED" : "INTEGER";
            }
        }
    }
}
