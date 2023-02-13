using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public static class Operators
    {
        // comparison operators
        public const string Equal = "eq";
        public const string NotEqual = "ne";
        public const string GreaterThan = "gt";
        public const string GreaterOrEqual = "ge";
        public const string LessThan = "lt";
        public const string LessOrEqual = "le";
        public const string Like = "lk";
        public const string StartsWith = "sw";
        public const string EndsWith = "ew";

        // quantifier operators
        public const string Any = "any";
        public const string All = "all";

        // logical operators
        public const string And = "and";
        public const string Or = "or";

        // math operators
        public const string Min = "min";
        public const string Max = "max";
        public const string Sum = "sum";

        // element operators
        public const string First = "first";
        public const string Last = "last";


        // sorting operators
        public const string Asc = "asc";
        public const string Desc = "desc";
    }
}
