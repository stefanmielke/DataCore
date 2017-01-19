using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Dapper;
using System.Linq.Expressions;
using System;
using DataCore.Attributes;

namespace DataCore.Database
{
    public abstract class Database : IDatabase
    {
        private readonly IDbConnection _connection;
        public ITranslator Translator { get; private set; }

        protected Database(IDbConnection connection, ITranslator translator)
        {
            _connection = connection;
            Translator = translator;
        }

        public Query<T> From<T>()
        {
            return new Query<T>(Translator);
        }

        public void Insert<T>(T obj)
        {
            var type = typeof(T);

            var tableName = type.Name;

            var properties = GetPropertiesForType(type).ToList();

            var parameters = new Parameters();

            var names = string.Join(", ", properties.Select(p => p.Name));
            var values = string.Join(", ",
                properties.Select(p =>
                {
                    var value = ExpressionHelper.GetValueFrom(Translator, p.PropertyType, p.GetValue(obj, null));
                    return parameters.Add(Translator, value);
                })
            );

            var query = Translator.GetInsertQuery(tableName, names, values, parameters);

            Execute(query, parameters);
        }

        public void Update<T>(T obj, Expression<Func<T, dynamic>> whereClause)
        {
            var type = typeof(T);

            var tableName = type.Name;

            var properties = GetPropertiesForType(type);

            var nameValues = new List<KeyValuePair<string, string>>();

            var parameters = new Parameters();
            foreach (var prop in properties)
            {
                var key = parameters.Add(Translator, ExpressionHelper.GetValueFrom(Translator, prop.PropertyType, prop.GetValue(obj, null)));
                nameValues.Add(new KeyValuePair<string, string>(prop.Name, key));
            }

            var newExpression = Expression.Lambda(new QueryVisitor().Visit(whereClause));

            var whereQuery = ExpressionHelper.GetQueryFromExpression(Translator, newExpression.Body, parameters);

            var query = Translator.GetUpdateQuery(tableName, nameValues, whereQuery, parameters);

            Execute(query, parameters);
        }

        public void UpdateOnly<T>(T obj, Expression<Func<T, dynamic>> onlyFields, Expression<Func<T, dynamic>> whereClause)
        {
            var type = typeof(T);

            var tableName = type.Name;

            var properties = GetPropertiesForType(type);

            var fields = ExpressionHelper.GetStringsFromArguments(onlyFields);

            var nameValues = new List<KeyValuePair<string, string>>();

            var parameters = new Parameters();
            foreach (var prop in properties.Where(p => fields.Contains(p.Name)))
            {
                var key = parameters.Add(Translator, ExpressionHelper.GetValueFrom(Translator, prop.PropertyType, prop.GetValue(obj, null)));
                nameValues.Add(new KeyValuePair<string, string>(prop.Name, key));
            }

            var newExpression = Expression.Lambda(new QueryVisitor().Visit(whereClause));
            var whereQuery = ExpressionHelper.GetQueryFromExpression(Translator, newExpression.Body, parameters);

            var query = Translator.GetUpdateQuery(tableName, nameValues, whereQuery, parameters);

            Execute(query, parameters);
        }

        public void Delete<T>(Expression<Func<T, bool>> whereClause)
        {
            var type = typeof(T);

            var tableName = type.Name;

            var parameters = new Parameters();

            var newExpression = Expression.Lambda(new QueryVisitor().Visit(whereClause));
            var whereQuery = ExpressionHelper.GetQueryFromExpression(Translator, newExpression.Body, parameters);

            var query = Translator.GetDeleteQuery(tableName, whereQuery, parameters);

            Execute(query, parameters);
        }

        public int CreateTableIfNotExists<T>()
        {
            var type = typeof(T);

            var tableName = type.Name;
            var fields =
                GetPropertiesForType(type)
                    .Select(
                        p =>
                            new FieldDefinition
                            {
                                Name = p.Name,
                                Nullable = false,
                                Size = 255,
                                Type = Translator.GetTypeForProperty(p),
                                IsPrimaryKey = p.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).Length > 0
                            });

