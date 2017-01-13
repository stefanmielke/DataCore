using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataCore
{
    public interface ITranslator
    {
        void Top<T>(Query<T> query, int count);
        void Count<T>(Query<T> query);

        string GetFormatFor(ExpressionType type);

        string GetBooleanValue(object constantExpressionValue);
        string GetStringValue(object value);
        string GetDateTimeValue(DateTime date);

        string GetTableName(string tableName);
        string GetCreateTableIfNotExistsQuery(string tableName, IEnumerable<FieldDefinition> fields);
        string GetDropTableIfExistsQuery(string tableName);
        string GetCreateColumnIfNoExistsQuery(string tableName, FieldDefinition field);
        string GetDropColumnIfExistsQuery(string tableName, string memberName);
    }
}