namespace DataCore
{
    /// <summary>
    /// Class holding methods used in Expression (methods are not called)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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

        public static string TrimSql(this string property)
        {
            return property;
        }

        public static int Length(this string property)
        {
            return property.Length;
        }

        public static string Upper(this string property)
        {
            return property;
        }

        public static string Lower(this string property)
        {
            return property;
        }

        public static T IsNull<T>(this T property, T whenNullValue)
        {
            return property;
        }

        public static TTo Cast<TFrom, TTo>(this TFrom property)
        {
            return default(TTo);
        }

        public static int Average<T>(this T property)
        {
            return default(int);
        }
    }
}
