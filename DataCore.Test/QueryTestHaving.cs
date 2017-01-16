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
            var data = new Query<TestClass>(new Translator()).Having(t => t.Id > 0);

            Assert.AreEqual("(TestClass.Id > 0)", data.SqlHaving);
        }

        [Test]
        public void CanGenerateHavingWithManyArguments()
        {
            var data = new Query<TestClass>(new Translator()).Having(t => t.Id > 0 && t.Name == "test");

            Assert.AreEqual("((TestClass.Id > 0) AND (TestClass.Name = 'test'))", data.SqlHaving);
        }

        [Test]
        public void CanGenerateHavingWithManyArgumentsAggregated()
        {
            var data = new Query<TestClass>(new Translator()).Having(t => t.Id > 0).Having(t => t.Name == "test");

            Assert.AreEqual("((TestClass.Id > 0)) AND ((TestClass.Name = 'test'))", data.SqlHaving);
        }
    }
}
