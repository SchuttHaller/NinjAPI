﻿using NinjAPI.Query;
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
                MemberExpression memberExpression => memberExpression.Member.DeclaringType!,
                MethodCallExpression methodCallExpression => methodCallExpression.Method.ReturnType,
                _ => throw new NotSupportedException(),
            };
        }
    }
}