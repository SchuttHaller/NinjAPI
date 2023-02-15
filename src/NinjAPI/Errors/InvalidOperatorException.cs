using NinjAPI.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Errors
{
    internal class InvalidOperatorException: Exception
    {
        public InvalidOperatorException(string op, string nav, Type type)
            :base($"Operator '{op}' used with '{nav}' property clause, cannot be used with '{type}'"){}
    }
}
