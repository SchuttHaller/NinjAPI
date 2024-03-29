﻿using Microsoft.VisualBasic.FileIO;
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
using T = NinjAPI.Query.Types;
using NinjAPI.Helpers;

namespace NinjAPI.Query
{
    public class QueryLexer
    {
        private readonly string _query;
        private bool isStr = false;
        private readonly QueryToken EndOfLine = new() { Type = TokenType.EndOfLine, Value = D.NullChar.ToString() };
        private readonly QueryToken SingleQuote = new() { Type = TokenType.SingleQuote, Value = D.SingleQuote.ToString() };
        public QueryLexer(string query) 
        {           
            _query = query ?? string.Empty;
        }
        private static bool IsDelimiter(char delimiter, char prevChar = '\x0000')
        {
            if (prevChar == '\x0000') return TokenCollections.Delimiters.Contains(delimiter);

            if (char.IsDigit(prevChar) && delimiter == D.Dot) return false;

            return TokenCollections.Delimiters.Contains(delimiter);
        }

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
                    isStr = true;
                    tokens.Add(SingleQuote);
                    continue;
                }
                if (currentChar == D.Backslash && (nextChar == D.DoubleQuote || nextChar == D.SingleQuote))
                {
                    continue;
                }
                if (currentChar == D.SingleQuote && inString && prevChar != D.Backslash)
                {
                    inString = false;
                }

                if ((currentChar == D.Space || currentChar == D.NullChar || IsDelimiter(currentChar, prevChar)) && !inString)
                {
                    if (tokenBuilder.Length > 0)
                    {
                        tokens.Add(GetNextToken(tokenBuilder.ToString())!);
                        tokenBuilder = tokenBuilder.Remove(0, tokenBuilder.Length);
                    }
                }

                if (IsDelimiter(currentChar, prevChar) && !inString)
                {
                    tokens.Add(new() { Type = MapDelimiter(currentChar), Value = currentChar.ToString() });
                }

                if (inString || (!IsDelimiter(currentChar, prevChar) && currentChar != D.Space && currentChar != D.NullChar))
                    tokenBuilder = tokenBuilder.Append(currentChar);

            }
            if (tokenBuilder.Length > 0)
                tokens.Add(GetNextToken(tokenBuilder.ToString())!);
            tokens.Add(EndOfLine);
            return tokens;
        }

        private QueryToken? GetNextToken(string current)
        {
            if (string.IsNullOrWhiteSpace(current)) 
                return null;

            if (isStr)
            {
                isStr = false;
                return QueryToken.New(TokenType.String, current);
            }

            if (char.IsDigit(current[0])) 
                return QueryToken.New(TokenType.Number, current);

            if (current == T.Null) 
                return QueryToken.New(TokenType.Null, current);

            if (TokenCollections.BooleanTypes.ContainsNoCase(current)) 
                return QueryToken.New(TokenType.Boolean, current);

            if (TokenCollections.LogicalOperators.ContainsNoCase(current)) 
                return QueryToken.New(TokenType.LogicalOperator, current);

            if (TokenCollections.ElementFunctions.ContainsNoCase(current)) 
                return QueryToken.New(TokenType.ElementFunction, current);

            if (TokenCollections.MathFunctions.ContainsNoCase(current)) 
                return QueryToken.New(TokenType.MathFunction, current);

            if (TokenCollections.SortingOperators.ContainsNoCase(current)) 
                return QueryToken.New(TokenType.SortingOperator, current);

            if (TokenCollections.QuantifierOperators.ContainsNoCase(current))
            {
                var type = current == O.All ? TokenType.QuantifierFunctionAll : TokenType.QuantifierFunctionAny;
                return QueryToken.New(type, current);
            }

            if (TokenCollections.ComparisionOperators.ContainsNoCase(current)) 
                return QueryToken.New(TokenType.ComparisionOperator, current);

            //identifier default
            return QueryToken.New(TokenType.Identifier, current);
        }

        private static TokenType MapDelimiter(char delimiter)
        {
            return delimiter switch
            {
                D.LeftParenthesis => TokenType.LeftParenthesis,
                D.RightParenthesis => TokenType.RightParenthesis,
                D.LeftBracket => TokenType.LeftBracket,
                D.RightBracket => TokenType.RightBracket,
                D.SingleQuote => TokenType.SingleQuote,
                D.Dollar => TokenType.Dollar,
                D.Comma => TokenType.Comma,
                D.Dot => TokenType.Dot,
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
            public static readonly ReadOnlyCollection<char> Delimiters = new(new char[] { D.LeftParenthesis, D.RightParenthesis, D.LeftBracket, D.RightBracket, D.Comma, D.Dot, D.SingleQuote, D.Dollar });
            public static readonly ReadOnlyCollection<string> ComparisionOperators = new(new string[] { O.Equal, O.NotEqual, O.GreaterThan, O.GreaterOrEqual, O.LessThan, O.LessOrEqual, O.Like, O.StartsWith, O.EndsWith, O.In });
            public static readonly ReadOnlyCollection<string> BooleanTypes = new(new string[] { T.False, T.True });

        }
    }
}
