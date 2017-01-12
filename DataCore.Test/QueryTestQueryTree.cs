using DataCore.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class QueryTestQueryTree
    {
        [TestMethod]
        public void CanGenerateAndClause()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id == 0 && t.Number == 1);

            Assert.AreEqual("((TestClass.Id = 0) AND (TestClass.Number = 1))", data.SqlWhere.ToString());
        }

        [TestMethod]
        public void CanGenerateOrClause()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id == 0 || t.Number == 1);

            Assert.AreEqual("((TestClass.Id = 0) OR (TestClass.Number = 1))", data.SqlWhere.ToString());
        }

        [TestMethod]
        public void CanGenerateAndClauseCapsuled()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => (t.Id == 0 && t.Number == 1) && t.Id == 1);

            Assert.AreEqual("(((TestClass.Id = 0) AND (TestClass.Number = 1)) AND (TestClass.Id = 1))", data.SqlWhere.ToString());
        }

        [TestMethod]
        public void CanGenerateClauseDoubleCapsuled()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => (t.Id == 0 && t.Number == 1) || (t.Id == 1 && t.Number == 0));

            Assert.AreEqual("(((TestClass.Id = 0) AND (TestClass.Number = 1)) OR ((TestClass.Id = 1) AND (TestClass.Number = 0)))", data.SqlWhere.ToString());
        }

        [TestMethod]
        public void CanGenerateClauseDoubleWhere()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id == 1 && t.Number == 0);
            data.Where(t => t.Name == "test");

            Assert.AreEqual("(((TestClass.Id = 1) AND (TestClass.Number = 0))) AND ((TestClass.Name = 'test'))", data.SqlWhere.ToString());
        }

        [TestMethod]
        public void CanGenerateClauseDoubleWhereWithOr()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id == 1 && t.Number == 0);
            data.Or(t => t.Name == "test");

            Assert.AreEqual("(((TestClass.Id = 1) AND (TestClass.Number = 0))) OR ((TestClass.Name = 'test'))", data.SqlWhere.ToString());
        }
    }
}
