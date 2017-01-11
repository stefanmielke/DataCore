using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DataCore.Test
{
    [TestClass]
    public class QueryTestPropAndProp
    {
        [TestMethod]
        public void CanTransformWhereClauseEquals()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id == t.Id);

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Id = TestClass.Id)");
        }

        [TestMethod]
        public void CanTransformWhereClauseGreaterThan()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id > t.Id);

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Id > TestClass.Id)");
        }

        [TestMethod]
        public void CanTransformWhereClauseGreaterThanOrEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id >= t.Id);

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Id >= TestClass.Id)");
        }

        [TestMethod]
        public void CanTransformWhereClauseLessThan()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id < t.Id);

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Id < TestClass.Id)");
        }

        [TestMethod]
        public void CanTransformWhereClauseLessThanOrEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id <= t.Id);

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Id <= TestClass.Id)");
        }

        [TestMethod]
        public void CanTransformWhereClauseNotEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id != t.Id);

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Id != TestClass.Id)");
        }

        [TestMethod]
        public void CanTransformWhereClauseString()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Name != "test");

            Assert.AreEqual(data.SqlWhere.ToString(), "(TestClass.Name != 'test')");
        }

        [TestMethod]
        public void CanTransformWhereClauseDateTimeInVariable()
        {
            var data = new Query<TestClass>(new Translator());

            var date = new DateTime(2017, 1, 1, 17, 25, 47, 789);

            data.Where(t => t.InsertDate > date);

            Assert.AreEqual("(TestClass.InsertDate > '2017-01-01 17:25:47.789')", data.SqlWhere.ToString());
        }

        [TestMethod]
        public void CanTransformWhereClauseDateTimeDirect()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.InsertDate > new DateTime(2017, 1, 1, 17, 25, 47, 789));

            Assert.AreEqual("(TestClass.InsertDate > '2017-01-01 17:25:47.789')", data.SqlWhere.ToString());
        }

        [TestMethod]
        public void CanTransformWhereClauseMethod()
        {
            var testClass = new TestClass { Name = "test" };

            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Name == GetNameString(testClass));

            Assert.AreEqual("(TestClass.Name = 'test')", data.SqlWhere.ToString());
        }

        private static string GetNameString(TestClass test)
        {
            return test.Name;
        }
    }
}
