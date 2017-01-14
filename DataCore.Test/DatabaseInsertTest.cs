using System;
using System.Data.SQLite;
using System.Linq;
using DataCore.Database.Sqlite;
using DataCore.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class DatabaseInsertTest
    {
        [TestMethod]
        public void CanInsert()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();

                database.Insert(new TestClass
                {
                    Id = 1,
                    Name = "test",
                    Number = 1,
                    Done = true,
                    InsertDate = DateTime.Now,
                    TestClass2Id = 1
                });

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                var results = database.Select(query);
                Assert.IsTrue(results.Any());

                connection.Close();
            }
        }
    }
}
