namespace Swan.Formatters
{
    /// <summary>
    /// Specifies options for the generic <see cref="TextSerializer"/>.
    /// </summary>
    public class TextSerializerOptions
    {
        /// <summary>
        /// Outputs Pascal-case, indented, JSON compliant sequences.
        /// </summary>
        public static readonly TextSerializerOptions JsonPrettyPrint = new();

        /// <summary>
        /// Outputs Pascal-case, compact, JSON compliant sequences.
        /// No whitespace or indenting is written to the output.
        /// </summary>
        public static readonly TextSerializerOptions JsonCompact = new() { WriteIndented = false };

        /// <summary>
        /// Outputs camel-case, compact, JSON compliant sequences.
        /// No whitespace or indenting is written to the output.
        /// </summary>
        public static readonly TextSerializerOptions JsonCompactCamel = new() { WriteIndented = false, UseCamelCase = true };

        /// <summary>
        /// 
        /// </summary>
        public static readonly TextSerializerOptions HumanReadable = new()
        {
            QuotePropertyNames = false,
            MaxStackDepth = 4,
            PropertySeparator = string.Empty,
            KeyValuePadding = -8,
            ObjectOpener = string.Empty,
            ObjectCloser = string.Empty,
            OutputTypeNames = true
        };

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
        /// Gets a value indicating whether dictionary keys and
        /// property names should have their first letter in lower-case.
        /// </summary>
        public bool UseCamelCase { get; init; }

        /// <summary>
        /// Gets a value indicating the max depth of the stack
        /// to reach the text representation of an object tree.
        /// Arrays and objects increment the stack by one and
        /// the stack level starts from the zeroth depth.
        /// </summary>
        public int MaxStackDepth { get; init; } = 16;

        /// <summary>
        /// Gets a value indicating whether objects and arrays that have been
        /// written to the output already will be ignored.
        /// </summary>
        public bool IgnoreRepeatedReferences { get; init; }

        /// <summary>
        /// Gets a value indicating whether the object type names should be written
        /// before object properties are written. Enabling this option prevents valid
        /// JSON from being written.
        /// </summary>
        public bool OutputTypeNames { get; init; }

        /// <summary>
        /// Gets the literal used to quote a string value.
        /// </summary>
        public string StringQuotation { get; init; } = "\"";

        /// <summary>
        /// Gets the literal used to show a null value.
        /// </summary>
        public string NullLiteral { get; init; } = "null";

        /// <summary>
        /// Gets the literal used to show a true value.
        /// </summary>
        public string TrueLiteral { get; init; } = "true";

        /// <summary>
        /// Gets the literal used to show a false value.
        /// </summary>
        public string FalseLiteral { get; init; } = "false";

        /// <summary>
        /// Gets the literal used to show the start of an object.
        /// </summary>
        public string ObjectOpener { get; init; } = "{";

        /// <summary>
        /// Gets the literal used to show the end of an object.
        /// </summary>
        public string ObjectCloser { get; init; } = "}";

        /// <summary>
        /// Gets the literal used to show the start of an array.
        /// </summary>
        public string ArrayOpener { get; init; } = "[";

        /// <summary>
        /// Gets the literal used to show the end of an array.
        /// </summary>
        public string ArrayCloser { get; init; } = "]";

        /// <summary>
        /// Gets the literal that goes between array items.
        /// </summary>
        public string ArrayItemSeparator { get; init; } = ",";

        /// <summary>
        /// Gets the literal that goes between key-value pairs.
        /// This is not to be confused with <see cref="KeyValueSeparator"/>
        /// which is the literal that separates property names from property values.
        /// </summary>
        public string PropertySeparator { get; init; } = ",";

        /// <summary>
        /// Gets the literal that goes between keys and values.
        /// In other words, it's the key-value pair separator literal.
        /// </summary>
        public string KeyValueSeparator { get; init; } = ":";

        /// <summary>
        /// Gets a value indicating the string padding option to use
        /// when object property names or formatted array indices ore written
        /// to the output. Positive integers align to the right and negative
        /// integers align to the left.
        /// </summary>
        public int? KeyValuePadding { get; init; }

        /// <summary>
        /// Gets a value indicating whether object property names are quoted.
        /// </summary>
        public bool QuotePropertyNames { get; init; } = true;
    }
}
