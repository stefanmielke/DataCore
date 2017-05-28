using System;
using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class DatabaseDeleteTest
    {
        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDeleteOne(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
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
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDeleteMany(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
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
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDeleteById(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();
                
                database.Insert(TestHelper.GetNewTestClass());

                database.DeleteById<TestClass>(1);

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.IsNull(result);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanDeleteByIds(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();

                var test1 = TestHelper.GetNewTestClass();
                var test2 = TestHelper.GetNewTestClass();
                test2.Id = 2;

                database.Insert(test1);
                database.Insert(test2);

                database.DeleteById<TestClass>(1, 2);

                var query = database.From<TestClass>().Where(t => t.Id.In(1, 2));

                var result = database.Select(query);
                Assert.IsEmpty(result);
            }
        }
    }
}
