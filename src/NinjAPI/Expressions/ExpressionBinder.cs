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
    internal abstract class ExpressionBinder<TEntity> where TEntity: class
    {
        protected static Type EntityType => typeof(TEntity);

        private static Expression PropertyNavigation(QueryNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Token == null) throw new ArgumentNullException(nameof(node.Token));
            if (node.Token is not QueryToken) throw new ArgumentException("token must be a valid identifier", nameof(node.Token));

            var navigationExp = Expression.Parameter(EntityType);

            return PropertyNavigation(navigationExp, (node.Token as QueryToken)!.Value);
        }

        private static Expression PropertyNavigation(Expression navigation, string identifier)
        {
            Type type = navigation.GetExpressionReturnType();
            var idPieces = identifier.Split('.', 2);
            var propertyKey = idPieces[0];

            var property = GetEntityProperty(type, propertyKey);
            var propertyExpression = Expression.Property(navigation, property);
  
            if (idPieces.Length > 1)
                return PropertyNavigation(propertyExpression, idPieces[1]);

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
