using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjAPI.Query
{
    public enum TokenType
    {
        /// Non-Terminals
        Expression = 0,
        ExpressionPredicate,
        Clause,
        ClausePredicate,
        Function,
        DataBaseFunction,
        Parameters,
        ParametersAggregate,
        Parameter,
        NullableExpression,
        Left,
        Right,
        Value,
        PropertyNavigation,
        PropertyNavigationAggregate,
        PropertyNavigationChain,

        // Delimiters
        LeftParenthesis,
        RightParenthesis,
        LeftBracket,
        RightBracket,
        SingleQuote,
        Dollar,
        Comma,
        Dot,

        // Operators
        LogicalOperator,
        ComparisionOperator,
        
        // id terminal
        Identifier, 
        
        // function operators
        QuantifierFunctionAny,
        QuantifierFunctionAll,
        ElementFunction,
        MathFunction,

        //
        String,
        Number,
        Boolean,
        Null,
        EndOfLine,
        SortingOperator //TODO: check if needed
    }
}
