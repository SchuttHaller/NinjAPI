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
            Stack<byte> transitionStack = new();
            transitionStack.PushRange(TokenType.EndOfLine, TokenType.Expression);

            // init the tree stack
            Stack<QueryNode> treeStack = new();
            QueryNode root = new (TokenType.Expression);
            treeStack.Push(root);


            int i = 0;
            byte top; // transition token
            QueryToken next;
            QueryNode node; // start the node with the root

            do
            {
                top = transitionStack.Pop();
                next = tokens[i];

                if (TokenType.IsTerminal(top))
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
        private static readonly Dictionary<byte, Dictionary<byte, byte[]>> _transitionTable = new()
        {
            {
                TokenType.Expression,
                new() {
                    { TokenType.LeftParenthesis, new byte[] { TokenType.Clause, TokenType.ExpressionPredicate } },
                    { TokenType.Identifier, new byte[] { TokenType.Clause, TokenType.ExpressionPredicate } }
                }
            },
            {
                TokenType.ExpressionPredicate,
                new() {
                    { TokenType.EndOfLine, Array.Empty<byte>() },
                    { TokenType.RigthParenthesis, Array.Empty<byte>() },
                    { TokenType.LogicalOperator, new byte[] { TokenType.LogicalOperator, TokenType.Expression } }
                }
            },
            {
                TokenType.Clause,
                new() {
                    { TokenType.LeftParenthesis, new byte[] { TokenType.LeftParenthesis, TokenType.Expression, TokenType.RigthParenthesis } },
                    { TokenType.Identifier,new byte[] { TokenType.Identifier, TokenType.ClausePredicate } }
                }
            },
            {
                TokenType.ClausePredicate,
                new() {
                    { TokenType.LeftBracket, new byte[] { TokenType.LeftBracket, TokenType.Function } },
                    { TokenType.ComparisionOperator,new byte[] { TokenType.ComparisionOperator, TokenType.Constant } }
                }
            },
            {
                TokenType.Function,
                new() {
                    { TokenType.QuantifierFunctionSome, new byte[] { TokenType.QuantifierFunctionSome, TokenType.LeftParenthesis, TokenType.NullableExpression, TokenType.RigthParenthesis, TokenType.RigthBracket } },
                    { TokenType.QuantifierFunctionAll, new byte[] { TokenType.QuantifierFunctionAll, TokenType.LeftParenthesis, TokenType.Expression, TokenType.RigthParenthesis, TokenType.RigthBracket } },
                    { TokenType.MathFunction, new byte[] { TokenType.MathFunction, TokenType.LeftParenthesis, TokenType.NullableIdentifier, TokenType.RigthParenthesis, TokenType.RigthBracket, TokenType.ComparisionOperator, TokenType.Constant } },
                    { TokenType.ElementFunction, new byte[] { TokenType.ElementFunction, TokenType.LeftParenthesis, TokenType.NullableExpression, TokenType.RigthParenthesis, TokenType.RigthBracket, TokenType.ComparisionOperator, TokenType.Constant } }
                }
            },
            {
                TokenType.NullableExpression,
                new() {
                    { TokenType.Identifier, new byte[] { TokenType.Expression } },
                    { TokenType.LeftParenthesis, new byte[] { TokenType.Expression } },
                    { TokenType.RigthParenthesis,  Array.Empty<byte>() }
                }
            },
            {
                TokenType.NullableIdentifier,
                new() {
                    { TokenType.Identifier, new byte[] { TokenType.Identifier } },
                    { TokenType.RigthParenthesis,  Array.Empty<byte>() }
                }
            }
        };
    }
}
