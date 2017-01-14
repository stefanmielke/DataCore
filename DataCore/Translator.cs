using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DataCore
{
    public class Translator : ITranslator
    {
        public void Top<T>(Query<T> query, int count)
        {
            query.SqlEnd = string.Concat("LIMIT ", count);
        }

        public void Count<T>(Query<T> query)
        {
            query.SqlSelectFormat = "COUNT({0})";
        }

        public void Paginate<T>(Query<T> query, int recordsPerPage, int currentPage)
        {
            query.SqlEnd = string.Concat("LIMIT ", recordsPerPage, ", ", (currentPage - 1) * recordsPerPage);
        }

        public string GetFormatFor(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                    return "({0} = {1})";
                case ExpressionType.GreaterThan:
                    return "({0} > {1})";
                case ExpressionType.GreaterThanOrEqual:
                    return "({0} >= {1})";
                case ExpressionType.LessThan:
                    return "({0} < {1})";
                case ExpressionType.LessThanOrEqual:
                    return "({0} <= {1})";
                case ExpressionType.NotEqual:
                    return "({0} != {1})";
                case ExpressionType.AndAlso:
                    return "({0} AND {1})";
                case ExpressionType.OrElse:
                    return "({0} OR {1})";
                default:
                    return "({0} {1})";
            }
        }

        public string GetBooleanValue(object value)
        {
            var isTrue = (bool)value;

            return isTrue ? "1" : "0";
        }

        public string GetStringValue(object value)
        {
            return string.Concat("'", value, "'");
        }

        public string GetDateTimeValue(DateTime date)
        {
            return string.Concat("'", date.ToString("yyyy-MM-dd HH:mm:ss.fff"), "'");
        }

        public string GetCreateTableIfNotExistsQuery(string tableName, IEnumerable<FieldDefinition> fields)
        {
            var query = new StringBuilder("CREATE TABLE IF NOT EXISTS ");
            query.Append(tableName)
                .Append(" (");

            query.Append(string.Join(",",
                fields.Select(
                    field => GetStringForColumn(field))
                ));

            query.Append(")");

            return query.ToString();
        }

        public string GetDropTableIfExistsQuery(string tableName)
        {
            return string.Concat("DROP TABLE IF EXISTS ", tableName);
        }

        private string GetFormatFor(FieldDefinition field)
        {
            switch (field.Type)
            {
                case DbType.Boolean:
                    return "{0} {1} {3}";
                case DbType.Double:
                case DbType.Decimal:
                case DbType.Single:
                    return "{0} {1} {3}";
                case DbType.Time:
                    return "{0} {1} {3}";
                case DbType.Binary:
                case DbType.Byte:
                case DbType.SByte:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return "{0} {1} {3}";
                case DbType.Currency:
                case DbType.VarNumeric:
                    return "{0} {1} {3}";
                case DbType.Guid:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Object:
                case DbType.Xml:
                    return "{0} {1}({2}) {3}";
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return "{0} {1} {3}";
                default:
                    return "{0} {1} {3}";
            }
        }

        private string GetTextFor(DbType type)
        {
            switch (type)
            {
                case DbType.Boolean:
                    return "BOOLEAN";
                case DbType.Double:
                case DbType.Decimal:
                case DbType.Single:
                    return "REAL";
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
                    return "REAL";
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
                    return "DATETIME";
                default:
                    return "INT";
            }
        }

        public string GetCreateColumnIfNoExistsQuery(string tableName, FieldDefinition field)
        {
            return string.Concat("ALTER TABLE ", tableName, " ADD COLUMN ", GetStringForColumn(field));
        }

        public virtual string GetTableName(string tableName)
        {
            return string.Concat(tableName, " WITH(NOLOCK)");
        }

        private string GetStringForColumn(FieldDefinition field)
        {
            return string.Format(GetFormatFor(field), field.Name, GetTextFor(field.Type), field.Size,
                           field.Nullable ? "NULL" : "NOT NULL");
        }

        public virtual string GetDropColumnIfExistsQuery(string tableName, string memberName)
        {
            return string.Concat("ALTER TABLE ", tableName, " DROP COLUMN ", memberName);
        }

        public string GetCreateIndexIfNotExistsQuery(string indexName, string tableName, string columns, bool unique)
        {
            return string.Concat("CREATE", unique ? " UNIQUE" : "", " INDEX IF NOT EXISTS ", indexName, " ON ", tableName, "(", columns, ")");
        }

        public string GetDropIndexIfExistsQuery(string tableName, string indexName)
        {
            return string.Concat("DROP INDEX IF EXISTS ", indexName);
        }

        public virtual string GetCreateForeignKeyIfNotExistsQuery(string indexName, string tableNameFrom, string columnNameFrom,
            string tableNameTo, string columnNameTo)
        {
            return string.Concat("ALTER TABLE ", tableNameFrom, " ADD CONSTRAINT ", indexName, " FOREIGN KEY (",
                columnNameFrom, ") REFERENCES ", tableNameTo, " (", columnNameTo, ")");
        }

        public virtual string GetDropForeignKeyIfExistsQuery(string tableName, string indexName)
        {
            return string.Concat("ALTER TABLE ", tableName, " DROP CONSTRAINT ", indexName);
        }
    }
}
