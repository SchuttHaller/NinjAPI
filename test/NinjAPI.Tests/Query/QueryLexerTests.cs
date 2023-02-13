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
        [DataRow("", 1)]
        [DataRow(" ", 1)]
        [DataRow("     ", 1)]
        [DataRow(null, 1)]
        public void WhenStringIsNullOrEmptyReturnsEmptyTable(string query, int expected)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokens();

            Assert.AreEqual(expected, result.Count());
        }

        [TestMethod]
        [DataRow("id eq 1", 4, 1, 1, 1)]
        [DataRow("name lk 'felipe'", 4, 1, 1, 1)]
        [DataRow("revenue gt 100", 4, 1, 1, 1)]
        [DataRow("revenue lt 200", 4, 1, 1, 1)]
        [DataRow("total eq 500", 4, 1, 1, 1)]
        [DataRow("description lk 'jimmy's test'", 4, 1, 1, 1)]
        [DataRow("description lk 'jimmy's \\\"test\\\"'", 4, 1, 1, 1)]
        public void WhenQueryIsSimpleExpressionReturnsTable(string query, int expectedCount, int expectedIdentifiers, int expectedOperators, int expectedLiterals)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokens();

            Assert.AreEqual(expectedCount, result.Count());
            Assert.AreEqual(expectedIdentifiers, result.IdentifierCount());
            Assert.AreEqual(expectedOperators, result.OperatorCount());
            Assert.AreEqual(expectedLiterals, result.ConstantCount());
        }

        [TestMethod]
        [DataRow("id eq 1 and resultdate eq null", 8, 2, 2, 2, 1)]
        [DataRow("id eq 1 and revenue eq null or revenue eq 0", 12, 3, 3, 3, 2)]
        [DataRow("(name lk felipe') or lastname lk 'duarte'", 10, 2, 2, 2, 1)]
        [DataRow("id lk test001 or id lk test222 or createddate gt 23/11/12 and revenue eq 0", 16, 4, 4, 4, 3)]
        [DataRow("description lk 'jimmy's \\\"test\\\"' or description lk 'pepe's test'", 8, 2, 2, 2, 1)]
        public void WhenQueryHasLogicalOperatorsReturnsTable(string query, int expectedCount, int expectedIdentifiers, int expectedOperators, int expectedLiterals, int expectedLogicals)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokens();

            Assert.AreEqual(expectedCount, result.Count());
            Assert.AreEqual(expectedIdentifiers, result.IdentifierCount());
            Assert.AreEqual(expectedOperators, result.OperatorCount());
            Assert.AreEqual(expectedLiterals, result.ConstantCount());
            Assert.AreEqual(expectedLogicals, result.LogicalOperatorCount());
        }

        [TestMethod]
        [DataRow("(id eq 1 and resultdate eq null)))(((", 15, 2, 2, 2, 1, 8)]
        [DataRow("(((()))id eq 1 and revenue eq null or revenue eq 0", 19, 3, 3, 3, 2, 8)]
        [DataRow("(id lk test001 or id lk test222 or))(() createddate gt 23/11/12 and revenue eq 0", 22, 4, 4, 4, 3, 7)]
        [DataRow("(id lk test001 or id lk test222) or ((createddate gt 23/11/12 and revenue eq 0) and resultdate eq 23/11/12)", 26, 5, 5, 5, 4, 7)]
        [DataRow("(description lk 'jimmy's test') or descr lk test", 10, 2, 2, 2, 1, 3)]
        public void WhenQueryHasDelimitersReturnsTable(string query, int expectedCount, int expectedIdentifiers, int expectedOperators, int expectedLiterals, int expectedLogicals, int expectedDelimiters)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokens();

            Assert.AreEqual(expectedCount, result.Count());
            Assert.AreEqual(expectedIdentifiers, result.IdentifierCount());
            Assert.AreEqual(expectedOperators, result.OperatorCount());
            Assert.AreEqual(expectedLiterals, result.ConstantCount());
            Assert.AreEqual(expectedLogicals, result.LogicalOperatorCount());
            Assert.AreEqual(expectedDelimiters, result.DelimiterCount());
        }

        [TestMethod]
        [DataRow("students [ all ( score gt 6 ) ]", 10, 2, 2,  1, 0, 5)]
        [DataRow("students [ first() ].id eq 10", 11, 2, 2, 1, 0, 6)]
        [DataRow("grade eq 2 and students [ all ( score gt 6 )]", 14, 3, 3, 2, 1, 5)]
        [DataRow("grade eq 2 and students [ any ( score lt 6 or absence eq 5 ) ]", 18, 4, 4, 3, 2, 5)]
        [DataRow("date gt 2012-01-01 and products[first()].price gt 10 or products[max(price)] gt 100", 25, 5, 5, 3, 2, 10)]
        [DataRow("date gt 2012-01-01 and products[first()].price gt 10 or products[max(price)] gt 100 and products[all(price gt 100)]", 35, 7, 7, 4, 3, 14)]
        public void WhenQueryHasChildrenReferenceReturnsTable(string query, int expectedCount, int expectedIdentifiers, int expectedOperators, int expectedLiterals, int expectedLogicals, int expectedDelimiters)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokens();

            Assert.AreEqual(expectedCount, result.Count());
            Assert.AreEqual(expectedIdentifiers, result.IdentifierCount());
            Assert.AreEqual(expectedOperators, result.OperatorCount());
            Assert.AreEqual(expectedLiterals, result.ConstantCount());
            Assert.AreEqual(expectedLogicals, result.LogicalOperatorCount());
            Assert.AreEqual(expectedDelimiters, result.DelimiterCount());
        }

        [TestMethod]
        [DataRow("id desc", 3, 1, 1)]
        [DataRow("id desc, name asc", 6, 2, 2)]
        public void WhenQueryHasSortingOperatorsReturnsTable(string query, int expectedCount, int expectedIdentifiers, int expectedOperators)
        {
            var queryAnalyzer = new QueryLexer(query);
            var result = queryAnalyzer.GetTokens();

            Assert.AreEqual(expectedCount, result.Count());
            Assert.AreEqual(expectedIdentifiers, result.IdentifierCount());
            Assert.AreEqual(expectedOperators, result.OperatorCount());
        }
    }
}
