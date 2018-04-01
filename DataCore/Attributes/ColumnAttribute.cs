using System;

namespace DataCore.Attributes
{
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; }
        public bool IsPrimaryKey { get; }
        public bool IsRequired { get; }
        public int Length { get; }
        public int Precision { get; }

        public ColumnAttribute(string columnName = null, bool isPrimaryKey = false, bool isRequired = true, int length = 255, int precision = 3)
        {
            ColumnName = columnName;
            IsPrimaryKey = isPrimaryKey;
            IsRequired = isRequired;
            Length = length;
            Precision = precision;
        }
    }
}
