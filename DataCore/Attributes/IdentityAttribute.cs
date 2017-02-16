using System;

namespace DataCore.Attributes
{
    public class IdentityAttribute : Attribute
    {
        public int Start { get; set; }
        public int Increment { get; set; }

        public IdentityAttribute(int start = 1, int increment = 1)
        {
            Start = start;
            Increment = increment;
        }
    }
}
