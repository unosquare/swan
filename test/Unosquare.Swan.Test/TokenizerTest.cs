namespace Unosquare.Swan.Test
{
    using Abstractions;
    using NUnit.Framework;
    using System.Linq;
    using Mocks;

    [TestFixture]
    public class TokenizerTest
    {
        [TestCase("HOLA",
            new[] { "HOLA" },
            new[] { TokenType.String })]
        [TestCase("=\"HOLA\"",
            new[] { "HOLA" },
            new[] { TokenType.String })]
        [TestCase("=1+1",
            new[] { "1", "+", "1" },
            new[] { TokenType.Number, TokenType.Operator, TokenType.Number })]
        [TestCase("= 1 + 1",
            new[] { "1", "+", "1" },
            new[] { TokenType.Number, TokenType.Operator, TokenType.Number })]
        [TestCase("= -1 * 1",
            new[] { "-1", "*", "1" },
            new[] { TokenType.Number, TokenType.Operator, TokenType.Number })]
        [TestCase("=1+1+1.1",
            new[] { "1", "+", "1", "+", "1.1" },
            new[] { TokenType.Number, TokenType.Operator, TokenType.Number, TokenType.Operator, TokenType.Number })]
        [TestCase("=10 mod 10",
            new[] { "10", "mod", "10" },
            new[] { TokenType.Number, TokenType.Operator, TokenType.Number })]
        [TestCase("=10 and -10",
            new[] { "10", "and", "-10" },
            new[] { TokenType.Number, TokenType.Operator, TokenType.Number })]
        [TestCase("=10 >= 10",
            new[] { "10", ">=", "10" },
            new[] { TokenType.Number, TokenType.Operator, TokenType.Number })]
        [TestCase("=(1+1)*1",
            new[] { "(", "1", "+", "1", ")", "*", "1" },
            new[] { TokenType.Parenthesis, TokenType.Number, TokenType.Operator, TokenType.Number, TokenType.Parenthesis, TokenType.Operator, TokenType.Number })]
        [TestCase("=First(1,1)",
            new[] { "first", "(", "1", ",", "1", ")" },
            new[] { TokenType.Function, TokenType.Parenthesis, TokenType.Number, TokenType.Comma, TokenType.Number, TokenType.Parenthesis })]
        [TestCase("=First(\"HOLA\",1)",
            new[] { "first", "(", "HOLA", ",", "1", ")" },
            new[] { TokenType.Function, TokenType.Parenthesis, TokenType.String, TokenType.Comma, TokenType.Number, TokenType.Parenthesis })]
        [TestCase("=First(Second(\"HOLA\",1))",
            new[] { "first", "(", "second", "(", "HOLA", ",", "1", ")", ")" },
            new[] { TokenType.Function, TokenType.Parenthesis, TokenType.Function, TokenType.Parenthesis, TokenType.String, TokenType.Comma, TokenType.Number, TokenType.Parenthesis, TokenType.Parenthesis })]
        [TestCase("=Global!PageNumber",
            new[] { "Global!PageNumber" },
            new[] { TokenType.Variable })]
        [TestCase("=[Global!PageNumber]",
            new[] { "[Global!PageNumber]" },
            new[] { TokenType.Variable })]
        [TestCase("=First(Global!PageNumber.Value,1)",
            new[] { "first", "(", "Global!PageNumber.Value", ",", "1", ")" },
            new[] { TokenType.Function, TokenType.Parenthesis, TokenType.Variable, TokenType.Comma, TokenType.Number, TokenType.Parenthesis })]
        [TestCase("=FormatDateTime(Parameters!DisplayDate.Value, DateFormat.ShortDate)",
            new[] { "formatdatetime", "(", "Parameters!DisplayDate.Value", ",", "dateformat.shortdate", ")" },
            new[] { TokenType.Function, TokenType.Parenthesis, TokenType.Variable, TokenType.Comma, TokenType.Function, TokenType.Parenthesis })]
        public void TokenizeExpression_ReturnsListOfTokens(string input, string[] tokens, TokenType[] tokensType)
        {
            var result = new RdlTokenizer(input);
            Assert.AreEqual(tokens, result.Tokens.Select(x => x.Value));
            Assert.AreEqual(tokensType, result.Tokens.Select(x => x.Type));
        }

        [TestCase("HOLA", new[] { "HOLA" }, true)]
        [TestCase("=\"HOLA\"", new[] { "HOLA" }, true)]
        [TestCase("=1+1", new[] { "1", "1", "+" }, true)]
        [TestCase("= 1 + 1", new[] { "1", "1", "+" }, true)]
        [TestCase("=1+1+1", new[] { "1", "1", "+", "1", "+" }, true)]
        [TestCase("=10 mod 10", new[] { "10", "10", "mod" }, true)]
        [TestCase("=10 >= 10", new[] { "10", "10", ">=" }, true)]
        [TestCase("=(1+1)*1", new[] { "1", "1", "+", "1", "*" }, true)]
        [TestCase("=First(1,1)", new[] { "(", "1", "1", "first" }, true)]
        [TestCase("=First(\"HOLA\",1)", new[] { "HOLA", "1", "first" }, false)]
        [TestCase("=First(10, Second(\"HOLA\",1))", new[] { "(", "10", "(", "HOLA", "1", "second", "first" }, true)]
        [TestCase("=Global!PageNumber", new[] { "Global!PageNumber" }, true)]
        [TestCase("=[Global!PageNumber]", new[] { "[Global!PageNumber]" }, true)]
        [TestCase("=First(Global!PageNumber.Value,1)", new[] { "Global!PageNumber.Value", "1", "first" }, false)]
        [TestCase("=FormatDateTime(Parameters!DisplayDate.Value, DateFormat.ShortDate)", new[] { "(", "Parameters!DisplayDate.Value", "dateformat.shortdate", "formatdatetime" }, true)]
        public void ShuntingStack_ReturnsListOfTokens(string input, string[] tokens, bool includeStopper)
        {
            var result = new RdlTokenizer(input);
            Assert.AreEqual(tokens, result.ShuntingYard(includeStopper).Select(x => x.Value));
        }
    }
}
