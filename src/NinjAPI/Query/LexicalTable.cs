using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public class LexicalTable
    {
        public List<Token> Tokens { get; set; } = new();
        public int IdentifierCount { get; set; }
        public int ComparisionOperatorCount { get; set; }
        public int ConstantCount { get; set; }
        public int LogicalOperatorCount { get; set; }
        public int DelimiterCount { get; set; }

        public void AddToken(string value, string code) => Tokens.Add(new () { Code = code, Value = value});

    }
}
