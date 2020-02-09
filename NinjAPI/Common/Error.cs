using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Common
{
    /// <summary>
    /// API Error model
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Error code
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// Error name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Error description
        /// </summary>
        public List<KeyValuePair<string, string>> Details { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Code"></param>
        /// <param name="Name"></param>
        /// <param name="Details"></param>
        public Error(int Code, string Name, List<KeyValuePair<string, string>> Details)
        {
            this.Code = Code;
            this.Name = Name;
            this.Details = Details;
        }
    }
}
