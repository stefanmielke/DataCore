﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataCore.Database.Oracle
{
    public class OracleTranslator : Translator
    {
        public override void Top<T>(Query<T> query, int count)
        {
            query.Where("ROWNUM <= " + count);
        }

        public override void Paginate<T>(Query<T> query, int recordsPerPage, int currentPage)
        {
            var min = recordsPerPage * (currentPage - 1);
            var max = recordsPerPage * currentPage;

            query.Where(string.Concat("ROWNUM > ", min, " AND ROWNUM <= ", max));
        }

        public override string GetExistsQuery(string query)
        {
            return string.Concat("SELECT 1 FROM DUAL WHERE EXISTS (", query, ")");
        }

        public override string GetIsNullFunctionName()
        {
            return "NVL";
        }

        public override IEnumerable<string> GetDropTableIfExistsQuery(string tableName)
        {
            yield return CatchException(string.Concat("DROP TABLE ", tableName), -942);

            yield return CatchException(string.Concat("DROP SEQUENCE ", tableName, "_sequence"));
            yield return CatchException(string.Concat("DROP TRIGGER ", tableName, "_on_insert"));
        }

        public override IEnumerable<string> GetCreateTableIfNotExistsQuery(string tableName, IEnumerable<FieldDefinition> fields)
        {
            var fieldList = fields.ToList();

            var query = new StringBuilder("CREATE TABLE ");
            query.Append(tableName)
                .Append(" (");

            query.Append(string.Join(",", fieldList.Select(GetStringForColumn)));
            query.Append(")");

            yield return CatchException(query.ToString(), -955);

            var identity = fieldList.FirstOrDefault(f => f.IsIdentity);
            if (identity != null)
            {
                var sequenceName = tableName + "_sequence";
                var triggerName = tableName + "_on_insert";

                yield return CatchException(string.Concat("CREATE SEQUENCE ", sequenceName));

                query.Clear();
                query.Append("CREATE OR REPLACE TRIGGER ");
                query.Append(triggerName);
                query.Append(" BEFORE INSERT ON ");
                query.Append(tableName);
                query.Append(" FOR EACH ROW BEGIN SELECT ");
                query.Append(sequenceName);
                query.Append(".nextval INTO :new.");
                query.Append(identity.Name);
                query.Append(" FROM dual; END;");

                yield return CatchException(query.ToString());
            }
        }

        public override string GetCreateColumnIfNotExistsQuery(string tableName, FieldDefinition field)
        {
            return string.Concat("ALTER TABLE ", tableName, " ADD ", GetStringForColumn(field));
        }

        public override string GetCreateIndexIfNotExistsQuery(string indexName, string tableName, string columns, bool unique)
        {
            return
                CatchException(
                    string.Concat("CREATE", unique ? " UNIQUE" : "", " INDEX ", indexName, " ON ", tableName, "(",
                        columns, ")"), -955);
        }

        public override string GetDropIndexIfExistsQuery(string tableName, string indexName)
        {
            return CatchException(string.Concat("DROP INDEX ", indexName), -1418);
        }

        protected override string GetStringForColumn(FieldDefinition field)
        {
            var nullable = field.Nullable ? "NULL" : "NOT NULL";
            var primaryKey = field.IsPrimaryKey ? " PRIMARY KEY" : "";

            var extra = string.Concat(nullable, primaryKey);

            return string.Format(GetFormatFor(field), field.Name, GetTextFor(field.Type), field.Size, extra);
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

        public override string GetSelectTableName(string tableName)
        {
            return tableName;
        }

        public override object GetBooleanValue(object value)
        {
            return (bool)value ? 1 : 0;
        }

        private string CatchException(string sql, int exceptionCode)
        {
            return string.Concat("BEGIN EXECUTE IMMEDIATE '", sql, "' ; EXCEPTION WHEN OTHERS THEN IF SQLCODE != ",
                exceptionCode, " THEN RAISE; END IF; END;");
        }

        private string CatchException(string sql)
        {
            return string.Concat("BEGIN EXECUTE IMMEDIATE '", sql, "' ; EXCEPTION WHEN OTHERS THEN NULL; END;");
        }
    }
}
