using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class QueryTestQueryTree
    {
        [Test]
        public void CanGenerateAndClause()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id == 0 && t.Number == 1);

            Assert.AreEqual("((TestClass.Id = @p0) AND (TestClass.Number = @p1))", query.SqlWhere);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(1, query.Parameters.GetValues()["@p1"]);
        }

        [Test]
        public void CanGenerateOrClause()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id == 0 || t.Number == 1);

            Assert.AreEqual("((TestClass.Id = @p0) OR (TestClass.Number = @p1))", query.SqlWhere);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(1, query.Parameters.GetValues()["@p1"]);
        }

        [Test]
        public void CanGenerateAndClauseCapsuled()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => (t.Id == 0 && t.Number == 1) && t.Id == 1);

            Assert.AreEqual("(((TestClass.Id = @p0) AND (TestClass.Number = @p1)) AND (TestClass.Id = @p2))", query.SqlWhere);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(1, query.Parameters.GetValues()["@p1"]);
            Assert.AreEqual(1, query.Parameters.GetValues()["@p2"]);
        }

        [Test]
        public void CanGenerateClauseDoubleCapsuled()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => (t.Id == 0 && t.Number == 1) || (t.Id == 1 && t.Number == 0));

            Assert.AreEqual("(((TestClass.Id = @p0) AND (TestClass.Number = @p1)) OR ((TestClass.Id = @p2) AND (TestClass.Number = @p3)))", query.SqlWhere);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(1, query.Parameters.GetValues()["@p1"]);
            Assert.AreEqual(1, query.Parameters.GetValues()["@p2"]);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p3"]);
        }

        [Test]
        public void CanGenerateClauseDoubleWhere()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id == 1 && t.Number == 0);
            query.Where(t => t.Name == "test");

            Assert.AreEqual("(((TestClass.Id = @p0) AND (TestClass.Number = @p1))) AND ((TestClass.Name = @p2))", query.SqlWhere);
            Assert.AreEqual(1, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p1"]);
            Assert.AreEqual("test", query.Parameters.GetValues()["@p2"]);
        }

        [Test]
        public void CanGenerateClauseDoubleWhereWithOr()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id == 1 && t.Number == 0);
            query.Or(t => t.Name == "test");

            Assert.AreEqual("(((TestClass.Id = @p0) AND (TestClass.Number = @p1))) OR ((TestClass.Name = @p2))", query.SqlWhere);
            Assert.AreEqual(1, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p1"]);
            Assert.AreEqual("test", query.Parameters.GetValues()["@p2"]);
        }
    }
}
