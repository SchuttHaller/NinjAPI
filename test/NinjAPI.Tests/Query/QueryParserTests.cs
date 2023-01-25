using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Tests.Query
{
    [TestClass]
    public class QueryParserTests
    {
        [TestMethod]
        [DataRow("id eq 10")]
        [DataRow("description lk 'its my room'")]
        [DataRow("(price gt 50 and date ge 2022-01-01) or price lt 10")]
        public void ValidQuery_IsValid(string query)
        {

        }

        [TestMethod]
        [DataRow("id eq 10()")]
        [DataRow("description lk 'its my room' eq")]
        [DataRow("()))))price gt 50 and date ge 2022-01-01) or price lt 10 price")]
        public void InvalidQuery_ThrowsError()
        {

        }
    }
}
