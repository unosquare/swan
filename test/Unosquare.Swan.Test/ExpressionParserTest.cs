namespace Unosquare.Swan.Test
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Mocks;

    [TestFixture]
    public class ExpressionParserTest
    {
        [TestCase("=MAX(1, 2)", 2)]
        [TestCase("=max(1, 2)", 2)]
        [TestCase("=min(5, 2)", 2)]
        [TestCase("=iif(5 < 10, 2, 1)", 2)]
        [TestCase("=iif(5 > 10, 1, 1+1)", 2)]
        [TestCase("=iif(5 > 10, min(2, 1), min(5, 2))", 2)]
        [TestCase("=iif(5 > 10, min(2, 1), min(MAX(5, MIN(10, 1)), 2))", 2)]
        public void FunctionCallExpression_ReturnsValue(string input, int expected)
        {
            var result = ExpressionParserMock.ResolveExpression<int>(input);

            Assert.AreEqual(expected, result);
        }

        [TestCase("= (1 + 1) * 10", 20)]
        [TestCase("= 1 + 1", 2)]
        public void MathExpression_ReturnsExpectedInteger(string input, int expected)
        {
            var result = ExpressionParserMock.ResolveExpression<int>(input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void MissingVariable_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ExpressionParserMock.ResolveExpression<string>("=Global!Variable1"));
        }

        [Test]
        public void VariableExpression_ReturnsValue()
        {
            var result =
                ExpressionParserMock.ResolveExpression<string>("=Global!Variable1",
                    new Dictionary<string, object> {{"Global!Variable1", "hola"}});

            Assert.AreEqual("hola", result);
        }

        [Test]
        public void StringExpression_ReturnsValue()
        {
            var result = ExpressionParserMock.ResolveExpression<string>("=\"hola\"");

            Assert.AreEqual("hola", result);
        }
    }
}