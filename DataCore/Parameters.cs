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

        public string Add(object value)
        {
            var key = "@p" + _values.Count;
            _values.Add(key, value);

            return key;
        }

        public void Add(Parameters parameters)
        {
            foreach (var parameter in parameters.GetValues())
            {
                _values.Add(parameter.Key, parameter.Value);
            }
        }

        public IDictionary<string, object> GetValues()
        {
            return _values;
        }
    }
}
