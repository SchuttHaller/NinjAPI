using NinjAPI.Query;

namespace NinjAPI.Tests.Query
{
    [TestClass]
    public class QueryParserTests
    {
        [TestMethod]
        [DynamicData(nameof(BasicClauseExpressions), DynamicDataSourceType.Method)]
        [DynamicData(nameof(LogicOperatorExpressions), DynamicDataSourceType.Method)]
        [DynamicData(nameof(FunctionExpressions), DynamicDataSourceType.Method)]
        public void Valid_Query_ReturnsTree(string query, string expected)
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
        [DataRow("sale.date gt '2023-01-12 and sale.date lt '2023-01-13'")]
        public void Invalid_Query_ThrowsError(string query)
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
                    "Expression { Clause { PropertyNavigation { sale . revenue } eq Value { 1000.50 } } }"
                },
                new object[] {
                    "deleted ne true",
                    "Expression { Clause { PropertyNavigation { deleted } ne Value { true } } }"
                },
                new object[] {
                    "revenue gt 1150",
                    "Expression { Clause { PropertyNavigation { revenue } gt Value { 1150 } } }"
                },
                new object[] {
                    "revenue ge 1500",
                    "Expression { Clause { PropertyNavigation { revenue } ge Value { 1500 } } }"
                },
                new object[] {
                    "revenue lt 1000",
                    "Expression { Clause { PropertyNavigation { revenue } lt Value { 1000 } } }"
                },
                new object[] {
                    "revenue le 1100",
                    "Expression { Clause { PropertyNavigation { revenue } le Value { 1100 } } }"
                },
                new object[] {
                    "description lk 'purchase'",
                    "Expression { Clause { PropertyNavigation { description } lk Value { ' purchase ' } } }"
                },
                new object[] {
                    "description.text sw 'pur'",
                    "Expression { Clause { PropertyNavigation { description . text } sw Value { ' pur ' } } }"
                },
                new object[] {
                    "description ew 'chase'",
                    "Expression { Clause { PropertyNavigation { description } ew Value { ' chase ' } } }"
                },
            };
        }

        private static IEnumerable<object[]> LogicOperatorExpressions()
        {
            return new[]
            {
                new object[] {
                    "id gt 1 and transaction lk '20012'",
                    "Expression { Clause { PropertyNavigation { id } gt Value { 1 } } and Clause { PropertyNavigation { transaction } lk Value { ' 20012 ' } } }"
                },
                new object[] {
                    "id eq 1 or id eq 2",
                    "Expression { Clause { PropertyNavigation { id } eq Value { 1 } } or Clause { PropertyNavigation { id } eq Value { 2 } } }"
                },
                new object[] {
                    "id eq 1 or id eq 2 and transaction lk '20012'",
                    "Expression { Clause { PropertyNavigation { id } eq Value { 1 } } or Clause { PropertyNavigation { id } eq Value { 2 } } and Clause { PropertyNavigation { transaction } lk Value { ' 20012 ' } } }"
                }
            };
        }

        private static IEnumerable<object[]> FunctionExpressions()
        {
            return new[]
            {
                new object[] {
                    "saleItems[any]",
                    "Expression { Clause { PropertyNavigation { saleItems [ any ] } } }"
                },
                new object[] {
                    "saleItems[any discount gt 0]",
                    "Expression { Clause { PropertyNavigation { saleItems [ any Expression { Clause { PropertyNavigation { discount } gt Value { 0 } } } ] } } }"
                },
                new object[] {
                    "saleItems[any (sale.date gt '2023-02-06')]",
                    "Expression { Clause { PropertyNavigation { saleItems [ any Expression { Clause { ( Expression { Clause { PropertyNavigation { sale . date } gt Value { ' 2023-02-06 ' } } } ) } } ] } } }"
                },
                new object[] {
                    "saleItems[all discount eq 0]",
                    "Expression { Clause { PropertyNavigation { saleItems [ all Expression { Clause { PropertyNavigation { discount } eq Value { 0 } } } ] } } }"
                },
                new object[] {
                    "books[first].name eq 'Der Steppenwolf'",
                    "Expression { Clause { PropertyNavigation { books [ first ] . name } eq Value { ' Der Steppenwolf ' } } }"
                },
                new object[] {
                    "books[last].price gt 10",
                    "Expression { Clause { PropertyNavigation { books [ last ] . price } gt Value { 10 } } }"
                },
                new object[] {
                    "books[first price gt 10].name lk 'er'",
                    "Expression { Clause { PropertyNavigation { books [ first Expression { Clause { PropertyNavigation { price } gt Value { 10 } } } ] . name } lk Value { ' er ' } } }"
                },
                new object[] {
                    "books[last price gt 10].name lk 'er'",
                    "Expression { Clause { PropertyNavigation { books [ last Expression { Clause { PropertyNavigation { price } gt Value { 10 } } } ] . name } lk Value { ' er ' } } }"
                },
                new object[] {
                    "items[sum finalprice] gt 25",
                    "Expression { Clause { PropertyNavigation { items [ sum PropertyNavigation { finalprice } ] } gt Value { 25 } } }"
                },
                new object[] {
                    "items[max finalprice] gt 12",
                    "Expression { Clause { PropertyNavigation { items [ max PropertyNavigation { finalprice } ] } gt Value { 12 } } }"
                },
                new object[] {
                    "items[min finalprice] lt 12",
                    "Expression { Clause { PropertyNavigation { items [ min PropertyNavigation { finalprice } ] } lt Value { 12 } } }"
                }
            };
        }

    }
}
