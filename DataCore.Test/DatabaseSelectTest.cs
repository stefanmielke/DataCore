using System.Linq;
using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class DatabaseSelectTest
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
        public void CanPaginateSelect(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                var query = database.From<TestClass>().Where(t => t.Id > 1).OrderBy(t => t.Id).Paginate(10, 2);

                database.Select(query);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void ExistsReturnTrueWhenExists(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestIgnore>();
                database.Insert(new TestIgnore { Id = 1, FloatNumber = 1, Name = "test" });

                var query = database.From<TestIgnore>().Where(t => t.Id == 1);

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
        public void CanSelectById(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();
                database.Insert(TestHelper.GetNewTestClass());

                var obj = database.SelectById<TestClass>(1);

                Assert.IsNotNull(obj);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanSelectByIds(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                var test1 = TestHelper.GetNewTestClass();
                var test2 = TestHelper.GetNewTestClass();
                test2.Id = 2;

                database.Insert(test1);
                database.Insert(test2);

                var result = database.SelectById<TestClass>(1, 2);

                Assert.AreEqual(2, result.Count());

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanSelectComplex(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();
                database.CreateTableIfNotExists<TestClass2>();
                database.CreateTableIfNotExists<TestClass3>();

                var query =
                    database.From<TestClass>()
                        .Join<TestClass2>((t, t2) => t.Id == t2.Id && t2.Id == 1)
                        .LeftJoin<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id && t2.Id > 1)
                        .Where(t => t.FloatNumber > 105)
                        .GroupBy(t => new { t.Id, t.Name })
                        .Having(t => t.Id > 0)
                        .OrderBy(t => t.Id)
                        .Select(t => new { t.Id, t.Name })
                        .Top(103);

                database.Select(query);

                connection.Close();
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanSelectWithSqlExtensions(TestHelper.DatabaseType dbType, string connectionString)
        {
            using (var connection = TestHelper.GetConnectionFor(dbType, connectionString))
            {
                var database = TestHelper.GetDatabaseFor(dbType, connection);

                database.CreateTableIfNotExists<TestClass>();

                var query =
                    database.From<TestClass>()
                        .Where(t => t.Name.Upper().Length() > 10 && t.Name.Lower().Like("%abcd%") && t.FloatNumber.IsNull(0f) > 0)
                        .GroupBy(t => t.Name)
                        .Having(t => t.Id.Min() > 1 && t.Id.Max() < 10 && t.Id.Sum() < 100)
                        .Select(t => new
                        {
                            Name = t.Name.TrimSql().As("Name"),
                            MinId = t.Id.Min().As("MinId"),
                            MaxId = t.Id.Max().As("MaxId"),
                            Sum = t.Id.Sum().As("Sum"),
                            NotNull1 = t.Name.IsNull("null").As("NotNullName"),
                            Cast = t.Name.Cast<string, int>().As("Casted"),
                            Avg = t.FloatNumber.Average().As("Avg")
                        });

                database.Select(query);

                connection.Close();
            }
        }
    }
}
