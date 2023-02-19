using NinjAPI.Errors;
using NinjAPI.Helpers;
using NinjAPI.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using O = NinjAPI.Query.Operators;

namespace NinjAPI.Expressions
{
    public class WhereExpressionBinder<TEntity> : ExpressionBinder<TEntity> where TEntity : class
    {
        public override Expression BindExpression(string query)
        {
            var parameter = Expression.Parameter(EntityType);
            var queryTree = new QueryLexer(query).GetTokens().Parse();
            
            return CreateExpression(queryTree, parameter);
        }

        private Expression CreateExpression(QueryNode node, Expression? aggregate = null)
        {
            if(node == null) throw new ArgumentNullException(nameof(node));
            if(node.Token.Type != TokenType.Expression) throw new InvalidTokenException(node.Token);

            aggregate ??= Expression.Parameter(EntityType);

            var clauseNode = node.Children.First(n => n.Type == TokenType.Clause);
            var predicateNode = node.Children.First(n => n.Type == TokenType.ExpressionPredicate);

            var clause = CreateClauseExpression(clauseNode, aggregate);
            var predicate = CreatePredicateExpression(predicateNode, out QueryToken? logicalOperator);

            if (predicate != null)
                return ConditionalExpression(clause, predicate, logicalOperator!);
            
            return clause;
        }

        private Expression? CreatePredicateExpression(QueryNode node, out QueryToken? logicalOperator)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Token.Type != TokenType.ExpressionPredicate) throw new InvalidTokenException(node.Token);

            if (!node.Children.Any())
            {
                logicalOperator = null;
                return null;
            }

            logicalOperator = node.Children.First(n => n.Type == TokenType.LogicalOperator).Token as QueryToken;
            var expressionNode = node.Children.First(n => n.Type == TokenType.Expression);

            return CreateExpression(expressionNode);
        }

        private Expression CreateClauseExpression(QueryNode node, Expression aggregate)
        {
            // check if clause produce an Expression type node
            var eNode = node.Children.FirstOrDefault(n => n.Type == TokenType.Expression);
            if (eNode != null)
                return CreateExpression(eNode, aggregate);

            // create navigation property
            var propertyNode = node.Children.First(n => n.Type == TokenType.Left);
            var navigation = PropertyNavigation(propertyNode, aggregate);

            // create predicate
            var predicateNode = node.Children.First(n => n.Type == TokenType.ClausePredicate);
            var predicate = CreateClausePredicate(predicateNode, navigation, out QueryToken? clauseOperator);

            return ComparisonExpression(navigation, predicate, clauseOperator!);
        }

        private Expression CreateClausePredicate(QueryNode node, Expression navigationExp, out QueryToken operatorToken)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Token.Type != TokenType.ClausePredicate) throw new InvalidTokenException(node.Token);

            // check if clause produce a Funtion type node
            var fNode = node.Children.FirstOrDefault(n => n.Type == TokenType.Function);
            if (fNode != null)
                return CreateFunctionExpression(fNode, out operatorToken);

            // get type of identifier
            var navExpType = navigationExp.GetExpressionReturnType();
            // get operator
            var operatorNode = node.Children.First(n => n.Type == TokenType.ComparisionOperator);
            operatorToken = (operatorNode.Token as QueryToken)!;
            // validate operator
            if(!IsValidOperator(navExpType, operatorToken.Value))
            {
                throw new InvalidOperatorException(operatorToken.Value, navigationExp.ToString(), navExpType);
            }

            // get constant
            var constantNode = node.Children.First(n => n.Type == TokenType.Constant);
            var constantToken = (constantNode.Token as QueryToken)!;
            // validate constant type
            if(!constantToken.Value.TryChangeType(navExpType, out object constantValue))
            {
                throw new InvalidCastException($"Value '{constantToken.Value}' cannot be cast to type '{navExpType}'");
            }

            return Expression.Constant(constantValue);
        }

        private Expression CreateFunctionExpression(QueryNode node, out QueryToken functionOperator)
        {
            throw new NotImplementedException();
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
                O.NotEqual
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
