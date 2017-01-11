using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class DataTestSelect
    {
        [TestMethod]
        public void DataTestSelectClauseNoWhere()
        {
            var data = new Data<TestClass>();

            data.Select();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT * FROM TestClass");
        }

        [TestMethod]
        public void DataTestSelectClauseWhere()
        {
            var data = new Data<TestClass>();

            data.Where(t => t.Id == 0).Select();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT * FROM TestClass WHERE Id = 0");
        }

        [TestMethod]
        public void DataTestSelectClauseWithTop()
        {
            var data = new Data<TestClass>();

            data.Top(10).Select();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT TOP (10) * FROM TestClass");
        }

        [TestMethod]
        public void DataTestSelectClauseWithTopAndWhere()
        {
            var data = new Data<TestClass>();

            data.Where(t => t.Id == 0).Top(10).Select();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT TOP (10) * FROM TestClass WHERE Id = 0");
        }
    }
}
