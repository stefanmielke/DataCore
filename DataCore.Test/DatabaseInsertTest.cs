using System;
using System.Linq;
using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class DatabaseInsertTest
    {
        [Test, TestCaseSource(typeof(SqlTestDataFactory), "TestCases")]
        public void CanInsert(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                database.Insert(new TestClass
                {
                    Id = 1,
                    Name = "test",
                    Number = 1,
                    Done = true,
                    InsertDate = DateTime.Now,
                    TestClass2Id = 1
                });

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                var results = database.Select(query);
                Assert.IsTrue(results.Any());

                connection.Close();
            }
        }
    }
}
