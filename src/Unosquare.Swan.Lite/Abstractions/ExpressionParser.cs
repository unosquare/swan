namespace Unosquare.Swan.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Represents a Expression Parser base class
    /// </summary>
    public abstract class ExpressionParser
    {
        /// <summary>
        /// Resolves the expression.
        /// </summary>
        /// <typeparam name="T">The type of expression result</typeparam>
        /// <param name="tokens">The tokens.</param>
        /// <returns>The representation of the expression parsed</returns>
        public virtual T ResolveExpression<T>(IEnumerable<Token> tokens)
        {
            var conversion = Expression.Convert(Parse(tokens), typeof(T));
            return Expression.Lambda<Func<T>>(conversion).Compile()();
        }

        /// <summary>
        /// Parses the specified tokens.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>The final expression</returns>
        public virtual Expression Parse(IEnumerable<Token> tokens)
        {
            var expressionStack = new Stack<Expression>();

            foreach (var token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.Number:
                        expressionStack.Push(Expression.Constant(Convert.ToDecimal(token.Value)));
                        break;
                    case TokenType.Variable:
                        ResolveVariable(token.Value, expressionStack);
                        break;
                    case TokenType.String:
                        expressionStack.Push(Expression.Constant(token.Value));
                        break;
                    case TokenType.Operator:
                        ResolveOperator(token.Value, expressionStack);
                        break;
                    case TokenType.Function:
                        ResolveFunction(token.Value, expressionStack);
                        break;
                }
            }

            return expressionStack.Pop();
        }

        /// <summary>
        /// Resolves the variable.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expressionStack">The expression stack.</param>
        public abstract void ResolveVariable(string value, Stack<Expression> expressionStack);

        /// <summary>
        /// Resolves the operator.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expressionStack">The expression stack.</param>
        public abstract void ResolveOperator(string value, Stack<Expression> expressionStack);

        /// <summary>
        /// Resolves the function.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="expressionStack">The expression stack.</param>
        public abstract void ResolveFunction(string value, Stack<Expression> expressionStack);
    }
}
