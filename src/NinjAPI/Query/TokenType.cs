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
        NullableIdentifier,
        Left,
        Right,
        Value,
        PropertyNavigation,
        PropertyNavigationAggregate,

        // Delimiters
        LeftParenthesis,
        RigthParenthesis,
        LeftBracket,
        RigthBracket,
        SingleQuote,
        Dollar,
        Comma,
        Dot,

        // Other Terminals
        LogicalOperator,
        ComparisionOperator,
        Identifier,
        QuantifierFunctionAny,
        QuantifierFunctionAll,
        ElementFunction,
        MathFunction,
        String,
        Number,
        Boolean,
        Null,
        EndOfLine,
        SortingOperator //TODO: check if needed
    }
}
