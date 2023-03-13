using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public static partial class QueryParser
    {
        /// <summary>
        /// Grammar Transition Table:
        /// </summary>
        private static readonly Dictionary<TokenType, Dictionary<TokenType, TokenType[]>> GrammarTable = new()
        {
            {
                TokenType.Expression,
                new() {
                    { TokenType.LeftParenthesis, new TokenType[] { TokenType.Clause, TokenType.ExpressionPredicate } },
                    { TokenType.Identifier, new TokenType[] { TokenType.Clause, TokenType.ExpressionPredicate } },
                    { TokenType.Dollar, new TokenType[] { TokenType.Clause, TokenType.ExpressionPredicate } }
                }
            },
            {
                TokenType.ExpressionPredicate,
                new() {
                    { TokenType.EndOfLine, Array.Empty<TokenType>() },
                    { TokenType.RightParenthesis, Array.Empty<TokenType>() },
                    { TokenType.RightBracket, Array.Empty<TokenType>() },
                    { TokenType.LogicalOperator, new TokenType[] { TokenType.LogicalOperator, TokenType.Expression } }
                }
            },
            {
                TokenType.Clause,
                new() {
                    { TokenType.LeftParenthesis, new TokenType[] { TokenType.LeftParenthesis, TokenType.Expression, TokenType.RightParenthesis } },
                    { TokenType.Identifier,new TokenType[] { TokenType.Left, TokenType.ClausePredicate } },
                    { TokenType.Dollar,new TokenType[] { TokenType.Left, TokenType.ClausePredicate } }
                }
            },
            {
                TokenType.ClausePredicate,
                new() {
                    { TokenType.EndOfLine, Array.Empty<TokenType>() },
                    { TokenType.LogicalOperator, Array.Empty<TokenType>() },
                    { TokenType.RightParenthesis, Array.Empty<TokenType>() },
                    { TokenType.RightBracket, Array.Empty<TokenType>() },
                    { TokenType.ComparisionOperator,new TokenType[] { TokenType.ComparisionOperator, TokenType.Right } }
                }
            },
            {
                TokenType.Function,
                new() {
                    { TokenType.QuantifierFunctionAny, new TokenType[] { TokenType.QuantifierFunctionAny, TokenType.NullableExpression, TokenType.RightBracket } },
                    { TokenType.QuantifierFunctionAll, new TokenType[] { TokenType.QuantifierFunctionAll, TokenType.Expression,  TokenType.RightBracket } },
                    { TokenType.MathFunction, new TokenType[] { TokenType.MathFunction, TokenType.PropertyNavigation,  TokenType.RightBracket } },
                    { TokenType.ElementFunction, new TokenType[] { TokenType.ElementFunction, TokenType.NullableExpression, TokenType.RightBracket, TokenType.PropertyNavigationChain} }
                }
            },
            {
                TokenType.DataBaseFunction,
                new() {
                    { TokenType.Dollar, new TokenType[] { TokenType.Dollar, TokenType.Identifier, TokenType.LeftParenthesis, TokenType.Parameters, TokenType.RightParenthesis } }
                }
            },
            {
                TokenType.Parameters,
                new() {
                    { TokenType.Null, new TokenType[] { TokenType.Parameter, TokenType.ParametersAggregate } },
                    { TokenType.Number, new TokenType[] { TokenType.Parameter, TokenType.ParametersAggregate } },
                    { TokenType.Boolean, new TokenType[] { TokenType.Parameter, TokenType.ParametersAggregate } },
                    { TokenType.SingleQuote, new TokenType[] { TokenType.Parameter, TokenType.ParametersAggregate } },
                    { TokenType.Identifier, new TokenType[] { TokenType.Parameter, TokenType.ParametersAggregate } }
                }
            },
            {
                TokenType.ParametersAggregate,
                new() {
                    { TokenType.RightParenthesis,  Array.Empty<TokenType>() },
                    { TokenType.Comma, new TokenType[] { TokenType.Comma, TokenType.Parameter } }
                }
            },
            {
                TokenType.NullableExpression,
                new() {
                    { TokenType.LeftParenthesis, new TokenType[] { TokenType.Expression } },
                    { TokenType.RightBracket,  Array.Empty<TokenType>() },
                    { TokenType.Identifier, new TokenType[] { TokenType.Expression } },
                    { TokenType.Dollar, new TokenType[] { TokenType.Expression } }
                }
            },
            {
                TokenType.Right,
                new() {
                    { TokenType.Dollar, new TokenType[] { TokenType.DataBaseFunction } },
                    { TokenType.Null, new TokenType[] { TokenType.Value } },
                    { TokenType.Number, new TokenType[] { TokenType.Value } },
                    { TokenType.Boolean, new TokenType[] { TokenType.Value } },
                    { TokenType.SingleQuote, new TokenType[] { TokenType.Value } },
                }
            },
            {
                TokenType.Value,
                new() {
                    { TokenType.Null, new TokenType[] { TokenType.Null } },
                    { TokenType.Number, new TokenType[] { TokenType.Number } },
                    { TokenType.Boolean, new TokenType[] { TokenType.Boolean } },
                    { TokenType.SingleQuote, new TokenType[] { TokenType.SingleQuote, TokenType.String, TokenType.SingleQuote } },
                }
            },
            {
                TokenType.Left,
                new() {
                    { TokenType.Dollar, new TokenType[] { TokenType.DataBaseFunction } },
                    { TokenType.Identifier, new TokenType[] { TokenType.PropertyNavigation } }
                }
            },
            {
                TokenType.PropertyNavigation,
                new() {
                    { TokenType.Identifier, new TokenType[] { TokenType.Identifier, TokenType.PropertyNavigationAggregate } }
                }
            },
            {
                TokenType.PropertyNavigationAggregate,
                new() {
                    { TokenType.EndOfLine, Array.Empty<TokenType>() },
                    { TokenType.LogicalOperator, Array.Empty<TokenType>() },
                    { TokenType.RightParenthesis, Array.Empty<TokenType>() },
                    { TokenType.ComparisionOperator, Array.Empty<TokenType>() },
                    { TokenType.RightBracket, Array.Empty<TokenType>() },
                    { TokenType.LeftBracket, new TokenType[] { TokenType.LeftBracket, TokenType.Function } },         
                    { TokenType.Comma, Array.Empty<TokenType>() },
                    { TokenType.Dot, new TokenType[] { TokenType.PropertyNavigationChain } },
                }
            },
            {
                TokenType.PropertyNavigationChain,
                new() {
                    { TokenType.Dot, new TokenType[] { TokenType.Dot, TokenType.PropertyNavigation } },
                }
            },
            {
                TokenType.Parameter,
                new() {
                    { TokenType.Null, new TokenType[] { TokenType.Value } },
                    { TokenType.Number, new TokenType[] { TokenType.Value } },
                    { TokenType.Boolean, new TokenType[] { TokenType.Value } },
                    { TokenType.SingleQuote, new TokenType[] { TokenType.Value } },
                    { TokenType.Identifier, new TokenType[] { TokenType.PropertyNavigation } },
                }
            },
        };
    }
}
