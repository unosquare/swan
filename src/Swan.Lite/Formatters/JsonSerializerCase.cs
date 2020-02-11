namespace Swan.Formatters
{
    /// <summary>
    /// Enumerates the JSON serializer cases to use: None (keeps the same case), PascalCase, or camelCase.
    /// </summary>
    public enum JsonSerializerCase
    {
        /// <summary>
        /// The none
        /// </summary>
        None,

        /// <summary>
        /// The pascal case (eg. PascalCase)
        /// </summary>
        PascalCase,

        /// <summary>
        /// The camel case (eg. camelCase)
        /// </summary>
        CamelCase,
    }
}
