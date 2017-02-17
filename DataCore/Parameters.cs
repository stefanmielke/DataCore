using System.Collections.Generic;

namespace DataCore
{
    public class Parameters
    {
        private readonly Dictionary<string, object> _values;

        public Parameters()
        {
            _values = new Dictionary<string, object>();
        }

        public string Add(ITranslator translator, object value)
        {
            var paramTag = translator.GetParameterTag();

            var key = paramTag + "p" + _values.Count;
            _values.Add(key, value);

            return key;
        }

        public IDictionary<string, object> GetValues()
        {
            return _values;
        }
    }
}
