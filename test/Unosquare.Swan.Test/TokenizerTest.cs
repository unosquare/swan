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
            new[] { "first", "(", "1", "1", ")" },
            new[] { TokenType.Function, TokenType.Parenthesis, TokenType.Number, TokenType.Number, TokenType.Parenthesis })]
        [TestCase("=First(\"HOLA\",1)", 
            new[] { "first", "(", "HOLA", "1", ")" },
            new[] { TokenType.Function, TokenType.Parenthesis, TokenType.String, TokenType.Number, TokenType.Parenthesis })]
        [TestCase("=First(Second(\"HOLA\",1))", 
            new[] { "first", "(", "second", "(", "HOLA", "1", ")", ")" },
            new[] { TokenType.Function, TokenType.Parenthesis, TokenType.Function, TokenType.Parenthesis, TokenType.String, TokenType.Number, TokenType.Parenthesis, TokenType.Parenthesis })]
        [TestCase("=Global!PageNumber", 
            new[] { "Global!PageNumber" },
            new[] { TokenType.Variable })]
        [TestCase("=[Global!PageNumber]", 
            new[] { "[Global!PageNumber]" },
            new[] { TokenType.Variable })]
        [TestCase("=First(Global!PageNumber.Value,1)", 
            new[] { "first", "(", "Global!PageNumber.Value", "1", ")" },
            new[] { TokenType.Function, TokenType.Parenthesis, TokenType.Variable, TokenType.Number, TokenType.Parenthesis })]
        [TestCase("=FormatDateTime(Parameters!DisplayDate.Value, DateFormat.ShortDate)", 
            new[] { "formatdatetime", "(", "Parameters!DisplayDate.Value", "dateformat.shortdate", ")" },
            new[] { TokenType.Function, TokenType.Parenthesis, TokenType.Variable, TokenType.Function, TokenType.Parenthesis })]
        public void TokenizeExpression_ReturnsListOfTokens(string input, string[] tokens, TokenType[] tokensType)
        {
            var result = new RdlTokenizer(input);
            Assert.AreEqual(tokens, result.Tokens.Select(x => x.Value));
            Assert.AreEqual(tokensType, result.Tokens.Select(x => x.Type));
        }

        [TestCase("HOLA", new[] { "HOLA" })]
        [TestCase("=\"HOLA\"", new[] { "HOLA" })]
        [TestCase("=1+1", new[] { "1", "1", "+" })]
        [TestCase("= 1 + 1", new[] { "1", "1", "+" })]
        [TestCase("=1+1+1", new[] { "1", "1", "+", "1", "+" })]
        [TestCase("=10 mod 10", new[] { "10", "10", "mod" })]
        [TestCase("=10 >= 10", new[] { "10", "10", ">=" })]
        [TestCase("=(1+1)*1", new[] { "1", "1", "+", "1", "*" })]
        [TestCase("=First(1,1)", new[] { "1", "1", "first" })]
        [TestCase("=First(\"HOLA\",1)", new[] { "HOLA", "1", "first" })]
        [TestCase("=First(Second(\"HOLA\",1))", new[] { "HOLA", "1", "second", "first" })]
        [TestCase("=Global!PageNumber", new[] { "Global!PageNumber" })]
        [TestCase("=[Global!PageNumber]", new[] { "[Global!PageNumber]" })]
        [TestCase("=First(Global!PageNumber.Value,1)", new[] { "Global!PageNumber.Value", "1", "first" })]
        [TestCase("=FormatDateTime(Parameters!DisplayDate.Value, DateFormat.ShortDate)", new[] { "Parameters!DisplayDate.Value", "dateformat.shortdate", "formatdatetime" })]
        public void ShuntingStack_ReturnsListOfTokens(string input, string[] tokens)
        {
            var result = new RdlTokenizer(input);
            Assert.AreEqual(tokens, result.ShuntingYard().Select(x => x.Value));
        }
    }
}
