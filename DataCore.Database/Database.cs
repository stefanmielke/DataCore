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
                default:
                    return DbType.Int32;
            }
        }
    }
}
