using System;

namespace DataCore.Attributes
{
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; private set; }
        public bool IsPrimaryKey { get; private set; }

        public ColumnAttribute(string columnName = null, bool isPrimaryKey = false)
        {
            ColumnName = columnName;
            IsPrimaryKey = isPrimaryKey;
        }
    }
}
