using System.Collections.Generic;

namespace DataCore.Database
{
    public interface IDatabase
    {
        IEnumerable<T> Select<T>(Query<T> query);

        IEnumerable<T> Execute<T>(string query);
    }
}
