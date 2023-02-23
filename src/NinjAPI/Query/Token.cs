using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public class Token
    {
        public TokenType Type { get; set; }

        public static Token New(TokenType tokenType)
        {
            return new Token() { Type = tokenType };
        }
    }
}
