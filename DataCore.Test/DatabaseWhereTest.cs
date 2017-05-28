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
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();
                database.Insert(TestHelper.GetNewTestClass());

                var obj = database.SelectSingle<TestClass>(t => t.Id == 1);

                Assert.IsNotNull(obj);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanUseSelectWhereOnDatabase(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();
                database.Insert(TestHelper.GetNewTestClass());

                var objs = database.Select<TestClass>(t => t.Id == 1);

                Assert.That(objs.Count(), Is.GreaterThan(0));
            }
        }
    }
}
