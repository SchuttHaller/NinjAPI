using NinjAPI.Query;
using System.Linq.Expressions;

namespace NinjAPI.Expressions
{
    public sealed class WhereExpressionBinder<TEntity> : ExpressionBinder<TEntity> where TEntity : class
    {
        public override Expression<Func<TEntity, bool>> BindExpression(string query)
        {
            var parameter = EntityParameter;
            var queryTree = new QueryLexer(query).GetTokens().Parse();
            var body = CreateExpression(queryTree, parameter);

            return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        }       
    }
}
