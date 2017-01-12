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
            query.SqlSelectFormat = "TOP (" + count + ") {0}";
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

        public string GetStringValue(object value)
        {
            return string.Concat("'", value, "'");
        }

        public string GetDateTimeValue(DateTime date)
        {
            return string.Concat("'", date.ToString("yyyy-MM-dd HH:mm:ss.fff"), "'");
        }

        public string GetCreateTableQuery(string tableName, IEnumerable<FieldDefinition> fields)
        {
            var query = new StringBuilder("CREATE TABLE IF NOT EXISTS ");
            query.Append(tableName)
                .Append(" (");

            query.Append(string.Join(",", 
                fields.Select(
                    field => string.Format(GetFormatFor(field), field.Name, GetTextFor(field.Type), field.Size,
                        field.Nullable ? "NULL" : "NOT NULL"))
                ));

            query.Append(")");

            return query.ToString();
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

        public virtual string GetTableName(string tableName)
        {
            return string.Concat(tableName, " WITH(NOLOCK)");
        }
    }
}
