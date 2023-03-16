using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Expressions
{
    public class PropertyNavigationExpression : Expression
    {
        public Expression NavigationExpression { get; }
        public Expression? NullPreventExpression { get; }
        public PropertyNavigationExpression(Expression navigationExpression, Expression? nullPreventExpression)
        {
            NavigationExpression = navigationExpression;
            NullPreventExpression = nullPreventExpression;
        }
        public override Type Type => NavigationExpression.GetExpressionReturnType();
    }
}
