using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public class OrderKey
    {
        public string Alias { get; set; }
        public string Property { get; set; }
        public string Direction
        {
            get
            {
                return dir == "desc" ? "Descending" : string.Empty;
            }
            set
            {
                dir = value;
            }

        }
        private string dir;
    }
}
