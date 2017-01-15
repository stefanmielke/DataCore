using System;
using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class QueryTestPropAndProp
    {
        [Test]
        public void CanTransformWhereClauseEquals()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id == t.Id);

            Assert.AreEqual(data.SqlWhere, "(TestClass.Id = TestClass.Id)");
        }

        [Test]
        public void CanTransformWhereClauseGreaterThan()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id > t.Id);

            Assert.AreEqual(data.SqlWhere, "(TestClass.Id > TestClass.Id)");
        }

        [Test]
        public void CanTransformWhereClauseGreaterThanOrEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id >= t.Id);

            Assert.AreEqual(data.SqlWhere, "(TestClass.Id >= TestClass.Id)");
        }

        [Test]
        public void CanTransformWhereClauseLessThan()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id < t.Id);

            Assert.AreEqual(data.SqlWhere, "(TestClass.Id < TestClass.Id)");
        }

        [Test]
        public void CanTransformWhereClauseLessThanOrEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id <= t.Id);

            Assert.AreEqual(data.SqlWhere, "(TestClass.Id <= TestClass.Id)");
        }

        [Test]
        public void CanTransformWhereClauseNotEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id != t.Id);

            Assert.AreEqual(data.SqlWhere, "(TestClass.Id != TestClass.Id)");
        }

        [Test]
        public void CanTransformWhereClauseString()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Name != "test");

            Assert.AreEqual(data.SqlWhere, "(TestClass.Name != 'test')");
        }

        [Test]
        public void CanTransformWhereClauseDateTimeInVariable()
        {
            var data = new Query<TestClass>(new Translator());

            var date = new DateTime(2017, 1, 1, 17, 25, 47, 789);

            data.Where(t => t.InsertDate > date);

            Assert.AreEqual("(TestClass.InsertDate > '2017-01-01 17:25:47.789')", data.SqlWhere);
        }

        [Test]
        public void CanTransformWhereClauseDateTimeDirect()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.InsertDate > new DateTime(2017, 1, 1, 17, 25, 47, 789));

            Assert.AreEqual("(TestClass.InsertDate > '2017-01-01 17:25:47.789')", data.SqlWhere);
        }

        [Test]
        public void CanTransformWhereClauseMethod()
        {
            var testClass = new TestClass { Name = "test" };

            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Name == GetNameString(testClass));

            Assert.AreEqual("(TestClass.Name = 'test')", data.SqlWhere);
        }

        private static string GetNameString(TestClass test)
        {
            return test.Name;
        }
    }
}
