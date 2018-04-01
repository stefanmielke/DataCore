﻿using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class QueryTestGroupBy
    {
        [Test]
        public void CanGenerateGroupByWithDynamic()
        {
            var query = new Query<TestClass>(new Translator());

            query.GroupBy(t => new { t.Id, t.Name });

            Assert.AreEqual("TestClass.Id, TestClass.Name", query.SqlGroupBy);
        }

        [Test]
        public void CanGenerateGroupByWithOneField()
        {
            var query = new Query<TestClass>(new Translator());

            query.GroupBy(t => t.Id);

            Assert.AreEqual("TestClass.Id", query.SqlGroupBy);
        }

        [Test]
        public void CanAggregateGroupBy()
        {
            var query = new Query<TestClass>(new Translator());

            query.GroupBy(t => t.Id);
            query.GroupBy(t => new { Number = t.FloatNumber, t.Name });

            Assert.AreEqual("TestClass.Id, TestClass.FloatNumber, TestClass.Name", query.SqlGroupBy);
        }

        [Test]
        public void CanAggregateGroupByOther()
        {
            var query = new Query<TestClass>(new Translator());
            query.Join<TestClass2>((t, t2) => t.TestClass2Id == t2.Id);

            query.GroupBy(t => t.Id);
            query.GroupBy(t => new { Number = t.FloatNumber, t.Name });
            query.GroupBy<TestClass2>(t2 => t2.Id);

            Assert.AreEqual("TestClass.Id, TestClass.FloatNumber, TestClass.Name, TestClass2.Id", query.SqlGroupBy);
        }
    }
}
