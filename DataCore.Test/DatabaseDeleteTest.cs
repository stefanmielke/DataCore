using System;
using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class DatabaseDeleteTest
    {
        [Test, TestCaseSource(typeof(SqlTestDataFactory), "TestCases")]
        public void CanDeleteOne(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                var testClass = new TestClass
                {
                    Id = 1,
                    Name = "test",
                    FloatNumber = 1,
                    Done = true,
                    InsertDate = DateTime.Now,
                    TestClass2Id = 1
                };
                database.Insert(testClass);

                database.Delete<TestClass>(t => t.Id == 1);

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.IsNull(result);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), "TestCases")]
        public void CanDeleteMany(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                var testClass = new TestClass
                {
                    Id = 1,
                    Name = "test2",
                    FloatNumber = 1,
                    Done = true,
                    InsertDate = DateTime.Now,
                    TestClass2Id = 1
                };
                database.Insert(testClass);

                testClass.Id = 2;
                testClass.Name = "test";
                database.Insert(testClass);

                database.Delete<TestClass>(t => t.Name == "test" || t.Id == 1);

                var query = database.From<TestClass>().Where(t => t.Name == "test" || t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.IsNull(result);

                connection.Close();
            }
        }
    }
}
