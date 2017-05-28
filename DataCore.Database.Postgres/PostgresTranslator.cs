using System.Data;

namespace DataCore.Database.Postgres
{
    public class PostgresTranslator : Translator
    {
        public override string GetCreateDatabaseIfNotExistsQuery(string name)
        {
            return string.Concat("DO $do$ BEGIN IF EXISTS(SELECT 1 FROM pg_database WHERE datname = '", name,
                "') THEN CREATE DATABASE ", name, "; END IF; END $do$ ");
        }
        
        public override void Paginate<T>(Query<T> query, int recordsPerPage, int currentPage)
        {
            query.SqlEnd = string.Concat("LIMIT ", recordsPerPage, " OFFSET ", (currentPage - 1) * recordsPerPage);
        }

        protected override string GetStringForColumn(FieldDefinition field)
        {
            var nullable = field.Nullable ? "NULL" : "NOT NULL";
            var primaryKey = field.IsPrimaryKey ? " PRIMARY KEY" : null;

            var extra = primaryKey ?? nullable;

            return string.Format(GetFormatFor(field), field.Name, field.IsIdentity ? "SERIAL" : GetTextFor(field.Type), field.Size, field.Precision, extra);
        }

        protected override string GetFormatFor(FieldDefinition field)
        {
            switch (field.Type)
            {
                case DbType.Boolean:
                    return "{0} {1} {4}";
                case DbType.Double:
                case DbType.Decimal:
                case DbType.Single:
                    return "{0} {1} {4}";
                case DbType.Time:
                    return "{0} {1} {4}";
                case DbType.Binary:
                case DbType.Byte:
                case DbType.SByte:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return "{0} {1} {4}";
                case DbType.Currency:
                case DbType.VarNumeric:
                    return "{0} {1} {4}";
                case DbType.Guid:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Object:
                case DbType.Xml:
                    return "{0} {1}({2}) {4}";
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return "{0} {1} {4}";
                default:
                    return "{0} {1} {4}";
            }
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
                    return "TIMESTAMP";
                default:
                    return "INTEGER";
            }
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
            return "char_length";
        }

        public override string GetAliasFormat()
        {
            return "\"{0}\"";
        }
    }
}
