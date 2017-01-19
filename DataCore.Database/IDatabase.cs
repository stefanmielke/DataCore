using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataCore.Database
{
    public interface IDatabase
    {
        ITranslator Translator { get; }

        int CreateColumnIfNotExists<T>(Expression<Func<T, dynamic>> clause);
        int CreateForeignKeyIfNotExists<TFrom, TTo>(Expression<Func<TFrom, dynamic>> columnFrom, Expression<Func<TTo, dynamic>> columnTo, string indexName = null);
        int CreateIndexIfNotExists<T>(Expression<Func<T, dynamic>> clause, bool unique = false, string indexName = null);
        int CreateTableIfNotExists<T>();
        void Delete<T>(Expression<Func<T, bool>> whereClause);
        void DeleteById<T>(object id);
        int DropColumnIfExists<T>(Expression<Func<T, dynamic>> clause);
        int DropForeignKeyIfExists<T>(string indexName);
        int DropIndexIfExists<T>(string indexName);
        int DropTableIfExists<T>();
        int Execute(string query);
        IEnumerable<T> Execute<T>(string query);
        bool Exists<T>(Query<T> query);
        Query<T> From<T>();
        void Insert<T>(T obj);
        IEnumerable<T> Select<T>(Query<T> query);
        IEnumerable<T> Select<T>(Expression<Func<T, bool>> clause);
        T SelectSingle<T>(Query<T> query);
        T SelectSingle<T>(Expression<Func<T, bool>> clause);
        T SelectById<T>(object id);
        void Update<T>(T obj, Expression<Func<T, dynamic>> whereClause);
        void UpdateOnly<T>(T obj, Expression<Func<T, dynamic>> onlyFields, Expression<Func<T, dynamic>> whereClause);
    }
}