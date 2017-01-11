using System;
using System.Linq.Expressions;

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
    }
}
