﻿using System.Data.SQLite;
using DataCore.Database.Sqlite;
using DataCore.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class DatabaseTest
    {
        [TestMethod]
        public void SelectTest()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.Execute("CREATE TABLE TestClass ( Id INT not null, Number INT not null, Name VARCHAR(250) not null, InsertDate DATETIME not null );");

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                var results = database.Select(query);

                connection.Close();
            }
        }
    }
}
