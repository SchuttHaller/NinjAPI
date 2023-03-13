using NinjAPI.Errors;
using NinjAPI.Helpers;
using NinjAPI.Query;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using O = NinjAPI.Query.Operators;

namespace NinjAPI.Expressions
{
    public abstract class ExpressionBinder<TEntity> where TEntity: class
    {
        protected readonly ParameterExpression EntityParameter;

        protected static readonly MethodInfo FirstGeneric = GenericMethodOf(_ => Enumerable.FirstOrDefault(default(IEnumerable<object>)!, i => true))!;
        protected static readonly MethodInfo FirstNoParamsGeneric = GenericMethodOf(_ => Enumerable.FirstOrDefault(default(IEnumerable<object>)!))!;
        protected static readonly MethodInfo LastGeneric = GenericMethodOf(_ => Enumerable.LastOrDefault(default(IEnumerable<object>)!, i => true))!;
        protected static readonly MethodInfo LastNoParamsGeneric = GenericMethodOf(_ => Enumerable.LastOrDefault(default(IEnumerable<object>)!))!;
        protected static readonly MethodInfo AnyGeneric = GenericMethodOf(_ => Enumerable.Any(default!, default(Func<object, bool>)!))!;
        protected static readonly MethodInfo AnyNoParamsGeneric = GenericMethodOf(_ => Enumerable.Any(default(IEnumerable<object>)!))!;
        protected static readonly MethodInfo AllGeneric = GenericMethodOf(_ => Enumerable.All(default!, default(Func<object, bool>)!))!;
        protected static readonly MethodInfo SumGeneric = GenericMethodOf(_ => Enumerable.Sum(default!, default(Func<object, decimal>)!))!;
        protected static readonly MethodInfo MinGeneric = GenericMethodOf(_ => Enumerable.Min(default!, default(Func<object, object>)!))!;
        protected static readonly MethodInfo MaxGeneric = GenericMethodOf(_ => Enumerable.Max(default!, default(Func<object, object>)!))!;
        public ExpressionBinder()
        {
            EntityParameter = Expression.Parameter(EntityType, "x");
        }

        public abstract Expression BindExpression(string expressionStr);

        protected Type EntityType => typeof(TEntity);

        protected Expression CreateExpression(QueryNode node, Expression? aggregate = null)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Token.Type != TokenType.Expression) throw new InvalidTokenException(node.Token);

            aggregate ??= EntityParameter;

            var clauseNode = node.GetChild(TokenType.Clause)!;
            var predicateNode = node.GetChild(TokenType.ExpressionPredicate)!;

            var clause = CreateClauseExpression(clauseNode, aggregate);
            var predicate = CreatePredicateExpression(predicateNode, out QueryToken? logicalOperator, aggregate);

            if (predicate != null)
                return ConditionalExpression(clause, predicate, logicalOperator!);

