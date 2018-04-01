using System;

namespace DataCore.Attributes
{
    public class IndexAttribute : Attribute
    {
        public string Name { get; }
        public bool Unique { get; }

        public IndexAttribute(string name = null, bool unique = false)
        {
            Name = name;
            Unique = unique;
        }
    }
}
