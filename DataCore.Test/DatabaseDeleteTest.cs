using System;
using System.Data.SQLite;
using DataCore.Database.Sqlite;
using DataCore.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class DatabaseDeleteTest
    {
        [TestMethod]
        public void CanDeleteOne()
        {
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

                database.Delete<TestClass>(t => t.Id == 1);

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.IsNull(result);

                connection.Close();
            }
        }

        [TestMethod]
        public void CanDeleteMany()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();

                var testClass = new TestClass
                {
                    Id = 1,
                    Name = "test2",
                    Number = 1,
                    Done = true,
                    InsertDate = DateTime.Now,
                    TestClass2Id = 1
                };
                database.Insert(testClass);

                testClass.Name = "test";
                database.Insert(testClass);

                database.Delete<TestClass>(t => t.Name == "test" || t.Id == 1);

                var query = database.From<TestClass>().Where(t => t.Name == "test" || t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.IsNull(result);

                connection.Close();
            }
        }
    }
}
