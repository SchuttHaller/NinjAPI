using Microsoft.VisualBasic.FileIO;
using NinjAPI.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NinjAPI.Query
{
    public class QueryLexer
    {
        private readonly string _query;
        private FlagType _flag = FlagType.Identifier;     

        public QueryLexer(string query) 
        {
            if (query is null) query = string.Empty;
            _query = query!.ToUpper();
        }

        private static bool IsDelimiter(char delimiter) => TokenCollections.Delimiters.Contains(delimiter);

        private int GetErrorPosition(string current) => _query.IndexOf(current);

        public IEnumerable<Token> GetTokens()
        {
            if (string.IsNullOrWhiteSpace(_query)) yield break;
            bool inString = false;
            int queryLength = _query.Length;
            StringBuilder tokenBuilder = new();
    
            for (int i = 0; i < queryLength; i++)
            {
                char currentChar = _query[i];
                char prevChar = i > 0 ? _query[i - 1] : Delimiter.NullChar;
                char nextChar = (i + 1 < _query.Length) ? _query[i + 1] : Delimiter.NullChar;

                if (currentChar == Delimiter.SingleQuote && (prevChar == Delimiter.NullChar || prevChar == Delimiter.Space) && !inString)
                {
                    inString = true;
                    continue;
                }
                if (currentChar == Delimiter.Backslash && (nextChar == Delimiter.DoubleQuote || nextChar == Delimiter.SingleQuote))
                {
                    continue;
                }
                if (currentChar == Delimiter.SingleQuote && (nextChar == Delimiter.NullChar || nextChar == Delimiter.Space || IsDelimiter(nextChar)) && inString && prevChar != Delimiter.Backslash)
                {
                    inString = false;
                    continue;
                }

                if ((currentChar == Delimiter.Space || currentChar == Delimiter.NullChar || IsDelimiter(currentChar)) && !inString)
                {
                    if (tokenBuilder.Length > 0)
                    {
                        yield return GetToken(tokenBuilder.ToString())!;
                        tokenBuilder = tokenBuilder.Remove(0, tokenBuilder.Length);
                    }
                }

                if (IsDelimiter(currentChar) && !inString)
                {
                    yield return new() { Code = MapDelimiter(currentChar), Value = currentChar.ToString() };
                }

                if (!IsDelimiter(currentChar) && currentChar != Delimiter.Space && currentChar != Delimiter.NullChar)
                    tokenBuilder = tokenBuilder.Append(currentChar);

            }
            var token = GetToken(tokenBuilder.ToString());
            if (token == null) yield break;
            yield return token;
        }

        public Token? GetToken(string current)
        {
            if (string.IsNullOrWhiteSpace(current)) return null;
            if (_flag == FlagType.Identifier)
            {
                _flag = FlagType.Operator;
                return new() { Code = TokenType.Identifier, Value = current };
            }
            if (_flag == FlagType.Logical)
            {
                var logicalOperator = TokenCollections.LogicalOperators.FirstOrDefault(w => current == w);
                if (logicalOperator == null)
                    throw new NotSupportedException($"Unexpected token {current} in position {GetErrorPosition(_query)}");

                _flag = FlagType.Identifier;
                return new() { Code = TokenType.LogicalOperator, Value = current };
            }
            if (_flag == FlagType.Operator)
            {
                if (!TokenCollections.ComparisionOperators.Contains(current))
                    throw new NotSupportedException($"Unexpected token {current} in position {GetErrorPosition(current)}");
                _flag = current == ComparisionOperator.Any || current == ComparisionOperator.All ? FlagType.Identifier  : FlagType.Constant;
                return new() { Code = TokenType.ComparisionOperator, Value = current };
            }

            //constant default
            _flag = FlagType.Logical;
            return new() { Code = TokenType.Constant, Value = current };
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
            switch(delimiter)
            {
                case Delimiter.LeftParenthesis: return TokenType.LeftParenthesis; 
                case Delimiter.RightParenthesis: return TokenType.RigthParenthesis;
                case Delimiter.LeftBracket: return TokenType.LeftBracket;
                case Delimiter.RightBracket: return TokenType.RigthBracket;
                default: return TokenType.EndOfLine;
            }
        }

        private static class TokenCollections
        {
            public static readonly ReadOnlyCollection<string> LogicalOperators = new(new string[] { LogicalOperator.AND, LogicalOperator.OR });
            public static readonly ReadOnlyCollection<char> Delimiters = new(new char[] { Delimiter.LeftParenthesis, Delimiter.RightParenthesis, Delimiter.LeftBracket, Delimiter.RightBracket });
            public static readonly ReadOnlyCollection<string> ComparisionOperators = new(new string[] { ComparisionOperator.Equal, ComparisionOperator.NotEqual, ComparisionOperator.GreaterThan, ComparisionOperator.GreaterOrEqual, ComparisionOperator.LessThan, ComparisionOperator.LessOrEqual, ComparisionOperator.Like, ComparisionOperator.StartsWith, ComparisionOperator.EndsWith, ComparisionOperator.All, ComparisionOperator.Any });
        }
    }
}
