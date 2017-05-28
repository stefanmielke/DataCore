using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataCore.Database
{
    public interface IDatabase
    {
        ITranslator Translator { get; }

        void Delete<T>(Expression<Func<T, bool>> whereClause);
        void DeleteById<T>(object id);
        void DeleteById<T>(params object[] ids);

        int CreateTable<T>(bool createReferences = false);
        int CreateTableIfNotExists<T>(bool createReferences = false);
        int DropTable<T>();
        int DropTableIfExists<T>();

        int CreateColumn<T>(Expression<Func<T, dynamic>> clause);
        int CreateColumnIfNotExists<T>(Expression<Func<T, dynamic>> clause);
        int DropColumn<T>(Expression<Func<T, dynamic>> clause);
        int DropColumnIfExists<T>(Expression<Func<T, dynamic>> clause);

        int CreateIndex<T>(Expression<Func<T, dynamic>> clause, bool unique = false, string indexName = null);
        int CreateIndexIfNotExists<T>(Expression<Func<T, dynamic>> clause, bool unique = false, string indexName = null);
        int DropIndex<T>(string indexName);
        int DropIndexIfExists<T>(string indexName);

        int CreateForeignKey<TFrom, TTo>(Expression<Func<TFrom, dynamic>> columnFrom, Expression<Func<TTo, dynamic>> columnTo, string indexName = null);
        int CreateForeignKeyIfNotExists<TFrom, TTo>(Expression<Func<TFrom, dynamic>> columnFrom, Expression<Func<TTo, dynamic>> columnTo, string indexName = null);
        int DropForeignKey<T>(string indexName);
        int DropForeignKeyIfExists<T>(string indexName);

        int Execute(string query);
        IEnumerable<T> Execute<T>(string query);
        bool Exists<T>(Query<T> query);
        bool Exists<T>(Expression<Func<T, bool>> clause);
        Query<T> From<T>();
        void Insert<T>(T obj);
        IEnumerable<T> Select<T>(Query<T> query);
        IEnumerable<TOther> Select<TOther>(IQuery query);
        IEnumerable<T> Select<T>(Expression<Func<T, bool>> clause);
        T SelectSingle<T>(Query<T> query);
        T SelectSingle<T>(Expression<Func<T, bool>> clause);
        T SelectById<T>(object id);
        IEnumerable<T> SelectById<T>(params object[] ids);
        void Update<T>(T obj, Expression<Func<T, dynamic>> whereClause);
        void UpdateOnly<T>(T obj, Expression<Func<T, dynamic>> onlyFields, Expression<Func<T, dynamic>> whereClause);
    }
}