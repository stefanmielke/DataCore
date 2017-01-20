using DataCore.Attributes;

namespace DataCore.Test.Models
{
    public class TestClass2
    {
        [Column(isPrimaryKey: true)]
        public int Id { get; set; }
    }
}
