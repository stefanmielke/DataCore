﻿using System.Collections.Generic;
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

            var tableName = GetTableName(type);

            var properties = GetPropertiesForType(type).Where(p => !GetFieldForProperty(p).IsIdentity).ToList();

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

            var tableName = GetTableName(type);

            var properties = GetPropertiesForType(type).Where(p => !GetFieldForProperty(p).IsIdentity);

            var nameValues = new List<KeyValuePair<string, string>>();

            var parameters = new Parameters();
            foreach (var prop in properties)
            {
                var key = parameters.Add(Translator, ExpressionHelper.GetValueFrom(Translator, prop.PropertyType, prop.GetValue(obj, null)));
                nameValues.Add(new KeyValuePair<string, string>(prop.Name, key));
            }

            var newExpression = Expression.Lambda(new QueryVisitor(parameters).Visit(whereClause));

            var whereQuery = ExpressionHelper.GetQueryFromExpression(Translator, newExpression.Body, parameters);

            var query = Translator.GetUpdateQuery(tableName, nameValues, whereQuery, parameters);

            Execute(query, parameters);
        }

        public void UpdateOnly<T>(T obj, Expression<Func<T, dynamic>> onlyFields, Expression<Func<T, dynamic>> whereClause)
        {
            var type = typeof(T);

            var tableName = GetTableName(type);

            var properties = GetPropertiesForType(type);

            var fields = ExpressionHelper.GetStringsFromArguments(onlyFields);

            var nameValues = new List<KeyValuePair<string, string>>();

            var parameters = new Parameters();
            foreach (var prop in properties.Where(p => fields.Contains(p.Name)))
            {
                var key = parameters.Add(Translator, ExpressionHelper.GetValueFrom(Translator, prop.PropertyType, prop.GetValue(obj, null)));
                nameValues.Add(new KeyValuePair<string, string>(prop.Name, key));
            }

            var newExpression = Expression.Lambda(new QueryVisitor(parameters).Visit(whereClause));
            var whereQuery = ExpressionHelper.GetQueryFromExpression(Translator, newExpression.Body, parameters);

            var query = Translator.GetUpdateQuery(tableName, nameValues, whereQuery, parameters);

            Execute(query, parameters);
        }

        public void Delete<T>(Expression<Func<T, bool>> whereClause)
        {
            var type = typeof(T);

            var tableName = GetTableName(type);

            var parameters = new Parameters();

            var newExpression = Expression.Lambda(new QueryVisitor(parameters).Visit(whereClause));
            var whereQuery = ExpressionHelper.GetQueryFromExpression(Translator, newExpression.Body, parameters);

            var query = Translator.GetDeleteQuery(tableName, whereQuery, parameters);

            Execute(query, parameters);
        }

        public void DeleteById<T>(object id)
        {
            var type = typeof(T);
            var tableName = GetTableName(type);

            var idProperty = GetIdPropertyForType(typeof(T));

            var parameters = new Parameters();
            var paramName = parameters.Add(Translator, id);

            var whereQuery = string.Concat(idProperty.Name, " = ", paramName);

            var query = Translator.GetDeleteQuery(tableName, whereQuery, parameters);

            Execute(query, parameters);
        }

        public void DeleteById<T>(params object[] ids)
        {
            var type = typeof(T);
            var tableName = GetTableName(type);

            var idProperty = GetIdPropertyForType(typeof(T));

            var parameters = new Parameters();
            foreach (var id in ids)
            {
                parameters.Add(Translator, id);
            }
            var inParams = string.Join(",", parameters.GetValues().Select(kv => kv.Key));

            var whereQuery = string.Concat(idProperty.Name, " IN (", inParams, ")");

            var query = Translator.GetDeleteQuery(tableName, whereQuery, parameters);

            Execute(query, parameters);
        }

        public int CreateTableIfNotExists<T>(bool createReferences = false)
        {
            var type = typeof(T);
            var tableName = GetTableName(type);

            var fields = GetPropertiesForType(type).Select(GetFieldForProperty).ToList();

            var queries = Translator.GetCreateTableIfNotExistsQuery(tableName, fields);
            foreach (var query in queries)
            {
                Execute(query); 
            }

            foreach (var field in fields.Where(f => f.HasIndex))
            {
                CreateIndexIfNotExists<T>(field.IndexUnique, field.IndexName, tableName, field.Name);
            }

            if (createReferences)
            {
                foreach (var field in fields.Where(f => f.IsReference))
                {
                    var idColumnTo = GetIdFieldForType(field.ReferenceTable);

                    CreateForeignKeyIfNotExists(field.ReferenceName, tableName, GetTableName(field.ReferenceTable), field.Name, idColumnTo.Name);
                } 
            }

            return 0;
        }

        public int DropTableIfExists<T>()
        {
            var queries = Translator.GetDropTableIfExistsQuery(Translator.GetTableName(typeof(T)));

            foreach (var query in queries)
            {
                Execute(query);
            }

            return 0;
        }

        public int CreateColumnIfNotExists<T>(Expression<Func<T, dynamic>> clause)
        {
            var arguments = ExpressionHelper.GetExpressionsFromDynamic(clause);
            if (arguments != null && arguments.Length > 0)
            {
                var tableName = GetTableName(typeof(T));

                var query = string.Join(";",
                    arguments.Select(
                        f =>
                            Translator.GetCreateColumnIfNotExistsQuery(
                                tableName, GetFieldForProperty(((MemberExpression)f).Member as PropertyInfo)
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
                var tableName = GetTableName(typeof(T));

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
                var tableName = GetTableName(typeof(T));
                var columns = string.Join(", ", arguments.Select(f => ((MemberExpression)f).Member.Name));

                return CreateIndexIfNotExists<T>(unique, indexName, tableName, columns);
            }

            return 0;
        }

        public int DropIndexIfExists<T>(string indexName)
        {
            var tableName = GetTableName(typeof(T));

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
                var tableNameFrom = GetTableName(typeof(TFrom));
                var tableNameTo = GetTableName(typeof(TTo));

                var columnNameFrom = ((MemberExpression)argumentsFrom.First()).Member.Name;
                var columnNameTo = ((MemberExpression)argumentsTo.First()).Member.Name;

                return CreateForeignKeyIfNotExists(indexName, tableNameFrom, tableNameTo, columnNameFrom, columnNameTo);
            }

            return 0;
        }

        public int DropForeignKeyIfExists<T>(string indexName)
        {
            var tableName = GetTableName(typeof(T));

            var query = Translator.GetDropForeignKeyIfExistsQuery(tableName, indexName);

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
            var idProperty = GetIdPropertyForType(typeof(T));

            var parameters = new Parameters();
            var paramName = parameters.Add(Translator, id);

            var query = From<T>().Where(idProperty.Name + " = " + paramName).Build();

            return Execute<T>(query.SqlCommand.ToString(), parameters).FirstOrDefault();
        }

        public IEnumerable<T> SelectById<T>(params object[] ids)
        {
            var idProperty = GetIdPropertyForType(typeof(T));

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

        private string GetTableName(Type type)
        {
            return Translator.GetTableName(type);
        }

        private FieldDefinition GetFieldForProperty(PropertyInfo p)
        {
            var columnName = p.Name;
            var isPrimaryKey = false;
            var length = 255;
            var nullable = false;

            var columnAttributes = p.GetCustomAttributes(typeof(ColumnAttribute), true);
            if (columnAttributes.Length > 0)
            {
                var columnAttribute = (ColumnAttribute)columnAttributes[0];
                columnName = columnAttribute.ColumnName ?? columnName;
                isPrimaryKey = columnAttribute.IsPrimaryKey;
                length = columnAttribute.Length;
                nullable = !columnAttribute.IsRequired && !isPrimaryKey;
            }

            var field = new FieldDefinition
            {
                Name = columnName,
                Nullable = nullable,
                Size = length,
                Type = Translator.GetTypeForProperty(p),
                IsPrimaryKey = isPrimaryKey
            };

            var identityAttributes = p.GetCustomAttributes(typeof(IdentityAttribute), true);
            if (identityAttributes.Length > 0)
            {
                var identityAttribute = (IdentityAttribute)identityAttributes[0];
                field.IsIdentity = true;
                field.IdentityStart = identityAttribute.Start;
                field.IdentityIncrement = identityAttribute.Increment;
            }

            var referenceAttributes = p.GetCustomAttributes(typeof(ReferenceAttribute), true);
            if (referenceAttributes.Length > 0)
            {
                var referenceAttribute = (ReferenceAttribute)referenceAttributes[0];
                field.IsReference = true;
                field.ReferenceName = referenceAttribute.FkName;
                field.ReferenceTable = referenceAttribute.Table;
            }

            var indexAttributes = p.GetCustomAttributes(typeof(IndexAttribute), true);
            if (indexAttributes.Length > 0)
            {
                var indexAttribute = (IndexAttribute)indexAttributes[0];
                field.HasIndex = true;
                field.IndexName = indexAttribute.Name;
                field.IndexUnique = indexAttribute.Unique;
            }

            return field;
        }

        private IEnumerable<PropertyInfo> GetPropertiesForType(Type type)
        {
            return type.GetProperties().Where(p => p.GetCustomAttributes(typeof(IgnoreAttribute), true).Length == 0);
        }

        private PropertyInfo GetIdPropertyForType(Type type)
        {
            return type.GetProperties()
                .FirstOrDefault(p =>
                    {
                        var columnAttributes = p.GetCustomAttributes(typeof(ColumnAttribute), true);

                        return columnAttributes.Length > 0 && ((ColumnAttribute)columnAttributes[0]).IsPrimaryKey;
                    }
                );
        }

        private FieldDefinition GetIdFieldForType(Type type)
        {
            return GetFieldForProperty(GetIdPropertyForType(type));
        }

        private int CreateForeignKeyIfNotExists(string indexName, string tableNameFrom, string tableNameTo, string columnNameFrom, string columnNameTo)
        {
            if (string.IsNullOrEmpty(indexName))
                indexName = string.Concat("FK_", tableNameFrom, "_", columnNameFrom, "_", tableNameTo, "_", columnNameTo);

            indexName = indexName.Substring(0, Math.Min(20, indexName.Length));

            var query = Translator.GetCreateForeignKeyIfNotExistsQuery(indexName, tableNameFrom, columnNameFrom, tableNameTo, columnNameTo);

            return Execute(query);
        }

        private int CreateIndexIfNotExists<T>(bool unique, string indexName, string tableName, string columns)
        {
            if (string.IsNullOrEmpty(indexName))
                indexName = string.Concat("IX_", tableName, "_", columns.Replace(", ", "_"));

            var query = Translator.GetCreateIndexIfNotExistsQuery(indexName, tableName, columns, unique);

            return Execute(query);
        }
    }
}
