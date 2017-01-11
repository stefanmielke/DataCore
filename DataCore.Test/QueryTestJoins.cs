using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class QueryTestJoins
    {
        [TestMethod]
        public void CanGenerateInnerJoin()
        {
            var data = new Query<TestClass>(new Translator());

            data.Join<TestClass2>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass INNER JOIN TestClass2 ON (TestClass.Id = TestClass2.Id)", data.SqlFrom.ToString());
        }

        [TestMethod]
        public void CanGenerateLeftJoin()
        {
            var data = new Query<TestClass>(new Translator());

            data.LeftJoin<TestClass2>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass LEFT JOIN TestClass2 ON (TestClass.Id = TestClass2.Id)", data.SqlFrom.ToString());
        }

        [TestMethod]
        public void CanGenerateRightJoin()
        {
            var data = new Query<TestClass>(new Translator());

            data.RightJoin<TestClass2>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass RIGHT JOIN TestClass2 ON (TestClass.Id = TestClass2.Id)", data.SqlFrom.ToString());
        }

        [TestMethod]
        public void CanGenerateInnerJoinAlso()
        {
            var data = new Query<TestClass>(new Translator());

            data.Join<TestClass2>((t, t2) => t.Id == t2.Id);
            data.Join<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass INNER JOIN TestClass2 ON (TestClass.Id = TestClass2.Id) INNER JOIN TestClass3 ON (TestClass2.Id = TestClass3.Id)", data.SqlFrom.ToString());
        }

        [TestMethod]
        public void CanGenerateLeftJoinAlso()
        {
            var data = new Query<TestClass>(new Translator());

            data.LeftJoin<TestClass2>((t, t2) => t.Id == t2.Id);
            data.LeftJoin<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass LEFT JOIN TestClass2 ON (TestClass.Id = TestClass2.Id) LEFT JOIN TestClass3 ON (TestClass2.Id = TestClass3.Id)", data.SqlFrom.ToString());
        }

        [TestMethod]
        public void CanGenerateRightJoinAlso()
        {
            var data = new Query<TestClass>(new Translator());

            data.RightJoin<TestClass2>((t, t2) => t.Id == t2.Id);
            data.RightJoin<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id);

            Assert.AreEqual("TestClass RIGHT JOIN TestClass2 ON (TestClass.Id = TestClass2.Id) RIGHT JOIN TestClass3 ON (TestClass2.Id = TestClass3.Id)", data.SqlFrom.ToString());
        }
    }
}
