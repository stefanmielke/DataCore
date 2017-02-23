﻿using DataCore.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DataCore
{
    public class Translator : ITranslator
    {
        public virtual void Top<T>(Query<T> query, int count)
        {
            query.SqlEnd = string.Concat("LIMIT ", count);
        }

        public void Count<T>(Query<T> query)
        {
            query.SqlSelectFormat = "COUNT({0})";
        }

        public virtual void Paginate<T>(Query<T> query, int recordsPerPage, int currentPage)
        {
            query.SqlEnd = string.Concat("LIMIT ", recordsPerPage, ", ", (currentPage - 1) * recordsPerPage);
        }

        public string GetInsertQuery(TableDefinition table, string names, string values)
        {
            if (string.IsNullOrWhiteSpace(names))
                return string.Concat("INSERT INTO ", table.Name, " DEFAULT VALUES");

            return string.Concat("INSERT INTO ", table.Name, "(", names, ") VALUES (", values, ")");
        }

        public string GetUpdateQuery(string tableName, IEnumerable<KeyValuePair<string, string>> nameValues, string where, Parameters parameters)
        {
            var query = string.Concat("UPDATE ", tableName, " SET ");
            query += string.Join(", ", nameValues.Select(nv => string.Concat(nv.Key, "=", nv.Value)));

            if (!string.IsNullOrEmpty(where))
                query += string.Concat(" WHERE ", where);

            return query;
        }

        public string GetDeleteQuery(string tableName, string whereQuery, Parameters parameters)
        {
            return string.Concat("DELETE FROM ", tableName, " WHERE ", whereQuery);
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

        public virtual object GetBooleanValue(object value)
        {
            return (bool)value;
        }

        public object GetStringValue(object value)
        {
            return (string)value;
        }

        public object GetDateTimeValue(object date)
        {
            return Convert.ToDateTime(date);
        }

        public virtual IEnumerable<string> GetCreateTableIfNotExistsQuery(string tableName, IEnumerable<FieldDefinition> fields)
        {
            var query = new StringBuilder("CREATE TABLE IF NOT EXISTS ");
            query.Append(tableName)
                .Append(" (");

            query.Append(string.Join(",", fields.Select(GetStringForColumn)));

            query.Append(")");

            yield return query.ToString();
        }

        public virtual IEnumerable<string> GetDropTableIfExistsQuery(string tableName)
        {
            yield return string.Concat("DROP TABLE IF EXISTS ", tableName);
        }

        protected virtual string GetFormatFor(FieldDefinition field)
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
                    return "{0} {1} {4} {5}";
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

        public string GetTextFor(FieldDefinition field)
        {
            return GetTextFor(field.Type);
        }

        protected virtual string GetTextFor(DbType type)
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
                    return "DATETIME";
                default:
                    return "INTEGER";
            }
        }

        public virtual string GetCreateColumnIfNotExistsQuery(string tableName, FieldDefinition field)
        {
            return string.Concat("ALTER TABLE ", tableName, " ADD COLUMN ", GetStringForColumn(field));
        }

        public string GetSelectTableName(TableDefinition table)
        {
            return GetSelectTableName(table.Name);
        }

        protected virtual string GetSelectTableName(string tableName)
        {
            return string.Concat(tableName, " WITH(NOLOCK)");
        }

        public virtual string GetDropColumnIfExistsQuery(string tableName, string memberName)
        {
            return string.Concat("ALTER TABLE ", tableName, " DROP COLUMN ", memberName);
        }

        public virtual string GetCreateIndexIfNotExistsQuery(string indexName, string tableName, string columns, bool unique)
        {
            return string.Concat("CREATE", unique ? " UNIQUE" : "", " INDEX IF NOT EXISTS ", indexName, " ON ", tableName, "(", columns, ")");
        }

        public virtual string GetDropIndexIfExistsQuery(string tableName, string indexName)
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

        public virtual string GetExistsQuery(string query)
        {
            return string.Concat("SELECT EXISTS (", query, ")");
        }

        public string GetOrderByDescendingFormat()
        {
            return "{0} DESC";
        }

        protected virtual string GetStringForColumn(FieldDefinition field)
        {
            var nullable = field.Nullable ? "NULL" : "NOT NULL";
            var primaryKey = field.IsPrimaryKey ? " PRIMARY KEY" : null;

            var extra = primaryKey ?? nullable;

            return string.Format(GetFormatFor(field), field.Name, GetTextFor(field.Type), field.Size, field.Precision, extra,
                field.IsIdentity ? "AUTOINCREMENT" : "");
        }

        public virtual string GetLengthFunctionName()
        {
            return "LEN";
        }

        public virtual string GetIsNullFunctionName()
        {
            return "ISNULL";
        }

        public virtual string GetParameterTag()
        {
            return "@";
        }

        public virtual string GetAliasFormat()
        {
            return "'{0}'";
        }
    }
}
