using System;

namespace DataCore.Attributes
{
    public class ReferenceAttribute : Attribute
    {
        public Type Table { get; private set; }
        public string FkName { get; private set; }

        public ReferenceAttribute(Type table, string fkName = null)
        {
            Table = table;
            FkName = fkName;
        }
    }
}
