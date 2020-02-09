using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public class FilterKey
    {
        public string Alias { get; set; }
        public string LogicalOperator { get; set; }
        public string Property { get; set; }
        public string Value { get; set; }
    }
}
