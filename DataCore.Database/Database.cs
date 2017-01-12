using System.Collections.Generic;
using System.Data;
using Dapper;

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

        public IEnumerable<T> Select<T>(Query<T> query)
        {
            if (!query.Built)
                query.Select();

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
    }
}
