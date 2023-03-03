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
using System.Xml.Linq;
using O = NinjAPI.Query.Operators;

namespace NinjAPI.Expressions
{
    public class WhereExpressionBinder<TEntity> : ExpressionBinder<TEntity> where TEntity : class
    {
        public override Expression<Func<TEntity, bool>> BindExpression(string query)
        {
            var parameter = EntityParameter;
            var queryTree = new QueryLexer(query).GetTokens().Parse();
            var body = CreateExpression(queryTree, parameter);

            return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        }

        private Expression CreateExpression(QueryNode node, Expression? aggregate = null)
        {
            if(node == null) throw new ArgumentNullException(nameof(node));
            if(node.Token.Type != TokenType.Expression) throw new InvalidTokenException(node.Token);

            aggregate ??= EntityParameter;

            var clauseNode = node.GetChildByType(TokenType.Clause)!;
            var predicateNode = node.GetChildByType(TokenType.ExpressionPredicate)!;

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

            logicalOperator = node.GetChildByType(TokenType.LogicalOperator)!.Token as QueryToken;
            var expressionNode = node.GetChildByType(TokenType.Expression)!;

            return CreateExpression(expressionNode);
        }

        private Expression CreateClauseExpression(QueryNode node, Expression aggregate)
        {
            // check if clause produce an Expression type node
            var eNode = node.GetChildByType(TokenType.Expression);
            if (eNode != null)
                return CreateExpression(eNode, aggregate);

            // create left side of the expression
            var leftNode = node.GetChildByType(TokenType.Left)!;
            var left = Left(leftNode, aggregate);

            // create predicate
            var predicateNode = node.GetChildByType(TokenType.ClausePredicate)!;
            var predicate = CreateClausePredicate(predicateNode, left, out QueryToken? clauseOperator);

            return ComparisonExpression(left, predicate, clauseOperator!);
        }

        private Expression Left(QueryNode node, Expression aggregate)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Type != TokenType.Left) throw new InvalidTokenException(node.Token);
            if (node.Type != TokenType.Left) throw new InvalidTokenException(node.Token, "Token has no children");

            // check child node type
            var childNode = node.Children.FirstOrDefault()!;

            if (childNode.Type == TokenType.PropertyNavigation)
                return PropertyNavigation(childNode, aggregate);

            return UserDefinedFunctionCall(childNode, aggregate);

        }

        private Expression CreateClausePredicate(QueryNode node, Expression navigationExp, out QueryToken operatorToken)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Token.Type != TokenType.ClausePredicate) throw new InvalidTokenException(node.Token);

            // check if clause produce a Funtion type node
            var fNode = node.GetChildByType(TokenType.Function);
            if (fNode != null)
                return CreateFunctionExpression(fNode, out operatorToken);

            // get type of identifier
            var navExpType = navigationExp.GetExpressionReturnType();
            // get operator
            var operatorNode = node.GetChildByType(TokenType.ComparisionOperator)!;
            operatorToken = (operatorNode.Token as QueryToken)!;
            // validate operator
            if(!IsValidOperator(navExpType, operatorToken.Value))
            {
                throw new InvalidOperatorException(operatorToken.Value, navigationExp.ToString(), navExpType);
            }

            var valueNode = node.GetChildByType(TokenType.Right)!;

            return Right(valueNode, navExpType, navigationExp);
        }

        private Expression CreateFunctionExpression(QueryNode node, out QueryToken functionOperator)
        {
            throw new NotImplementedException();
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

            var valueNode = node.GetTypedChild()!;

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
