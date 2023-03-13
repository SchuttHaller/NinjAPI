using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public static class TokenExtensions
    {
        public static bool IsTerminal(this TokenType tokenType)
        {
            return tokenType >= TokenType.LeftParenthesis 
                && tokenType <= TokenType.EndOfLine;
        }

        public static bool IsOperator(this TokenType tokenType)
        {
            return tokenType == TokenType.ComparisionOperator 
                || tokenType == TokenType.SortingOperator 
                || tokenType == TokenType.ElementFunction
                || tokenType == TokenType.MathFunction 
                || tokenType == TokenType.QuantifierFunctionAll 
                || tokenType == TokenType.QuantifierFunctionAny;
        }

        public static bool IsTransitionNode(this TokenType tokenType)
        {
            return tokenType == TokenType.ExpressionPredicate
                || tokenType == TokenType.ClausePredicate
                || tokenType == TokenType.ParametersAggregate
                || tokenType == TokenType.PropertyNavigationAggregate
                || tokenType == TokenType.PropertyNavigationChain
                || tokenType == TokenType.NullableExpression;
        }

        public static bool IsFunctionOperator(this TokenType tokenType)
        {
            return tokenType >= TokenType.QuantifierFunctionAny
                && tokenType <= TokenType.MathFunction;
        }

        public static bool IsQuantifierFunctionOperator(this TokenType tokenType)
        {
            return tokenType == TokenType.QuantifierFunctionAny
                || tokenType == TokenType.QuantifierFunctionAll;
        }

        public static int IdentifierCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.Identifier);
        public static int OperatorCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type.IsOperator());
        public static int ConstantCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.String || x.Type == TokenType.Null || x.Type == TokenType.Number || x.Type == TokenType.Boolean);
        public static int NullCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.Null);
        public static int LogicalOperatorCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.LogicalOperator);
        public static int DelimiterCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.EndOfLine || x.Type == TokenType.LeftBracket || x.Type == TokenType.LeftParenthesis || x.Type == TokenType.RightBracket || x.Type == TokenType.RightParenthesis || x.Type == TokenType.Dollar || x.Type == TokenType.SingleQuote || x.Type == TokenType.Comma || x.Type == TokenType.Dot);
        public static bool Contains(this ICollection<QueryNode> nodes, TokenType type) => nodes?.Any( n => n.Type == type) ?? false;

    }
}
