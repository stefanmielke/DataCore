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

            Assert.AreEqual("(TestClass.Id > 0)", query.SqlWhere);
        }

        [Test]
        public void CanUseFloat()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Number > 0);

            Assert.AreEqual("(TestClass.Number > 0)", query.SqlWhere);
        }

        [Test]
        public void CanUseBool()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Done);

            Assert.AreEqual("(TestClass.Done = 1)", query.SqlWhere);
        }

        [Test]
        public void CanUseNotBool()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => !t.Done);

            Assert.AreEqual("(TestClass.Done != 1)", query.SqlWhere);
        }

        [Test]
        public void CanUseString()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name != "test name");

            Assert.AreEqual("(TestClass.Name != 'test name')", query.SqlWhere);
        }

        [Test]
        public void CanUseDateTime()
        {
            var query = new Query<TestClass>(new Translator());

            var now = DateTime.Now;

            query.Where(t => t.InsertDate != now);

            Assert.AreEqual("(TestClass.InsertDate != '" + now.ToString("yyyy-MM-dd HH:mm:ss.fff")  + "')", query.SqlWhere);
        }
    }
}
