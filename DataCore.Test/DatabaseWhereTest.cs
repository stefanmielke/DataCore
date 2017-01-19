using System.Linq;
using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class DatabaseWhereTest
    {
        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanUseSelectSingleWhereOnDatabase(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();
                database.Insert(TestHelper.GetNewTestClass());

                var obj = database.SelectSingle<TestClass>(t => t.Id == 1);

                Assert.IsNotNull(obj);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanUseSelectWhereOnDatabase(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();
                database.Insert(TestHelper.GetNewTestClass());

                var objs = database.Select<TestClass>(t => t.Id == 1);

                Assert.That(objs.Count(), Is.GreaterThan(0));

                connection.Close();
            }
        }
    }
}
