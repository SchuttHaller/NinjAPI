using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public class Token
    {
        public byte Type { get; set; }
        public bool IsTerminal => TokenType.IsTerminal(Type);
        public bool IsOperator => TokenType.IsOperator(Type);

        public static Token New(byte tokenType)
        {
            return new Token() { Type = tokenType };
        }
    }
}
