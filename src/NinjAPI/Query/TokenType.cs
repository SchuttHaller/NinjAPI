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
        Rigth,
        Value,

    // Delimiters
        LeftParenthesis,
        RigthParenthesis,
        LeftBracket,
        RigthBracket,
        SingleQuote,
        Dollar,
        Comma,

    // Other Terminals
        LogicalOperator,
        Identifier,
        QuantifierFunctionAny,
        QuantifierFunctionAll,
        ElementFunction,
        MathFunction,
        ComparisionOperator,
        String,
        Number,
        Boolean,
        Null,
        EndOfLine,
        SortingOperator //TODO: check if needed
    }
}
