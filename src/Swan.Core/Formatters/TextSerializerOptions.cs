using System.Text;

namespace Swan.Formatters
{
    public class TextSerializerOptions
    {
        public static readonly TextSerializerOptions JsonPrettyPrint = new();
        public static readonly TextSerializerOptions JsonCompact = new() { WriteIndented = false };

        public bool WriteIndented { get; init; } = true;

        public string NullLiteral { get; init; } = "null";

        public string TrueLiteral { get; init; } = "true";

        public string FalseLiteral { get; init; } = "false";

        public byte IndentSpaces { get; init; } = 4;

        public string OpenObjectSequence { get; init; } = "{";

        public string CloseObjectSequence { get; init; } = "}";

        public string OpenArraySequence { get; init; } = "[";

        public string CloseArraySequence { get; init; } = "]";

        public string ItemSeparator { get; init; } = ",";

        public string PropertySeparator { get; init; } = ":";

        internal void OpenObject(StringBuilder builder)
        {
            builder.Append(OpenObjectSequence);

            if (WriteIndented)
                builder.AppendLine();
        }

        internal void CloseObject(StringBuilder builder, int stackDepth)
        {
            var indentDepth = stackDepth > 0 ? stackDepth - 1 : 0;

            if (WriteIndented)
                builder.AppendLine().Append(IndentString(indentDepth));

            builder.Append(CloseObjectSequence);
        }

        internal string PropertySeparation =>
            WriteIndented ? $"{PropertySeparator} " : PropertySeparator;

        internal string IndentString(int stackDepth) => WriteIndented
            ? new(' ', stackDepth * IndentSpaces)
            : string.Empty;
    }

}
