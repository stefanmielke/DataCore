using DataCore.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class QueryTestPropAndConst
    {
        [TestMethod]
        public void CanTransformWhereClauseEquals()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id == 1);

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Id = 1)");
        }

        [TestMethod]
        public void CanTransformWhereClauseGreaterThan()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id > 1);

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Id > 1)");
        }

        [TestMethod]
        public void CanTransformWhereClauseGreaterThanOrEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id >= 1);

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Id >= 1)");
        }

        [TestMethod]
        public void CanTransformWhereClauseLessThan()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id < 1);

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Id < 1)");
        }

        [TestMethod]
        public void CanTransformWhereClauseLessThanOrEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id <= 1);

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Id <= 1)");
        }

        [TestMethod]
        public void CanTransformWhereClauseNotEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id != 1);

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Id != 1)");
        }
    }
}
