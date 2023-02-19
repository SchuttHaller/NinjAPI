using NinjAPI.Query;
using NinjAPI.Tests.Query.Mocks;
using System;
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
        private readonly ReadOnlyCollection<string> _validQueries = new(new string[] {
            //"revenue gt 100",
            //"revenue lt 200",
            //"id eq 1 and name lk 'pancho lopez'",
            //"(id ne 1 and id ne 3) or id gt 5",
            //"(id ne 1 and id ne 3) or id gt 5 and children[all(anotherid gt 0)]",
            //"id ne 1 and (id ne 3 or id gt 5) and children[any(name sw 'test')]",
            "$myDBFunction(id) gt 0",
            "$myDBFunction('string') gt 10",
            "$myDBFunction('string', 0, null) gt 10",
            "revenue gt $myDBFunction('string', 0, null)",
        });

        [TestMethod]
        public void ValidQuery_ReturnsTree()
        {
           foreach(var query in _validQueries)
            {
                var tokens = new QueryLexer(query).GetTokens();
                var tree = QueryParser.Parse(tokens);
                Assert.IsNotNull(tree);
            }
        }

        [TestMethod]
        public void InvalidQuery_ThrowsError()
        {

        }

    }
}
