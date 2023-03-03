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
            WhereExpressionBinderTestHelper(GetBasicTestData<Author>());
            WhereExpressionBinderTestHelper(GetBasicTestData<Sale>());
        }

        [TestMethod]
        public void Filter_WithChildAccessClause_ReturnsResult()
        {
            WhereExpressionBinderTestHelper(GetChildAccessTestData<Book>());
            WhereExpressionBinderTestHelper(GetChildAccessTestData<SaleItem>());
        }

        [TestMethod]
        public void Filter_WithComplexClause_ReturnsResult()
        {
            WhereExpressionBinderTestHelper(GetComplexTestData<Book>());
            WhereExpressionBinderTestHelper(GetComplexTestData<SaleItem>());
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

        private static Dictionary<string, Func<T, bool>> GetChildAccessTestData<T>()
        {
            if (typeof(T) == typeof(Book))
            {
                return (ChildAccessFilterForBookModel as Dictionary<string, Func<T, bool>>)!;
            }

            if (typeof(T) == typeof(SaleItem))
            {
                return (ChildAccessFilterForSaleItemModel as Dictionary<string, Func<T, bool>>)!;
            }

            throw new Exception($"Unsoported type {typeof(T)}");
        }

        private static Dictionary<string, Func<T, bool>> GetComplexTestData<T>()
        {
            if (typeof(T) == typeof(Book))
            {
                return (ComplexFilterForBookModel as Dictionary<string, Func<T, bool>>)!;
            }

            if (typeof(T) == typeof(SaleItem))
            {
                return (ComplexFilterForSaleItemModel as Dictionary<string, Func<T, bool>>)!;
            }

            throw new Exception($"Unsoported type {typeof(T)}");
        }

        #region Basic Expressions Data

        private static readonly Dictionary<string, Func<Book, bool>> BasicFilterForBookModel = new()
        {
            { "name eq 'For Whom the Bell Tolls'", x => x.Name == "For Whom the Bell Tolls" },
            { "name lk 'Bell'", x => x.Name.Contains("Bell") },
            { "stock gt 10", x => x.Stock > 10 },
            { "stock lt 12", x => x.Stock < 12 },
            { "id eq '11521b88-4d59-4bba-9083-a5299f2cf41f'", x => x.Id == new Guid("11521b88-4d59-4bba-9083-a5299f2cf41f") },
            { "id ne 11521b88-4d59-4bba-9083-a5299f2cf41f", x => x.Id != new Guid("11521b88-4d59-4bba-9083-a5299f2cf41f") }
        };

        private static readonly Dictionary<string, Func<Author, bool>> BasicFilterForAuthorModel = new()
        {
            { "name lk 'erm'", x => x.Name.Contains("erm") },
            { "birthdate eq '1917-05-16'", x => x.BirthDate == new DateTime(1917,05,16) }
        };

        private static readonly Dictionary<string, Func<Sale, bool>> BasicFilterForSaleModel = new()
        {
            { "date lt '2023-02-06T18:10:45'", x => x.Date < new DateTime(2023,2,6,18,10,45) },
            { "date le '2023-02-06T18:10:45'", x => x.Date <= new DateTime(2023,2,6,18,10,45) },
            { "date gt '2023-02-06T09:10:45'", x => x.Date > new DateTime(2023,2,6,9,10,45) },
            { "date ge '2023-02-06T09:10:45'", x => x.Date >= new DateTime(2023,2,6,9,10,45) }
        };

        #endregion

        #region Child Access Expressions Data
        private static readonly Dictionary<string, Func<Book, bool>> ChildAccessFilterForBookModel = new ()
        {
            { "author.name eq 'Hermann Hesse'", x => x.Author.Name == "Hermann Hesse" },
            { "author.name lk 'er'", x => x.Author.Name.Contains("er") },
            { "author.name sw 'er'", x => x.Author.Name.StartsWith("er") },
        };

        private static readonly Dictionary<string, Func<SaleItem, bool>> ChildAccessFilterForSaleItemModel = new()
        {
            { "sale.date lt '2023-02-06T18:10:45'", x => x.Sale.Date < new DateTime(2023,2,6,18,10,45) },
            { "sale.date le '2023-02-06T18:10:45'", x => x.Sale.Date <= new DateTime(2023,2,6,18,10,45) },
            { "sale.date gt '2023-02-06T09:10:45'", x => x.Sale.Date > new DateTime(2023,2,6,9,10,45) },
            { "sale.date ge '2023-02-06T09:10:45'", x => x.Sale.Date >= new DateTime(2023,2,6,9,10,45) },
            { "book.name eq 'For Whom the Bell Tolls'", x => x.Book.Name == "For Whom the Bell Tolls" },
            { "book.name lk 'Bell'", x => x.Book.Name.Contains("Bell") },
            { "book.stock gt 10", x => x.Book.Stock > 10 },
            { "book.stock lt 12", x => x.Book.Stock < 12 },
            { "book.author.name lk 'erm'", x => x.Book.Author.Name.Contains("erm") },
            { "book.author.birthdate eq '1917-05-16'", x => x.Book.Author.BirthDate == new DateTime(1917,05,16) }
        };
        #endregion

        #region Function Call Expressions Data
        #endregion

        #region Complex Expressions Data
        private static readonly Dictionary<string, Func<Book, bool>> ComplexFilterForBookModel = new()
        {
            { "name lk 'er' and author.name sw 'Fe'", x => x.Name.Contains("er") && x.Author.Name.StartsWith("Fe") },
            { "name lk 'er' or name lk 'ar' and author.name sw 'Fe'", x => x.Name.Contains("er") || x.Name.Contains("ar") && x.Author.Name.StartsWith("Fe") },
            { "author.name sw 'Fe' and name lk 'er' or name lk 'ar'", x => x.Author.Name.StartsWith("Fe") && (x.Name.Contains("er") || x.Name.Contains("ar")) },
            { "(author.name sw 'Fe' and name lk 'er') or name lk 'ar'", x => (x.Author.Name.StartsWith("Fe") && x.Name.Contains("er")) || x.Name.Contains("ar") }
        };

        private static readonly Dictionary<string, Func<SaleItem, bool>> ComplexFilterForSaleItemModel = new()
        {
            { "finalprice gt 10 and sale.date gt '2023-01-12' and sale.date lt '2023-01-13'", x => x.FinalPrice > 10 && x.Sale.Date > new DateTime(2023,1,12) && x.Sale.Date < new DateTime(2023,1,13) },
            { "discount gt 2 or finalprice eq 9 and discount eq 0", x => x.Discount > 2 || (x.FinalPrice == 9 && x.Discount == 0) },
            { "(discount gt 2 or finalprice eq 9) and discount eq 0", x => (x.Discount > 2 || x.FinalPrice == 9) && x.Discount == 0 },
            { "(finalprice gt 13 and sale.date gt '2023-02-06') or (discount gt 2 and finalprice lt 9)", x => (x.FinalPrice > 13 && x.Sale.Date > new DateTime(2023,2,6)) || (x.Discount > 2 &&  x.FinalPrice < 9) }
        };
        #endregion

        #region User-Defined Function Call Expressions Data
        #endregion
    }
}
