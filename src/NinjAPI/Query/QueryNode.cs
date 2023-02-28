using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public class QueryNode
    {
        public Token Token { get; set; }
        public TokenType Type => Token.Type; 
        public QueryNode? Parent { get; set; }
        public ICollection<QueryNode> Children { get; set; }
        public bool IsRoot => Parent == null;
        public int Level => IsRoot ? 0 : Parent!.Level + 1;

        public QueryNode(Token token, QueryNode? parent = null)
        {
            Token = token;
            Parent = parent;
            Children = new List<QueryNode>();
        }

        public QueryNode(TokenType tokenType, QueryNode? parent = null)
        {
            Token = Token.New(tokenType);
            Children = new List<QueryNode>();
            if (parent != null)
            {
                Parent = parent!;
                Parent.AddChild(this);
            }
        }

        public void AddChild(QueryNode child)
        {
            if (Type.IsTerminal())
            {
                throw new NotSupportedException("A terminal child cannot have children nodes");
            }

            Children.Add(child);
        }

        public QueryNode? GetChildByType(TokenType type)
        {
            return Children.FirstOrDefault(n => n.Type == type);
        }

        public QueryNode? GetTypedChild()
        {
            return Children.FirstOrDefault(n =>
                n.Type == TokenType.String
                || n.Type == TokenType.Boolean 
                || n.Type == TokenType.Number
                || n.Type == TokenType.Null);
        }

        public override string ToString()
        {
            if (Type.IsTerminal())
                return (Token as QueryToken)!.Value;

            if (!Children.Any()) 
                return string.Empty;

            var children = string.Join(" ", 
                    Children.Select(c => c.ToString()).Where(s => !string.IsNullOrEmpty(s))
                ).Trim();

            if (Type.IsPredicateOrAggregate() 
                || (Parent != null && Parent.Type.IsPredicateOrAggregate())
                || Type == TokenType.Left)
            {
                return $"{children}";
            }

            return $"{Token.Type} {{ {children} }}";

        }

    }
}
