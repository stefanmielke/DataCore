using System;
using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class QueryTestPropAndConst
    {
        [Test]
        public void CanTransformWhereClauseEquals()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id == 1);

            Assert.AreEqual(data.SqlWhere, "(TestClass.Id = 1)");
        }

        [Test]
        public void CanTransformWhereClauseGreaterThan()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id > 1);

            Assert.AreEqual(data.SqlWhere, "(TestClass.Id > 1)");
        }

        [Test]
        public void CanTransformWhereClauseGreaterThanOrEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id >= 1);

            Assert.AreEqual(data.SqlWhere, "(TestClass.Id >= 1)");
        }

        [Test]
        public void CanTransformWhereClauseLessThan()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id < 1);

            Assert.AreEqual(data.SqlWhere, "(TestClass.Id < 1)");
        }

        [Test]
        public void CanTransformWhereClauseLessThanOrEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id <= 1);

            Assert.AreEqual(data.SqlWhere, "(TestClass.Id <= 1)");
        }

        [Test]
        public void CanTransformWhereClauseNotEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id != 1);

            Assert.AreEqual(data.SqlWhere, "(TestClass.Id != 1)");
        }

        [Test]
        public void CanGenerateBetweenInt()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id.Between(1, 10));

            Assert.That(query.SqlWhere, Is.EqualTo("(TestClass.Id BETWEEN 1 AND 10)"));
        }

        [Test]
        public void CanGenerateBetweenDateTime()
        {
            var query = new Query<TestClass>(new Translator());

            var dateFrom = new DateTime(2016, 01, 01);
            var dateTo = new DateTime(2016, 01, 02);

            query.Where(t => t.InsertDate.Between(dateFrom, dateTo));

            Assert.That(query.SqlWhere, Is.EqualTo("(TestClass.InsertDate BETWEEN '2016-01-01 00:00:00.000' AND '2016-01-02 00:00:00.000')"));
        }
    }
}
