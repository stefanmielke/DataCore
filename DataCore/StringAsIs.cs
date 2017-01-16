namespace DataCore
{
    public struct StringAsIs
    {
        public StringAsIs(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
