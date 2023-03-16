using NinjAPI.Helpers;
using NinjAPI.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Expressions
{
    public static class ExpressionExtensions
    {
        public static IQueryable<TEntity> Where<TEntity>(this IQueryable<TEntity> source, string query) where TEntity: class
        {
            throw new NotImplementedException();
        }

        public static Type GetExpressionReturnType(this Expression expression)
        {
            return expression switch
            {              
                MethodCallExpression methodCallExpression => methodCallExpression.Method.ReturnType,
                LambdaExpression lambdaExpression => lambdaExpression.ReturnType,
                _ => expression.Type,
            };
        }

        public static PropertyNavigationExpression AsNavigationExpression(this Expression source, Expression? nullPreventExpression = null)
        {
            return new PropertyNavigationExpression(source, nullPreventExpression);
        }

        public static Expression? ToPlainExpression(this PropertyNavigationExpression source)
        {
            if (source == null) return null;

            if (source.NullPreventExpression != null)
                return Expression.AndAlso(source.NullPreventExpression, source.NavigationExpression);

            return source.NavigationExpression;
        }
    }
}
