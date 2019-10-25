namespace Swan.Parsers
{
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
}
