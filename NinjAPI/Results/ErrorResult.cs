using NinjAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Results
{ 
    /// <summary>
    /// Error payload
    /// </summary>
    public class ErrorResult
    {
        public List<Error> Errors { get; set; }

        public ErrorResult() { }

        public ErrorResult(List<Error> errors)
        {
            this.Errors = errors;
        }
    }
}
