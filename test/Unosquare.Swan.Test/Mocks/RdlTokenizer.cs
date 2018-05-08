namespace Unosquare.Swan.Test.Mocks
{
    using System.Linq;
    using Abstractions;

    public class RdlTokenizer : Tokenizer
    {
        private const char OpenExpressionChar = '=';

        private static readonly string[] OperatorStrings =
            {"mod", "like", "and", "not", "or", "xor", "andalso", "orelse", "is"};

        public RdlTokenizer(string input)
            : base(input)
        {
        }

        public override bool EvaluateFunctionOrMember(string input, int position)
        {
            return input[position] == '[';
        }

        public override bool ValidateInput(string input, out int startIndex)
        {
            startIndex = 1;

            if (!string.IsNullOrWhiteSpace(input) && input[0] == OpenExpressionChar) return true;

            Tokens.Add(new Token(TokenType.String, input));
            return false;
        }

        public override TokenType ResolveFunctionOrMemberType(string input)
        {
            if (input.IndexOf('!') > 0)
                return TokenType.Variable;

            return OperatorStrings.Contains(input.ToLowerInvariant()) ? TokenType.Operator : TokenType.Function;
        }
    }
}
