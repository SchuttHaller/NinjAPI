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
        private FlagType _flag = FlagType.Identifier;     

        public QueryLexer(string query) 
        {
            query ??= string.Empty;
            _query = query!.ToLower();
        }

        public List<QueryToken> GetTokens()
        {
            var tokens = GenerateTokens().ToList();
            tokens.Add(new QueryToken() { Type = TokenType.EndOfLine, Value = "$" });
            return tokens;
        }

        private static bool IsDelimiter(char delimiter) => TokenCollections.Delimiters.Contains(delimiter);

        private int GetErrorPosition(string current) => _query.IndexOf(current);

        private IEnumerable<QueryToken> GenerateTokens()
        {
            if (string.IsNullOrWhiteSpace(_query)) yield break;
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
                        yield return GetNextToken(tokenBuilder.ToString())!;
                        tokenBuilder = tokenBuilder.Remove(0, tokenBuilder.Length);
                    }
                }

                if (IsDelimiter(currentChar) && !inString)
                {
                    yield return new() { Type = MapDelimiter(currentChar), Value = currentChar.ToString() };
                }

                if (!IsDelimiter(currentChar) && currentChar != D.Space && currentChar != D.NullChar)
                    tokenBuilder = tokenBuilder.Append(currentChar);

            }
            var token = GetNextToken(tokenBuilder.ToString());
            if (token == null) yield break;
            yield return token;
        }

        private QueryToken? GetNextToken(string current)
        {
            if (string.IsNullOrWhiteSpace(current)) return null;
            if (_flag == FlagType.Identifier)
            {
                _flag = FlagType.Operator;
                return new() { Type = TokenType.Identifier, Value = current };
            }
            if (_flag == FlagType.Logical)
            {
                var logicalOperator = TokenCollections.LogicalOperators.FirstOrDefault(w => current == w);
                if (logicalOperator == null)
                    throw new NotSupportedException($"Unexpected token {current} in position {GetErrorPosition(_query)}");

                _flag = FlagType.Identifier;
                return new() { Type = TokenType.LogicalOperator, Value = current };
            }
            if (_flag == FlagType.Operator)
            {
                if (!TokenCollections.ComparisionOperators.Contains(current))
                    throw new NotSupportedException($"Unexpected token {current} in position {GetErrorPosition(current)}");
                _flag = current == Operators.Any || current == Operators.All ? FlagType.Identifier  : FlagType.Constant;
                return new() { Type = TokenType.ComparisionOperator, Value = current };
            }

            //constant default
            _flag = FlagType.Logical;
            return new() { Type = TokenType.Constant, Value = current };
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
            public static readonly ReadOnlyCollection<char> Delimiters = new(new char[] { D.LeftParenthesis, D.RightParenthesis, D.LeftBracket, D.RightBracket });
            public static readonly ReadOnlyCollection<string> ComparisionOperators = new(new string[] { O.Equal, O.NotEqual, O.GreaterThan, O.GreaterOrEqual, O.LessThan, O.LessOrEqual, O.Like, O.StartsWith, O.EndsWith, O.All, O.Any });
        }
    }
}
