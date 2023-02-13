using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using O = NinjAPI.Query.Operators;
using D = NinjAPI.Query.Delimiters;

namespace NinjAPI.Query
{
    public class QueryLexer
    {
        private readonly string _query;
        private bool isConstant = false;
        private readonly QueryToken EndOfLine = new() { Type = TokenType.EndOfLine, Value = "$" };
        public QueryLexer(string query) 
        {
            query ??= string.Empty;
            _query = query!.ToLower();
        }
        private static bool IsDelimiter(char delimiter) => TokenCollections.Delimiters.Contains(delimiter);

        public List<QueryToken> GetTokens()
        {
            if (string.IsNullOrWhiteSpace(_query)) return new List<QueryToken> { EndOfLine };

            var tokens = new List<QueryToken>();

            bool inString = false;
            int queryLength = _query.Length;
            StringBuilder tokenBuilder = new();
    
            for (int i = 0; i < queryLength; i++)
            {
                char currentChar = _query[i];
                char prevChar = i > 0 ? _query[i - 1] : D.NullChar;
                char nextChar = (i + 1 < _query.Length) ? _query[i + 1] : Delimiters.NullChar;

                if (currentChar == D.SingleQuote && (prevChar == D.NullChar || prevChar == D.Space) && !inString)
                {
                    inString = true;
                    continue;
                }
                if (currentChar == D.Backslash && (nextChar == D.DoubleQuote || nextChar == D.SingleQuote))
                {
                    continue;
                }
                if (currentChar == D.SingleQuote && (nextChar == D.NullChar || nextChar == D.Space || IsDelimiter(nextChar)) && inString && prevChar != D.Backslash)
                {
                    inString = false;
                    continue;
                }

                if ((currentChar == D.Space || currentChar == D.NullChar || IsDelimiter(currentChar)) && !inString)
                {
                    if (tokenBuilder.Length > 0)
                    {
                        tokens.Add(GetNextToken(tokenBuilder.ToString())!);
                        tokenBuilder = tokenBuilder.Remove(0, tokenBuilder.Length);
                    }
                }

                if (IsDelimiter(currentChar) && !inString)
                {
                    tokens.Add(new() { Type = MapDelimiter(currentChar), Value = currentChar.ToString() });
                }

                if (!IsDelimiter(currentChar) && currentChar != D.Space && currentChar != D.NullChar)
                    tokenBuilder = tokenBuilder.Append(currentChar);

            }
            if (tokenBuilder.Length > 0)
                tokens.Add(GetNextToken(tokenBuilder.ToString())!);
            tokens.Add(EndOfLine);
            return tokens;
        }

        private QueryToken? GetNextToken(string current)
        {
            if (string.IsNullOrWhiteSpace(current)) return null;

            if (isConstant)
            {
                isConstant = false;
                return new() { Type = TokenType.Constant, Value = current };
            }

            if (TokenCollections.LogicalOperators.Contains(current)) return new() { Type = TokenType.LogicalOperator, Value = current };

            if (TokenCollections.ElementFunctions.Contains(current)) return new() { Type = TokenType.ElementFunction, Value = current };

            if (TokenCollections.MathFunctions.Contains(current)) return new() { Type = TokenType.MathFunction, Value = current };

            if (TokenCollections.SortingOperators.Contains(current)) return new() { Type = TokenType.SortingOperator, Value = current };

            if (TokenCollections.QuantifierOperators.Contains(current))
            {
                var type = current == O.All ? TokenType.QuantifierFunctionAll : TokenType.QuantifierFunctionAny;
                return new() { Type = type, Value = current };
            }

            if (TokenCollections.ComparisionOperators.Contains(current))
            {
                isConstant = true;
                return new() { Type = TokenType.ComparisionOperator, Value = current };
            }

      
            //identifier default
            return new() { Type = TokenType.Identifier, Value = current };
        }

        private enum FlagType
        {
            Identifier,
            Logical,
            Operator,
            Constant,

        }

        private static byte MapDelimiter(char delimiter)
        {
            return delimiter switch
            {
                D.LeftParenthesis => TokenType.LeftParenthesis,
                D.RightParenthesis => TokenType.RigthParenthesis,
                D.LeftBracket => TokenType.LeftBracket,
                D.RightBracket => TokenType.RigthBracket,
                _ => TokenType.EndOfLine,
            };
        }

        private static class TokenCollections
        {
            public static readonly ReadOnlyCollection<string> LogicalOperators = new(new string[] { O.And, O.Or });
            public static readonly ReadOnlyCollection<string> SortingOperators = new(new string[] { O.Desc, O.Asc });
            public static readonly ReadOnlyCollection<string> ElementFunctions = new(new string[] { O.First, O.Last });
            public static readonly ReadOnlyCollection<string> QuantifierOperators = new(new string[] { O.All, O.Any });
            public static readonly ReadOnlyCollection<string> MathFunctions = new(new string[] { O.Min, O.Max, O.Sum });
            public static readonly ReadOnlyCollection<char> Delimiters = new(new char[] { D.LeftParenthesis, D.RightParenthesis, D.LeftBracket, D.RightBracket, D.Comma, D.Dot });
            public static readonly ReadOnlyCollection<string> ComparisionOperators = new(new string[] { O.Equal, O.NotEqual, O.GreaterThan, O.GreaterOrEqual, O.LessThan, O.LessOrEqual, O.Like, O.StartsWith, O.EndsWith });


        }
    }
}
