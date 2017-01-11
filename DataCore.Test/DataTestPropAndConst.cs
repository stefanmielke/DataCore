using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class DataTestPropAndProp
    {
        [TestMethod]
        public void CanTransformWhereClauseEquals()
        {
            var data = new Data<TestClass>();

            data.Where(t => t.Id == t.Id);

            Assert.AreEqual(data.SqlWhere.ToString(), "Id = Id");
        }

        [TestMethod]
        public void CanTransformWhereClauseGreaterThan()
        {
            var data = new Data<TestClass>();

            data.Where(t => t.Id > t.Id);

            Assert.AreEqual(data.SqlWhere.ToString(), "Id > Id");
        }

        [TestMethod]
        public void CanTransformWhereClauseGreaterThanOrEqual()
        {
            var data = new Data<TestClass>();

            data.Where(t => t.Id >= t.Id);

            Assert.AreEqual(data.SqlWhere.ToString(), "Id >= Id");
        }

        [TestMethod]
        public void CanTransformWhereClauseLessThan()
        {
            var data = new Data<TestClass>();

            data.Where(t => t.Id < t.Id);

            Assert.AreEqual(data.SqlWhere.ToString(), "Id < Id");
        }

        [TestMethod]
        public void CanTransformWhereClauseLessThanOrEqual()
        {
            var data = new Data<TestClass>();

            data.Where(t => t.Id <= t.Id);

            Assert.AreEqual(data.SqlWhere.ToString(), "Id <= Id");
        }
    }
}
