using System;
using DataCore.Attributes;

namespace DataCore.Test.Models
{
    public class TestClass4
    {
        [PrimaryKey]
        public int Id { get; set; }

        public float Number { get; set; }
        public string Name { get; set; }
        public DateTime InsertDate { get; set; }
    }
}
