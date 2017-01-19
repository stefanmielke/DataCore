using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class QueryTestJoins
    {
        [Test]
        public void CanGenerateInnerJoin()
        {
            var data = new Query<TestClass>(new Translator());

            data.Join<TestClass2>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass WITH(NOLOCK) INNER JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id)", data.SqlFrom);
        }

        [Test]
        public void CanGenerateLeftJoin()
        {
            var data = new Query<TestClass>(new Translator());

            data.LeftJoin<TestClass2>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass WITH(NOLOCK) LEFT JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id)", data.SqlFrom);
        }

        [Test]
        public void CanGenerateRightJoin()
        {
            var data = new Query<TestClass>(new Translator());

            data.RightJoin<TestClass2>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass WITH(NOLOCK) RIGHT JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id)", data.SqlFrom);
        }

        [Test]
        public void CanGenerateInnerJoinAlso()
        {
            var data = new Query<TestClass>(new Translator());

            data.Join<TestClass2>((t, t2) => t.Id == t2.Id);
            data.Join<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass WITH(NOLOCK) INNER JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id) INNER JOIN TestClass3 WITH(NOLOCK) ON (TestClass2.Id = TestClass3.Id)", data.SqlFrom);
        }

        [Test]
        public void CanGenerateLeftJoinAlso()
        {
            var data = new Query<TestClass>(new Translator());

            data.LeftJoin<TestClass2>((t, t2) => t.Id == t2.Id);
            data.LeftJoin<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass WITH(NOLOCK) LEFT JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id) LEFT JOIN TestClass3 WITH(NOLOCK) ON (TestClass2.Id = TestClass3.Id)", data.SqlFrom);
        }

        [Test]
        public void CanGenerateRightJoinAlso()
        {
            var data = new Query<TestClass>(new Translator());

            data.RightJoin<TestClass2>((t, t2) => t.Id == t2.Id);
            data.RightJoin<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass WITH(NOLOCK) RIGHT JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id) RIGHT JOIN TestClass3 WITH(NOLOCK) ON (TestClass2.Id = TestClass3.Id)", data.SqlFrom);
        }

        [Test]
        public void CanGenerateWhereOnJoinedTable()
        {
            var query = new Query<TestClass>(new Translator());

            query.Join<TestClass2>((t, t2) => t.Id == t2.Id);
            query.Where<TestClass2>(t2 => t2.Id > 1);

            Assert.AreEqual("(TestClass2.Id > @p0)", query.SqlWhere);
            Assert.AreEqual(1, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateWhereAndOnJoinedTable()
        {
            var query = new Query<TestClass>(new Translator());

            query.Join<TestClass2>((t, t2) => t.Id == t2.Id);
            query.Where<TestClass2>(t2 => t2.Id > 1);
            query.And<TestClass2>(t2 => t2.Id < 10);

            Assert.AreEqual("((TestClass2.Id > @p0)) AND ((TestClass2.Id < @p1))", query.SqlWhere);
            Assert.AreEqual(1, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(10, query.Parameters.GetValues()["@p1"]);
        }

        [Test]
        public void CanGenerateWhereOrOnJoinedTable()
        {
            var query = new Query<TestClass>(new Translator());

            query.Join<TestClass2>((t, t2) => t.Id == t2.Id);
            query.Where<TestClass2>(t2 => t2.Id > 1);
            query.Or<TestClass2>(t2 => t2.Id < 10);

            Assert.AreEqual("((TestClass2.Id > @p0)) OR ((TestClass2.Id < @p1))", query.SqlWhere);
            Assert.AreEqual(1, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(10, query.Parameters.GetValues()["@p1"]);
        }

        [Test]
        public void CanGenerateJoinWhereOnJoinedTable()
        {
            var query = new Query<TestClass>(new Translator());

            query.Join<TestClass2>((t, t2) => t.Id == t2.Id);
            query.Where<TestClass2>((t, t2) => t2.Id > t.Id);

            Assert.AreEqual("(TestClass2.Id > TestClass.Id)", query.SqlWhere);
        }

        [Test]
        public void CanGenerateJoinWhereAndOnJoinedTable()
        {
            var query = new Query<TestClass>(new Translator());

            query.Join<TestClass2>((t, t2) => t.Id == t2.Id);
            query.Where<TestClass2>((t, t2) => t2.Id > t.Id);
            query.And<TestClass2>((t, t2) => t2.Id < t.Id);

            Assert.AreEqual("((TestClass2.Id > TestClass.Id)) AND ((TestClass2.Id < TestClass.Id))", query.SqlWhere);
        }

        [Test]
        public void CanGenerateJoinWhereOrOnJoinedTable()
        {
            var query = new Query<TestClass>(new Translator());

            query.Join<TestClass2>((t, t2) => t.Id == t2.Id);
            query.Where<TestClass2>((t, t2) => t2.Id > t.Id);
            query.Or<TestClass2>((t, t2) => t2.Id < t.Id);

            Assert.AreEqual("((TestClass2.Id > TestClass.Id)) OR ((TestClass2.Id < TestClass.Id))", query.SqlWhere);
        }

        [Test]
        public void CanGenerateJoinWhereOnJoinedTables()
        {
            var query = new Query<TestClass>(new Translator());

            query.Join<TestClass2>((t, t2) => t.Id == t2.Id);
            query.Join<TestClass2, TestClass3>((t2, t3) => t2.Id == t3.Id);

            query.Where<TestClass2, TestClass3>((t2, t3) => t2.Id > t3.Id);

            Assert.AreEqual("(TestClass2.Id > TestClass3.Id)", query.SqlWhere);
        }

        [Test]
        public void CanGenerateJoinWhereAndOnJoinedTables()
        {
            var query = new Query<TestClass>(new Translator());

            query.Join<TestClass2>((t, t2) => t.Id == t2.Id);
            query.Join<TestClass2, TestClass3>((t2, t3) => t2.Id == t3.Id);

            query.Where<TestClass2, TestClass3>((t2, t3) => t2.Id > t3.Id);
            query.And<TestClass2, TestClass3>((t2, t3) => t2.Id < t3.Id);

            Assert.AreEqual("((TestClass2.Id > TestClass3.Id)) AND ((TestClass2.Id < TestClass3.Id))", query.SqlWhere);
        }

        [Test]
        public void CanGenerateJoinWhereOrOnJoinedTables()
        {
            var query = new Query<TestClass>(new Translator());

            query.Join<TestClass2>((t, t2) => t.Id == t2.Id);
            query.Join<TestClass2, TestClass3>((t2, t3) => t2.Id == t3.Id);

            query.Where<TestClass2, TestClass3>((t2, t3) => t2.Id > t3.Id);
            query.Or<TestClass2, TestClass3>((t2, t3) => t2.Id < t3.Id);

            Assert.AreEqual("((TestClass2.Id > TestClass3.Id)) OR ((TestClass2.Id < TestClass3.Id))", query.SqlWhere);
        }
    }
}
