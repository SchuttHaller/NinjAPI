using NinjAPI.Query;
using NinjAPI.Tests.Query.Mocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Tests.Query
{
    [TestClass]
    public class QueryParserTests
    {
        [TestMethod]
        [DynamicData(nameof(BasicClauseExpressions), DynamicDataSourceType.Method)]
        [DynamicData(nameof(LogicOperatorExpressions), DynamicDataSourceType.Method)]
        public void Expression_WithBasicClause_ReturnsTree(string query, string expected)
        {
            var tokens = new QueryLexer(query).GetTokens();
            var tree = QueryParser.Parse(tokens);
            Assert.IsNotNull(tree);
            Assert.IsTrue(tree.Children.Count > 0);
            Assert.AreEqual(expected, tree.ToString());
        }

        [TestMethod]
        [DataRow("id 1 and transaction '20012'")]
        [DataRow("id eq and transaction lk '20012'")]
        [DataRow("id transaction lk '20012'")]
        [DataRow("23453 transaction lk '20012'")]
        [DataRow("id transaction lk '20012")]
        public void InvalidQuery_ThrowsError(string query)
        {
            var tokens = new QueryLexer(query).GetTokens();
            Assert.ThrowsException<Exception>(() => QueryParser.Parse(tokens));
        }

        private static IEnumerable<object[]> BasicClauseExpressions()
        {
            return new[]
            {
                new object[] {
                    "sale.revenue eq 1000.50",
                    "Expression { Clause { Left { PropertyNavigation { sale . revenue } } eq Right { Value { 1000.50 } } } }"
                },
                new object[] {
                    "deleted ne true",
                    "Expression { Clause { Left { PropertyNavigation { deleted } } ne Right { Value { true } } } }"
                },
                new object[] {
                    "revenue gt 1150",
                    "Expression { Clause { Left { PropertyNavigation { revenue } } gt Right { Value { 1150 } } } }"
                },
                new object[] {
                    "revenue ge 1500",
                    "Expression { Clause { Left { PropertyNavigation { revenue } } ge Right { Value { 1500 } } } }"
                },
                new object[] {
                    "revenue lt 1000",
                    "Expression { Clause { Left { PropertyNavigation { revenue } } lt Right { Value { 1000 } } } }"
                },
                new object[] {
                    "revenue le 1100",
                    "Expression { Clause { Left { PropertyNavigation { revenue } } le Right { Value { 1100 } } } }"
                },
                new object[] {
                    "description lk 'purchase'",
                    "Expression { Clause { Left { PropertyNavigation { description } } lk Right { Value { ' purchase ' } } } }"
                },
                new object[] {
                    "description.text sw 'pur'",
                    "Expression { Clause { Left { PropertyNavigation { description . text } } sw Right { Value { ' pur ' } } } }"
                },
                new object[] {
                    "description ew 'chase'",
                    "Expression { Clause { Left { PropertyNavigation { description } } ew Right { Value { ' chase ' } } } }"
                },
            };
        }

        private static IEnumerable<object[]> LogicOperatorExpressions()
        {
            return new[]
            {
                new object[] {
                    "id gt 1 and transaction lk '20012'",
                    "Expression { Clause { Left { PropertyNavigation { id } } gt Right { Value { 1 } } } and Expression { Clause { Left { PropertyNavigation { transaction } } lk Right { Value { ' 20012 ' } } } } }"
                },
                new object[] {
                    "id eq 1 or id eq 2",
                    "Expression { Clause { Left { PropertyNavigation { id } } eq Right { Value { 1 } } } or Expression { Clause { Left { PropertyNavigation { id } } eq Right { Value { 2 } } } } }"
                },
                new object[] {
                    "id eq 1 or id eq 2 and transaction lk '20012'",
                    "Expression { Clause { Left { PropertyNavigation { id } } eq Right { Value { 1 } } } or Expression { Clause { Left { PropertyNavigation { id } } eq Right { Value { 2 } } } and Expression { Clause { Left { PropertyNavigation { transaction } } lk Right { Value { ' 20012 ' } } } } } }"
                }
            };
        }

    }
}
