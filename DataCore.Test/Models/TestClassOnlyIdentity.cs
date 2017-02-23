using DataCore.Attributes;

namespace DataCore.Test.Models
{
    public class TestClassOnlyIdentity
    {
        [Column(isPrimaryKey: true), Identity]
        public int Id { get; set; }
    }
}
