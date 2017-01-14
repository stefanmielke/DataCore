using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataCore
{
    public interface ITranslator
    {
        void Top<T>(Query<T> query, int count);
        void Count<T>(Query<T> query);
        void Paginate<T>(Query<T> query, int recordsPerPage, int currentPage);

        string GetInsertQuery(string tableName, string names, string values);
        string GetUpdateQuery(string tableName, IEnumerable<KeyValuePair<string, string>> nameValues, string where);
        string GetDeleteQuery(string tableName, string whereQuery);

        string GetFormatFor(ExpressionType type);

        string GetBooleanValue(object constantExpressionValue);
        string GetStringValue(object value);
        string GetDateTimeValue(DateTime date);

        string GetTableName(string tableName);
        string GetCreateTableIfNotExistsQuery(string tableName, IEnumerable<FieldDefinition> fields);
        string GetDropTableIfExistsQuery(string tableName);
        string GetCreateColumnIfNoExistsQuery(string tableName, FieldDefinition field);
        string GetDropColumnIfExistsQuery(string tableName, string memberName);
        string GetCreateIndexIfNotExistsQuery(string indexName, string tableName, string columns, bool unique);
        string GetDropIndexIfExistsQuery(string tableName, string indexName);
        string GetCreateForeignKeyIfNotExistsQuery(string indexName, string tableNameFrom, string columnNameFrom, string tableNameTo, string columnNameTo);
        string GetDropForeignKeyIfExistsQuery(string tableName, string indexName);
        string GetExistsQuery(string query);
    }
}