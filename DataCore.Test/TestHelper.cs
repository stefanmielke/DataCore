using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using DataCore.Database;
using DataCore.Database.MySql;
using DataCore.Database.Oracle;
using DataCore.Database.Postgres;
using DataCore.Database.Sqlite;
using DataCore.Database.SqlServer;
using DataCore.Test.Models;
using MySql.Data.MySqlClient;
using Npgsql;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;

namespace DataCore.Test
{
    public class SqlTestDataFactory
    {
        public static IEnumerable TestCases
        {
            get
            {
                yield return new TestCaseData(TestHelper.DatabaseType.Sqlite, "Data Source=:memory:");
                yield return new TestCaseData(TestHelper.DatabaseType.SqlServer, @"Server=localhost;User Id=sa; Password=YourStrong!Passw0rd;");
                yield return new TestCaseData(TestHelper.DatabaseType.Postgres, @"Server=localhost;User Id=postgres; Password=postgres;");
                yield return new TestCaseData(TestHelper.DatabaseType.MariaDb, @"Server=localhost;Database=db;Uid=root;Pwd=mariadb;");
                //yield return new TestCaseData(TestHelper.DatabaseType.MySql, @"Server=localhost;Port=3307;Database=db;Uid=root;Pwd=mysql;");
                yield return new TestCaseData(TestHelper.DatabaseType.Oracle, @"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=xe)));User Id=system;Password=oracle;");
            }
        }

        public static IEnumerable TestCasesNoOracle
        {
            get
            {
                yield return new TestCaseData(TestHelper.DatabaseType.Sqlite, "Data Source=:memory:");
                yield return new TestCaseData(TestHelper.DatabaseType.Postgres, @"Server=localhost;User Id=postgres; Password=postgres;");
                yield return new TestCaseData(TestHelper.DatabaseType.MariaDb, @"Server=localhost;Database=db;Uid=root;Pwd=mariadb;");
                //yield return new TestCaseData(TestHelper.DatabaseType.MySql, @"Server=localhost;Port=3307;Uid=root;Pwd=mysql;Database:db;");
                yield return new TestCaseData(TestHelper.DatabaseType.SqlServer, @"Server=localhost;User Id=sa; Password=YourStrong!Passw0rd;");
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
            database.DropTableIfExists<TestOverride>();
            database.DropTableIfExists<TestClassRef2>();
            database.DropTableIfExists<TestClassRef1>();
            database.DropTableIfExists<TestClassNoReference>();
            database.DropTableIfExists<TestClassOnlyIdentity>();
            database.DropTableIfExists<TestNullableProperty>();
            
            return connection;
        }

        private static IDbConnection GetConnectionForInternal(DatabaseType dbType, string connectionString)
        {
            switch (dbType)
            {
                case DatabaseType.Sqlite:
                    return new SQLiteConnection(connectionString);
                case DatabaseType.Oracle:
                    return new OracleConnection(connectionString);
                case DatabaseType.Postgres:
                    return new NpgsqlConnection(connectionString);
                case DatabaseType.MariaDb:
                case DatabaseType.MySql:
                    return new MySqlConnection(connectionString);
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
                case DatabaseType.Oracle:
                    return new OracleDatabase(connection);
                case DatabaseType.Postgres:
                    return new PostgresDatabase(connection);
                case DatabaseType.MariaDb:
                case DatabaseType.MySql:
                    return new MySqlDatabase(connection);
                default:
                    return new SqlServerDatabase(connection);
            }
        }

        public static TestClass GetNewTestClass(int id = 1, int number = 1, string name = "test", bool done = true, int testClass2Id = 1)
        {
            return new TestClass
            {
                Id = id,
                FloatNumber = number,
                Name = name,
                Done = done,
                InsertDate = DateTime.Now,
                TestClass2Id = testClass2Id
            };
        }

        public enum DatabaseType
        {
            Sqlite,
            SqlServer,
            Oracle,
            Postgres,
            MySql,
            MariaDb
        }
    }
}
