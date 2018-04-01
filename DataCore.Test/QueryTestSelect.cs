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
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id == 0).Build();

            Assert.AreEqual(query.SqlCommand.ToString(), "SELECT * FROM TestClass WITH(NOLOCK) WHERE (TestClass.Id = @p0)");
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
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
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id == 0).Top(10).Build();

            Assert.AreEqual(query.SqlCommand.ToString(), "SELECT * FROM TestClass WITH(NOLOCK) WHERE (TestClass.Id = @p0) LIMIT 10");
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void ComplexQueryTest()
        {
            var query =
                new Query<TestClass>(new Translator())
                    .Join<TestClass2>((t, t2) => t.Id == t2.Id)
                    .LeftJoin<TestClass2>((t, t2) => t.Id == t2.Id && t2.Id == 1)
                    .RightJoin<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id && t2.Id > 1)
                    .Where(t => t.FloatNumber > 105)
                    .GroupBy(t => new { t.Id, t.Name })
                    .Having(t => t.Id > 0)
                    .OrderBy(t => t.Id)
                    .Select(t => new { t.Id, t.Name })
                    .Top(103)
                    .Build();

            const string expected = "SELECT TestClass.Id, TestClass.Name"
                                    + " FROM TestClass WITH(NOLOCK)"
                                    + " INNER JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id)"
                                    + " LEFT JOIN TestClass2 WITH(NOLOCK) ON ((TestClass.Id = TestClass2.Id) AND (TestClass2.Id = @p0))"
                                    + " RIGHT JOIN TestClass3 WITH(NOLOCK) ON ((TestClass2.Id = TestClass3.Id) AND (TestClass3.Id > @p1))"
                                    + " WHERE (TestClass.FloatNumber > @p2)"
                                    + " GROUP BY TestClass.Id, TestClass.Name"
                                    + " HAVING (TestClass.Id > @p3)"
                                    + " ORDER BY TestClass.Id"
                                    + " LIMIT 103";

            Assert.AreEqual(expected, query.SqlCommand.ToString());
            Assert.AreEqual(1, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(1, query.Parameters.GetValues()["@p1"]);
            Assert.AreEqual(105, query.Parameters.GetValues()["@p2"]);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p3"]);
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
            query.Select(t => t.Id).Select(t => new { t.Name, Number = t.FloatNumber });

            Assert.AreEqual("TestClass.Id, TestClass.Name, TestClass.FloatNumber", query.SqlColumns);
        }

        [Test]
        public void CanGenerateSelectColumnsWithTop()
        {
            var query = new Query<TestClass>(new Translator());
            query.Top(10).Select(t => new { t.Id, t.Name }).Build();

            Assert.AreEqual("SELECT TestClass.Id, TestClass.Name FROM TestClass WITH(NOLOCK) LIMIT 10", query.SqlCommand.ToString());
        }

        [Test]
        public void CanGenerateSelectWithMin()
        {
            var query = new Query<TestClass>(new Translator());
            query.Select(t => t.Id.Min());

            Assert.That(query.SqlColumns, Is.EqualTo("MIN(TestClass.Id)"));
        }

        [Test]
        public void CanGenerateSelectWithMax()
        {
            var query = new Query<TestClass>(new Translator());
            query.Select(t => t.Id.Max());

            Assert.That(query.SqlColumns, Is.EqualTo("MAX(TestClass.Id)"));
        }

        [Test]
        public void CanGenerateSelectWithMaxAndMin()
        {
            var query = new Query<TestClass>(new Translator());
            query.Select(t => new { Max = t.Id.Max(), Min = t.Id.Min() });

            Assert.That(query.SqlColumns, Is.EqualTo("MAX(TestClass.Id), MIN(TestClass.Id)"));
        }

        [Test]
        public void CanGenerateHavingWithMin()
        {
            var query = new Query<TestClass>(new Translator());
            query.Having(t => t.Id.Min() == 10);

            Assert.That(query.SqlHaving, Is.EqualTo("(MIN(TestClass.Id) = @p0)"));
            Assert.AreEqual(10, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateHavingWithMax()
        {
            var query = new Query<TestClass>(new Translator());
            query.Having(t => t.Id.Max() != 10);

            Assert.That(query.SqlHaving, Is.EqualTo("(MAX(TestClass.Id) != @p0)"));
            Assert.AreEqual(10, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateHavingWithMaxAndMin()
        {
            var query = new Query<TestClass>(new Translator());
            query.Having(t => t.Id.Max() > 0 && t.Id.Min() < 10);

            Assert.That(query.SqlHaving, Is.EqualTo("((MAX(TestClass.Id) > @p0) AND (MIN(TestClass.Id) < @p1))"));
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(10, query.Parameters.GetValues()["@p1"]);
        }

        [Test]
        public void CanGenerateSelectWithSum()
        {
            var query = new Query<TestClass>(new Translator());
            query.Select(t => t.Id.Sum());

            Assert.That(query.SqlColumns, Is.EqualTo("SUM(TestClass.Id)"));
        }

        [Test]
        public void CanGenerateHavingWithSum()
        {
            var query = new Query<TestClass>(new Translator());
            query.Having(t => t.Id.Sum() > 0);

            Assert.That(query.SqlHaving, Is.EqualTo("(SUM(TestClass.Id) > @p0)"));
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateSelectWithCount()
        {
            var query = new Query<TestClass>(new Translator());
            query.Select(t => t.Id.Count());

            Assert.That(query.SqlColumns, Is.EqualTo("COUNT(TestClass.Id)"));
        }

        [Test]
        public void CanGenerateHavingWithCount()
        {
            var query = new Query<TestClass>(new Translator());
            query.Having(t => t.Id.Count() > 0);

            Assert.That(query.SqlHaving, Is.EqualTo("(COUNT(TestClass.Id) > @p0)"));
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateSelectWithAlias()
        {
            var query = new Query<TestClass>(new Translator());
            query.Select(t => t.Id.Sum().As("ID TEST"));

            Assert.That(query.SqlColumns, Is.EqualTo("SUM(TestClass.Id) AS 'ID TEST'"));
        }

        [Test]
        public void CanGenerateSelectWithAliases()
        {
            var query = new Query<TestClass>(new Translator());
            query.Select(t => new { Sum = t.Id.Sum().As("ID TEST SUM"), NewName = t.Name.As("NewName") });

            Assert.That(query.SqlColumns, Is.EqualTo("SUM(TestClass.Id) AS 'ID TEST SUM', TestClass.Name AS 'NewName'"));
        }

        [Test]
        public void CanGenerateUnion()
        {
            var query = new Query<TestClass>(new Translator());
            query.Union(new Query<TestClass>(new Translator()).OrderBy(t => t.Id)).Build();

            Assert.That(query.SqlCommand.ToString(),
                Is.EqualTo(
                    "SELECT * FROM TestClass WITH(NOLOCK) UNION SELECT * FROM TestClass WITH(NOLOCK) ORDER BY TestClass.Id"));
        }

        [Test]
        public void CanGenerateUnionAll()
        {
            var query = new Query<TestClass>(new Translator());
            query.UnionAll(new Query<TestClass>(new Translator()).OrderBy(t => t.Id)).Build();

            Assert.That(query.SqlCommand.ToString(),
                Is.EqualTo(
                    "SELECT * FROM TestClass WITH(NOLOCK) UNION ALL SELECT * FROM TestClass WITH(NOLOCK) ORDER BY TestClass.Id"));
        }

        [Test]
        public void CanGenerateSelectWithMultipleTables()
        {
            var data = new Query<TestClass>(new Translator());
            data.Join<TestClass2>((a, b) => a.TestClass2Id == b.Id);
            data.Select(a => new { a.Id, a.Done }).Select<TestClass2>(b => new { b.Id });
            data.Top(10).Build();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT TestClass.Id, TestClass.Done, TestClass2.Id FROM TestClass WITH(NOLOCK) INNER JOIN TestClass2 WITH(NOLOCK) ON (TestClass.TestClass2Id = TestClass2.Id) LIMIT 10");
        }
    }
}
