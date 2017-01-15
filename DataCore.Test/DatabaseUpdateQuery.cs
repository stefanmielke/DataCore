using System;
using System.Data.SQLite;
using DataCore.Database.Sqlite;
using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class DatabaseUpdateQuery
    {
        [Test]
        public void CanUpdate()
        {
            var updatedName = "test updated";

            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();

                var testClass = new TestClass
                {
                    Id = 1,
                    Name = "test",
                    Number = 1,
                    Done = true,
                    InsertDate = DateTime.Now,
                    TestClass2Id = 1
                };
                database.Insert(testClass);

                testClass.Name = updatedName;
                database.Update(testClass, t => t.Id == 1);

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.AreEqual(updatedName, result.Name);

                connection.Close();
            }
        }

        [Test]
        public void CanUpdateOnlyOneField()
        {
            var updatedName = "test updated";
            var updatedNumber = 2;

            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();

                var testClass = new TestClass
                {
                    Id = 1,
                    Name = "test",
                    Number = 1,
                    Done = true,
                    InsertDate = DateTime.Now,
                    TestClass2Id = 1
                };
                database.Insert(testClass);

                testClass.Name = updatedName;
                testClass.Number = updatedNumber;

                database.UpdateOnly(testClass, t => t.Name, t => t.Id == 1);

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.AreEqual(updatedName, result.Name);
                Assert.AreNotEqual(updatedNumber, result.Number);

                connection.Close();
            }
        }

        [Test]
        public void CanUpdateOnlyManyFields()
        {
            var updatedName = "test updated";
            var updatedNumber = 2;

            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();

                var testClass = new TestClass
                {
                    Id = 1,
                    Name = "test",
                    Number = 1,
                    Done = true,
                    InsertDate = DateTime.Now,
                    TestClass2Id = 1
                };
                database.Insert(testClass);

                testClass.Name = updatedName;
                testClass.Number = updatedNumber;

                database.UpdateOnly(testClass, t => new { t.Name }, t => t.Id == 1);

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.AreEqual(updatedName, result.Name);
                Assert.AreNotEqual(updatedNumber, result.Number);

                connection.Close();
            }
        }
    }
}
