using System;
using System.Collections.Generic;
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
            var query = new StringBuilder("CREATE TABLE ");
            query.Append(tableName)
                .Append(" (");

            query.Append(string.Join(",", 
                fields.Select(
                    field =>
                    {
                        var format = field.Type == FieldType.Varchar ? "{0} {1}({2}) {3}" : "{0} {1} {3}";

                        return string.Format(format, field.Name, GetTextFor(field.Type), field.Size,
                            field.Nullable ? "NULL" : "NOT NULL");
                    })
                ));

            query.Append(")");

            return query.ToString();
        }

        private static string GetTextFor(FieldType type)
        {
            switch (type)
            {
                case FieldType.Varchar:
                    return "VARCHAR";
                case FieldType.Int:
                    return "INT";
                case FieldType.Bool:
                    return "BOOLEAN";
                case FieldType.Float:
                    return "REAL";
                case FieldType.Decimal:
                    return "REAL";
                default:
                    return "";
            }
        }
    }
}
