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

        public QueryNode(byte tokenType, QueryNode? parent = null)
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
            Children.Add(child);
        }

    }
}
