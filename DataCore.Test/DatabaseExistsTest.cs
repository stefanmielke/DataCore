using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class DatabaseExistsTest
    {
        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCasesNoSqliteAndOracle))]
        public void CanCheckIfDatabaseExists(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var db = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                db.DropDatabaseIfExists("test_db");

                db.CreateDatabase("test_db");
                Assert.That(db.DatabaseExists("test_db"), Is.True);

                db.DropDatabase("test_db");
                Assert.That(db.DatabaseExists("test_db"), Is.False);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCheckIfTableExists(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var db = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                db.CreateTable<TestClass>();
                Assert.That(db.TableExists<TestClass>(), Is.True);

                db.DropTable<TestClass>();
                Assert.That(db.TableExists<TestClass>(), Is.False);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCasesNoSqlite))]
        public void CanCheckIfColumnExists(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var db = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                db.CreateTable<TestClass>();

                db.DropColumnIfExists<TestClass>(t => t.Name);

                db.CreateColumn<TestClass>(t => t.Name);
                Assert.That(db.ColumnExists<TestClass>(t => t.Name), Is.True);
                Assert.That(db.ColumnExists<TestClass>("Name"), Is.True);

                db.DropColumn<TestClass>(t => t.Name);
                Assert.That(db.ColumnExists<TestClass>(t => t.Name), Is.False);
                Assert.That(db.ColumnExists<TestClass>("Name"), Is.False);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCheckIfIndexExistsWithDefaultName(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var db = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                db.CreateTable<TestClass>();

                db.CreateIndex<TestClass>(t => t.Name);
                Assert.That(db.IndexExists<TestClass>(t => t.Name), Is.True);

                db.DropIndex<TestClass>("IX_TestClass_Name");
                Assert.That(db.IndexExists<TestClass>(t => t.Name), Is.False);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCheckIfIndexExistsWithNamedIndex(TestHelper.DatabaseType dbType, string connectionString)
        {
            const string indexName = "IX_NAME";

            using (var db = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                db.CreateTable<TestClass>();

                db.CreateIndex<TestClass>(t => t.Name, false, indexName);
                Assert.That(db.IndexExists<TestClass>(indexName), Is.True);

                db.DropIndex<TestClass>(indexName);
                Assert.That(db.IndexExists<TestClass>(indexName), Is.False);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCasesNoSqlite))]
        public void CanCheckIfForeignKeyExistsWithDefaultName(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var db = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                db.CreateTables(typeof(TestClass), typeof(TestClass2));

                db.CreateForeignKey<TestClass, TestClass2>(t => t.TestClass2Id, t2 => t2.Id);
                Assert.That(db.ForeignKeyExists<TestClass, TestClass2>(t => t.TestClass2Id, t2 => t2.Id), Is.True);

                db.DropForeignKey<TestClass>("FK_TestClass_TestCla");
                Assert.That(db.ForeignKeyExists<TestClass, TestClass2>(t => t.TestClass2Id, t2 => t2.Id), Is.False);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCasesNoSqlite))]
        public void CanCheckIfForeignKeyExistsWithNamedIndex(TestHelper.DatabaseType dbType, string connectionString)
        {
            const string indexName = "FK_NAME";

            using (var db = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                db.CreateTables(typeof(TestClass), typeof(TestClass2));

                db.CreateForeignKey<TestClass, TestClass2>(t => t.TestClass2Id, t2 => t2.Id, indexName);
                Assert.That(db.ForeignKeyExists<TestClass>(indexName), Is.True);

                db.DropForeignKey<TestClass>(indexName);
                Assert.That(db.ForeignKeyExists<TestClass>(indexName), Is.False);
            }
        }
    }
}
