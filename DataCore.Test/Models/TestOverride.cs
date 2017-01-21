using DataCore.Attributes;

namespace DataCore.Test.Models
{
    [Table("TB_Testing")]
    public class TestOverride
    {
        [Column(isPrimaryKey: true, columnName: "ID_Testing")]
        public int Id { get; set; }
    }
}
