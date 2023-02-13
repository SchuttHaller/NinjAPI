using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public static class TokenExtensions
    {
        public static int IdentifierCount(this IEnumerable<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.Identifier);
        public static int ComparisionOperatorCount(this IEnumerable<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.ComparisionOperator);
        public static int ConstantCount(this IEnumerable<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.Constant);
        public static int LogicalOperatorCount(this IEnumerable<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.LogicalOperator);
        public static int DelimiterCount(this IEnumerable<QueryToken> tokens) => tokens.Count(x => x.Type != TokenType.Identifier && x.Type != TokenType.ComparisionOperator && x.Type != TokenType.Constant && x.Type != TokenType.LogicalOperator);

    }
}
