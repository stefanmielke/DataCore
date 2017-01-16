namespace DataCore
{
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
    }
}
