﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace DataCore
{
    public interface ITranslator
    {
        void Top<T>(Query<T> query, int count);
        void Count<T>(Query<T> query);
        void Paginate<T>(Query<T> query, int recordsPerPage, int currentPage);

        string GetInsertQuery(TableDefinition table, string names, string values);
        string GetUpdateQuery(string tableName, IEnumerable<KeyValuePair<string, string>> nameValues, string where, Parameters parameters);
        string GetDeleteQuery(string tableName, string whereQuery, Parameters parameters);

        string GetParameterTag();
        string GetFormatFor(ExpressionType type);
        string GetTextFor(Type type);
        DbType GetTypeForProperty(Type type);
        DbType GetTypeForProperty(PropertyInfo propertyInfo);

        object GetBooleanValue(object constantExpressionValue);
        object GetStringValue(object value);
        object GetDateTimeValue(object date);

        string GetPropertyName(PropertyInfo type);
        string GetTableName(Type type);
        string GetSelectTableName(Type type);
        string GetSelectTableName(string tableName);
        IEnumerable<string> GetCreateTableIfNotExistsQuery(string tableName, IEnumerable<FieldDefinition> fields);
        IEnumerable<string> GetDropTableIfExistsQuery(string tableName);
        string GetCreateColumnIfNotExistsQuery(string tableName, FieldDefinition field);
        string GetDropColumnIfExistsQuery(string tableName, string memberName);
        string GetCreateIndexIfNotExistsQuery(string indexName, string tableName, string columns, bool unique);
        string GetDropIndexIfExistsQuery(string tableName, string indexName);
        string GetCreateForeignKeyIfNotExistsQuery(string indexName, string tableNameFrom, string columnNameFrom, string tableNameTo, string columnNameTo);
        string GetDropForeignKeyIfExistsQuery(string tableName, string indexName);
        string GetExistsQuery(string query);
        string GetOrderByDescendingFormat();
        string GetLengthFunctionName();
        string GetIsNullFunctionName();

        string GetAliasFormat();
    }
}