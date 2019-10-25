namespace Swan.Parsers
{
    /// <summary>
    /// Represents a Token structure.
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
