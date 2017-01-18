using System;
using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class QueryTestTypes
    {
        [Test]
        public void CanUseInt()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id > 0);

            Assert.AreEqual("(TestClass.Id > @p0)", query.SqlWhere);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanUseFloat()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.FloatNumber > 0);

            Assert.AreEqual("(TestClass.FloatNumber > @p0)", query.SqlWhere);
            Assert.AreEqual(0, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanUseBool()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Done);

            Assert.AreEqual("(TestClass.Done = @p0)", query.SqlWhere);
            Assert.AreEqual(true, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanUseNotBool()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => !t.Done);

            Assert.AreEqual("(TestClass.Done != @p0)", query.SqlWhere);
            Assert.AreEqual(true, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanUseString()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name != "test name");

            Assert.AreEqual("(TestClass.Name != @p0)", query.SqlWhere);
            Assert.AreEqual("test name", query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanUseDateTime()
        {
            var query = new Query<TestClass>(new Translator());

            var now = DateTime.Now;

            query.Where(t => t.InsertDate != now);

            Assert.AreEqual("(TestClass.InsertDate != @p0)", query.SqlWhere);
            Assert.AreEqual(now, query.Parameters.GetValues()["@p0"]);
        }
    }
}
