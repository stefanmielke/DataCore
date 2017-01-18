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

            Assert.AreEqual("(TestClass.Id = @p0)", data.SqlWhere);
            Assert.AreEqual(1, data.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanTransformWhereClauseGreaterThan()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id > 1);

            Assert.AreEqual("(TestClass.Id > @p0)", data.SqlWhere);
            Assert.AreEqual(1, data.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanTransformWhereClauseGreaterThanOrEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id >= 1);

            Assert.AreEqual("(TestClass.Id >= @p0)", data.SqlWhere);
            Assert.AreEqual(1, data.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanTransformWhereClauseLessThan()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id < 1);

            Assert.AreEqual("(TestClass.Id < @p0)", data.SqlWhere);
            Assert.AreEqual(1, data.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanTransformWhereClauseLessThanOrEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id <= 1);

            Assert.AreEqual("(TestClass.Id <= @p0)", data.SqlWhere);
            Assert.AreEqual(1, data.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanTransformWhereClauseNotEqual()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id != 1);

            Assert.AreEqual("(TestClass.Id != @p0)", data.SqlWhere);
            Assert.AreEqual(1, data.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateBetweenInt()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id.Between(1, 10));

            Assert.That(query.SqlWhere, Is.EqualTo("(TestClass.Id BETWEEN @p0 AND @p1)"));
            Assert.AreEqual(1, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(10, query.Parameters.GetValues()["@p1"]);
        }

        [Test]
        public void CanGenerateBetweenDateTime()
        {
            var query = new Query<TestClass>(new Translator());

            var dateFrom = new DateTime(2016, 01, 01);
            var dateTo = new DateTime(2016, 01, 02);

            query.Where(t => t.InsertDate.Between(dateFrom, dateTo));

            Assert.That(query.SqlWhere, Is.EqualTo("(TestClass.InsertDate BETWEEN @p0 AND @p1)"));
            Assert.AreEqual(dateFrom, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(dateTo, query.Parameters.GetValues()["@p1"]);
        }

        [Test]
        public void CanGenerateInInt()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Id.In(1, 2, 3));

            Assert.That(query.SqlWhere, Is.EqualTo("(TestClass.Id IN (@p0,@p1,@p2))"));
            Assert.AreEqual(1, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(2, query.Parameters.GetValues()["@p1"]);
            Assert.AreEqual(3, query.Parameters.GetValues()["@p2"]);
        }

        [Test]
        public void CanGenerateInString()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name.In("test", "test2"));

            Assert.That(query.SqlWhere, Is.EqualTo("(TestClass.Name IN (@p0,@p1))"));
            Assert.AreEqual("test", query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual("test2", query.Parameters.GetValues()["@p1"]);
        }

        [Test]
        public void CanGenerateInDateTime()
        {
            var query = new Query<TestClass>(new Translator());

            var dateOne = new DateTime(2016, 01, 01);
            var dateTwo = new DateTime(2016, 01, 02);

            query.Where(t => t.InsertDate.In(dateOne, dateTwo));

            Assert.That(query.SqlWhere, Is.EqualTo("(TestClass.InsertDate IN (@p0,@p1))"));
            Assert.AreEqual(dateOne, query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual(dateTwo, query.Parameters.GetValues()["@p1"]);
        }

        [Test]
        public void CanGenerateLike()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name.Like("%test%"));

            Assert.That(query.SqlWhere, Is.EqualTo("(TestClass.Name LIKE @p0)"));
            Assert.AreEqual("%test%", query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateTrimOnSelect()
        {
            var query = new Query<TestClass>(new Translator());

            query.Select(t => t.Name.TrimSql().As("Name"));

            Assert.That(query.SqlColumns, Is.EqualTo("LTRIM(RTRIM(TestClass.Name)) AS 'Name'"));
        }

        [Test]
        public void CanGenerateTrimOnWhere()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name.TrimSql() == "test");

            Assert.That(query.SqlWhere, Is.EqualTo("(LTRIM(RTRIM(TestClass.Name)) = @p0)"));
            Assert.AreEqual("test", query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateLengthOnSelect()
        {
            var query = new Query<TestClass>(new Translator());

            query.Select(t => t.Name.Length().As("Name Length"));

            Assert.That(query.SqlColumns, Is.EqualTo("LEN(TestClass.Name) AS 'Name Length'"));
        }

        [Test]
        public void CanGenerateLengthOnWhere()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name.Length() > 10);

            Assert.That(query.SqlWhere, Is.EqualTo("(LEN(TestClass.Name) > @p0)"));
            Assert.AreEqual(10, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateUpperOnSelect()
        {
            var query = new Query<TestClass>(new Translator());

            query.Select(t => t.Name.Upper().As("Name"));

            Assert.That(query.SqlColumns, Is.EqualTo("UPPER(TestClass.Name) AS 'Name'"));
        }

        [Test]
        public void CanGenerateUpperOnWhere()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name.Upper() == "TEST");

            Assert.That(query.SqlWhere, Is.EqualTo("(UPPER(TestClass.Name) = @p0)"));
            Assert.AreEqual("TEST", query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateLowerOnSelect()
        {
            var query = new Query<TestClass>(new Translator());

            query.Select(t => t.Name.Lower().As("Name"));

            Assert.That(query.SqlColumns, Is.EqualTo("LOWER(TestClass.Name) AS 'Name'"));
        }

        [Test]
        public void CanGenerateLowerOnWhere()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name.Lower() == "test");

            Assert.That(query.SqlWhere, Is.EqualTo("(LOWER(TestClass.Name) = @p0)"));
            Assert.AreEqual("test", query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateIsNullOnSelect()
        {
            var query = new Query<TestClass>(new Translator());

            query.Select(t => t.Name.IsNull("test").As("Name"));

            Assert.That(query.SqlColumns, Is.EqualTo("ISNULL(TestClass.Name,@p0) AS 'Name'"));
            Assert.AreEqual("test", query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateIsNullOnWhere()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name.IsNull("test1") == "test");

            Assert.That(query.SqlWhere, Is.EqualTo("(ISNULL(TestClass.Name,@p0) = @p1)"));
            Assert.AreEqual("test1", query.Parameters.GetValues()["@p0"]);
            Assert.AreEqual("test", query.Parameters.GetValues()["@p1"]);
        }

        [Test]
        public void CanGenerateCastOnSelect()
        {
            var query = new Query<TestClass>(new Translator());

            query.Select(t => t.Name.Cast<string, int>().As("Name"));

            Assert.That(query.SqlColumns, Is.EqualTo("CAST(TestClass.Name AS INT) AS 'Name'"));
        }

        [Test]
        public void CanGenerateCastOnWhere()
        {
            var query = new Query<TestClass>(new Translator());

            query.Where(t => t.Name.Cast<string, int>() == 10);

            Assert.That(query.SqlWhere, Is.EqualTo("(CAST(TestClass.Name AS INT) = @p0)"));
            Assert.AreEqual(10, query.Parameters.GetValues()["@p0"]);
        }

        [Test]
        public void CanGenerateAverageOnSelect()
        {
            var query = new Query<TestClass>(new Translator());

            query.Select(t => t.FloatNumber.Average().As("Name"));

            Assert.That(query.SqlColumns, Is.EqualTo("AVG(TestClass.FloatNumber) AS 'Name'"));
        }

        [Test]
        public void CanGenerateAverageOnHaving()
        {
            var query = new Query<TestClass>(new Translator());

            query.Having(t => t.FloatNumber.Average() > 10);

            Assert.That(query.SqlHaving, Is.EqualTo("(AVG(TestClass.FloatNumber) > @p0)"));
            Assert.AreEqual(10, query.Parameters.GetValues()["@p0"]);
        }
    }
}
