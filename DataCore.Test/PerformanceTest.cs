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
            using (var db = TestHelper.GetDatabaseFor(TestHelper.DatabaseType.Sqlite, "Data Source=:memory:"))
            {
                db.CreateTableIfNotExists<TestClass>();
                db.Insert(TestHelper.GetNewTestClass());

                for (int i = 0; i < 100000; i++)
                {
                    db.SelectById<TestClass>(1);
                }
            }
        }

        [Test]
        public void TimedSelectWhere()
        {
            using (var db = TestHelper.GetDatabaseFor(TestHelper.DatabaseType.Sqlite, "Data Source=:memory:"))
            {
                db.CreateTableIfNotExists<TestClass>();
                db.Insert(TestHelper.GetNewTestClass());

                for (int i = 0; i < 100000; i++)
                {
                    db.Select<TestClass>(t => t.Id == 1);
                }
            }
        }
    }
}
