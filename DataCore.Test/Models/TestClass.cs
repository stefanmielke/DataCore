using System;
using DataCore.Attributes;

namespace DataCore.Test.Models
{
    public class TestClass
    {
        [PrimaryKey]
        public int Id { get; set; }

        public float Number { get; set; }
        public string Name { get; set; }
        public bool Done { get; set; }
        public DateTime InsertDate { get; set; }
        public int TestClass2Id { get; set; }
    }
}
