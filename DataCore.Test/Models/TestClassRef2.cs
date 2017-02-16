using DataCore.Attributes;

namespace DataCore.Test.Models
{
    public class TestClassRef2
    {
        [Column(isPrimaryKey: true), Identity]
        public int Id { get; set; }

        [Reference(typeof(TestClassRef1), "FK_TestClassRef2")]
        public int Ref1Id { get; set; }
    }
}
