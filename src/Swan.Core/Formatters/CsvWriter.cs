namespace Swan.Formatters;

using Swan.Threading;
using System.IO;

/// <summary>
/// Represents a writer that writes sets of strings in CSV format into a stream.
/// </summary>
public class CsvWriter : IDisposable
{
    private const int BufferSize = 4096;

    private readonly StreamWriter _writer;
    private readonly AtomicLong _count = new();
    private readonly AtomicBoolean _isDisposed = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvWriter" /> class.
    /// </summary>
    /// <param name="outputStream">The output stream.</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="separatorChar">The field separator character.</param>
    /// <param name="escapeChar">The escape character.</param>
    /// <param name="newLineSequence">Specifies the new line character sequence.</param>
    /// <param name="leaveOpen">true to leave the stream open after the stream reader object is disposed; otherwise, false.</param>
    public CsvWriter(Stream outputStream,
        Encoding? encoding = default,
        char separatorChar = Csv.DefaultSeparatorChar,
        char escapeChar = Csv.DefaultEscapeChar,
        string? newLineSequence = default,
        bool? leaveOpen = default)
    {
        _writer = new(outputStream,
            encoding ?? Csv.DefaultEncoding,
            BufferSize,
            leaveOpen ?? false);

        SeparatorChar = separatorChar;
        EscapeChar = escapeChar;
        NewLineSequence = newLineSequence ?? Environment.NewLine;
    }

    /// <summary>
    /// Gets or sets the field separator character.
    /// </summary>
    public char SeparatorChar { get; }

    /// <summary>
    /// Gets or sets the escape character to use to escape and enclose field values.
    /// </summary>
    public char EscapeChar { get; }

    /// <summary>
    /// Gets or sets the new line character sequence to use when writing a line.
    /// </summary>
    /// <value>
    /// The new line sequence.
    /// </value>
    public string NewLineSequence { get; }

    /// <summary>
    /// Gets number of lines that have been written, including the headings line.
    /// </summary>
    public long Count => _count.Value;

    /// <summary>
    /// Clears all buffers from the current writer and causes all data to be written to the underlying stream.
    /// </summary>
    public void Flush() => _writer.Flush();

    /// <summary>
    /// Writes a CSV record with the specified values.
    /// Individual items found to be null will be written out as empty strings.
    /// </summary>
    /// <param name="items">The set of strings to write out.</param>
    public void WriteLine(IEnumerable<string?> items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        var isFirst = true;
        foreach (var currentItem in items)
        {
            var item = currentItem ?? string.Empty;

            // Determine if we need the string to be enclosed 
            // (it either contains an escape, new line, or separator char)
            var needsEnclosing = item.Contains(SeparatorChar, StringComparison.Ordinal)
                             || item.Contains(EscapeChar, StringComparison.Ordinal)
                             || item.Contains('\r', StringComparison.Ordinal)
                             || item.Contains('\n', StringComparison.Ordinal);

            // Escape the escape characters by repeating them twice for every instance
            var textValue = item.Replace($"{EscapeChar}",
                $"{EscapeChar}{EscapeChar}", StringComparison.Ordinal);

            // Enclose the text value if we need to
            if (needsEnclosing)
                textValue = $"{EscapeChar}{textValue}{EscapeChar}";

            if (isFirst)
            {
                _writer.Write(textValue);
                isFirst = false;
                continue;
            }

            _writer.Write($"{SeparatorChar}{textValue}");
        }

        // output the newline sequence
        _writer.Write(NewLineSequence);
        _count.Increment();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="alsoManaged"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool alsoManaged)
    {
        if (_isDisposed) return;
        _isDisposed.Value = true;

        if (!alsoManaged)
            return;

        _writer.Flush();
        _writer.Dispose();
    }
}
