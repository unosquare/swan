namespace Swan.Parsers
{
    /// <summary>
    /// Enums the token types.
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
        Operator,

        /// <summary>
        /// The comma
        /// </summary>
        Comma,

        /// <summary>
        /// The wall, used to specified the end of argument list of the following function
        /// </summary>
        Wall,
    }
}
