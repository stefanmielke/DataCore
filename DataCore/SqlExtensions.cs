namespace DataCore
{
    /// <summary>
    /// Class holding methods used in Expression (methods are not called)
    /// </summary>
    public static class SqlExtensions
    {
        public static T Min<T>(this T property)
        {
            return property;
        }

        public static T Max<T>(this T property)
        {
            return property;
        }

        public static T Sum<T>(this T property)
        {
            return property;
        }

        public static T As<T>(this T property, string alias)
        {
            return property;
        }

        public static bool Between<T>(this T property, T start, T end)
        {
            return true;
        }

        public static bool In<T>(this T property, params T[] pars)
        {
            return true;
        }

        public static bool Like(this string property, string likeString)
        {
            return true;
        }
    }
}
