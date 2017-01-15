using System;

namespace DataCore
{
    public static class SqlExtensions
    {
        public static dynamic Min<T>(this T property)
        {
            return property;
        }

        public static dynamic Max<T>(this T property)
        {
            return property;
        }
    }
}
