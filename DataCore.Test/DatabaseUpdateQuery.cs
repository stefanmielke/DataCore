﻿using System;
using DataCore.Test.Models;
using NUnit.Framework;

namespace DataCore.Test
{
    [TestFixture]
    public class DatabaseUpdateQuery
    {
        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanUpdate(TestHelper.DatabaseType dbType, string connectionString)
        {
            const string updatedName = "test updated";

            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();

                var testClass = new TestClass
                {
                    Id = 1,
                    Name = "test",
                    FloatNumber = 1,
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
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanUpdateOnlyOneField(TestHelper.DatabaseType dbType, string connectionString)
        {
            const string updatedName = "test updated";
            const int updatedNumber = 2;

            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();

                var testClass = new TestClass
                {
                    Id = 1,
                    Name = "test",
                    FloatNumber = 1,
                    Done = true,
                    InsertDate = DateTime.Now,
                    TestClass2Id = 1
                };
                database.Insert(testClass);

                testClass.Name = updatedName;
                testClass.FloatNumber = updatedNumber;

                database.UpdateOnly(testClass, t => t.Name, t => t.Id == 1);

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.AreEqual(updatedName, result.Name);
                Assert.AreNotEqual(updatedNumber, result.FloatNumber);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanUpdateOnlyManyFields(TestHelper.DatabaseType dbType, string connectionString)
        {
            const string updatedName = "test updated";
            const int updatedNumber = 2;

            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestClass>();

                var testClass = new TestClass
                {
                    Id = 1,
                    Name = "test",
                    FloatNumber = 1,
                    Done = true,
                    InsertDate = DateTime.Now,
                    TestClass2Id = 1
                };
                database.Insert(testClass);

                testClass.Name = updatedName;
                testClass.FloatNumber = updatedNumber;

                database.UpdateOnly(testClass, t => new { t.Name }, t => t.Id == 1);

                var query = database.From<TestClass>().Where(t => t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.AreEqual(updatedName, result.Name);
                Assert.AreNotEqual(updatedNumber, result.FloatNumber);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanUpdateWithoutIgnoredField(TestHelper.DatabaseType dbType, string connectionString)
        {
            const string updatedName = "test updated";

            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestIgnore>();

                var testClass = new TestIgnore
                {
                    Id = 1,
                    Name = "test",
                    Ignored = "ignored"
                };
                database.Insert(testClass);

                testClass.Name = updatedName;
                database.Update(testClass, t => t.Id == 1);

                var query = database.From<TestIgnore>().Where(t => t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.AreEqual(updatedName, result.Name);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanUpdateOnlyOneFieldWithoutIgnoredField(TestHelper.DatabaseType dbType, string connectionString)
        {
            const string updatedName = "test updated";
            const int updatedNumber = 2;

            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestIgnore>();

                var testClass = new TestIgnore
                {
                    Id = 1,
                    Name = "test",
                    Ignored = "ignored"
                };
                database.Insert(testClass);

                testClass.Name = updatedName;
                testClass.FloatNumber = updatedNumber;

                database.UpdateOnly(testClass, t => t.Name, t => t.Id == 1);

                var query = database.From<TestIgnore>().Where(t => t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.AreEqual(updatedName, result.Name);
                Assert.AreNotEqual(updatedNumber, result.FloatNumber);
            }
        }

        [Test, TestCaseSource(typeof(SqlTestDataFactory), nameof(SqlTestDataFactory.TestCases))]
        public void CanUpdateOnlyManyFieldsWithoutIgnoredField(TestHelper.DatabaseType dbType, string connectionString)
        {
            const string updatedName = "test updated";
            const int updatedNumber = 2;

            using (var database = TestHelper.GetDatabaseFor(dbType, connectionString))
            {
                database.CreateTableIfNotExists<TestIgnore>();

                var testClass = new TestIgnore
                {
                    Id = 1,
                    Name = "test",
                    Ignored = "ignored"
                };
                database.Insert(testClass);

                testClass.Name = updatedName;
                testClass.FloatNumber = updatedNumber;

                database.UpdateOnly(testClass, t => new { t.Name }, t => t.Id == 1);

                var query = database.From<TestIgnore>().Where(t => t.Id == 1);

                var result = database.SelectSingle(query);
                Assert.AreEqual(updatedName, result.Name);
                Assert.AreNotEqual(updatedNumber, result.FloatNumber);
            }
        }
    }
}
