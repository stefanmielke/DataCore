using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataCore.Database.SqlServer
{
    public class SqlServerTranslator : Translator
    {
        public override string GetCreateTableIfNotExistsQuery(string tableName, IEnumerable<FieldDefinition> fields)
        {
            var query = new StringBuilder("IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = '").Append(tableName)
                .Append("') CREATE TABLE ")
                .Append(tableName)
                .Append(" (");

            query.Append(string.Join(",", fields.Select(GetStringForColumn)));

            query.Append(")");

            return query.ToString();
        }

        public override string GetCreateForeignKeyIfNotExistsQuery(string indexName, string tableNameFrom, string columnNameFrom,
            string tableNameTo, string columnNameTo)
        {
            return string.Concat("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = '", indexName,
                "' AND object_id = OBJECT_ID('", tableNameFrom, "')) ALTER TABLE ", tableNameFrom, " ADD CONSTRAINT ",
                indexName, " FOREIGN KEY (", columnNameFrom, ") REFERENCES ", tableNameTo, " (", columnNameTo, ")");
        }

        public override string GetDropForeignKeyIfExistsQuery(string tableName, string indexName)
        {
            return string.Concat("IF EXISTS (SELECT * FROM sys.indexes WHERE name = '", indexName,
                "' AND object_id = OBJECT_ID('", tableName, "')) ALTER TABLE ", tableName, " DROP CONSTRAINT ",
                indexName);
        }

        public override string GetCreateIndexIfNotExistsQuery(string indexName, string tableName, string columns, bool unique)
        {
            return string.Concat("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = '", indexName,
                "' AND object_id = OBJECT_ID('", tableName, "')) CREATE", unique ? " UNIQUE" : "",
                " INDEX ", indexName, " ON ", tableName, "(", columns, ")");
        }

        public override string GetDropIndexIfExistsQuery(string tableName, string indexName)
        {
            return string.Concat("DROP INDEX IF EXISTS ", indexName, " ON ", tableName);
        }

        public override string GetCreateColumnIfNotExistsQuery(string tableName, FieldDefinition field)
        {
            return string.Concat("ALTER TABLE ", tableName, " ADD ", GetStringForColumn(field));
        }

        public override string GetExistsQuery(string query)
        {
            return string.Concat("IF EXISTS (", query, ") SELECT 1 ELSE SELECT 0");
        }

        protected override string GetTextFor(DbType type)
        {
            switch (type)
            {
                case DbType.Boolean:
                    return "BIT";
                case DbType.Double:
                case DbType.Decimal:
                case DbType.Single:
                    return "FLOAT";
                case DbType.Time:
                    return "DATETIME";
                case DbType.Binary:
                case DbType.Byte:
                case DbType.SByte:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return "INT";
                case DbType.Currency:
                case DbType.VarNumeric:
                    return "FLOAT";
                case DbType.Guid:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Object:
                case DbType.Xml:
                    return "VARCHAR";
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return "DATETIME2";
                default:
                    return "INT";
            }
        }
    }
}
