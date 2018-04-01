using System;
using System.Collections;
using DataCore.Database;
using DataCore.Database.MySql;
using DataCore.Database.Oracle;
using DataCore.Database.Postgres;
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

        public static IEnumerable TestCasesNoSqlite
        {
            get
            {
                yield return new TestCaseData(TestHelper.DatabaseType.Oracle, @"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=xe)));User Id=system;Password=oracle;");
                yield return new TestCaseData(TestHelper.DatabaseType.Postgres, @"Server=localhost;User Id=postgres; Password=postgres;");
                yield return new TestCaseData(TestHelper.DatabaseType.MariaDb, @"Server=localhost;Database=db;Uid=root;Pwd=mariadb;");
                //yield return new TestCaseData(TestHelper.DatabaseType.MySql, @"Server=localhost;Port=3307;Uid=root;Pwd=mysql;Database:db;");
                yield return new TestCaseData(TestHelper.DatabaseType.SqlServer, @"Server=localhost;User Id=sa; Password=YourStrong!Passw0rd;");
            }
        }

        public static IEnumerable TestCasesNoSqliteAndOracle
        {
            get
            {
                yield return new TestCaseData(TestHelper.DatabaseType.Postgres, @"Server=localhost;User Id=postgres; Password=postgres;");
                yield return new TestCaseData(TestHelper.DatabaseType.MariaDb, @"Server=localhost;Database=db;Uid=root;Pwd=mariadb;");
                //yield return new TestCaseData(TestHelper.DatabaseType.MySql, @"Server=localhost;Port=3307;Uid=root;Pwd=mysql;Database:db;");
                yield return new TestCaseData(TestHelper.DatabaseType.SqlServer, @"Server=localhost;User Id=sa; Password=YourStrong!Passw0rd;");
            }
        }
    }

    public static class TestHelper
    {
        public static IDatabase GetDatabaseFor(DatabaseType dbType, string connectionString)
        {
            IDatabaseDefinition dbDefinition;
            switch (dbType)
            {
                case DatabaseType.Sqlite:
                    dbDefinition = new SqliteDatabase();
                    break;
                case DatabaseType.Oracle:
                    dbDefinition = new OracleDatabase();
                    break;
                case DatabaseType.Postgres:
                    dbDefinition = new PostgresDatabase();
                    break;
                case DatabaseType.MariaDb:
                case DatabaseType.MySql:
                    dbDefinition = new MySqlDatabase();
                    break;
                default:
                    dbDefinition = new SqlServerDatabase();
                    break;
            }

            var db = new DataCoreDatabase(dbDefinition, connectionString);
            db.DropTableIfExists<TestClass>();
            db.DropTableIfExists<TestClass2>();
            db.DropTableIfExists<TestClass3>();
            db.DropTableIfExists<TestClass4>();
            db.DropTableIfExists<TestIgnore>();
            db.DropTableIfExists<TestOverride>();
            db.DropTableIfExists<TestClassRef2>();
            db.DropTableIfExists<TestClassRef1>();
            db.DropTableIfExists<TestClassNoReference>();
            db.DropTableIfExists<TestClassOnlyIdentity>();
            db.DropTableIfExists<TestNullableProperty>();

            return db;
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
