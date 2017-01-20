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

        [Column(isRequired: false)]
        public DateTime InsertDate { get; set; }
    }
}
