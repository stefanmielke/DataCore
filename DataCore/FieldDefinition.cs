using System.Data;

namespace DataCore
{
    public class FieldDefinition
    {
        public string Name { get; set; }
        public DbType Type { get; set; }
        public int Size { get; set; }
        public bool Nullable { get; set; }
    }
}
