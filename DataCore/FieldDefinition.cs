using System;
using System.Data;

namespace DataCore
{
    public class FieldDefinition
    {
        public string Name { get; set; }
        public DbType Type { get; set; }
        public int Size { get; set; }
        public bool Nullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
        public int IdentityStart { get; set; }
        public int IdentityIncrement { get; set; }
        public bool IsReference { get; set; }
        public string ReferenceName { get; set; }
        public Type ReferenceTable { get; set; }
        public bool HasIndex { get; set; }
        public string IndexName { get; set; }
        public bool IndexUnique { get; set; }
    }
}
