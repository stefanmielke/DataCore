using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Dapper;
using System.Linq.Expressions;
using System;

namespace DataCore.Database
{
    public abstract class Database : IDatabase
    {
        private readonly IDbConnection _connection;
        private readonly ITranslator _translator;

        protected Database(IDbConnection connection, ITranslator translator)
        {
            _connection = connection;
            _translator = translator;
        }

        public Query<T> From<T>()
        {
            return new Query<T>(_translator);
        }

        public void Insert<T>(T obj)
        {
            Insert(typeof(T), obj);
        }

        private void Insert<T>(Type type, T obj)
        {
            var tableName = type.Name;

            var properties = GetPropertiesForType(type);

            var names = string.Join(", ", properties.Select(p => p.Name));
            var values = string.Join(", ",
                properties.Select(p => ExpressionHelper.GetValueFrom(_translator, p.PropertyType, p.GetValue(obj, null))));

            var query = _translator.GetInsertQuery(tableName, names, values);

            Execute(query);
        }

        public int CreateTableIfNotExists<T>()
        {
            var type = typeof(T);

            var tableName = type.Name;
            var fields =
                type.GetProperties()
                    .Select(p => new FieldDefinition { Name = p.Name, Nullable = false, Size = 255, Type = GetTypeForProperty(p) });

            var query = _translator.GetCreateTableIfNotExistsQuery(tableName, fields);

            return Execute(query);
        }

        public int DropTableIfExists<T>()
        {
            var query = _translator.GetDropTableIfExistsQuery(typeof(T).Name);

            return Execute(query);
        }

        public int CreateColumnIfNotExists<T>(Expression<Func<T, dynamic>> clause)
        {
            var arguments = ExpressionHelper.GetExpressionsFromDynamic(clause);
            if (arguments != null && arguments.Length > 0)
            {
                var tableName = typeof(T).Name;

                var query = string.Join(";",
                    arguments.Select(
                        f =>
                            _translator.GetCreateColumnIfNoExistsQuery(
                                tableName,
                                new FieldDefinition
                                {
                                    Name = ((MemberExpression)f).Member.Name,
                                    Nullable = true,
                                    Size = 255,
                                    Type = GetTypeForProperty(((MemberExpression)f).Member as PropertyInfo)
                                }
                            )
                    )
                );

                return Execute(query);
            }

            return 0;
        }

        public int DropColumnIfExists<T>(Expression<Func<T, dynamic>> clause)
        {
            var arguments = ExpressionHelper.GetExpressionsFromDynamic(clause);
            if (arguments != null && arguments.Length > 0)
            {
                var tableName = typeof(T).Name;

                var query = string.Join(";",
                    arguments.Select(
                        f =>
                            _translator.GetDropColumnIfExistsQuery(tableName, ((MemberExpression)f).Member.Name)
                    )
                );

                return Execute(query);
            }

            return 0;
        }

        public int CreateIndexIfNotExists<T>(Expression<Func<T, dynamic>> clause, bool unique = false, string indexName = null)
        {
            var arguments = ExpressionHelper.GetExpressionsFromDynamic(clause);
            if (arguments != null && arguments.Length > 0)
            {
                var tableName = typeof(T).Name;
                var columns = string.Join(", ", arguments.Select(f => ((MemberExpression)f).Member.Name));
                if (string.IsNullOrEmpty(indexName))
                    indexName = string.Concat("IX_", tableName, "_", columns.Replace(", ", "_"));

                var query = _translator.GetCreateIndexIfNotExistsQuery(indexName, tableName, columns, unique);

                return Execute(query);
            }

            return 0;
        }

        public int DropIndexIfExists<T>(string indexName)
        {
            var tableName = typeof(T).Name;

            var query = _translator.GetDropIndexIfExistsQuery(tableName, indexName);

            return Execute(query);
        }

        public int CreateForeignKeyIfNotExists<TFrom, TTo>(Expression<Func<TFrom, dynamic>> columnFrom, Expression<Func<TTo, dynamic>> columnTo, string indexName = null)
        {
            var argumentsFrom = ExpressionHelper.GetExpressionsFromDynamic(columnFrom);
            var argumentsTo = ExpressionHelper.GetExpressionsFromDynamic(columnTo);

            if (argumentsFrom != null && argumentsFrom.Length > 0
               && argumentsTo != null && argumentsTo.Length > 0)
            {
                var tableNameFrom = typeof(TFrom).Name;
                var tableNameTo = typeof(TTo).Name;

                var columnNameFrom = ((MemberExpression)argumentsFrom.First()).Member.Name;
                var columnNameTo = ((MemberExpression)argumentsTo.First()).Member.Name;

                if (string.IsNullOrEmpty(indexName))
                    indexName = string.Concat("FK_", tableNameFrom, "_", columnNameFrom, "_", tableNameTo, "_", columnNameTo);

                var query = _translator.GetCreateForeignKeyIfNotExistsQuery(indexName, tableNameFrom, columnNameFrom, tableNameTo, columnNameTo);

                return Execute(query);
            }

            return 0;
        }

        public int DropForeignKeyIfExists<T>(string indexName)
        {
            var tableName = typeof(T).Name;

            var query = _translator.GetDropForeignKeyIfExistsQuery(tableName, indexName);

            return Execute(query);
        }

        public IEnumerable<T> Select<T>(Query<T> query)
        {
            if (!query.Built)
                query.Build();

            return Execute<T>(query.SqlCommand.ToString());
        }

        public IEnumerable<T> Execute<T>(string query)
        {
            return _connection.Query<T>(query);
        }

        public int Execute(string query)
        {
            return _connection.Execute(query);
        }

        private static DbType GetTypeForProperty(PropertyInfo propertyInfo)
        {
            switch (propertyInfo.PropertyType.Name)
            {
                case "String":
                    return DbType.String;
                case "Int":
                    return DbType.Int32;
                case "Boolean":
                    return DbType.Boolean;
                case "Float":
                    return DbType.Single;
                case "Decimal":
                    return DbType.Decimal;
                case "DateTime":
                    return DbType.DateTime;
                default:
                    return DbType.Int32;
            }
        }

        private PropertyInfo[] GetPropertiesForType(Type type)
        {
            return type.GetProperties();
        }
    }
}
