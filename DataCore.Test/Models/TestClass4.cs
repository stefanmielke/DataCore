using System;
using DataCore.Attributes;

namespace DataCore.Test.Models
{
    public class TestClass4
    {
        [Column(isPrimaryKey: true)]
        public int Id { get; set; }

        public float Number { get; set; }
        public string Name { get; set; }
        public DateTime InsertDate { get; set; }
    }
}
