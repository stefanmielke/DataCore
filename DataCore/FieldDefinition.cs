namespace DataCore
{
    public class FieldDefinition
    {
        public string Name { get; set; }
        public FieldType Type { get; set; }
        public int Size { get; set; }
        public bool Nullable { get; set; }
    }

    public enum FieldType
    {
        Varchar,
        Int,
        Bool,
        Float,
        Decimal
    }
}
