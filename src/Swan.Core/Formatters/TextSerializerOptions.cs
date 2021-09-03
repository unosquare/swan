namespace Swan.Formatters
{
    public class TextSerializerOptions
    {
        public static readonly TextSerializerOptions JsonPrettyPrint = new();
        public static readonly TextSerializerOptions JsonCompact = new() { WriteIndented = false };
        public static readonly TextSerializerOptions JsonCompactCamel = new() { WriteIndented = false, UseCamelCase = true };

        /// <summary>
        /// Gets a value indicating whether the serializer
        /// outputs whitespace and standard indentation.
        /// </summary>
        public bool WriteIndented { get; init; } = true;

        /// <summary>
        /// Gets the number of spaces per indentation.
        /// </summary>
        public byte IndentSpaces { get; init; } = 4;

        /// <summary>
        /// For object serialization, changes property names
        /// starting with uppercase characters into lowercase
        /// characters.
        /// </summary>
        public bool UseCamelCase { get; init; }

        public int MaxStackDepth { get; init; } = 1;

        public bool IgnoreCircularReferences { get; init; } = true;

        public bool OutputTypeMetadata { get; init; }

        public bool OutputArrayCounts { get; init; }

        public bool OutputArrayIndices { get; init; }

        /// <summary>
        /// Gets the literal value that is used to enclose
        /// string sequences.
        /// </summary>
        public string StringQuotation { get; init; } = "\"";

        /// <summary>
        /// The literal value to output when 
        /// </summary>
        public string NullLiteral { get; init; } = "null";

        /// <summary>
        /// The literal string to output when a true boolean
        /// value is encountered.
        /// </summary>
        public string TrueLiteral { get; init; } = "true";

        /// <summary>
        /// The literal string to output when a false boolean
        /// value is encountered.
        /// </summary>
        public string FalseLiteral { get; init; } = "false";

        public string ObjectOpener { get; init; } = "{";

        public string ObjectCloser { get; init; } = "}";

        public string ArrayOpener { get; init; } = "[";

        public string ArrayCloser { get; init; } = "]";

        /// <summary>
        /// For nested output, the literal that separates array items
        /// and proerty key-value pairs.
        /// </summary>
        public string ItemSeparator { get; init; } = ",";

        /// <summary>
        /// The string that goes between keys and values in
        /// object properties and dictionaries.
        /// </summary>
        public string KeyValueSeparator { get; init; } = ":";
    }

}
