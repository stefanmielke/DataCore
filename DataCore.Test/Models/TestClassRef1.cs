using DataCore.Attributes;

namespace DataCore.Test.Models
{
    public class TestClassRef1
    {
        [Column(isPrimaryKey: true), Identity]
        public int Id { get; set; }

        [Index]
        public int Id2 { get; set; }

        [Index("IX_TestTest")]
        public int Id3 { get; set; }
    }
}
