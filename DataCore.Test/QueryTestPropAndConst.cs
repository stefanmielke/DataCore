using System;
using DataCore;
using DataCore.Test.Models;
using NUnit.Framework;

namespace queryCore.Test
{
    [TestFixture]
    public class QueryTestPropAndProp
    {
        [Test]
        public void CanTransformWhereClauseEquals()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id == t.Id);

            Assert.AreEqual(query.SqlWhere, "(TestClass.Id = TestClass.Id)");
        }

        [Test]
        public void CanTransformWhereClauseGreaterThan()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id > t.Id);

            Assert.AreEqual(query.SqlWhere, "(TestClass.Id > TestClass.Id)");
        }

        [Test]
        public void CanTransformWhereClauseGreaterThanOrEqual()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id >= t.Id);

            Assert.AreEqual(query.SqlWhere, "(TestClass.Id >= TestClass.Id)");
        }

        [Test]
        public void CanTransformWhereClauseLessThan()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id < t.Id);

            Assert.AreEqual(query.SqlWhere, "(TestClass.Id < TestClass.Id)");
        }

        [Test]
        public void CanTransformWhereClauseLessThanOrEqual()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id <= t.Id);

            Assert.AreEqual(query.SqlWhere, "(TestClass.Id <= TestClass.Id)");
        }

        [Test]
        public void CanTransformWhereClauseNotEqual()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id != t.Id);

            Assert.AreEqual(query.SqlWhere, "(TestClass.Id != TestClass.Id)");
        }

        [Test]
        public void CanTransformWhereClauseString()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name != "test");

            Assert.AreEqual("(TestClass.Name != @p0)", query.SqlWhere);
            Assert.AreEqual("test", query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanTransformWhereClauseDateTimeInVariable()
        {
            var query = new Query<TestClass>(new Translator());

            var date = new DateTime(2017, 1, 1, 17, 25, 47, 789);

            query.Where(t => t.InsertDate > date);

            Assert.AreEqual("(TestClass.InsertDate > @p0)", query.SqlWhere);
            Assert.AreEqual(date, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanTransformWhereClauseDateTimeDirect()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.InsertDate > new DateTime(2017, 1, 1, 17, 25, 47, 789));

            Assert.AreEqual("(TestClass.InsertDate > @p0)", query.SqlWhere);
            Assert.AreEqual(new DateTime(2017, 1, 1, 17, 25, 47, 789), query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanTransformWhereClauseMethod()
        {
            var testClass = new TestClass { Name = "test" };

            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name == GetNameString(testClass));

            Assert.AreEqual("(TestClass.Name = @p0)", query.SqlWhere);
            Assert.AreEqual("test", query.Parameters.GetValues()["@p0"]);
        }

        private static string GetNameString(TestClass test)
        {
            return test.Name;
        }
    }
}
