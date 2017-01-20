using DataCore.Attributes;

namespace DataCore.Test.Models
{
    public class TestClass3
    {
        [Column(isPrimaryKey: true)]
        public int Id { get; set; }
    }
}
