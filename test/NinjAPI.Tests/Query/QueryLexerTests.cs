using NinjAPI.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Tests.Query
{
    [TestClass]
    public class QueryLexerTests
    {

        [TestMethod]
        [DataRow("", 0)]
        [DataRow(" ", 0)]
        [DataRow("     ", 0)]
        [DataRow(null, 0)]
        public void WhenStringIsNullOrEmptyReturnsEmptyTable(string query, int expected)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokens();

            Assert.AreEqual(expected, result.Count());
        }

        [TestMethod]
        [DataRow("id eq 1", 3, 1, 1, 1)]
        [DataRow("name lk 'felipe'", 3, 1, 1, 1)]
        [DataRow("revenue gt 100", 3, 1, 1, 1)]
        [DataRow("revenue lt 200", 3, 1, 1, 1)]
        [DataRow("total eq 500", 3, 1, 1, 1)]
        [DataRow("description lk 'jimmy's test'", 3, 1, 1, 1)]
        [DataRow("description lk 'jimmy's \\\"test\\\"'", 3, 1, 1, 1)]
        public void WhenQueryIsSimpleExpressionReturnsTable(string query, int expectedCount, int expectedIdentifiers, int expectedOperators, int expectedLiterals)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokens().ToList();

            Assert.AreEqual(expectedCount, result.Count());
            Assert.AreEqual(expectedIdentifiers, result.IdentifierCount());
            Assert.AreEqual(expectedOperators, result.ComparisionOperatorCount());
            Assert.AreEqual(expectedLiterals, result.ConstantCount());
        }

        [TestMethod]
        [DataRow("id eq 1 and resultdate eq null", 7, 2, 2, 2, 1)]
        [DataRow("id eq 1 and revenue eq null or revenue eq 0", 11, 3, 3, 3, 2)]
        [DataRow("(name lk felipe') or lastname lk 'duarte'", 9, 2, 2, 2, 1)]
        [DataRow("id lk test001 or id lk test222 or createddate gt 23/11/12 and revenue eq 0", 15, 4, 4, 4, 3)]
        [DataRow("description lk 'jimmy's \\\"test\\\"' or description lk 'pepe's test'", 7, 2, 2, 2, 1)]
        public void WhenQueryHasLogicalOperatorsReturnsTable(string query, int expectedCount, int expectedIdentifiers, int expectedOperators, int expectedLiterals, int expectedLogicals)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokens().ToList();

            Assert.AreEqual(expectedCount, result.Count());
            Assert.AreEqual(expectedIdentifiers, result.IdentifierCount());
            Assert.AreEqual(expectedOperators, result.ComparisionOperatorCount());
            Assert.AreEqual(expectedLiterals, result.ConstantCount());
            Assert.AreEqual(expectedLogicals, result.LogicalOperatorCount());
        }

        [TestMethod]
        [DataRow("(id eq 1 and resultdate eq null)))(((", 14, 2, 2, 2, 1, 7)]
        [DataRow("(((()))id eq 1 and revenue eq null or revenue eq 0", 18, 3, 3, 3, 2, 7)]
        [DataRow("(id lk test001 or id lk test222 or))(() createddate gt 23/11/12 and revenue eq 0", 21, 4, 4, 4, 3, 6)]
        [DataRow("(id lk test001 or id lk test222) or ((createddate gt 23/11/12 and revenue eq 0) and resultdate eq 23/11/12)", 25, 5, 5, 5, 4, 6)]
        [DataRow("(description lk 'jimmy's test') or desc lk test", 9, 2, 2, 2, 1, 2)]
        public void WhenQueryHasDelimitersReturnsTable(string query, int expectedCount, int expectedIdentifiers, int expectedOperators, int expectedLiterals, int expectedLogicals, int expectedDelimiters)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokens().ToList();

            Assert.AreEqual(expectedCount, result.Count());
            Assert.AreEqual(expectedIdentifiers, result.IdentifierCount());
            Assert.AreEqual(expectedOperators, result.ComparisionOperatorCount());
            Assert.AreEqual(expectedLiterals, result.ConstantCount());
            Assert.AreEqual(expectedLogicals, result.LogicalOperatorCount());
            Assert.AreEqual(expectedDelimiters, result.DelimiterCount());
        }

        [TestMethod]
        [DataRow("students [ all ( score gt 6 ) ]", 9, 2, 2,  1, 0, 4)]
        [DataRow("grade eq 2 and students [ all ( score gt 6 )]", 13, 3, 3, 2, 1, 4)]
        [DataRow("grade eq 2 and students [ any ( score lt 6 or absence eq 5 ) ]", 17, 4, 4, 3, 2, 4)]
        public void WhenQueryHasChildrenReferenceReturnsTable(string query, int expectedCount, int expectedIdentifiers, int expectedOperators, int expectedLiterals, int expectedLogicals, int expectedDelimiters)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokens().ToList();

            Assert.AreEqual(expectedCount, result.Count());
            Assert.AreEqual(expectedIdentifiers, result.IdentifierCount());
            Assert.AreEqual(expectedOperators, result.ComparisionOperatorCount());
            Assert.AreEqual(expectedLiterals, result.ConstantCount());
            Assert.AreEqual(expectedLogicals, result.LogicalOperatorCount());
            Assert.AreEqual(expectedDelimiters, result.DelimiterCount());
        }

        [TestMethod]
        [DataRow("id test")]
        [DataRow("id eq test and test test")]
        [DataRow("itest test testttt")]
        [DataRow("ites(((((((((t test testttt")]
        [DataRow("description lk jimmy's \\\"test\\\"\\\"")]
        public void WhenQueryIsInvalidThrowsInvalidTypeException(string query)
        {
            var queryAnalyzer = new QueryLexer(query); 
            Assert.ThrowsException<NotSupportedException>(() => queryAnalyzer.GetTokens().ToList());
        }
    }
}
