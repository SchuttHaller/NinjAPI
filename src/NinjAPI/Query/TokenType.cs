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
        public const byte Expression = 0;
        public const byte ExpressionPredicate = 1;
        public const byte Clause = 2;
        public const byte ClausePredicate = 3;
        public const byte Function = 4;
        public const byte NullableExpression = 5;
        public const byte NullableIdentifier = 6;

        // Delimiters
        public const byte LeftParenthesis = 7;
        public const byte RigthParenthesis = 8;
        public const byte LeftBracket = 9;
        public const byte RigthBracket = 10;

        // Other Terminals
        public const byte LogicalOperator = 11;
        public const byte Identifier = 12;
        public const byte QuantifierFunctionSome = 13;
        public const byte QuantifierFunctionAll = 14;
        public const byte ElementFunction = 15;
        public const byte MathFunction = 16;
        public const byte ComparisionOperator = 17;
        public const byte Constant = 18;
        public const byte EndOfLine = 19;

        public static bool IsTerminal(byte tokenType)
        {
            return tokenType >= LeftParenthesis && tokenType <= EndOfLine;
        }

        public static bool IsOperator(byte tokenType)
        {
            return tokenType == LogicalOperator || tokenType == ComparisionOperator;
        }
    }
}
