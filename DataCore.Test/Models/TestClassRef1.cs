using DataCore.Attributes;

namespace DataCore.Test.Models
{
    public class TestClassRef1
    {
        [Column(isPrimaryKey: true), Identity]
        public int Id { get; set; }
    }
}
