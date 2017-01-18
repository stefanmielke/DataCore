using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataCore.Database.Oracle
{
    public class OracleTranslator : Translator
    {
        public override string GetDropTableIfExistsQuery(string tableName)
        {
            return CatchException(string.Concat("DROP TABLE ", tableName), -942);
        }

        public override string GetCreateTableIfNotExistsQuery(string tableName, IEnumerable<FieldDefinition> fields)
        {
            var query = new StringBuilder("CREATE TABLE ");
            query.Append(tableName)
                .Append(" (");

            query.Append(string.Join(",", fields.Select(GetStringForColumn)));

            query.Append(")");

            return CatchException(query.ToString(), 955);
        }

        protected override string GetFormatFor(FieldDefinition field)
        {
            switch (field.Type)
            {
                case DbType.Boolean:
                    return "{0} {1}(1) {3}";
                case DbType.Time:
                    return "{0} {1} {3}";
                case DbType.Double:
                case DbType.Decimal:
                case DbType.Single:
                case DbType.Binary:
                case DbType.Byte:
                case DbType.SByte:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.Currency:
                case DbType.VarNumeric:
                    return "{0} {1}(3) {3}";
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
                    return "{0} {1}(3) {3}";
            }
        }

        public override string GetTextFor(DbType type)
        {
            switch (type)
            {
                case DbType.Boolean:
                    return "NUMBER";
                case DbType.Time:
                    return "TIMESTAMP";
                case DbType.Double:
                case DbType.Decimal:
                case DbType.Single:
                case DbType.Binary:
                case DbType.Byte:
                case DbType.SByte:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.Currency:
                case DbType.VarNumeric:
                    return "NUMBER";
                case DbType.Guid:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Object:
                case DbType.Xml:
                    return "VARCHAR2";
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return "TIMESTAMP";
                default:
                    return "NUMBER";
            }
        }

        public override string GetParameterTag()
        {
            return ":";
        }

        public override string GetTableName(string tableName)
        {
            return tableName;
        }

        private string CatchException(string sql, int exceptionCode)
        {
            return string.Concat("BEGIN EXECUTE IMMEDIATE '", sql, "' ; EXCEPTION WHEN OTHERS THEN IF SQLCODE != ",
                exceptionCode, " THEN RAISE; END IF; END");
        }
    }
}
