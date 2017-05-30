using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataCore.Database
{
    public interface IDatabase : IDisposable
    {
        void OpenConnection(string connectionString);

        ITranslator Translator { get; }

        int Execute(string query);
        IEnumerable<T> Execute<T>(string query);

        bool DatabaseExists(string name);
        int CreateDatabase(string name);
        int CreateDatabaseIfNotExists(string name);
        int DropDatabase(string name);
        int DropDatabaseIfExists(string name);

        bool TableExists<T>();
        int CreateTable<T>(bool createReferences = false);
        int CreateTable(Type table, bool createReferences = false);
        int CreateTables(params Type[] tables);
        int CreateTables(IEnumerable<Type> tables, bool createReferences = false);
        int CreateTableIfNotExists<T>(bool createReferences = false);
        int CreateTableIfNotExists(Type table, bool createReferences = false);
        int CreateTablesIfNotExists(params Type[] tables);
        int CreateTablesIfNotExists(IEnumerable<Type> tables, bool createReferences = false);

        int DropTable<T>();
        int DropTable(Type table);
        int DropTables(params Type[] tables);
        int DropTableIfExists<T>();
        int DropTableIfExists(Type table);
        int DropTablesIfExists(params Type[] tables);

        int DropAndCreateTable<T>(bool createReferences = false);
        int DropAndCreateTable(Type table, bool createReferences = false);
        int DropAndCreateTables(params Type[] tables);
        int DropAndCreateTables(IEnumerable<Type> tables, bool createReferences = false);

        bool ColumnExists<T>(string columnName);
        bool ColumnExists<T>(Expression<Func<T, dynamic>> clause);
        int CreateColumn<T>(Expression<Func<T, dynamic>> clause);
        int CreateColumnIfNotExists<T>(Expression<Func<T, dynamic>> clause);
        int DropColumn<T>(Expression<Func<T, dynamic>> clause);
        int DropColumnIfExists<T>(Expression<Func<T, dynamic>> clause);

        bool IndexExists<T>(string indexName);
        bool IndexExists<T>(Expression<Func<T, dynamic>> clause);
        int CreateIndex<T>(Expression<Func<T, dynamic>> clause, bool unique = false, string indexName = null);
        int CreateIndexIfNotExists<T>(Expression<Func<T, dynamic>> clause, bool unique = false, string indexName = null);
        int DropIndex<T>(string indexName);
        int DropIndexIfExists<T>(string indexName);

        bool ForeignKeyExists<TFrom>(string indexName);
        bool ForeignKeyExists<TFrom, TTo>(Expression<Func<TFrom, dynamic>> columnFrom, Expression<Func<TTo, dynamic>> columnTo);
        int CreateForeignKey<TFrom, TTo>(Expression<Func<TFrom, dynamic>> columnFrom, Expression<Func<TTo, dynamic>> columnTo, string indexName = null);
        int CreateForeignKeyIfNotExists<TFrom, TTo>(Expression<Func<TFrom, dynamic>> columnFrom, Expression<Func<TTo, dynamic>> columnTo, string indexName = null);
        int DropForeignKey<T>(string indexName);
        int DropForeignKeyIfExists<T>(string indexName);

        void Insert<T>(T obj);

        void Update<T>(T obj, Expression<Func<T, dynamic>> whereClause);
        void UpdateOnly<T>(T obj, Expression<Func<T, dynamic>> onlyFields, Expression<Func<T, dynamic>> whereClause);

        void Delete<T>(Expression<Func<T, bool>> whereClause);
        void DeleteById<T>(object id);
        void DeleteById<T>(params object[] ids);

        Query<T> From<T>();

        bool Exists<T>(Query<T> query);
        bool Exists<T>(Expression<Func<T, bool>> clause);

        IEnumerable<T> Select<T>(Query<T> query);
        IEnumerable<TOther> Select<TOther>(IQuery query);
        IEnumerable<T> Select<T>(Expression<Func<T, bool>> clause);

        T SelectSingle<T>(Query<T> query);
        T SelectSingle<T>(Expression<Func<T, bool>> clause);

        T SelectById<T>(object id);
        IEnumerable<T> SelectById<T>(params object[] ids);
    }
}