namespace Swan.Test.Mocks
{
    using Swan.Parsers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class ExpressionParserMock : ExpressionParser
    {
        private static readonly Dictionary<string, Func<Expression[], Expression>> Functions =
            new()
            {
                {
                    "max",
                    x => Expression.Call(null,
                        typeof(Math).GetMethod(nameof(Math.Max), new[] { typeof(int), typeof(int) }),
                        Expression.Convert(x.First(), typeof(int)),
                        Expression.Convert(x.Last(), typeof(int)))
                },
                {
                    "min",
                    x => Expression.Call(null,
                        typeof(Math).GetMethod(nameof(Math.Min), new[] { typeof(int), typeof(int) }),
                        Expression.Convert(x.First(), typeof(int)),
                        Expression.Convert(x.Last(), typeof(int)))
                },
                {
                    "iif",
                    x => Expression.Condition(x.First(), x.Skip(1).First(), x.Last())
                },
                { "+", x => Expression.Add(x.First(), x.Last()) },
                { "-", x => Expression.Subtract(x.First(), x.Last()) },
                { "*", x => Expression.Multiply(x.First(), x.Last()) },
                { "/", x => Expression.Divide(x.First(), x.Last()) },
                { "<", x => Expression.LessThan(x.First(), x.Last()) },
                { ">", x => Expression.GreaterThan(x.First(), x.Last()) },
            };

        private readonly Dictionary<string, object> _variables;

        public ExpressionParserMock(Dictionary<string, object> variables)
        {
            _variables = variables ?? new Dictionary<string, object>();
        }

        public static T ResolveExpression<T>(string input, Dictionary<string, object> variables = null) => new ExpressionParserMock(variables).ResolveExpression<T>(GetTokens(input));

        public override void ResolveVariable(string value, Stack<Expression> expressionStack)
        {
            if (!_variables.ContainsKey(value))
                throw new ArgumentOutOfRangeException(nameof(value));

            expressionStack.Push(Expression.Constant(_variables[value]));
        }

        public override void ResolveOperator(string value, Stack<Expression> expressionStack)
        {
            ResolveFunctionOrOperator(value, expressionStack, true);
        }

        public override void ResolveFunction(string value, Stack<Expression> expressionStack)
        {
            ResolveFunctionOrOperator(value, expressionStack, false);
        }

        private static void ResolveFunctionOrOperator(string value, Stack<Expression> expressionStack, bool isOperator)
        {
            var capacity = isOperator ? 2 : 10;

            var expressions = new List<Expression>(capacity);

            while (expressionStack.Count > 0 && expressions.Count < capacity)
                expressions.Add(expressionStack.Pop());

            expressions.Reverse();
            expressionStack.Push(Functions[value](expressions.ToArray()));
        }

        private static IEnumerable<Token> GetTokens(string input) => new RdlTokenizer(input).ShuntingYard();
    }
}