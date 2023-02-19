using Microsoft.VisualBasic;
using NinjAPI.Expressions;
using NinjAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public static class QueryParser
    {               
        public static QueryNode Parse(this List<QueryToken> tokens)
        {
            tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));

            // init the transition stack
            Stack<TokenType> transitionStack = new();
            transitionStack.PushRange(TokenType.EndOfLine, TokenType.Expression);

            // init the tree stack
            Stack<QueryNode> treeStack = new();
            QueryNode root = new (TokenType.Expression);
            treeStack.Push(root);


            int i = 0;
            TokenType top; // transition token
            QueryToken next;
            QueryNode node; // start the node with the root

            do
            {
                top = transitionStack.Pop();
                next = tokens[i];

                if (top.IsTerminal())
                {
                    // if top of the stack is a terminar and the next token doesn't match it
                    // it's an error
                    if(top != next.Type)
                        throw new Exception($"Unexpected token {next.Value}");

                    // is the token matches, then consume the input
                    i++;
                    var terminalNode = treeStack.Pop();
                    terminalNode.Token = next;
                    continue;
                }

                // if transition table doesn't contain a valid transition for the non-terminal token
                // it's a syntanx error
                if (!_transitionTable[top].ContainsKey(next.Type))
                {
                    throw new Exception($"Syntax error: Unexpected token '{next.Value}'");
                }

                // get next production for the non-terminal
                var production = _transitionTable[top][next.Type];
                
                if (production.Any())
                {
                    // expand the transition stack
                    transitionStack.PushRange(production.Reverse());

                    node = treeStack.Pop();
                    // create nodes based on production
                    var newNodes = production
                        .Select(t => new QueryNode(t, node))
                        .ToArray();
                    // expand the tree stack
                    treeStack.PushRange(newNodes.Reverse());
                }               

            }
            while (top != TokenType.EndOfLine);

            return root;
        }


        /// <summary>
        /// Grammar Transition Table:
        /// </summary>
        private static readonly Dictionary<TokenType, Dictionary<TokenType, TokenType[]>> _transitionTable = new()
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
                    { TokenType.RigthParenthesis, Array.Empty<TokenType>() },
                    { TokenType.LogicalOperator, new TokenType[] { TokenType.LogicalOperator, TokenType.Expression } }
                }
            },
            {
                TokenType.Clause,
                new() {
                    { TokenType.LeftParenthesis, new TokenType[] { TokenType.LeftParenthesis, TokenType.Expression, TokenType.RigthParenthesis } },
                    { TokenType.Identifier,new TokenType[] { TokenType.Left, TokenType.ClausePredicate } },
                    { TokenType.Dollar,new TokenType[] { TokenType.Left, TokenType.ClausePredicate } }
                }
            },
            {
                TokenType.ClausePredicate,
                new() {
                    { TokenType.LeftBracket, new TokenType[] { TokenType.LeftBracket, TokenType.Function } },
                    { TokenType.ComparisionOperator,new TokenType[] { TokenType.ComparisionOperator, TokenType.Rigth } }
                }
            },
            {
                TokenType.Function,
                new() {
                    { TokenType.QuantifierFunctionAny, new TokenType[] { TokenType.QuantifierFunctionAny, TokenType.LeftParenthesis, TokenType.NullableExpression, TokenType.RigthParenthesis, TokenType.RigthBracket } },
                    { TokenType.QuantifierFunctionAll, new TokenType[] { TokenType.QuantifierFunctionAll, TokenType.LeftParenthesis, TokenType.Expression, TokenType.RigthParenthesis, TokenType.RigthBracket } },
                    { TokenType.MathFunction, new TokenType[] { TokenType.MathFunction, TokenType.LeftParenthesis, TokenType.NullableIdentifier, TokenType.RigthParenthesis, TokenType.RigthBracket, TokenType.ComparisionOperator, TokenType.Rigth } },
                    { TokenType.ElementFunction, new TokenType[] { TokenType.ElementFunction, TokenType.LeftParenthesis, TokenType.NullableExpression, TokenType.RigthParenthesis, TokenType.RigthBracket, TokenType.ComparisionOperator, TokenType.Rigth } }
                }
            },
            {
                TokenType.DataBaseFunction,
                new() {
                    { TokenType.Dollar, new TokenType[] { TokenType.Dollar, TokenType.Identifier, TokenType.LeftParenthesis, TokenType.Parameters, TokenType.RigthParenthesis } }
                }
            },
            {
                TokenType.Parameters,
                new() {
                    { TokenType.NullValue, new TokenType[] { TokenType.Parameter, TokenType.ParametersAggregate } },
                    { TokenType.Constant, new TokenType[] { TokenType.Parameter, TokenType.ParametersAggregate } },
                    { TokenType.SingleQuote, new TokenType[] { TokenType.Parameter, TokenType.ParametersAggregate } },
                    { TokenType.Identifier, new TokenType[] { TokenType.Parameter, TokenType.ParametersAggregate } }
                }
            },
            {
                TokenType.ParametersAggregate,
                new() {
                    { TokenType.RigthParenthesis,  Array.Empty<TokenType>() },
                    { TokenType.Comma, new TokenType[] { TokenType.Comma, TokenType.Parameter } }
                }
            },
            {
                TokenType.NullableExpression,
                new() {
                    { TokenType.Identifier, new TokenType[] { TokenType.Expression } },
                    { TokenType.Dollar, new TokenType[] { TokenType.Expression } },
                    { TokenType.LeftParenthesis, new TokenType[] { TokenType.Expression } },
                    { TokenType.RigthParenthesis,  Array.Empty<TokenType>() }
                }
            },
            {
                TokenType.NullableIdentifier,
                new() {
                    { TokenType.Identifier, new TokenType[] { TokenType.Left } },
                    { TokenType.Dollar, new TokenType[] { TokenType.Expression } },
                    { TokenType.RigthParenthesis,  Array.Empty<TokenType>() }
                }
            },
            {
                TokenType.Rigth,
                new() {
                    { TokenType.Dollar, new TokenType[] { TokenType.DataBaseFunction } },
                    { TokenType.NullValue, new TokenType[] { TokenType.Value } },
                    { TokenType.Constant, new TokenType[] { TokenType.Value } },
                    { TokenType.SingleQuote, new TokenType[] { TokenType.Value } },
                }
            },
            {
                TokenType.Value,
                new() {
                    { TokenType.NullValue, new TokenType[] { TokenType.NullValue } },
                    { TokenType.Constant, new TokenType[] { TokenType.Constant } },
                    { TokenType.SingleQuote, new TokenType[] { TokenType.SingleQuote, TokenType.Constant, TokenType.SingleQuote } },
                }
            },
            {
                TokenType.Left,
                new() {
                    { TokenType.Dollar, new TokenType[] { TokenType.DataBaseFunction } },
                    { TokenType.Identifier, new TokenType[] { TokenType.Identifier } }
                }
            },
            {
                TokenType.Parameter,
                new() {
                    { TokenType.NullValue, new TokenType[] { TokenType.Value } },
                    { TokenType.Constant, new TokenType[] { TokenType.Value } },
                    { TokenType.SingleQuote, new TokenType[] { TokenType.Value } },
                    { TokenType.Identifier, new TokenType[] { TokenType.Identifier } },
                }
            },
        };
    }
}
