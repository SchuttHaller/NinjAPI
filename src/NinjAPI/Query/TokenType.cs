using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public static class TokenType
    {
        /// Non-Terminals
        public const byte Start = 0;
        public const byte Expression = 1;
        public const byte ExpressionPredicate = 2;
        public const byte Clause = 3;
        public const byte ClausePredicate = 4;


        // Delimiters
        public const byte LeftParenthesis = 5;
        public const byte RigthParenthesis = 6;
        public const byte LeftBracket = 7;
        public const byte RigthBracket = 8;

        // Terminals
        public const byte LogicalOperator = 9;
        public const byte Identifier = 10;
        public const byte ScalarFunction = 11;
        public const byte ComparisionOperator = 12;
        public const byte Constant = 13;
        public const byte EndOfLine = 14;

        public static bool IsTerminal(byte tokenType)
        {
            return tokenType >= LeftParenthesis && tokenType <= EndOfLine;
        }
    }
}
