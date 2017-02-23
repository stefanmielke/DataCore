using DataCore.Attributes;

namespace DataCore.Test.Models
{
    public class TestNullableProperty
    {
        [Column(isPrimaryKey: true)]
        public int Id { get; set; }

        public int? IdMaybe { get; set; }
    }
}
