using NinjAPI.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Expressions
{
    internal class FilterBinder<TEntity> : ExpressionBinder<TEntity> where TEntity : class
    {
        internal static Expression BindFilterExpression(string query)
        {
            var parameter = Expression.Parameter(EntityType);

            var queryTree = new QueryLexer(query).GetTokens().Parse();

            Expression expression = null;
            QueryNode? currentNode = queryTree;

            while (queryTree.Children.Any())
            {
                currentNode = queryTree.Children.FirstOrDefault(n => n.Token.Type == TokenType.Clause);


            }

            return expression;
        }

        private static Expression CreateClauseExpression(QueryNode node) 
        {
            throw new NotImplementedException();
        }
    }
}
