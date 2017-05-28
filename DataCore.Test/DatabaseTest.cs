using System;
using System.Linq;
using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class DatabaseTest
    {
        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTable(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestClass>();

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                database.Select(query);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTables(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTables(typeof(TestClass), typeof(TestClass2));

                var query = database.From<TestClass>().Where(t => t.Id == 1);
                database.Select(query);

                var query2 = database.From<TestClass2>().Where(t => t.Id == 1);
                database.Select(query2);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableWithOverridedName(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestOverride>();

                var query = database.From<TestOverride>().Where(t => t.Id == 1);

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

                database.CreateTable<TestIgnore>();

                var query = database.From<TestIgnore>().Where(t => t.Ignored == "exception!");

                Assert.Throws(Is.InstanceOf<Exception>(), () => database.Select(query));

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableWithIdentity(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestClass>();

                database.Insert(TestHelper.GetNewTestClass(0));
                database.Insert(TestHelper.GetNewTestClass(0));

                var result = database.SelectById<TestClass>(1, 2);

                Assert.AreEqual(2, result.Count());

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableWithReferences(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestClassRef1>();
                database.CreateTable<TestClassRef2>(true);

                database.DropForeignKey<TestClassRef2>("FK_TestClassRef2");

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableIndex(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestClassRef1>();

                database.DropIndex<TestClassRef1>("IX_TestClassRef1_Id2");
                database.DropIndex<TestClassRef1>("IX_TestTest");

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropTable(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestClass>();

                database.Select(database.From<TestClass>().Where(t => t.Id == 1));

                database.DropTable<TestClass>();

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateColumn(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);
                var translator = database.Translator;

                var tableDefinition = new TableDefinition(typeof(TestClass4));

                var textInt = translator.GetTextFor(tableDefinition.Fields.First(f => f.Name == "Id"));
                var textString = translator.GetTextFor(tableDefinition.Fields.First(f => f.Name == "Name"));

                database.Execute("CREATE TABLE TestClass4 ( Id " + textInt + " not null, FormatNumber " + textInt + " not null, Name " + textString + "(250) not null )");

                database.CreateColumn<TestClass4>(t => t.InsertDate);

                var date = DateTime.Now;

                var query = database.From<TestClass4>().Where(t => t.InsertDate == date);

                database.Select(query);

                database.DropTable<TestClass4>();

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropColumn(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestClass>();

                database.DropColumn<TestClass>(t => t.Name);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateIndex(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestClass>();

                database.CreateIndex<TestClass>(t => new { t.Id, t.Name }, true);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropIndex(TestHelper.DatabaseType dbType, string connectionString)
        {
            string indexName = "IX_TestClass_Id_Name";

            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestClass>();

                database.CreateIndex<TestClass>(t => new { t.Id, t.Name }, true, indexName);
                database.DropIndex<TestClass>(indexName);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateForeignKey(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestClass>();
                database.CreateTable<TestClass2>();

                database.CreateForeignKey<TestClass, TestClass2>(t => t.TestClass2Id, t => t.Id);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDropForeignKey(TestHelper.DatabaseType dbType, string connectionString)
        {
            string indexName = "FK_TestClass_Id";

            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestClass>();
                database.CreateTable<TestClass2>();

                database.CreateForeignKey<TestClass, TestClass2>(t => t.TestClass2Id, t => t.Id, indexName);
                database.DropForeignKey<TestClass>(indexName);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableNoReference(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestClassNoReference>();
                database.Insert(new TestClassNoReference { Id = 1 });

                var result = database.SelectById<TestClassNoReference>(1);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.Ref, Is.Null);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCasesNoOracle))]
        public void CanCreateTableOnlyIdentityColumn(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestClassOnlyIdentity>();

                database.Insert(new TestClassOnlyIdentity());

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanCreateTableWithNullProperty(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTable<TestNullableProperty>();

                database.Insert(new TestNullableProperty { Id = 1 });

                var result = database.SelectById<TestNullableProperty>(1);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.Id, Is.EqualTo(1));
                Assert.That(result.IdMaybe, Is.Null);

                connection.Close();
            }
        }
    }
}
