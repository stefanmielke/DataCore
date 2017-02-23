using System.Collections.Generic;

namespace DataCore
{
    public class TableDefinition
    {
        public string Name { get; set; }

        public FieldDefinition IdField { get; set; }

        public List<FieldDefinition> Fields { get; set; }
    }
}
