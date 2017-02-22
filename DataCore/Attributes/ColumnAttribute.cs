using System;

namespace DataCore.Attributes
{
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; private set; }
        public bool IsPrimaryKey { get; private set; }
        public bool IsRequired { get; private set; }
        public int Length { get; private set; }
        public int Precision { get; private set; }

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
