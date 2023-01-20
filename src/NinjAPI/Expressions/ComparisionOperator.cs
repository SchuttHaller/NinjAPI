using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Expressions
{
    public static class ComparisionOperator
    {
        public static readonly string Equal = "EQ";
        public static readonly string NotEqual = "NE";
        public static readonly string GreaterThan = "GT";
        public static readonly string GreaterOrEqual = "GE";
        public static readonly string LessThan = "LT";
        public static readonly string LessOrEqual = "LE";
        public static readonly string Like = "LK";
        public static readonly string StartsWith = "SW";
        public static readonly string EndsWith = "EW";
    }
}
