using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class PerformanceTest
    {
        [Test]
        public void TimedSelect()
        {
            using (var connection = TestHelper.GetConnectionFor(TestHelper.DatabaseType.Sqlite, "Data Source=:memory:"))
            {
                var db = TestHelper.GetDatabaseFor(TestHelper.DatabaseType.Sqlite, connection);

                db.CreateTableIfNotExists<TestClass>();
                db.Insert(TestHelper.GetNewTestClass());

                for (int i = 0; i < 100000; i++)
                {
                    db.SelectById<TestClass>(1);
                }

                connection.Close();
            }
        }

        [Test]
        public void TimedSelectWhere()
        {
            using (var connection = TestHelper.GetConnectionFor(TestHelper.DatabaseType.Sqlite, "Data Source=:memory:"))
            {
                var db = TestHelper.GetDatabaseFor(TestHelper.DatabaseType.Sqlite, connection);

                db.CreateTableIfNotExists<TestClass>();
                db.Insert(TestHelper.GetNewTestClass());

                for (int i = 0; i < 100000; i++)
                {
                    db.Select<TestClass>(t => t.Id == 1);
                }

                connection.Close();
            }
        }
    }
}
