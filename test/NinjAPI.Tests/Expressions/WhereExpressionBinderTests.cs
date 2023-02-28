using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NinjAPI.Expressions;
using NinjAPI.Query;
using NinjAPI.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NinjAPI.Tests.Expressions
{
    [TestClass]
    public class WhereExpressionBinderTests: SqliteInMemoryTests
    {
        [TestMethod]
        public void Filter_WithBasicClause_ReturnsResult()
        {
            WhereExpressionBinderTestHelper(GetBasicTestData<Book>());
            //WhereExpressionBinderTestHelper(GetBasicTestData<Author>());
            //WhereExpressionBinderTestHelper(GetBasicTestData<Sale>());
        }


        private void WhereExpressionBinderTestHelper<T>(Dictionary<string, Func<T, bool>> testData) where T : class         {
            using var context = CreateContext();
            var dataSet = context.Set<T>();

            foreach(var item in testData)
            {
                var expression = new WhereExpressionBinder<T>().BindExpression(item.Key);
                // result from parsed query
                var result = dataSet.Where(expression.Compile()).ToArray();
                //expected
                var exp = dataSet.Where(item.Value).ToArray();

                Assert.AreEqual(exp.Length, result.Length);
                for(int i = 0; i < result.Length; i++)
                {
                    var itemResult = result[i];
                    var itemExpected = exp[i];

                    Assert.AreEqual(itemExpected, itemResult);
                }

            }

        }


        private static Dictionary<string, Func<T, bool>> GetBasicTestData<T>()
        {
            if(typeof(T) == typeof(Book))
            {
                return (BasicFilterForBookModel as Dictionary<string, Func<T, bool>>)!;
            }
            
            if(typeof(T) == typeof(Author))
            {
                return (BasicFilterForAuthorModel as Dictionary<string, Func<T, bool>>)!;
            }

            if (typeof(T) == typeof(Sale))
            {
                return (BasicFilterForSaleModel as Dictionary<string, Func<T, bool>>)!;
            }

            throw new Exception($"Unsoported type {typeof(T)}");
        }

        private static readonly Dictionary<string, Func<Book, bool>> BasicFilterForBookModel = new()
        {
            { "name eq 'For Whom the Bell Tolls'", p => p.Name == "For Whom the Bell Tolls" }
        };
        private static readonly Dictionary<string, Func<Author, bool>> BasicFilterForAuthorModel = new()
        {
            { "name lk 'erm'", p => p.Name.Contains("erm") }
        };
        private static readonly Dictionary<string, Func<Sale, bool>> BasicFilterForSaleModel = new()
        {
            { "items[sum(finalprice)] gt 30", p => p.Items.Sum(i => i.FinalPrice) > 30 }
        };


    }
}
