using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class QueryTestSelect
    {
        [Test]
        public void DataTestSelectClauseNoWhere()
        {
            var data = new Query<TestClass>(new Translator());

            data.Build();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT * FROM TestClass WITH(NOLOCK)");
        }

        [Test]
        public void DataTestSelectClauseWhere()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id == 0).Build();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT * FROM TestClass WITH(NOLOCK) WHERE (TestClass.Id = 0)");
        }

        [Test]
        public void DataTestSelectClauseWithTop()
        {
            var data = new Query<TestClass>(new Translator());

            data.Top(10).Build();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT * FROM TestClass WITH(NOLOCK) LIMIT 10");
        }

        [Test]
        public void DataTestSelectClauseWithCount()
        {
            var data = new Query<TestClass>(new Translator());

            data.Count().Build();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT COUNT(*) FROM TestClass WITH(NOLOCK)");
        }

        [Test]
        public void DataTestSelectClauseWithTopAndWhere()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id == 0).Top(10).Build();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT * FROM TestClass WITH(NOLOCK) WHERE (TestClass.Id = 0) LIMIT 10");
        }

        [Test]
        public void ComplexQueryTest()
        {
            var query =
                new Query<TestClass>(new Translator())
                    .Join<TestClass2>((t, t2) => t.Id == t2.Id)
                    .LeftJoin<TestClass2>((t, t2) => t.Id == t2.Id && t2.Id == 1)
                    .RightJoin<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id && t2.Id > 1)
                    .Where(t => t.Number > 105)
                    .GroupBy(t => new { t.Id, t.Name })
                    .OrderBy(t => t.Id)
                    .Select(t => new { t.Id, t.Name })
                    .Top(103)
                    .Build();

            var expected = "SELECT TestClass.Id, TestClass.Name"
                            + " FROM TestClass WITH(NOLOCK)"
                            + " INNER JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id)"
                            + " LEFT JOIN TestClass2 WITH(NOLOCK) ON ((TestClass.Id = TestClass2.Id) AND (TestClass2.Id = 1))"
                            + " RIGHT JOIN TestClass3 WITH(NOLOCK) ON ((TestClass2.Id = TestClass3.Id) AND (TestClass3.Id > 1))"
                            + " WHERE (TestClass.Number > 105)"
                            + " GROUP BY TestClass.Id, TestClass.Name"
                            + " ORDER BY TestClass.Id"
                            + " LIMIT 103";

            Assert.AreEqual(expected, query.SqlCommand.ToString());
        }

        [Test]
        public void CanGenerateSelectColumns()
        {
            var query = new Query<TestClass>(new Translator());
            query.Select(t => new { t.Id, t.Name });

            Assert.AreEqual("TestClass.Id, TestClass.Name", query.SqlColumns);
        }

        [Test]
        public void CanGenerateSelectColumnsFromSingle()
        {
            var query = new Query<TestClass>(new Translator());
            query.Select(t => t.Id);

            Assert.AreEqual("TestClass.Id", query.SqlColumns);
        }

        [Test]
        public void CanGenerateSelectColumnsFromMultipleSelects()
        {
            var query = new Query<TestClass>(new Translator());
            query.Select(t => t.Id).Select(t => new { t.Name, t.Number });

            Assert.AreEqual("TestClass.Id, TestClass.Name, TestClass.Number", query.SqlColumns);
        }

        [Test]
        public void CanGenerateSelectColumnsWithTop()
        {
            var query = new Query<TestClass>(new Translator());
            query.Top(10).Select(t => new { t.Id, t.Name }).Build();

            Assert.AreEqual("SELECT TestClass.Id, TestClass.Name FROM TestClass WITH(NOLOCK) LIMIT 10", query.SqlCommand.ToString());
        }
    }
}
