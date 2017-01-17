using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using DataCore.Database;
using DataCore.Database.Sqlite;
using DataCore.Database.SqlServer;
using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    public class SqlTestDataFactory
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(TestHelper.DatabaseType.Sqlite, "Data Source=:memory:");
                yield return new TestCaseData(TestHelper.DatabaseType.SqlServer, @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;");
            }
        }
    }

    public static class TestHelper
    {
        public static IDbConnection GetConnectionFor(DatabaseType dbType, string connectionString)
        {
            var connection = GetConnectionForInternal(dbType, connectionString);
            connection.Open();

            var database = GetDatabaseFor(dbType, connection);

            database.DropTableIfExists<TestClass>();
            database.DropTableIfExists<TestClass2>();
            database.DropTableIfExists<TestClass3>();
            database.DropTableIfExists<TestClass4>();
            database.DropTableIfExists<TestIgnore>();

            return connection;
        }

        private static IDbConnection GetConnectionForInternal(DatabaseType dbType, string connectionString)
        {
            switch (dbType)
            {
                case DatabaseType.Sqlite:
                    return new SQLiteConnection(connectionString);
                default:
                    return new SqlConnection(connectionString);
            }
        }

        public static IDatabase GetDatabaseFor(DatabaseType dbType, IDbConnection connection)
        {
            switch (dbType)
            {
                case DatabaseType.Sqlite:
                    return new SqliteDatabase(connection);
                default:
                    return new SqlServerDatabase(connection);
            }
        }

        public static TestClass GetNewTestClass(int id = 1, int number = 1, string name = "test", bool done = true, int testClass2Id = 1)
        {
            return new TestClass
            {
                Id = id,
                Number = number,
                Name = name,
                Done = done,
                InsertDate = DateTime.Now,
                TestClass2Id = testClass2Id
            };
        }

        public enum DatabaseType
        {
            Sqlite,
            SqlServer
        }
    }
}
