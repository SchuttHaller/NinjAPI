using NinjAPI.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Expressions
{
    public abstract class ExpressionBinder<TEntity> where TEntity: class
    {
        protected readonly ParameterExpression EntityParameter;

        public ExpressionBinder()
        {
            EntityParameter = Expression.Parameter(EntityType, "x");
        }

        public abstract Expression BindExpression(string expressionStr);

        protected Type EntityType => typeof(TEntity);

        protected Expression ConditionalExpression(Expression left, Expression rigth, QueryToken logicalOperator)
        {
            return logicalOperator.Value switch
            {
                Operators.And => Expression.AndAlso(left, rigth),
                Operators.Or => Expression.OrElse(left, rigth),
                _ => throw new ArgumentException($"Invalid logical operator: {logicalOperator.Value}"),
            };
        }

        protected Expression ComparisonExpression(Expression left, Expression rigth, QueryToken comparisonOperator)
        {
            return comparisonOperator.Value switch
            {
                Operators.Equal => Expression.Equal(left, rigth),
                Operators.NotEqual => Expression.NotEqual(left, rigth),
                Operators.GreaterThan => Expression.GreaterThan(left, rigth),
                Operators.GreaterOrEqual => Expression.GreaterThanOrEqual(left, rigth),
                Operators.LessThan => Expression.LessThan(left, rigth),
                Operators.LessOrEqual => Expression.LessThanOrEqual(left, rigth),
                Operators.Like => Expression.Call(left, left.Type.GetMethod("Contains", new[] { typeof(string)} )!, rigth),
                Operators.StartsWith => Expression.Call(left, left.Type.GetMethod("StartsWith", new[] { typeof(string) })!, rigth),
                Operators.EndsWith => Expression.Call(left, left.Type.GetMethod("EndsWith", new[] { typeof(string) })!, rigth),
                _ => throw new ArgumentException($"Invalid comparison operator: {comparisonOperator.Value}"),
            };
        }

        protected Expression PropertyNavigation(QueryNode node, Expression navigationExp)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Token == null) throw new ArgumentNullException(nameof(node), "Token cannot be null");
            if (node.Type != TokenType.PropertyNavigation) throw new ArgumentException("Token must be a valid identifier", nameof(node));
            if (!node.Children.Any()) throw new ArgumentException("node must have children", nameof(node));


            Type type = navigationExp.GetExpressionReturnType();

            var propertyKey = node.GetChildByType(TokenType.Identifier)!.Token as QueryToken;
            var aggregate = node.GetChildByType(TokenType.PropertyNavigationAggregate);

            var property = GetEntityProperty(type, propertyKey!.Value);
            var propertyExpression = Expression.Property(navigationExp, property);

            if (aggregate!.Children.Any())
                return PropertyNavigation(aggregate.GetChildByType(TokenType.PropertyNavigation)!, propertyExpression);

            return propertyExpression;
        }

        private static PropertyInfo GetEntityProperty(Type type, string propertyName)
        {
            return GetPropertyByColumnAttr(type, propertyName) 
                ?? GetPropertyByName(type, propertyName)
                ?? throw new ArgumentException($"Property {propertyName} not found in type {type.Name}");
        }

        private static PropertyInfo? GetPropertyByName(Type type, string propertyName)
        {
            return type?.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        }

        private static PropertyInfo? GetPropertyByColumnAttr(Type type, string columnName)
        {
            return type?.GetProperties().FirstOrDefault(p =>
                p.GetCustomAttributes(typeof(ColumnAttribute)).Any(attr => ((ColumnAttribute)attr).Name?.ToLower() == columnName.ToLower()));
        }
    }
}
