using System;
using DataCore.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class QueryTestTypes
    {
        [TestMethod]
        public void CanUseInt()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id > 0);

            Assert.AreEqual("(TestClass.Id > 0)", query.SqlWhere);
        }

        [TestMethod]
        public void CanUseFloat()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Number > 0);

            Assert.AreEqual("(TestClass.Number > 0)", query.SqlWhere);
        }

        [TestMethod]
        public void CanUseBool()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Done);

            Assert.AreEqual("(TestClass.Done = 1)", query.SqlWhere);
        }

        [TestMethod]
        public void CanUseNotBool()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => !t.Done);

            Assert.AreEqual("(TestClass.Done != 1)", query.SqlWhere);
        }

        [TestMethod]
        public void CanUseString()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name != "test name");

            Assert.AreEqual("(TestClass.Name != 'test name')", query.SqlWhere);
        }

        [TestMethod]
        public void CanUseDateTime()
        {
            var query = new Query<TestClass>(new Translator());

            var now = DateTime.Now;

            query.Where(t => t.InsertDate != now);

            Assert.AreEqual("(TestClass.InsertDate != '" + now.ToString("yyyy-MM-dd HH:mm:ss.fff")  + "')", query.SqlWhere);
        }
    }
}
