using NinjAPI.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NinjAPI.Query
{
    public class QueryLexer
    {
        private readonly string _originalQuery;
        private readonly LexicalTable _queryTable = new();
        private FlagType _flag = FlagType.Identifier;     

        public QueryLexer(string query) 
        {
            if (query is null) _originalQuery = string.Empty;
            _originalQuery = query!.ToUpper();
        }

        public LexicalTable GetTokenTable()
        {
            if (string.IsNullOrWhiteSpace(_originalQuery)) return _queryTable;
            GetConcurrences(_originalQuery);
            return _queryTable;
        }

        private static bool IsDelimiter(char delimiter) => TokenCollections.Delimiters.Contains(delimiter);

        private int GetErrorPosition(string query) => _originalQuery.Length - query.Length;

        private void GetConcurrences(string query)
        {
            if (query.Length < 1) return;

            var startChar = query[0];
            var newQuery = "";

            if (IsDelimiter(startChar))
            {
                newQuery = GetDelimiter(query);
            }
            else if (_flag == FlagType.Identifier)
            {
                newQuery = GetNextWord(query);
            }

            else if (_flag == FlagType.Operator)
            {
                newQuery = GetOperator(query);
                _flag = FlagType.Constant;
            }
            else if (_flag == FlagType.Constant)
            {
                newQuery = GetNextConstant(query);
                _flag = FlagType.Identifier;
            }
            newQuery = newQuery.Trim();

            if (newQuery.Length > 0) GetConcurrences(newQuery);
        }

        private string GetDelimiter(string query)
        {
            var del = query[0];
            if (!IsDelimiter(del))
                throw new NotSupportedException($"Unexpected delimiter {del} in position {GetErrorPosition(query)}");

            _queryTable.AddToken(del.ToString(), del.ToString());
            _queryTable.DelimiterCount++;
            return query[1..];
        }

        private string GetNextWord(string query)
        {
            var endIdx = 0;
            while (endIdx < query.Length && !IsDelimiter(query[endIdx])) ++endIdx;

            var word = query[..endIdx];
            var newQuery = query[endIdx..];

            var logicalOperator = TokenCollections.LogicalOperators.FirstOrDefault(w => word.ToUpper() == w);

            if (logicalOperator != null)
            {
                _queryTable.AddToken(logicalOperator, TokenType.LogicalOperator);
                _queryTable.LogicalOperatorCount++;

                _flag = FlagType.Identifier;
            }
            else
            {
                _queryTable.AddToken(word, TokenType.Identifier);
                _queryTable.IdentifierCount++;

                _flag = FlagType.Operator;
            }
            return newQuery;
        }

        private string GetOperator(string query)
        {
            var token2 = query[..2]; //operators length == 2

            if (!TokenCollections.ComparisionOperators.Contains(token2))
                throw new NotSupportedException($"Unexpected token {token2[0]} in position {GetErrorPosition(query)}");

            _queryTable.AddToken(token2, TokenType.ComparisionOperator);
            _queryTable.ComparisionOperatorCount++;

            return query[2..];
        }

        private string GetNextConstant(string query)
        {
            var endIdx = 0;
            while (endIdx < query.Length && !IsDelimiter(query[endIdx])) ++endIdx;

            var token = query[..endIdx];
            var newQuery = query[endIdx..];

            _queryTable.AddToken(token, TokenType.Constant);
            _queryTable.ConstantCount++;

            return newQuery;          
        }

        private enum FlagType
        {
            Identifier,
            Operator,
            Constant
        }

        private static class TokenCollections
        {
            public static readonly ReadOnlyCollection<string> LogicalOperators = new(new string[] { LogicalOperator.AND, LogicalOperator.OR });
            public static readonly ReadOnlyCollection<char> Delimiters = new(new char[] { Delimiter.OpenParent, Delimiter.CloseParent, Delimiter.SingleQuote, Delimiter.Space });
            public static readonly ReadOnlyCollection<string> ComparisionOperators = new(new string[] { ComparisionOperator.Equal, ComparisionOperator.NotEqual, ComparisionOperator.GreaterThan, ComparisionOperator.GreaterOrEqual, ComparisionOperator.LessThan, ComparisionOperator.LessOrEqual, ComparisionOperator.Like, ComparisionOperator.StartsWith, ComparisionOperator.EndsWith });
        }
    }
}
