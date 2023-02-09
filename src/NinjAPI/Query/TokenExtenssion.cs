using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public static class TokenExtenssion
    {
        public static int IdentifierCount(this IEnumerable<Token> tokens) => tokens.Count(x => x.Code == TokenType.Identifier);
        public static int ComparisionOperatorCount(this IEnumerable<Token> tokens) => tokens.Count(x => x.Code == TokenType.ComparisionOperator);
        public static int ConstantCount(this IEnumerable<Token> tokens) => tokens.Count(x => x.Code == TokenType.Constant);
        public static int LogicalOperatorCount(this IEnumerable<Token> tokens) => tokens.Count(x => x.Code == TokenType.LogicalOperator);
        public static int DelimiterCount(this IEnumerable<Token> tokens) => tokens.Count(x => x.Code != TokenType.Identifier && x.Code != TokenType.ComparisionOperator && x.Code != TokenType.Constant && x.Code != TokenType.LogicalOperator);

    }
}
