using System;
using System.Collections.Generic;
using System.Linq;

namespace Swan.Parsers
{
    /// <summary>
    /// Represents a generic tokenizer.
    /// </summary>
    public abstract class Tokenizer
    {
        private const char PeriodChar = '.';
        private const char CommaChar = ',';
        private const char StringQuotedChar = '"';
        private const char OpenFuncChar = '(';
        private const char CloseFuncChar = ')';
        private const char NegativeChar = '-';

        private const string OpenFuncStr = "(";

        private readonly List<Operator> _operators = new List<Operator>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer"/> class.
        /// This constructor will use the following default operators:
        ///
        /// <list type="table">
        ///     <listheader>
        ///     <term>Operator</term>
        ///     <description>Precedence</description>
        ///     </listheader>
        /// <item>
        /// <term>=</term>
        /// <description>1</description>
        /// </item>
        /// <item>
        /// <term>!=</term>
        /// <description>1</description>
        /// </item>
        /// <item>
        /// <term>&gt;</term>
        /// <description>2</description>
        /// </item>
        /// <item>
        /// <term>&lt;</term>
        /// <description>2</description>
        /// </item>
        /// <item>
        /// <term>&gt;=</term>
        /// <description>2</description>
        /// </item>
        /// <item>
        /// <term>&lt;=</term>
        /// <description>2</description>
        /// </item>
        /// <item>
        /// <term>+</term>
        /// <description>3</description>
        /// </item>
        /// <item>
        /// <term>&amp;</term>
        /// <description>3</description>
        /// </item>
        /// <item>
        /// <term>-</term>
        /// <description>3</description>
        /// </item>
        /// <item>
        /// <term>*</term>
        /// <description>4</description>
        /// </item>
        /// <item>
        /// <term>(backslash)</term>
        /// <description>4</description>
        /// </item>
        /// <item>
        /// <term>/</term>
        /// <description>4</description>
        /// </item>
        /// <item>
        /// <term>^</term>
        /// <description>4</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="input">The input.</param>
        protected Tokenizer(string input)
        {
            _operators.AddRange(GetDefaultOperators());
            Tokenize(input);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer" /> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="operators">The operators to use.</param>
        protected Tokenizer(string input, IEnumerable<Operator> operators)
        {
            _operators.AddRange(operators);
            Tokenize(input);
        }

        /// <summary>
        /// Gets the tokens.
        /// </summary>
        /// <value>
        /// The tokens.
        /// </value>
        public List<Token> Tokens { get; } = new List<Token>();

        /// <summary>
        /// Validates the input and return the start index for tokenizer.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="startIndex">The start index.</param>
        /// <returns><c>true</c> if the input is valid, otherwise <c>false</c>.</returns>
        public abstract bool ValidateInput(string input, out int startIndex);

        /// <summary>
        /// Resolves the type of the function or member.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The token type.</returns>
        public abstract TokenType ResolveFunctionOrMemberType(string input);

        /// <summary>
        /// Evaluates the function or member.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="position">The position.</param>
        /// <returns><c>true</c> if the input is a valid function or variable, otherwise <c>false</c>.</returns>
        public virtual bool EvaluateFunctionOrMember(string input, int position) => false;

        /// <summary>
        /// Gets the default operators.
        /// </summary>
        /// <returns>An array with the operators to use for the tokenizer.</returns>
        public virtual Operator[] GetDefaultOperators() => new[]
        {
            new Operator {Name = "=", Precedence = 1},
            new Operator {Name = "!=", Precedence = 1},
            new Operator {Name = ">", Precedence = 2},
            new Operator {Name = "<", Precedence = 2},
            new Operator {Name = ">=", Precedence = 2},
            new Operator {Name = "<=", Precedence = 2},
            new Operator {Name = "+", Precedence = 3},
            new Operator {Name = "&", Precedence = 3},
            new Operator {Name = "-", Precedence = 3},
            new Operator {Name = "*", Precedence = 4},
            new Operator {Name = "/", Precedence = 4},
            new Operator {Name = "\\", Precedence = 4},
            new Operator {Name = "^", Precedence = 4},
        };

        /// <summary>
        /// Shunting the yard.
        /// </summary>
        /// <param name="includeFunctionStopper">if set to <c>true</c> [include function stopper] (Token type <c>Wall</c>).</param>
        /// <returns>
        /// Enumerable of the token in in.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Wrong token
        /// or
        /// Mismatched parenthesis.
        /// </exception>
        public virtual IEnumerable<Token> ShuntingYard(bool includeFunctionStopper = true)
        {
            var stack = new Stack<Token>();

            foreach (var tok in Tokens)
            {
                switch (tok.Type)
                {
                    case TokenType.Number:
                    case TokenType.Variable:
                    case TokenType.String:
                        yield return tok;
                        break;
                    case TokenType.Function:
                        stack.Push(tok);
                        break;
                    case TokenType.Operator:
                        while (stack.Any() && stack.Peek().Type == TokenType.Operator &&
                               CompareOperators(tok.Value, stack.Peek().Value))
                            yield return stack.Pop();

                        stack.Push(tok);
                        break;
                    case TokenType.Comma:
                        while (stack.Any() && (stack.Peek().Type != TokenType.Comma &&
                                               stack.Peek().Type != TokenType.Parenthesis))
                            yield return stack.Pop();

                        break;
                    case TokenType.Parenthesis:
                        if (tok.Value == OpenFuncStr)
                        {
                            if (stack.Any() && stack.Peek().Type == TokenType.Function)
                            {
                                if (includeFunctionStopper)
                                    yield return new Token(TokenType.Wall, tok.Value);
                            }

                            stack.Push(tok);
                        }
                        else
                        {
                            while (stack.Peek().Value != OpenFuncStr)
                                yield return stack.Pop();

                            stack.Pop();

                            if (stack.Any() && stack.Peek().Type == TokenType.Function)
                            {
                                yield return stack.Pop();
                            }
                        }

                        break;
                    default:
                        throw new InvalidOperationException("Wrong token");
                }
            }

            while (stack.Any())
            {
                var tok = stack.Pop();
                if (tok.Type == TokenType.Parenthesis)
                    throw new InvalidOperationException("Mismatched parenthesis");

                yield return tok;
            }
        }

        private static bool CompareOperators(Operator op1, Operator op2) => op1.RightAssociative
            ? op1.Precedence < op2.Precedence
            : op1.Precedence <= op2.Precedence;

        private void Tokenize(string input)
        {
            if (!ValidateInput(input, out var startIndex))
            {
                return;
            }

            for (var i = startIndex; i < input.Length; i++)
            {
                if (char.IsWhiteSpace(input, i)) continue;

                if (input[i] == CommaChar)
                {
                    Tokens.Add(new Token(TokenType.Comma, new string(new[] { input[i] })));
                    continue;
                }

                if (input[i] == StringQuotedChar)
                {
                    i = ExtractString(input, i);
                    continue;
                }

                if (char.IsLetter(input, i) || EvaluateFunctionOrMember(input, i))
                {
                    i = ExtractFunctionOrMember(input, i);

                    continue;
                }

                if (char.IsNumber(input, i) || (
                        input[i] == NegativeChar &&
                        ((Tokens.Any() && Tokens.Last().Type != TokenType.Number) || !Tokens.Any())))
                {
                    i = ExtractNumber(input, i);
                    continue;
                }

                if (input[i] == OpenFuncChar ||
                    input[i] == CloseFuncChar)
                {
                    Tokens.Add(new Token(TokenType.Parenthesis, new string(new[] { input[i] })));
                    continue;
                }

                i = ExtractOperator(input, i);
            }
        }

        private int ExtractData(
            string input,
            int i,
            Func<string, TokenType> tokenTypeEvaluation,
            Func<char, bool> evaluation,
            int right = 0,
            int left = -1)
        {
            var charCount = 0;
            for (var j = i + right; j < input.Length; j++)
            {
                if (evaluation(input[j]))
                    break;

                charCount++;
            }

            // Extract and set the value
            var value = input.SliceLength(i + right, charCount);
            Tokens.Add(new Token(tokenTypeEvaluation(value), value));

            i += charCount + left;
            return i;
        }

        private int ExtractOperator(string input, int i) =>
            ExtractData(
                input,
                i,
                x => TokenType.Operator,
                x => x == OpenFuncChar ||
                                                                x == CommaChar ||
                                                                x == PeriodChar ||
                                                                x == StringQuotedChar ||
                                                                char.IsWhiteSpace(x) ||
                                                                char.IsNumber(x));

        private int ExtractFunctionOrMember(string input, int i) =>
            ExtractData(
                input,
                i,
                ResolveFunctionOrMemberType,
                x => x == OpenFuncChar ||
                                                                    x == CloseFuncChar ||
                                                                    x == CommaChar ||
                                                                    char.IsWhiteSpace(x));

        private int ExtractNumber(string input, int i) =>
            ExtractData(
                input,
                i,
                x => TokenType.Number,
                x => !char.IsNumber(x) && x != PeriodChar && x != NegativeChar);

        private int ExtractString(string input, int i)
        {
            var length = ExtractData(input, i, x => TokenType.String, x => x == StringQuotedChar, 1, 1);

            // open string, report issue
            if (length == input.Length && input[length - 1] != StringQuotedChar)
                throw new FormatException($"Parser error (Position {i}): Expected '\"' but got '{input[length - 1]}'.");

            return length;
        }

        private bool CompareOperators(string op1, string op2)
            => CompareOperators(GetOperatorOrDefault(op1), GetOperatorOrDefault(op2));

        private Operator GetOperatorOrDefault(string op)
            => _operators.FirstOrDefault(x => x.Name == op) ?? new Operator { Name = op, Precedence = 0 };
    }
}