            return clause;
        }

        private Expression? CreatePredicateExpression(QueryNode node, out QueryToken? logicalOperator, Expression? aggregate = null)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Token.Type != TokenType.ExpressionPredicate) throw new InvalidTokenException(node.Token);

            if (!node.Children.Any())
            {
                logicalOperator = null;
                return null;
            }

            logicalOperator = node.GetChild(TokenType.LogicalOperator)!.Token as QueryToken;
            var expressionNode = node.GetChild(TokenType.Expression)!;

            return CreateExpression(expressionNode, aggregate);
        }

        private Expression CreateClauseExpression(QueryNode node, Expression aggregate)
        {
            // check if clause produce an Expression type node
            var eNode = node.GetChild(TokenType.Expression);
            if (eNode != null)
                return CreateExpression(eNode, aggregate);

            // create left side of the expression
            var leftNode = node.GetChild(TokenType.Left)!;
            var left = Left(leftNode, aggregate);

            // create predicate
            var predicateNode = node.GetChild(TokenType.ClausePredicate)!;

            // if no clause predicate, then return only the left side
            if (!predicateNode.Children.Any())
                return left;

            var predicate = CreateClausePredicate(predicateNode, left, out QueryToken clauseOperator);
            return ComparisonExpression(left, predicate!, clauseOperator!);
        }

        private Expression Left(QueryNode node, Expression aggregate)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Type != TokenType.Left) throw new InvalidTokenException(node.Token);
            if (node.Type != TokenType.Left) throw new InvalidTokenException(node.Token, "Token has no children");

            // check child node type
            var childNode = node.Children.FirstOrDefault()!;

            if (childNode.Type == TokenType.PropertyNavigation)
            {
                return PropertyNavigation(childNode, aggregate);
            }
               

            return UserDefinedFunctionCall(childNode, aggregate);
        }

        private Expression? CreateClausePredicate(QueryNode node, Expression navigationExp, out QueryToken operatorToken)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Token.Type != TokenType.ClausePredicate) throw new InvalidTokenException(node.Token);          

            // get type of identifier
            var navExpType = navigationExp.GetExpressionReturnType();

            // get operator
            var operatorNode = node.GetChild(TokenType.ComparisionOperator)!;
            operatorToken = (operatorNode.Token as QueryToken)!;
            // validate operator
            if (!IsValidOperator(navExpType, operatorToken.Value))
            {
                throw new InvalidOperatorException(operatorToken.Value, navigationExp.ToString(), navExpType);
            }

            var valueNode = node.GetChild(TokenType.Right)!;

            return Right(valueNode, navExpType, navigationExp);
        }

        private Expression UserDefinedFunctionCall(QueryNode node, Expression aggregate)
        {
            throw new NotImplementedException();
        }

        private Expression Right(QueryNode node, Type comparisonType, Expression aggregate)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Type != TokenType.Right) throw new InvalidTokenException(node.Token);
            if (node.Type != TokenType.Right) throw new InvalidTokenException(node.Token, "Token has no children");

            // check child node type
            var childNode = node.Children.FirstOrDefault()!;

            if (childNode.Type == TokenType.Value)
                return ValueExpression(childNode, comparisonType);

            return UserDefinedFunctionCall(childNode, aggregate);
        }

        private Expression ValueExpression(QueryNode node, Type comparisonType)
        {

            var valueNode = node.GetValueTypedChild()!;

            if(valueNode.Type == TokenType.Null)
            {
                return Expression.Constant(comparisonType.GetNullValueForType());
            }

            var valueToken = (valueNode.Token as QueryToken)!;
            
            // validate constant type
            if (!valueToken.Value.TryChangeType(comparisonType, out object constantValue))
            {
                throw new InvalidCastException($"Value '{valueToken.Value}' cannot be cast to type '{comparisonType}'");
            }

            return Expression.Constant(constantValue);
        }

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

        protected Expression FunctionExpression(Expression navigation, QueryNode functionNode, out QueryNode? navigationContinueNode)
        {
            var predicateExpression = CreateFunctionPredicateExpression(functionNode, navigation.GetExpressionReturnType(), out QueryToken functionOperator);

            if (!navigation.GetExpressionReturnType().TryGetTypeFromEnumerable(out Type? type))
            {
                throw new ArgumentException($"Invalid function call '{functionOperator.Value}'  for type: {navigation.GetExpressionReturnType()}");
            }

            navigationContinueNode = functionNode
                .GetChild(TokenType.PropertyNavigationChain)?
                .GetChild(TokenType.PropertyNavigation);

            MethodInfo method;
            switch (functionOperator.Value)
            {
                case O.Any:
                    if(predicateExpression == null)
                    {
                        method = AnyNoParamsGeneric.MakeGenericMethod(type!);
                        return Expression.Call(method, navigation);
                    }
                    else
                    {
                        method = AnyGeneric.MakeGenericMethod(type!);
                        return Expression.Call(method, navigation, predicateExpression);
                    }    
                   case O.All:
                    method = AllGeneric.MakeGenericMethod(type!);
                    return Expression.Call(method, navigation, predicateExpression!);
                case O.Min:
                    method = MinGeneric.MakeGenericMethod(type!, predicateExpression!.GetExpressionReturnType());
                    return Expression.Call(method, navigation, predicateExpression!);
                case O.Max:
                    method = MaxGeneric.MakeGenericMethod(type!, predicateExpression!.GetExpressionReturnType());
                    return Expression.Call(method, navigation, predicateExpression!);
                case O.Sum:
                    method = SumGeneric.MakeGenericMethod(type!);
                    return Expression.Call(method, navigation, predicateExpression!);
                case O.First:
                    if (predicateExpression == null)
                    {
                        method = FirstNoParamsGeneric.MakeGenericMethod(type!);
                        return Expression.Call(method, navigation);
                    }
                    else
                    {
                        method = FirstGeneric.MakeGenericMethod(type!);
                        return Expression.Call(method, navigation, predicateExpression);
                    }
                case O.Last:
                    if (predicateExpression == null)
                    {
                        method = LastNoParamsGeneric.MakeGenericMethod(type!);
                        return Expression.Call(method, navigation);
                    }
                    else
                    {
                        method = LastGeneric.MakeGenericMethod(type!);
                        return Expression.Call(method, navigation, predicateExpression);
                    }
                default: throw new ArgumentException($"Invalid comparison operator: {functionOperator.Value}");
            }

        }

        protected Expression PropertyNavigation(QueryNode node, Expression navigationExp)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Token == null) throw new ArgumentNullException(nameof(node), "Token cannot be null");
            if (node.Type != TokenType.PropertyNavigation) throw new ArgumentException("Token must be a valid identifier", nameof(node));
            if (!node.Children.Any()) throw new ArgumentException("node must have children", nameof(node));


            Type type = navigationExp.GetExpressionReturnType();

            var propertyKey = node.GetChild(TokenType.Identifier)!.Token as QueryToken;
            var property = GetEntityProperty(type, propertyKey!.Value);
            var propertyExpression = Expression.Property(navigationExp, property);

            var aggregate = node.GetChild(TokenType.PropertyNavigationAggregate)!;

            if (!aggregate.Children.Any())
                return propertyExpression;


            if (aggregate.Children.Contains(TokenType.PropertyNavigationChain))
            {
                var navigation = aggregate.GetChild(TokenType.PropertyNavigationChain)!.GetChild(TokenType.PropertyNavigation);
                return PropertyNavigation(navigation!, propertyExpression);
            }


            if (aggregate.Children.Contains(TokenType.Function))
            {
                var fNode = aggregate.GetChild(TokenType.Function)!;             
                var functionExp = FunctionExpression(propertyExpression, fNode, out QueryNode? navigationContinue);
                
                if (navigationContinue != null)
                    return PropertyNavigation(navigationContinue, functionExp);
                
                return functionExp;
            }

            throw new ArgumentException($"Invalid aggregate: { aggregate }", nameof(node));
        }

        protected Expression? CreateFunctionPredicateExpression(QueryNode node, Type navExpType, out QueryToken functionOperator)
        {
            // get operator
            var operatorNode = node.GetFunctionChild()!;
            functionOperator = (operatorNode.Token as QueryToken)!;

            if (!navExpType.TryGetTypeFromEnumerable(out Type? paramType))
            {
                throw new ArgumentException($"Invalid function call '{functionOperator.Value}'  for type: {navExpType}");
            }

            var expNode = node.GetChild(TokenType.Expression)
                          ?? node.GetChild(TokenType.NullableExpression)?.GetChild(TokenType.Expression);

            if(expNode != null)
            {
                var param = Expression.Parameter(paramType!, "y");
                return Expression.Lambda(CreateExpression(expNode, param), param);
            }

            var propNode = node.GetChild(TokenType.PropertyNavigation);
            if(propNode != null)
            {
                var param = Expression.Parameter(paramType!, "y");
                return Expression.Lambda(PropertyNavigation(propNode, param), param);
            }

            return null;
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

        private static MethodInfo? GenericMethodOf<TReturn>(Expression<Func<object, TReturn>> expression)
        {
            return GenericMethodOf(expression as LambdaExpression);
        }

        private static MethodInfo? GenericMethodOf(LambdaExpression lambdaExpression)
        {
            return (lambdaExpression.Body as MethodCallExpression)?.Method.GetGenericMethodDefinition();
        }

        private static bool IsValidOperator(Type type, string op)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return OperatorsByType.ContainsKey(type) && OperatorsByType[type].Contains(op);
        }

        private static readonly string[] CommonOperators =
            new string[] {
                O.Equal,
                O.NotEqual,
                O.GreaterThan,
                O.GreaterOrEqual,
                O.LessThan,
                O.LessOrEqual
            };

        private static readonly string[] EqualityOperators =
            new string[] {
                O.Equal,
                O.NotEqual
            };

        private static readonly string[] StringOperators =
            new string[] {
                O.Equal,
                O.NotEqual,
                O.Like,
                O.StartsWith,
                O.EndsWith
            };

        private static readonly Dictionary<Type, string[]> OperatorsByType = new()
        {
            { typeof(char), CommonOperators },
            { typeof(byte), CommonOperators },
            { typeof(short), CommonOperators },
            { typeof(int), CommonOperators },
            { typeof(long), CommonOperators },
            { typeof(double), CommonOperators },
            { typeof(decimal), CommonOperators },
            { typeof(float), CommonOperators },
            { typeof(DateTime), CommonOperators },
            { typeof(DateTimeOffset), CommonOperators },
            { typeof(TimeSpan), CommonOperators },
            { typeof(Guid),  EqualityOperators },
            { typeof(bool),  EqualityOperators },
            { typeof(string),  StringOperators },
        };
    }
}
