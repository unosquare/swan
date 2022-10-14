namespace Swan.Formatters;

using System.IO;
using System.Runtime.CompilerServices;
using Threading;

/// <summary>
/// Represents a writer that writes sets of strings in CSV format into a stream.
/// </summary>
public class CsvWriter : IDisposable, IAsyncDisposable
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
    /// Clears all buffers from the current writer and causes all data to be written to the underlying stream.
    /// </summary>
    public async ValueTask FlushAsync() => await _writer.FlushAsync().ConfigureAwait(false);

    /// <summary>
    /// Writes a CSV record with the specified values.
    /// Individual items found to be null will be written out as empty strings.
    /// </summary>
    /// <param name="items">The set of strings to write out. If no items are specified, an empty line is written to the stream.</param>
    public void WriteLine(params string?[] items)
    {
        try
        {
            _writer.Write(SerializeValues(NewLineSequence, EscapeChar, SeparatorChar, items));
            _count.Increment();
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Asynchronously writes a CSV record with the specified values.
    /// Individual items found to be null will be written out as empty strings.
    /// </summary>
    /// <param name="items">The set of strings to write out. If no items are specified, an empty line is written to the stream.</param>
    public async ValueTask WriteLineAsync(params string?[] items)
    {
        try
        {
            await _writer.WriteAsync(SerializeValues(NewLineSequence, EscapeChar, SeparatorChar, items)).ConfigureAwait(false);
            _count.Increment();
        }
        catch
        {
            throw;
        }
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

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="alsoManaged"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual async ValueTask DisposeAsync(bool alsoManaged)
    {
        if (_isDisposed) return;
        _isDisposed.Value = true;

        if (!alsoManaged)
            return;

        await _writer.FlushAsync().ConfigureAwait(false);
        await _writer.DisposeAsync();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static StringBuilder SerializeValues(string newLineSequence, char escapeChar, char separatorChar, params string?[] items)
    {
        var newLine = (newLineSequence ?? Environment.NewLine).AsSpan();

        if (items is null || items.Length <= 0)
            return new StringBuilder(newLine.Length).Append(newLine);

        // try to guess the size of the string builder to avoid reallocations
        var length = items.Sum(c => c is null ? 0 : c.Length + 8) + newLine.Length;
        var builder = new StringBuilder(length);

        // The first item will not have a leading (comma) separator
        var isFirst = true;
        foreach (var currentItem in items)
        {
            var item = currentItem.AsSpan();

            // Add a comma (separator) before we start a new, non-first field value.
            if (!isFirst)
                builder.Append(separatorChar);

            var builderIndex = builder.Length;

            // Determine if we need the string to be enclosed 
            // (it either contains an escape, new line, or separator char)
            var needsEnclosing = false;
            foreach (var c in item)
            {
                // check for characters in the value to see if the output need enclosing.
                if (!needsEnclosing && (c == separatorChar || c == escapeChar || c == '\r' || c == '\n'))
                    needsEnclosing = true;

                // Escape the escape characters by repeating them for every instance
                if (c == escapeChar)
                    builder.Append(escapeChar);

                // append the character
                builder.Append(c);
            }
            
            // Enclose the value in escape characters if needed
            if (needsEnclosing)
            {
                builder.Insert(builderIndex, escapeChar);
                builder.Append(escapeChar);
            }

            // Finally, mark the next field as not the first one
            isFirst = false;
        }

        // output the newline sequence to signal the end of the record.
        builder.Append(newLine);

        return builder;
    }
}
