using DataCore.Attributes;

namespace DataCore.Test.Models
{
    public class TestIgnore
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int FloatNumber { get; set; }

        [Ignore]
        public string Ignored { get; set; }
    }
}
