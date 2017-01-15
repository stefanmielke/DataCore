using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class QueryTestGroupBy
    {
        [Test]
        public void CanGenerateGroupByWithDynamic()
        {
            var query = new Query<TestClass>(new Translator());

            query.GroupBy(t => new { t.Id, t.Name });

            Assert.AreEqual("TestClass.Id, TestClass.Name", query.SqlGroupBy);
        }

        [Test]
        public void CanGenerateGroupByWithOneField()
        {
            var query = new Query<TestClass>(new Translator());

            query.GroupBy(t => t.Id);

            Assert.AreEqual("TestClass.Id", query.SqlGroupBy);
        }

        [Test]
        public void CanAggregateGroupBy()
        {
            var query = new Query<TestClass>(new Translator());

            query.GroupBy(t => t.Id);
            query.GroupBy(t => new { t.Number, t.Name });

            Assert.AreEqual("TestClass.Id, TestClass.Number, TestClass.Name", query.SqlGroupBy);
        }
    }
}
