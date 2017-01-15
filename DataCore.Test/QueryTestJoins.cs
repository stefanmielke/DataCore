using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class QueryTestJoins
    {
        [Test]
        public void CanGenerateInnerJoin()
        {
            var data = new Query<TestClass>(new Translator());

            data.Join<TestClass2>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass WITH(NOLOCK) INNER JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id)", data.SqlFrom.ToString());
        }

        [Test]
        public void CanGenerateLeftJoin()
        {
            var data = new Query<TestClass>(new Translator());

            data.LeftJoin<TestClass2>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass WITH(NOLOCK) LEFT JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id)", data.SqlFrom.ToString());
        }

        [Test]
        public void CanGenerateRightJoin()
        {
            var data = new Query<TestClass>(new Translator());

            data.RightJoin<TestClass2>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass WITH(NOLOCK) RIGHT JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id)", data.SqlFrom.ToString());
        }

        [Test]
        public void CanGenerateInnerJoinAlso()
        {
            var data = new Query<TestClass>(new Translator());

            data.Join<TestClass2>((t, t2) => t.Id == t2.Id);
            data.Join<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass WITH(NOLOCK) INNER JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id) INNER JOIN TestClass3 WITH(NOLOCK) ON (TestClass2.Id = TestClass3.Id)", data.SqlFrom.ToString());
        }

        [Test]
        public void CanGenerateLeftJoinAlso()
        {
            var data = new Query<TestClass>(new Translator());

            data.LeftJoin<TestClass2>((t, t2) => t.Id == t2.Id);
            data.LeftJoin<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass WITH(NOLOCK) LEFT JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id) LEFT JOIN TestClass3 WITH(NOLOCK) ON (TestClass2.Id = TestClass3.Id)", data.SqlFrom.ToString());
        }

        [Test]
        public void CanGenerateRightJoinAlso()
        {
            var data = new Query<TestClass>(new Translator());

            data.RightJoin<TestClass2>((t, t2) => t.Id == t2.Id);
            data.RightJoin<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass WITH(NOLOCK) RIGHT JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id) RIGHT JOIN TestClass3 WITH(NOLOCK) ON (TestClass2.Id = TestClass3.Id)", data.SqlFrom.ToString());
        }
    }
}