            var query = Translator.GetCreateTableIfNotExistsQuery(tableName, fields);

            return Execute(query);
        }

        public int DropTableIfExists<T>()
        {
            var query = Translator.GetDropTableIfExistsQuery(typeof(T).Name);

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
                            Translator.GetCreateColumnIfNotExistsQuery(
                                tableName,
                                new FieldDefinition
                                {
                                    Name = ((MemberExpression)f).Member.Name,
                                    Nullable = true,
                                    Size = 255,
                                    Type = Translator.GetTypeForProperty(((MemberExpression)f).Member as PropertyInfo)
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
                            Translator.GetDropColumnIfExistsQuery(tableName, ((MemberExpression)f).Member.Name)
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

                var query = Translator.GetCreateIndexIfNotExistsQuery(indexName, tableName, columns, unique);

                return Execute(query);
            }

            return 0;
        }

        public int DropIndexIfExists<T>(string indexName)
        {
            var tableName = typeof(T).Name;

            var query = Translator.GetDropIndexIfExistsQuery(tableName, indexName);

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

                indexName = indexName.Substring(0, Math.Min(20, indexName.Length));

                var query = Translator.GetCreateForeignKeyIfNotExistsQuery(indexName, tableNameFrom, columnNameFrom, tableNameTo, columnNameTo);

                return Execute(query);
            }

            return 0;
        }

        public int DropForeignKeyIfExists<T>(string indexName)
        {
            var tableName = typeof(T).Name;

            var query = Translator.GetDropForeignKeyIfExistsQuery(tableName, indexName);

            return Execute(query);
        }

        public IEnumerable<T> Select<T>(Query<T> query)
        {
            if (!query.Built)
                query.Build();

            return Execute<T>(query.SqlCommand.ToString(), query.Parameters);
        }

        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> clause)
        {
            var query = From<T>().Where(clause);
            if (!query.Built)
                query.Build();

            return Execute<T>(query.SqlCommand.ToString(), query.Parameters);
        }

        public T SelectSingle<T>(Query<T> query)
        {
            if (!query.Built)
                query.Build();

            return Execute<T>(query.SqlCommand.ToString(), query.Parameters).FirstOrDefault();
        }

        public T SelectSingle<T>(Expression<Func<T, bool>> clause)
        {
            var query = From<T>().Where(clause).Top(1);
            if (!query.Built)
                query.Build();

            return Execute<T>(query.SqlCommand.ToString(), query.Parameters).FirstOrDefault();
        }

        public T SelectById<T>(object id)
        {
            var idProperty = GetIdPropertyForType(typeof(T));

            var parameters = new Parameters();
            var paramName = parameters.Add(Translator, id);

            var query = From<T>().Where(idProperty.Name + " = " + paramName).Build();

            return Execute<T>(query.SqlCommand.ToString(), query.Parameters).FirstOrDefault();
        }

        public bool Exists<T>(Query<T> query)
        {
            if (!query.Built)
                query.Build();

            var queryWithExists = Translator.GetExistsQuery(query.SqlCommand.ToString());

            return Execute<int>(queryWithExists, query.Parameters).FirstOrDefault() == 1;
        }

        public IEnumerable<T> Execute<T>(string query)
        {
            return _connection.Query<T>(query);
        }

        public IEnumerable<T> Execute<T>(string query, Parameters parameters)
        {
            return _connection.Query<T>(query, parameters.GetValues());
        }

        public int Execute(string query)
        {
            return _connection.Execute(query);
        }

        public int Execute(string query, Parameters parameters)
        {
            return _connection.Execute(query, parameters.GetValues());
        }

        private IEnumerable<PropertyInfo> GetPropertiesForType(Type type)
        {
            return type.GetProperties().Where(p => p.GetCustomAttributes(typeof(IgnoreAttribute), true).Length == 0);
        }

        private PropertyInfo GetIdPropertyForType(Type type)
        {
            return type.GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).Length > 0);
        }
    }
}
