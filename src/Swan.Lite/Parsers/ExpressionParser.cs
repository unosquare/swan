using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Swan.Parsers
{
    /// <summary>
    /// Represents a generic expression parser.
    /// </summary>
    public abstract class ExpressionParser
    {
        /// <summary>
        /// Resolves the expression.
        /// </summary>
        /// <typeparam name="T">The type of expression result.</typeparam>
        /// <param name="tokens">The tokens.</param>
        /// <returns>The representation of the expression parsed.</returns>
        public virtual T ResolveExpression<T>(IEnumerable<Token> tokens) =>
            ResolveExpression<T>(tokens, System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Resolves the expression.
        /// </summary>
        /// <typeparam name="T">The type of expression result.</typeparam>
        /// <param name="tokens">The tokens.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>The representation of the expression parsed.</returns>
        public virtual T ResolveExpression<T>(IEnumerable<Token> tokens, IFormatProvider formatProvider)
        {
            var conversion = Expression.Convert(Parse(tokens,formatProvider), typeof(T));
            return Expression.Lambda<Func<T>>(conversion).Compile()();
        }

        /// <summary>
        /// Parses the specified tokens.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns>
        /// The final expression.
        /// </returns>
        public virtual Expression Parse(IEnumerable<Token> tokens) =>
            Parse(tokens, System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Parses the specified tokens.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// The final expression.
        /// </returns>
        public virtual Expression Parse(IEnumerable<Token> tokens, IFormatProvider formatProvider)
        {
            var expressionStack = new List<Stack<Expression>>();

            foreach (var token in tokens)
            {
                if (expressionStack.Any() == false)
                    expressionStack.Add(new Stack<Expression>());

                switch (token.Type)
                {
                    case TokenType.Wall:
                        expressionStack.Add(new Stack<Expression>());
                        break;
                    case TokenType.Number:
                        expressionStack.Last().Push(Expression.Constant(Convert.ToDecimal(token.Value, formatProvider)));
                        break;
                    case TokenType.Variable:
                        ResolveVariable(token.Value, expressionStack.Last());
                        break;
                    case TokenType.String:
                        expressionStack.Last().Push(Expression.Constant(token.Value));
                        break;
                    case TokenType.Operator:
                        ResolveOperator(token.Value, expressionStack.Last());
                        break;
                    case TokenType.Function:
                        ResolveFunction(token.Value, expressionStack.Last());

                        if (expressionStack.Count > 1 && expressionStack.Last().Count == 1)
                        {
                            var lastValue = expressionStack.Last().Pop();
                            expressionStack.Remove(expressionStack.Last());
                            expressionStack.Last().Push(lastValue);
                        }

                        break;
                }
            }

            return expressionStack.Last().Pop();
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
