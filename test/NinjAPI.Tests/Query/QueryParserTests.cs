using NinjAPI.Query;
using NinjAPI.Tests.Query.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Tests.Query
{
    [TestClass]
    public class QueryParserTests
    {
        private readonly IEnumerable<(string str, Expression<Func<MockEntity, bool>> exp)> _validQueries;
        public QueryParserTests()
        {
            _validQueries = new (string str, Expression<Func<MockEntity, bool>> exp)[]
            {
                ("revenue gt 100", e => e.Revenue >= 100),
                ("revenue lt 200", e => e.Revenue <= 200),
                ("id eq 1 and name lk 'pancho lopez'", e => e.Id == 1 && e.Name!.Contains("pancho lopez")),
                ("(id ne 1 and id ne 3) or id gt 5", e => (e.Id != 1 && e.Id != 3) || e.Id > 5),
                ("(id ne 1 and id ne 3) or id gt 5 and children[all(anotherid gt 0)]", e => (e.Id != 1 && e.Id != 3) || e.Id > 5 && e.Children!.All(c => c.AnotherId > 0)),
                ("id ne 1 and (id ne 3 or id gt 5) and children[any(name sw 'test')]", e => e.Id != 1 && (e.Id != 3 || e.Id > 5) && e.Children!.Any(c => c.Name!.StartsWith("test"))),
            };
        }

        [TestMethod]
        public void ValidQuery_ReturnsTree()
        {
           foreach(var query in _validQueries)
            {
                var tokens = new QueryLexer(query.str).GetTokens();
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
