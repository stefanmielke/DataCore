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

            var tableDefinition = GetTableDefinition(type);

            var properties = tableDefinition.Fields.Where(p => !p.IsIdentity).ToList();

            var parameters = new Parameters();

            var names = string.Join(", ", properties.Select(p => p.Name));
            var values = string.Join(", ",
                properties.Select(p =>
                {
                    var value = ExpressionHelper.GetValueFrom(Translator, p.PropertyInfo.PropertyType, p.PropertyInfo.GetValue(obj, null));
                    return parameters.Add(Translator, value);
                })
            );

            var query = Translator.GetInsertQuery(tableDefinition, names, values);

            Execute(query, parameters);
        }

        public void Update<T>(T obj, Expression<Func<T, dynamic>> whereClause)
        {
            var type = typeof(T);

            var tableDefinition = GetTableDefinition(type);

            var properties = tableDefinition.Fields.Where(p => !p.IsIdentity).ToList();

            var nameValues = new List<KeyValuePair<string, string>>();

            var parameters = new Parameters();
            foreach (var prop in properties)
            {
                var key = parameters.Add(Translator, ExpressionHelper.GetValueFrom(Translator, prop.PropertyInfo.PropertyType, prop.PropertyInfo.GetValue(obj, null)));
                nameValues.Add(new KeyValuePair<string, string>(prop.Name, key));
            }

            var newExpression = Expression.Lambda(new QueryVisitor(parameters).Visit(whereClause));

            var whereQuery = ExpressionHelper.GetQueryFromExpression(Translator, newExpression.Body, parameters);

            var query = Translator.GetUpdateQuery(tableDefinition.Name, nameValues, whereQuery, parameters);

            Execute(query, parameters);
        }

        public void UpdateOnly<T>(T obj, Expression<Func<T, dynamic>> onlyFields, Expression<Func<T, dynamic>> whereClause)
        {
            var type = typeof(T);

            var tableDefinition = GetTableDefinition(type);

            var properties = tableDefinition.Fields;

            var fields = ExpressionHelper.GetStringsFromArguments(onlyFields);

            var nameValues = new List<KeyValuePair<string, string>>();

            var parameters = new Parameters();
            foreach (var prop in properties.Where(p => fields.Contains(p.Name)))
            {
                var key = parameters.Add(Translator, ExpressionHelper.GetValueFrom(Translator, prop.PropertyInfo.PropertyType, prop.PropertyInfo.GetValue(obj, null)));
                nameValues.Add(new KeyValuePair<string, string>(prop.Name, key));
            }

            var newExpression = Expression.Lambda(new QueryVisitor(parameters).Visit(whereClause));
            var whereQuery = ExpressionHelper.GetQueryFromExpression(Translator, newExpression.Body, parameters);

            var query = Translator.GetUpdateQuery(tableDefinition.Name, nameValues, whereQuery, parameters);

            Execute(query, parameters);
        }

        public void Delete<T>(Expression<Func<T, bool>> whereClause)
        {
            var type = typeof(T);

            var tableDefinition = GetTableDefinition(type);

            var parameters = new Parameters();

            var newExpression = Expression.Lambda(new QueryVisitor(parameters).Visit(whereClause));
            var whereQuery = ExpressionHelper.GetQueryFromExpression(Translator, newExpression.Body, parameters);

            var query = Translator.GetDeleteQuery(tableDefinition.Name, whereQuery, parameters);

            Execute(query, parameters);
        }

        public void DeleteById<T>(object id)
        {
            var type = typeof(T);

            var tableDefinition = GetTableDefinition(type);

            var parameters = new Parameters();
            var paramName = parameters.Add(Translator, id);

            var whereQuery = string.Concat(tableDefinition.IdField.Name, " = ", paramName);

            var query = Translator.GetDeleteQuery(tableDefinition.Name, whereQuery, parameters);

            Execute(query, parameters);
        }

        public void DeleteById<T>(params object[] ids)
        {
            var type = typeof(T);

            var tableDefinition = GetTableDefinition(type);

            var parameters = new Parameters();
            foreach (var id in ids)
            {
                parameters.Add(Translator, id);
            }
            var inParams = string.Join(",", parameters.GetValues().Select(kv => kv.Key));

            var whereQuery = string.Concat(tableDefinition.IdField.Name, " IN (", inParams, ")");

            var query = Translator.GetDeleteQuery(tableDefinition.Name, whereQuery, parameters);

            Execute(query, parameters);
        }

        public int CreateTable<T>(bool createReferences = false)
        {
            var type = typeof(T);

            var tableDefinition = GetTableDefinition(type);

            var fields = tableDefinition.Fields;

            var queries = Translator.GetCreateTableQuery(tableDefinition.Name, fields);
            foreach (var query in queries)
            {
                Execute(query);
            }

            foreach (var field in fields.Where(f => f.HasIndex))
            {
                CreateIndex(field.IndexUnique, field.IndexName, tableDefinition.Name, field.Name);
            }

            if (createReferences)
            {
                foreach (var field in fields.Where(f => f.IsReference))
                {
                    var referencedTable = GetTableDefinition(type);
                    var idColumnTo = referencedTable.IdField;

                    CreateForeignKey(field.ReferenceName, tableDefinition.Name, referencedTable.Name, field.Name, idColumnTo.Name);
                }
            }

            return 0;
        }

        public int CreateTableIfNotExists<T>(bool createReferences = false)
        {
            var type = typeof(T);

            var tableDefinition = GetTableDefinition(type);

            var fields = tableDefinition.Fields;

            var queries = Translator.GetCreateTableIfNotExistsQuery(tableDefinition.Name, fields);
            foreach (var query in queries)
            {
                Execute(query); 
            }

            foreach (var field in fields.Where(f => f.HasIndex))
            {
                CreateIndexIfNotExists(field.IndexUnique, field.IndexName, tableDefinition.Name, field.Name);
            }

            if (createReferences)
            {
                foreach (var field in fields.Where(f => f.IsReference))
                {
                    var referencedTable = GetTableDefinition(type);
                    var idColumnTo = referencedTable.IdField;

                    CreateForeignKeyIfNotExists(field.ReferenceName, tableDefinition.Name, referencedTable.Name, field.Name, idColumnTo.Name);
                } 
            }

            return 0;
        }

        public int DropTable<T>()
        {
            var tableDefinition = GetTableDefinition(typeof(T));

            var queries = Translator.GetDropTableQuery(tableDefinition.Name);

            foreach (var query in queries)
            {
                Execute(query);
            }

            return 0;
        }

        public int DropTableIfExists<T>()
        {
            var tableDefinition = GetTableDefinition(typeof(T));

            var queries = Translator.GetDropTableIfExistsQuery(tableDefinition.Name);

            foreach (var query in queries)
            {
                Execute(query);
            }

            return 0;
        }

        public int CreateColumn<T>(Expression<Func<T, dynamic>> clause)
        {
            var arguments = ExpressionHelper.GetExpressionsFromDynamic(clause);
            if (arguments != null && arguments.Length > 0)
            {
                var tableDefinition = GetTableDefinition(typeof(T));

                var query = string.Join(";",
                    arguments.Select(
                        f =>
                            Translator.GetCreateColumnQuery(
                                tableDefinition.Name, tableDefinition.Fields.First(fld => fld.PropertyInfo.Name == ((PropertyInfo)((MemberExpression)f).Member).Name)
                            )
                    )
                );

                return Execute(query);
            }

            return 0;
        }

        public int CreateColumnIfNotExists<T>(Expression<Func<T, dynamic>> clause)
        {
            var arguments = ExpressionHelper.GetExpressionsFromDynamic(clause);
            if (arguments != null && arguments.Length > 0)
            {
                var tableDefinition = GetTableDefinition(typeof(T));

                var query = string.Join(";",
                    arguments.Select(
                        f =>
                            Translator.GetCreateColumnIfNotExistsQuery(
                                tableDefinition.Name, tableDefinition.Fields.First(fld => fld.PropertyInfo.Name == ((PropertyInfo)((MemberExpression)f).Member).Name)
                            )
                    )
                );

                return Execute(query);
            }

            return 0;
        }

        public int DropColumn<T>(Expression<Func<T, dynamic>> clause)
        {
            var arguments = ExpressionHelper.GetExpressionsFromDynamic(clause);
            if (arguments != null && arguments.Length > 0)
            {
                var tableDefinition = GetTableDefinition(typeof(T));

                var query = string.Join(";",
                    arguments.Select(
                        f =>
                            Translator.GetDropColumnQuery(tableDefinition.Name,
                                tableDefinition.Fields.First(
                                    fld => fld.PropertyInfo.Name == ((PropertyInfo)((MemberExpression)f).Member).Name).Name)
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
                var tableDefinition = GetTableDefinition(typeof(T));

                var query = string.Join(";",
                    arguments.Select(
                        f =>
                            Translator.GetDropColumnIfExistsQuery(tableDefinition.Name,
                                tableDefinition.Fields.First(
                                        fld => fld.PropertyInfo.Name == ((PropertyInfo) ((MemberExpression) f).Member).Name).Name)
                    )
                );

                return Execute(query);
            }

            return 0;
        }

        public int CreateIndex<T>(Expression<Func<T, dynamic>> clause, bool unique = false, string indexName = null)
        {
            var arguments = ExpressionHelper.GetExpressionsFromDynamic(clause);
            if (arguments != null && arguments.Length > 0)
            {
                var tableDefinition = GetTableDefinition(typeof(T));

                var columns = string.Join(", ", arguments.Select(f => tableDefinition.Fields.First(fld => fld.PropertyInfo.Name == ((PropertyInfo)((MemberExpression)f).Member).Name)));

                return CreateIndex(unique, indexName, tableDefinition.Name, columns);
            }

            return 0;
        }

        private int CreateIndex(bool unique, string indexName, string tableName, string columns)
        {
            if (string.IsNullOrEmpty(indexName))
                indexName = string.Concat("IX_", tableName, "_", columns.Replace(", ", "_"));

            var query = Translator.GetCreateIndexIfNotExistsQuery(indexName, tableName, columns, unique);

            return Execute(query);
        }

        public int CreateIndexIfNotExists<T>(Expression<Func<T, dynamic>> clause, bool unique = false, string indexName = null)
        {
            var arguments = ExpressionHelper.GetExpressionsFromDynamic(clause);
            if (arguments != null && arguments.Length > 0)
            {
                var tableDefinition = GetTableDefinition(typeof(T));

                var columns = string.Join(", ", arguments.Select(f => tableDefinition.Fields.First(fld => fld.PropertyInfo.Name == ((PropertyInfo)((MemberExpression)f).Member).Name)));

                return CreateIndexIfNotExists(unique, indexName, tableDefinition.Name, columns);
            }

            return 0;
        }

        private int CreateIndexIfNotExists(bool unique, string indexName, string tableName, string columns)
        {
            if (string.IsNullOrEmpty(indexName))
                indexName = string.Concat("IX_", tableName, "_", columns.Replace(", ", "_"));

            var query = Translator.GetCreateIndexIfNotExistsQuery(indexName, tableName, columns, unique);

            return Execute(query);
        }

        public int DropIndex<T>(string indexName)
        {
            var tableDefinition = GetTableDefinition(typeof(T));

            var query = Translator.GetDropIndexQuery(tableDefinition.Name, indexName);

            return Execute(query);
        }

        public int DropIndexIfExists<T>(string indexName)
        {
            var tableDefinition = GetTableDefinition(typeof(T));

            var query = Translator.GetDropIndexIfExistsQuery(tableDefinition.Name, indexName);

            return Execute(query);
        }

        public int CreateForeignKey<TFrom, TTo>(Expression<Func<TFrom, dynamic>> columnFrom, Expression<Func<TTo, dynamic>> columnTo, string indexName = null)
        {
            var argumentsFrom = ExpressionHelper.GetExpressionsFromDynamic(columnFrom);
            var argumentsTo = ExpressionHelper.GetExpressionsFromDynamic(columnTo);

            if (argumentsFrom != null && argumentsFrom.Length > 0
                && argumentsTo != null && argumentsTo.Length > 0)
            {
                var tableFrom = GetTableDefinition(typeof(TFrom));
                var tableTo = GetTableDefinition(typeof(TTo));

                var columnNameFrom = tableFrom.Fields.First(
                    fld =>
                        fld.PropertyInfo.Name == ((PropertyInfo)((MemberExpression)argumentsFrom.First()).Member).Name).Name;

                var columnNameTo = tableTo.Fields.First(
                    fld =>
                        fld.PropertyInfo.Name == ((PropertyInfo)((MemberExpression)argumentsTo.First()).Member).Name).Name;

                return CreateForeignKey(indexName, tableFrom.Name, tableTo.Name, columnNameFrom, columnNameTo);
            }

            return 0;
        }

        private int CreateForeignKey(string indexName, string tableNameFrom, string tableNameTo, string columnNameFrom, string columnNameTo)
        {
            if (string.IsNullOrEmpty(indexName))
                indexName = string.Concat("FK_", tableNameFrom, "_", columnNameFrom, "_", tableNameTo, "_", columnNameTo);

            indexName = indexName.Substring(0, Math.Min(20, indexName.Length));

            var query = Translator.GetCreateForeignKeyQuery(indexName, tableNameFrom, columnNameFrom, tableNameTo, columnNameTo);

            return Execute(query);
        }

        public int CreateForeignKeyIfNotExists<TFrom, TTo>(Expression<Func<TFrom, dynamic>> columnFrom, Expression<Func<TTo, dynamic>> columnTo, string indexName = null)
        {
            var argumentsFrom = ExpressionHelper.GetExpressionsFromDynamic(columnFrom);
            var argumentsTo = ExpressionHelper.GetExpressionsFromDynamic(columnTo);

            if (argumentsFrom != null && argumentsFrom.Length > 0
               && argumentsTo != null && argumentsTo.Length > 0)
            {
                var tableFrom = GetTableDefinition(typeof(TFrom));
                var tableTo = GetTableDefinition(typeof(TTo));

                var columnNameFrom = tableFrom.Fields.First(
                    fld =>
                        fld.PropertyInfo.Name == ((PropertyInfo)((MemberExpression)argumentsFrom.First()).Member).Name).Name;

                var columnNameTo = tableTo.Fields.First(
                    fld =>
                        fld.PropertyInfo.Name == ((PropertyInfo)((MemberExpression)argumentsTo.First()).Member).Name).Name;

                return CreateForeignKeyIfNotExists(indexName, tableFrom.Name, tableTo.Name, columnNameFrom, columnNameTo);
            }

            return 0;
        }

        private int CreateForeignKeyIfNotExists(string indexName, string tableNameFrom, string tableNameTo, string columnNameFrom, string columnNameTo)
        {
            if (string.IsNullOrEmpty(indexName))
                indexName = string.Concat("FK_", tableNameFrom, "_", columnNameFrom, "_", tableNameTo, "_", columnNameTo);

            indexName = indexName.Substring(0, Math.Min(20, indexName.Length));

            var query = Translator.GetCreateForeignKeyIfNotExistsQuery(indexName, tableNameFrom, columnNameFrom, tableNameTo, columnNameTo);

            return Execute(query);
        }

        public int DropForeignKey<T>(string indexName)
        {
            var tableDefinition = GetTableDefinition(typeof(T));

            var query = Translator.GetDropForeignKeyQuery(tableDefinition.Name, indexName);

            return Execute(query);
        }

        public int DropForeignKeyIfExists<T>(string indexName)
        {
            var tableDefinition = GetTableDefinition(typeof(T));

            var query = Translator.GetDropForeignKeyIfExistsQuery(tableDefinition.Name, indexName);

            return Execute(query);
        }

        public IEnumerable<T> Select<T>(Query<T> query)
        {
            if (!query.Built)
                query.Build();

            return Execute<T>(query.SqlCommand.ToString(), query.Parameters);
        }

        public IEnumerable<TOther> Select<TOther>(IQuery query)
        {
            if (!query.Built)
                query.Build();

            return Execute<TOther>(query.SqlCommand.ToString(), query.Parameters);
        }

        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> clause)
        {
            var query = From<T>().Where(clause).Build();

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
            var query = From<T>().Where(clause).Top(1).Build();

            return Execute<T>(query.SqlCommand.ToString(), query.Parameters).FirstOrDefault();
        }

        public T SelectById<T>(object id)
        {
            var tableDefinition = GetTableDefinition(typeof(T));
            var idProperty = tableDefinition.IdField;

            var parameters = new Parameters();
            var paramName = parameters.Add(Translator, id);

            var query = From<T>().Where(idProperty.Name + " = " + paramName).Build();

            return Execute<T>(query.SqlCommand.ToString(), parameters).FirstOrDefault();
        }

        public IEnumerable<T> SelectById<T>(params object[] ids)
        {
            var tableDefinition = GetTableDefinition(typeof(T));
            var idProperty = tableDefinition.IdField;

            var parameters = new Parameters();
            foreach (var id in ids)
            {
                parameters.Add(Translator, id);
            }
            var inParams = string.Join(",", parameters.GetValues().Select(kv => kv.Key));

            var query = From<T>().Where(idProperty.Name + " IN (" + inParams + ")").Build();

            return Execute<T>(query.SqlCommand.ToString(), parameters);
        }

        public bool Exists<T>(Query<T> query)
        {
            if (!query.Built)
                query.Build();

            var queryWithExists = Translator.GetExistsQuery(query.SqlCommand.ToString());

            return Execute<int>(queryWithExists, query.Parameters).FirstOrDefault() == 1;
        }

        public bool Exists<T>(Expression<Func<T, bool>> clause)
        {
            var query = From<T>().Where(clause).Build();

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

        private TableDefinition GetTableDefinition(Type type)
        {
            return new TableDefinition(type);
        }
    }
}
