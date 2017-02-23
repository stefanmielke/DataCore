using DataCore.Attributes;

namespace DataCore.Test.Models
{
    public class TestClassNoReference
    {
        [Column(isPrimaryKey:true)]
        public int Id { get; set; }

        public TestClass Ref { get; set; }
    }
}
