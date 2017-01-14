using DataCore.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class QueryTestPagination
    {
        [TestMethod]
        public void CanGeneratePagination()
        {
            var query = new Query<TestClass>(new Translator());

            query.Paginate(10, 5).Build();

            Assert.AreEqual("SELECT * FROM TestClass WITH(NOLOCK) LIMIT 10, 40", query.SqlCommand.ToString());
        }
    }
}
