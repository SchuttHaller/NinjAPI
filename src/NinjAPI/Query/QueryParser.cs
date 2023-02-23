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
    public static partial class QueryParser
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
            treeStack.PushRange(new(TokenType.EndOfLine), root);


            int i = 0;
            TokenType top; // transition token
            QueryToken next;
            QueryNode node; // start the node with the root

            do
            {
                top = transitionStack.Pop();
                node = treeStack.Pop();
                next = tokens[i];

                if (top.IsTerminal())
                {
                    // if top of the stack is a terminar and the next token doesn't match it
                    // it's an error
                    if(top != next.Type)
                        throw new Exception($"Unexpected token {next.Value}");

                    // is the token matches, then consume the input
                    i++;

                    // if we are consuming the same node set the value
                    if(node.Type == next.Type)
                    {
                        node.Token = next;
                    } 
                    continue;
                }

                // if transition table doesn't contain a valid transition for the non-terminal token
                // it's a syntanx error
                if (!GrammarTable[top].ContainsKey(next.Type))
                {
                    throw new Exception($"Syntax error: Unexpected token '{next.Value}'");
                }

                // get next production for the non-terminal
                var production = GrammarTable[top][next.Type];
                
                if (production.Any())
                {
                    // expand the transition stack
                    transitionStack.PushRange(production.Reverse());
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
    }
}
