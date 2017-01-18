namespace DataCore
{
    public class StringAsIsWithParameter
    {
        public StringAsIsWithParameter(string value, Parameters parameter)
        {
            Value = value;
            Parameter = parameter;
        }

        public string Value { get; private set; }
        public Parameters Parameter { get; private set; }
    }
}
