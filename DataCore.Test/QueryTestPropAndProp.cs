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

        [Test]
        public void CanGenerateInInt()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id.In(1, 2, 3));

            Assert.That(query.SqlWhere, Is.EqualTo("(TestClass.Id IN (1,2,3)"));
        }

        [Test]
        public void CanGenerateInString()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name.In("test", "test2"));

            Assert.That(query.SqlWhere, Is.EqualTo("(TestClass.Name IN ('test','test2')"));
        }

        [Test]
        public void CanGenerateInDateTime()
        {
            var query = new Query<TestClass>(new Translator());

            var dateOne = new DateTime(2016, 01, 01);
            var dateTwo = new DateTime(2016, 01, 02);

            query.Where(t => t.InsertDate.In(dateOne, dateTwo));

            Assert.That(query.SqlWhere, Is.EqualTo("(TestClass.InsertDate IN ('2016-01-01 00:00:00.000','2016-01-02 00:00:00.000')"));
        }
    }
}
