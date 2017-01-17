using DataCore.Test.Models;
using System;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class DatabaseTest
    {
        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanSelect(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                database.Select(query);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanSelectSingle(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();
                database.Insert(TestHelper.GetNewTestClass());

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                Assert.IsNotNull(database.SelectSingle(query));

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void ExistsReturnTrueWhenExists(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();
                database.Insert(TestHelper.GetNewTestClass());

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                Assert.IsTrue(database.Exists(query));

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void ExistsReturnFalseWhenNotExists(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                Assert.IsFalse(database.Exists(query));

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTable(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                database.Select(query);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableWithoutIgnoredAttributes(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestIgnore>();

                var query = database.From<TestIgnore>().Where(t => t.Ignored == "exception!");

                Assert.Throws(Is.InstanceOf<Exception>(), () => database.Select(query));

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void DoNotErrorOnDoubleCreateTable(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();
                database.CreateTableIfNotExists<TestClass>();

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                database.Select(query);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropTable(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                database.Select(database.From<TestClass>().Where(t => t.Id == 1));

                database.DropTableIfExists<TestClass>();

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateColumn(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.DropTableIfExists<TestClass4>();
                database.Execute("CREATE TABLE TestClass4 ( Id INT not null, Number INT not null, Name VARCHAR(250) not null )");

                database.CreateColumnIfNotExists<TestClass4>(t => t.InsertDate);

                var date = DateTime.Now;

                var query = database.From<TestClass4>().Where(t => t.InsertDate == date);

                database.Select(query);

                database.Execute("DROP TABLE TestClass4");

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropColumn(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                database.DropColumnIfExists<TestClass>(t => t.Name);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateIndex(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                database.CreateIndexIfNotExists<TestClass>(t => new { t.Id, t.Name }, true);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropIndex(TestHelper.DatabaseType dbType, string connectionString)
        {
            string indexName = "IX_TestClass" + Guid.NewGuid().ToString().Replace("-", "");

            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                database.CreateIndexIfNotExists<TestClass>(t => new { t.Id, t.Name }, true, indexName);
                database.DropIndexIfExists<TestClass>(indexName);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateForeignKey(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();
                database.CreateTableIfNotExists<TestClass2>();

                database.CreateForeignKeyIfNotExists<TestClass, TestClass2>(t => t.TestClass2Id, t => t.Id);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropForeignKey(TestHelper.DatabaseType dbType, string connectionString)
        {
            string indexName = "FK_TestClass" + Guid.NewGuid().ToString().Replace("-", "");

            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();
                database.CreateTableIfNotExists<TestClass2>();

                database.CreateForeignKeyIfNotExists<TestClass, TestClass2>(t => t.TestClass2Id, t => t.Id, indexName);
                database.DropForeignKeyIfExists<TestClass>(indexName);

                connection.Close();
            }
        }
    }
}
