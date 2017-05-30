using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataCore
{
    public interface ITranslator
    {
        string GetDatabaseExistsQuery(string name);
        string GetCreateDatabaseQuery(string name);
        string GetCreateDatabaseIfNotExistsQuery(string name);
        string GetDropDatabaseQuery(string name);
        string GetDropDatabaseIfExistsQuery(string name);

        void Top<T>(Query<T> query, int count);
        void Count<T>(Query<T> query);
        void Paginate<T>(Query<T> query, int recordsPerPage, int currentPage);

        string GetInsertQuery(TableDefinition table, string names, string values);
        string GetUpdateQuery(string tableName, IEnumerable<KeyValuePair<string, string>> nameValues, string where, Parameters parameters);
        string GetDeleteQuery(string tableName, string whereQuery, Parameters parameters);

        string GetParameterTag();
        string GetFormatFor(ExpressionType type);
        string GetTextFor(FieldDefinition field, bool isCasting = false);

        object GetBooleanValue(object constantExpressionValue);
        object GetStringValue(object value);
        object GetDateTimeValue(object date);
        
        string GetSelectTableName(TableDefinition table);

        string GetTableExistsQuery(string tableName);
        IEnumerable<string> GetCreateTableQuery(string tableName, IEnumerable<FieldDefinition> fields);
        IEnumerable<string> GetCreateTableIfNotExistsQuery(string tableName, IEnumerable<FieldDefinition> fields);
        IEnumerable<string> GetDropTableQuery(string tableName);
        IEnumerable<string> GetDropTableIfExistsQuery(string tableName);

        string GetColumnExistsQuery(string tableName, string columnName);
        string GetCreateColumnQuery(string tableName, FieldDefinition field);
        string GetCreateColumnIfNotExistsQuery(string tableName, FieldDefinition field);
        string GetDropColumnQuery(string tableName, string memberName);
        string GetDropColumnIfExistsQuery(string tableName, string memberName);

        string GetIndexExistsQuery(string indexName, string tableName);
        string GetCreateIndexQuery(string indexName, string tableName, string columns, bool unique);
        string GetCreateIndexIfNotExistsQuery(string indexName, string tableName, string columns, bool unique);
        string GetDropIndexQuery(string tableName, string indexName);
        string GetDropIndexIfExistsQuery(string tableName, string indexName);

        string GetForeignKeyExistsQuery(string indexName, string tableNameFrom);
        string GetCreateForeignKeyQuery(string indexName, string tableNameFrom, string columnNameFrom, string tableNameTo, string columnNameTo);
        string GetCreateForeignKeyIfNotExistsQuery(string indexName, string tableNameFrom, string columnNameFrom, string tableNameTo, string columnNameTo);
        string GetDropForeignKeyQuery(string tableName, string indexName);
        string GetDropForeignKeyIfExistsQuery(string tableName, string indexName);

        string GetExistsQuery(string query);
        string GetOrderByDescendingFormat();
        string GetLengthFunctionName();
        string GetIsNullFunctionName();

        string GetAliasFormat();
    }
}