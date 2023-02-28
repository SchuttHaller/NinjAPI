using NinjAPI.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Errors
{
    internal class InvalidTokenException: Exception
    {
        public InvalidTokenException(Token token): base($"Invalid token type: { token.Type }")
        {}

        public InvalidTokenException(Token token, string message) : base($"Invalid token type: {token.Type} - {message}")
        {}
    }
}
