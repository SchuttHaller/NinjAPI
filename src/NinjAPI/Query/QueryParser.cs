using Microsoft.VisualBasic;
using NinjAPI.Expressions;
using NinjAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public class QueryParser<TEntity> where TEntity : class
    {       

        private readonly List<Token> _tokens;

        public Expression? FilterExpression { get; private set; }

        public bool IsValid { get; private set; }

        public QueryParser(IEnumerable<Token> _tokens) 
        {
            this._tokens = _tokens.ToList();
            this._tokens.Add(new Token() { Code = TokenType.EndOfLine, Value = "$" });

            try
            {
                this.Parse();
            }
            catch(Exception ex)
            {
                IsValid = false;
            }
        }

        private void Parse()
        {
            // init the transition stack
            Stack<byte> transitionStack = new();
            transitionStack.PushRange(TokenType.Start);


            int i = 0;
            byte tToken; // transition token
            Token token;

            do
            {
                tToken = transitionStack.Pop();
                token = _tokens[i];

                if (TokenType.IsTerminal(tToken))
                {
                    if (tToken == token.Code) i++;
                    else throw new Exception($"Unexpected token {token}");
                }
                else
                {
                    if (_transitionTable[tToken].TryGetValue(token.Code, out var production))
                    {
                        if(production.Length > 0)
                        {
                            transitionStack.PushRange(production.Reverse());
                        }
                    }
                    else
                    {
                        throw new Exception($"Syntax error: Unexpected token '{token.Value}'");
                    } 
                }

            }
            while (tToken != TokenType.EndOfLine);

            IsValid = true;
        }


        /// <summary>
        /// Grammar Transition Table:
        ///|        |   $       |   L       |   I       |   (       |   )   |   O       |   C   |   [       |   A   |   ]   |
        ///|:---:   |:---:      |:---:      |:---:      |:---:      |:---:  |:---:      |:---:  |:---:      |:---:  |:---:  |
        ///|    S   |           |           |S::= E $   |S::= E $   |       |           |       |           |       |       |
        ///|    E   |           |           |E::= K EP  |E::= K EP  |       |           |       |           |       |       |
        ///|    EP  |EP::= ε    |EP::= L E  |           |           |EP::= ε|           |       |           |       |       |
        ///|    K   |           |           |K::= I KP  |K::= (E )  |       |           |       |           |       |       |
        ///|    KP  |           |           |           |           |       |KP::= O C  |       |KP::=[A(E)]|       |       |
        /// </summary>
        private static readonly Dictionary<byte, Dictionary<byte, byte[]>> _transitionTable = new()
        {
            {
                TokenType.Start,
                new() {
                    { TokenType.LeftParenthesis, new byte[] { TokenType.Expression, TokenType.EndOfLine } },
                    { TokenType.Identifier, new byte[] { TokenType.Expression, TokenType.EndOfLine } }
                }
            },
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
                    { TokenType.LeftBracket, new byte[] { TokenType.LeftBracket, TokenType.ScalarFunction, TokenType.LeftParenthesis, TokenType.Expression, TokenType.RigthParenthesis, TokenType.RigthBracket } },
                    { TokenType.ComparisionOperator,new byte[] { TokenType.ComparisionOperator, TokenType.Constant } }
                }
            }
        };
    }
}
