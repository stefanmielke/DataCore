using System;

namespace DataCore.Attributes
{
    public class ReferenceAttribute : Attribute
    {
        public Type Table { get; }
        public string FkName { get; }

        public ReferenceAttribute(Type table, string fkName = null)
        {
            Table = table;
            FkName = fkName;
        }
    }
}
