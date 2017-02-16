using System;

namespace DataCore.Attributes
{
    public class IndexAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Unique { get; set; }

        public IndexAttribute(string name = null, bool unique = false)
        {
            Name = name;
            Unique = unique;
        }
    }
}
