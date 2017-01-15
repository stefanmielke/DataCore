using System.Data.SQLite;
using DataCore.Database.Sqlite;
using DataCore.Test.Models;
using System;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class DatabaseTest
    {
        [Test]
        public void CanSelect()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.Execute("CREATE TABLE TestClass ( Id INT not null, Number INT not null, Name VARCHAR(250) not null, InsertDate DATETIME not null );");

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                database.Select(query);

                connection.Close();
            }
        }

        [Test]
        public void CanSelectSingle()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.Execute("CREATE TABLE TestClass ( Id INT not null, Number INT not null);INSERT INTO TestClass (Id, Number) VALUES (1, 1)");

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                Assert.IsNotNull(database.SelectSingle(query));

                connection.Close();
            }
        }

        [Test]
        public void ExistsReturnTrueWhenExists()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.Execute("CREATE TABLE TestClass ( Id INT not null, Number INT not null);INSERT INTO TestClass (Id, Number) VALUES (1, 1)");

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                Assert.IsTrue(database.Exists(query));

                connection.Close();
            }
        }

        [Test]
        public void ExistsReturnFalseWhenNotExists()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.Execute("CREATE TABLE TestClass (Id INT not null, Number INT not null);");

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                Assert.IsFalse(database.Exists(query));

                connection.Close();
            }
        }

        [Test]
        public void CanCreateTable()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                database.Select(query);

                connection.Close();
            }
        }

        [Test]
        public void DoNotErrorOnDoubleCreateTable()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();
                database.CreateTableIfNotExists<TestClass>();

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                database.Select(query);

                connection.Close();
            }
        }

        [Test]
        public void CanDropTable()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();

                database.Select(database.From<TestClass>().Where(t => t.Id == 1));

                database.DropTableIfExists<TestClass>();

                connection.Close();
            }
        }

        [Test]
        public void CanCreateColumn()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.Execute("CREATE TABLE TestClass ( Id INT not null, Number INT not null, Name VARCHAR(250) not null );");

                database.CreateColumnIfNotExists<TestClass>(t => t.InsertDate);

                var date = DateTime.Now;

                var query = database.From<TestClass>().Where(t => t.InsertDate == date);

                database.Select(query);

                connection.Close();
            }
        }

        [Test]
        public void CanDropColumn()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();

                database.DropColumnIfExists<TestClass>(t => t.Id);

                connection.Close();
            }
        }

        [Test]
        public void CanCreateIndex()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();

                database.CreateIndexIfNotExists<TestClass>(t => new { t.Id, t.Name }, true);

                connection.Close();
            }
        }

        [Test]
        public void CanDropIndex()
        {
            const string indexName = "IX_TestClass";

            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();

                database.CreateIndexIfNotExists<TestClass>(t => new { t.Id, t.Name }, true, indexName);
                database.DropIndexIfExists<TestClass>(indexName);

                connection.Close();
            }
        }

        [Test]
        public void CanCreateForeignKey()
        {
            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();
                database.CreateTableIfNotExists<TestClass2>();

                database.CreateForeignKeyIfNotExists<TestClass, TestClass2>(t => t.TestClass2Id, t => t.Id);

                connection.Close();
            }
        }

        [Test]
        public void CanDropForeignKey()
        {
            const string indexName = "FK_TestClass";

            using (var connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var database = new SqliteDatabase(connection);

                database.CreateTableIfNotExists<TestClass>();

                database.CreateForeignKeyIfNotExists<TestClass, TestClass2>(t => t.TestClass2Id, t => t.Id, indexName);
                database.DropForeignKeyIfExists<TestClass>(indexName);

                connection.Close();
            }
        }
    }
}
