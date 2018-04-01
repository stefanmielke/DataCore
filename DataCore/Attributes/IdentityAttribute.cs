using System;

namespace DataCore.Attributes
{
    public class IdentityAttribute : Attribute
    {
        public int Start { get; }
        public int Increment { get; }

        public IdentityAttribute(int start = 1, int increment = 1)
        {
            Start = start;
            Increment = increment;
        }
    }
}
