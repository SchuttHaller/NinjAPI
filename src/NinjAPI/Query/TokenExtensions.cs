using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public static class TokenExtensions
    {
        public static int IdentifierCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.Identifier);
        public static int OperatorCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.ComparisionOperator || x.Type == TokenType.SortingOperator || x.Type == TokenType.ElementFunction || x.Type == TokenType.MathFunction || x.Type == TokenType.QuantifierFunctionAll || x.Type == TokenType.QuantifierFunctionAny);
        public static int ConstantCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.Constant);
        public static int LogicalOperatorCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.LogicalOperator);
        public static int DelimiterCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.EndOfLine || x.Type == TokenType.LeftBracket || x.Type == TokenType.LeftParenthesis || x.Type == TokenType.RigthBracket || x.Type == TokenType.RigthParenthesis);

    }
}
