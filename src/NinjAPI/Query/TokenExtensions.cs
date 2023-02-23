using System;
using System.Collections.Generic;
using System.Linq;
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

        public static bool IsPredicateOrAggregate(this TokenType tokenType)
        {
            return tokenType == TokenType.ExpressionPredicate
                || tokenType == TokenType.ClausePredicate
                || tokenType == TokenType.ParametersAggregate
                || tokenType == TokenType.PropertyNavigationAggregate;
        }

        public static int IdentifierCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.Identifier);
        public static int OperatorCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type.IsOperator());
        public static int ConstantCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.String || x.Type == TokenType.Null || x.Type == TokenType.Number || x.Type == TokenType.Boolean);
        public static int NullCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.Null);
        public static int LogicalOperatorCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.LogicalOperator);
        public static int DelimiterCount(this List<QueryToken> tokens) => tokens.Count(x => x.Type == TokenType.EndOfLine || x.Type == TokenType.LeftBracket || x.Type == TokenType.LeftParenthesis || x.Type == TokenType.RigthBracket || x.Type == TokenType.RigthParenthesis || x.Type == TokenType.Dollar || x.Type == TokenType.SingleQuote || x.Type == TokenType.Comma);

    }
}
