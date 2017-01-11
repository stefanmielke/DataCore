using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class DataTestPropAndConst
    {
        [TestMethod]
        public void CanTransformWhereClauseEquals()
        {
            var data = new Data<TestClass>();

            data.Where(t => t.Id == 1);

            Assert.AreEqual(data.SqlWhere.ToString(), "Id = 1");
        }

        [TestMethod]
        public void CanTransformWhereClauseGreaterThan()
        {
            var data = new Data<TestClass>();

            data.Where(t => t.Id > 1);

            Assert.AreEqual(data.SqlWhere.ToString(), "Id > 1");
        }

        [TestMethod]
        public void CanTransformWhereClauseGreaterThanOrEqual()
        {
            var data = new Data<TestClass>();

            data.Where(t => t.Id >= 1);

            Assert.AreEqual(data.SqlWhere.ToString(), "Id >= 1");
        }

        [TestMethod]
        public void CanTransformWhereClauseLessThan()
        {
            var data = new Data<TestClass>();

            data.Where(t => t.Id < 1);

            Assert.AreEqual(data.SqlWhere.ToString(), "Id < 1");
        }

        [TestMethod]
        public void CanTransformWhereClauseLessThanOrEqual()
        {
            var data = new Data<TestClass>();

            data.Where(t => t.Id <= 1);

            Assert.AreEqual(data.SqlWhere.ToString(), "Id <= 1");
        }
    }
}
