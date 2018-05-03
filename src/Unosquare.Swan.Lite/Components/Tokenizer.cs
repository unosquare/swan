namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class Tokenizer
    {
        private const char PeriodChar = '.';
        private const char CommaChar = ',';
        private const char StringQuotedChar = '"';
        private const char OpenFuncChar = '(';
        private const char CloseFuncChar = ')';
        private const char NegativeChar = '-';

        private const string OpenFuncStr = "(";

        private static readonly Operator[] DefaultOperators = {
            new Operator {Name = ">", Precedence = 1},
            new Operator {Name = "<", Precedence = 1},
            new Operator {Name = "=", Precedence = 1},
            new Operator {Name = "+", Precedence = 1},
            new Operator {Name = "&", Precedence = 1},
            new Operator {Name = "-", Precedence = 1},
            new Operator {Name = "*", Precedence = 2},
            new Operator {Name = "/", Precedence = 2},
            new Operator {Name = "\\", Precedence = 2},
            new Operator {Name = "^", Precedence = 2},
        };

        private readonly List<Operator> _operators = new List<Operator>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        protected Tokenizer(string input)
            : this(input, DefaultOperators)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer" /> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="operators">The operators.</param>
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
        /// <returns><c>true</c> if the input is valid, otherwise <c>false</c></returns>
        public abstract bool ValidateInput(string input, out int startIndex);

        /// <summary>
        /// Resolves the type of the function or member.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The token type</returns>
        public abstract TokenType ResolveFunctionOrMemberType(string input);

        /// <summary>
        /// Shuntings the yard.
        /// </summary>
        /// <returns>Enumerable of the token in in</returns>
        /// <exception cref="Exception">
        /// Wrong token
        /// or
        /// Mismatched parenthesis
        /// </exception>
        public IEnumerable<Token> ShuntingYard()
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
                    case TokenType.Parenthesis:
                        if (tok.Value == OpenFuncStr)
                        {
                            stack.Push(tok);
                        }
                        else
                        {
                            while (stack.Peek().Value != OpenFuncStr)
                                yield return stack.Pop();

                            stack.Pop();

                            if (stack.Any() && stack.Peek().Type == TokenType.Function)
                                yield return stack.Pop();
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
                if (char.IsWhiteSpace(input, i) || input[i] == CommaChar) continue;

                if (input[i] == StringQuotedChar)
                {
                    i = ExtractString(input, i);
                    continue;
                }

                if (char.IsLetter(input, i))
                {
                    i = ExtractFunctionOrMember(input, i);

                    continue;
                }

                if (char.IsNumber(input, i) || (
                        input[i] == NegativeChar && ((Tokens.Any() && Tokens.Last().Type != TokenType.Number) || !Tokens.Any())))
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
            ExtractData(input, i, x => TokenType.Operator, x => x == OpenFuncChar ||
                                                                x == CommaChar ||
                                                                x == PeriodChar ||
                                                                x == StringQuotedChar ||
                                                                char.IsWhiteSpace(x) ||
                                                                char.IsNumber(x));

        private int ExtractFunctionOrMember(string input, int i) =>
            ExtractData(input, i, ResolveFunctionOrMemberType, x => x == OpenFuncChar ||
                                                    x == CommaChar ||
                                                    char.IsWhiteSpace(x));

        private int ExtractNumber(string input, int i) =>
            ExtractData(input, i, x => TokenType.Number, x => !char.IsNumber(x) && x != PeriodChar && x != NegativeChar);

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

    /// <summary>
    /// Represents an operator with precedence.
    /// </summary>
    public class Operator
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the precedence.
        /// </summary>
        /// <value>
        /// The precedence.
        /// </value>
        public int Precedence { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [right associative].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [right associative]; otherwise, <c>false</c>.
        /// </value>
        public bool RightAssociative { get; set; }
    }

    /// <summary>
    /// Enums the token types
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// The number
        /// </summary>
        Number,

        /// <summary>
        /// The string
        /// </summary>
        String,

        /// <summary>
        /// The variable
        /// </summary>
        Variable,

        /// <summary>
        /// The function
        /// </summary>
        Function,

        /// <summary>
        /// The parenthesis
        /// </summary>
        Parenthesis,

        /// <summary>
        /// The operator
        /// </summary>
        Operator
    }

    /// <summary>
    /// Represents a Token structure
    /// </summary>
    public struct Token
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> struct.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        public Token(TokenType type, string value)
        {
            Type = type;
            Value = type == TokenType.Function || type == TokenType.Operator ? value.ToLowerInvariant() : value;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public TokenType Type { get; set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; }
    }
}
