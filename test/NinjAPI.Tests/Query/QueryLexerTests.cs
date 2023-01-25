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
        public void Filter_NullOrEmptyString_ReturnsEmptyTable(string query, int expected)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokenTable();

            Assert.AreEqual(expected, result.Tokens.Count);
        }

        [TestMethod]
        [DataRow("id eq 1", 3, 1, 1, 1)]
        [DataRow("name lk 'felipe'", 5, 1, 1, 1)]
        [DataRow("revenue gt 100", 3, 1, 1, 1)]
        [DataRow("revenue lt 200", 3, 1, 1, 1)]
        [DataRow("total eq 500", 3, 1, 1, 1)]
        public void Filter_QueryWithSimpleExpression_ReturnsTable(string query, int expectedCount, int expectedIdentifiers, int expectedOperators, int expectedLiterals)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokenTable();

            Assert.AreEqual(expectedCount, result.Tokens.Count);
            Assert.AreEqual(expectedIdentifiers, result.IdentifierCount);
            Assert.AreEqual(expectedOperators, result.ComparisionOperatorCount);
            Assert.AreEqual(expectedLiterals, result.ConstantCount);
        }

        [TestMethod]
        [DataRow("id eq 1 and resultdate eq null", 7, 2, 2, 2, 1)]
        [DataRow("id eq 1 and revenue eq null or revenue eq 0", 11, 3, 3, 3, 2)]
        [DataRow("id lk test001 or id lk test222 or createddate gt 23/11/12 and revenue eq 0", 15, 4, 4, 4, 3)]
        public void Filter_QueryWithLogicalOperators_ReturnsTable(string query, int expectedCount, int expectedIdentifiers, int expectedOperators, int expectedLiterals, int expectedLogicals)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokenTable();

            Assert.AreEqual(expectedCount, result.Tokens.Count);
            Assert.AreEqual(expectedIdentifiers, result.IdentifierCount);
            Assert.AreEqual(expectedOperators, result.ComparisionOperatorCount);
            Assert.AreEqual(expectedLiterals, result.ConstantCount);
            Assert.AreEqual(expectedLogicals, result.LogicalOperatorCount);
        }

        [TestMethod]
        [DataRow("(id eq 1 and resultdate eq null)))(((", 14, 2, 2, 2, 1, 7)]
        [DataRow("(((()))id eq 1 and revenue eq null or revenue eq 0", 18, 3, 3, 3, 2, 7)]
        [DataRow("(id lk test001 or id lk test222 or))(() createddate gt 23/11/12 and revenue eq 0", 21, 4, 4, 4, 3, 6)]
        [DataRow("(id lk test001 or id lk test222) or ((createddate gt 23/11/12 and revenue eq 0) and resultdate eq 23/11/12)", 25, 5, 5, 5, 4, 6)]
        public void Filter_QueryWithDelimiters_ReturnsTable(string query, int expectedCount, int expectedIdentifiers, int expectedOperators, int expectedLiterals, int expectedLogicals, int expectedDelimiters)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokenTable();

            Assert.AreEqual(expectedCount, result.Tokens.Count);
            Assert.AreEqual(expectedIdentifiers, result.IdentifierCount);
            Assert.AreEqual(expectedOperators, result.ComparisionOperatorCount);
            Assert.AreEqual(expectedLiterals, result.ConstantCount);
            Assert.AreEqual(expectedLogicals, result.LogicalOperatorCount);
            Assert.AreEqual(expectedDelimiters, result.DelimiterCount);
        }

        [TestMethod]
        [DataRow("id test")]
        [DataRow("id eq test and test test")]
        [DataRow("itest test testttt")]
        [DataRow("ites(((((((((t test testttt")]
        public void Filter_WrongQuery_ThrowsInvalidTypeException(string query)
        {
            var queryAnalyzer = new QueryLexer(query); 
            Assert.ThrowsException<NotSupportedException>(() => queryAnalyzer.GetTokenTable());
        }
    }
}
