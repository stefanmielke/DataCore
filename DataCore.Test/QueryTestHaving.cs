using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class QueryTestHaving
    {
        [Test]
        public void CanGenerateHavingWithOneArgument()
        {
            var query = new Query<TestClass>(new Translator()).Having(t => t.Id > 0);

            Assert.AreEqual("(TestClass.Id > @p0)", query.SqlHaving);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateHavingWithManyArguments()
        {
            var query = new Query<TestClass>(new Translator()).Having(t => t.Id > 0 && t.Name == "test");

            Assert.AreEqual("((TestClass.Id > @p0) AND (TestClass.Name = @p1))", query.SqlHaving);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual("test", query.Parameters.GetValues()["@p1"]);
        }

        [Test]
        public void CanGenerateHavingWithManyArgumentsAggregated()
        {
            var query = new Query<TestClass>(new Translator()).Having(t => t.Id > 0).Having(t => t.Name == "test");

            Assert.AreEqual("((TestClass.Id > @p0)) AND ((TestClass.Name = @p1))", query.SqlHaving);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual("test", query.Parameters.GetValues()["@p1"]);
        }
        
        [Test]
        public void CanGenerateHavingWithJoinedTable()
        {
            var query = new Query<TestClass>(new Translator()).Having<TestClass2>(t => t.Id > 0);

            Assert.AreEqual("(TestClass2.Id > @p0)", query.SqlHaving);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateHavingWithJoinedTableAndManyArgumentsAggregated()
        {
            var query = new Query<TestClass>(new Translator()).Having<TestClass2>(t => t.Id > 0).Having(t => t.Name == "test");

            Assert.AreEqual("((TestClass2.Id > @p0)) AND ((TestClass.Name = @p1))", query.SqlHaving);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual("test", query.Parameters.GetValues()["@p1"]);
        }
    }
}
