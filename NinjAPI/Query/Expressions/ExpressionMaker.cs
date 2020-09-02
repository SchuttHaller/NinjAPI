using NinjAPI.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query.Expressions
{
    public static class ExpressionMaker
    {
        private static readonly MethodInfo QueryableWhereGeneric = GenericMethodOf(_ => Queryable.Where<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));
        private static readonly MethodInfo QueryableTakeGeneric = GenericMethodOf(_ => Queryable.Take<int>(default(IQueryable<int>), default(int)));
        private static readonly MethodInfo EnumerableTakeGeneric = GenericMethodOf(_ => Enumerable.Take<int>(default(IEnumerable<int>), default(int)));
        private static readonly MethodInfo QueryableSkipGeneric = GenericMethodOf(_ => Queryable.Skip<int>(default(IQueryable<int>), default(int)));
        private static readonly MethodInfo QueryableCountGeneric = GenericMethodOf(_ => Queryable.LongCount<int>(default(IQueryable<int>)));
        private static readonly MethodInfo CreateQueryGeneric = GetCreateQueryGenericMethod();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static MethodInfo GenericMethodOf<TReturn>(Expression<Func<object, TReturn>> expression)
        {
            return GenericMethodOf(expression as Expression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static MethodInfo GenericMethodOf(Expression expression)
        {
            LambdaExpression lambdaExpression = expression as LambdaExpression;

            Contract.Assert(expression.NodeType == ExpressionType.Lambda);
            Contract.Assert(lambdaExpression != null);
            Contract.Assert(lambdaExpression.Body.NodeType == ExpressionType.Call);

            return (lambdaExpression.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Order methods on IQueryable
        /// </summary>
        static readonly string[] orderMethods = new string[2] { "OrderBy", "ThenBy" };

        /// <summary>
        /// Sorts dynamically according to the parameters read from the 'order' field in a Collection request
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="query"></param>
        /// <param name="keys"></param>
        public static IQueryable OrderBy(this IQueryable query, Type entityType, params OrderKey[] keys)
        {
            //se puede ordernar maximo por dos columnas
            byte orderApplied = 0;
            ParameterExpression parameter = Expression.Parameter(entityType, "p");
            foreach (OrderKey key in keys)
            {
                if (!entityType.HasProperty(key.Property)) continue;
                PropertyInfo property = entityType.GetPropertyNoCase(key.Property);
                MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, property);
                LambdaExpression orderByExp = Expression.Lambda(propertyAccess, parameter);
                Type[] typeArguments = new Type[] { entityType, property.PropertyType };
                string methodName = orderMethods[orderApplied] + key.Direction;
                MethodCallExpression sortExpression = Expression.Call(typeof(Queryable), methodName, typeArguments, query.Expression, Expression.Quote(orderByExp));
                query = query.Provider.CreateQuery(sortExpression);
                orderApplied++;
                //just allow two columns;
                if (orderApplied > 1) break;
            }
            return query;
        }

        /// <summary>
        /// Filters dynamically according to the parameters read from the 'filter' field in Collection request
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="query"></param>
        /// <param name="columns"></param>
        public static IQueryable Where(this IQueryable query, Type entityType, IEnumerable<FilterKey[]> keys)
        {

            ParameterExpression parameter = Expression.Parameter(entityType, "p");
            var _lambdaParameters = new Dictionary<string, ParameterExpression>
            {
                { "p", parameter }
            };

            //ParameterExpression parameter = Expression.Parameter(entityType, "p");
            Expression body = null;

            if (keys.Count() > 0)
                body = keys.Select(x => x.ToExpressions(parameter, entityType).Aggregate((a, b) => MixExpressions(a, b, "or")))
                    .Aggregate((a, b) => MixExpressions(a, b, "and"));

            if (body != null)
            {
                LambdaExpression expression = Expression.Lambda(body, parameter);
                MethodInfo whereMethod = QueryableWhereGeneric.MakeGenericMethod(entityType);

                query = whereMethod.Invoke(null, new object[] { query, expression }) as IQueryable;
            }


            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="entityType"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IQueryable Take(this IQueryable query, Type entityType, int count)
        {
            Expression takeQuery = Take(query.Expression, count, entityType);
            var createMethod = CreateQueryGeneric.MakeGenericMethod(entityType);

            return createMethod.Invoke(query.Provider, new[] { takeQuery }) as IQueryable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="count"></param>
        /// <param name="entityType"></param>
        /// <param name="parameterize"></param>
        /// <returns></returns>
        public static IQueryable Skip(this IQueryable query, Type entityType, int count)
        {
            MethodInfo skipMethod = QueryableSkipGeneric.MakeGenericMethod(entityType);
            Expression skipValueExpression = Expression.Constant(count);
            Expression skipQuery = Expression.Call(null, skipMethod, new[] { query.Expression, skipValueExpression });

            var createMethod = CreateQueryGeneric.MakeGenericMethod(entityType);

            return createMethod.Invoke(query.Provider, new[] { skipQuery }) as IQueryable;
        }

        public static Func<long> Count(this IQueryable query, Type type)
        {
            MethodInfo countMethod = QueryableCountGeneric.MakeGenericMethod(type);
            long func() => (long)countMethod.Invoke(null, new object[] { query });
            return func;
        }

        /// <summary>
        /// Mixes left and right expressions with a logical operatos
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="logicOperator"></param>
        public static Expression MixExpressions(Expression left, Expression right, string logicOperator)
        {
            switch (logicOperator)
            {
                case "lk":
                    return Expression.Call(left, left.Type.GetMethod("Contains", new[] { typeof(string) }), right);
                case "eq":
                    return Expression.Equal(left, right);
                case "ne":
                    return Expression.NotEqual(left, right);
                case "gt":
                    return Expression.GreaterThan(left, right);
                case "ge":
                    return Expression.GreaterThanOrEqual(left, right);
                case "lt":
                    return Expression.LessThan(left, right);
                case "le":
                    return Expression.LessThanOrEqual(left, right);
                case "or":
                    return Expression.OrElse(left, right);
                case "and":
                default:
                    return Expression.AndAlso(left, right);
            }
        }

        /// <summary>
        /// Transforms value from filter field to its property type and make a expression constant value
        /// </summary>
        /// <param name="propertyType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Expression GetConstantValue(Type propertyType, string value)
        {
            Expression result = null;
            try
            {
                if (propertyType == typeof(string))
                    result = Expression.Constant(value, propertyType);
                else
                {

                    Type t = Nullable.GetUnderlyingType(propertyType);
                    if (t != null)
                        result = Expression.Convert(Expression.Constant(Convert.ChangeType(value, t), t), propertyType);
                    else
                        result = Expression.Constant(Convert.ChangeType(value, propertyType), propertyType);
                }

            }
            catch
            {
                //Do nothing in catch, so we return null [?have to be a better aproach]
            }

            return result;
        }

        /// <summary>
        /// Create expression from filter keys
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="parameter"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<Expression> ToExpressions(this IEnumerable<FilterKey> keys, ParameterExpression parameter, Type type)
        {
            MemberExpression left = null;
            Expression right = null;

            var result = keys.Select(key =>
            {
                if (!type.HasProperty(key.Property)) return null;
                PropertyInfo property = type.GetPropertyNoCase(key.Property);
                // property to the left
                left = Expression.Property(parameter, key.Property);
                // value to the right
                right = GetConstantValue(property.PropertyType, key.Value.Replace("\'", ""));
                // expression
                return MixExpressions(left, right, key.LogicalOperator);
            });

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <param name="elementType"></param>
        /// <param name="parameterize"></param>
        /// <returns></returns>
        public static Expression Take(Expression source, int count, Type elementType)
        {
            MethodInfo takeMethod;
            if (typeof(IQueryable).IsAssignableFrom(source.Type))
            {
                takeMethod = QueryableTakeGeneric.MakeGenericMethod(elementType);
            }
            else
            {
                takeMethod = EnumerableTakeGeneric.MakeGenericMethod(elementType);
            }

            Expression takeValueExpression = Expression.Constant(count);
            Expression takeQuery = Expression.Call(null, takeMethod, new[] { source, takeValueExpression });
            return takeQuery;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static MethodInfo GetCreateQueryGenericMethod()
        {
            return typeof(IQueryProvider).GetTypeInfo()
                .GetDeclaredMethods("CreateQuery")
                .Where(m => m.IsGenericMethod)
                .FirstOrDefault();
        }
    }
}
