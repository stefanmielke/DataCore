using DataCore.Test.Models;
using System;
using System.Linq;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class DatabaseCheckedTest
    {
        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateAndDropDatabase(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var db = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                db.DropDatabaseIfExists("test_db");
                db.CreateDatabaseIfNotExists("test_db");
                db.CreateDatabaseIfNotExists("test_db");
                db.DropDatabaseIfExists("test_db");
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTable(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                database.Select(query);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTables(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTablesIfNotExists(typeof(TestClass), typeof(TestClass2));

                var query = database.From<TestClass>().Where(t => t.Id == 1);
                database.Select(query);

                var query2 = database.From<TestClass2>().Where(t => t.Id == 1);
                database.Select(query2);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableWithOverridedName(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestOverride>();

                var query = database.From<TestOverride>().Where(t => t.Id == 1);

                database.Select(query);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableWithoutIgnoredAttributes(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestIgnore>();

                var query = database.From<TestIgnore>().Where(t => t.Ignored == "exception!");

                Assert.Throws(Is.InstanceOf<Exception>(), () => database.Select(query));
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableWithIdentity(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();

                database.Insert(TestHelper.GetNewTestClass(0));
                database.Insert(TestHelper.GetNewTestClass(0));

                var result = database.SelectById<TestClass>(1, 2);

                Assert.AreEqual(2, result.Count());
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void DoNotErrorOnDoubleCreateTable(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();
                database.CreateTableIfNotExists<TestClass>();

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                database.Select(query);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableWithReferences(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClassRef1>();
                database.CreateTableIfNotExists<TestClassRef2>(true);

                database.DropForeignKeyIfExists<TestClassRef2>("FK_TestClassRef2");
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableIndex(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClassRef1>();

                database.DropIndexIfExists<TestClassRef1>("IX_TestClassRef1_Id2");
                database.DropIndexIfExists<TestClassRef1>("IX_TestTest");
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropTable(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();

                database.Select(database.From<TestClass>().Where(t => t.Id == 1));

                database.DropTableIfExists<TestClass>();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropTables(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTablesIfNotExists(typeof(TestClass), typeof(TestClass2));

                database.Select(database.From<TestClass>().Where(t => t.Id == 1));
                database.Select(database.From<TestClass2>().Where(t => t.Id == 1));

                database.DropTablesIfExists(typeof(TestClass), typeof(TestClass2));
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateColumn(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                var translator = database.Translator;

                var tableDefinition = new TableDefinition(typeof(TestClass4));

                var textInt = translator.GetTextFor(tableDefinition.Fields.First(f => f.Name == "Id"));
                var textString = translator.GetTextFor(tableDefinition.Fields.First(f => f.Name == "Name"));

                database.Execute("CREATE TABLE TestClass4 ( Id " + textInt + " not null, FormatNumber " + textInt + " not null, Name " + textString + "(250) not null )");

                database.CreateColumnIfNotExists<TestClass4>(t => t.InsertDate);

                var date = DateTime.Now;

                var query = database.From<TestClass4>().Where(t => t.InsertDate == date);

                database.Select(query);

                database.DropTableIfExists<TestClass4>();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropColumn(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();

                database.DropColumnIfExists<TestClass>(t => t.Name);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateIndex(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();

                database.CreateIndexIfNotExists<TestClass>(t => new { t.Id, t.Name }, true);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropIndex(TestHelper.DatabaseType dbType, string connectionString)
        {
            string indexName = "IX_TestClass_Id_Name";

            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();

                database.CreateIndexIfNotExists<TestClass>(t => new { t.Id, t.Name }, true, indexName);
                database.DropIndexIfExists<TestClass>(indexName);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateForeignKey(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();
                database.CreateTableIfNotExists<TestClass2>();

                database.CreateForeignKeyIfNotExists<TestClass, TestClass2>(t => t.TestClass2Id, t => t.Id);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropForeignKey(TestHelper.DatabaseType dbType, string connectionString)
        {
            string indexName = "FK_TestClass_Id";

            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();
                database.CreateTableIfNotExists<TestClass2>();

                database.CreateForeignKeyIfNotExists<TestClass, TestClass2>(t => t.TestClass2Id, t => t.Id, indexName);
                database.DropForeignKeyIfExists<TestClass>(indexName);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableNoReference(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClassNoReference>();
                database.Insert(new TestClassNoReference { Id = 1 });

                var result = database.SelectById<TestClassNoReference>(1);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.Ref, Is.Null);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCasesNoOracle))]
        public void CanCreateTableOnlyIdentityColumn(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClassOnlyIdentity>();

                database.Insert(new TestClassOnlyIdentity());
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableWithNullProperty(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestNullableProperty>();

                database.Insert(new TestNullableProperty { Id = 1 });

                var result = database.SelectById<TestNullableProperty>(1);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(1));
                Assert.That(result.IdMaybe, Is.Null);
            }
        }
    }
}
