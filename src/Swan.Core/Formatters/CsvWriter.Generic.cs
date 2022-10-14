namespace Swan.Formatters;

using Reflection;
using System.IO;

/// <summary>
/// Represents a CSV writer that can transform objects into their corresponding CSV representation.
/// </summary>
/// <typeparam name="T">The type of item to write.</typeparam>
public class CsvWriter<T> : CsvWriter
{
    private readonly ITypeInfo _typeInfo = typeof(T).TypeInfo();
    private readonly Dictionary<string, Func<T, string>> _propertyMap;

    /// <summary>
    /// Creates a new instance of the <see cref="CsvWriter{T}"/> class.
    /// </summary>
    /// <param name="outputStream">The output stream.</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="writesHeadings">If the writer automatically writes the mapped headings.</param>
    /// <param name="separatorChar">The field separator character.</param>
    /// <param name="escapeChar">The escape character.</param>
    /// <param name="newLineSequence">Specifies the new line character sequence.</param>
    /// <param name="leaveOpen">true to leave the stream open after the stream reader object is disposed; otherwise, false.</param>
    public CsvWriter(Stream outputStream,
        Encoding? encoding = default,
        bool writesHeadings = true,
        char separatorChar = Csv.DefaultSeparatorChar,
        char escapeChar = Csv.DefaultEscapeChar,
        string? newLineSequence = default,
        bool? leaveOpen = default)
        : base(outputStream, encoding, separatorChar, escapeChar, newLineSequence, leaveOpen)
    {
        WritesHeadings = writesHeadings;
        _propertyMap = _typeInfo.Properties()
            .Where(p =>
                p.CanRead &&
                p.PropertyType.IsBasicType &&
                !p.PropertyName.Contains('.', StringComparison.Ordinal))
            .ToDictionary(
                p => p.PropertyName,
                p => new Func<T, string>((instance) =>
                {
                    if (instance is null || !p.TryRead(instance, out var value))
                        return string.Empty;

                    return p.PropertyType.ToStringInvariant(value);
                }));
    }

    /// <summary>
    /// Gets a dictionary where keys are the output headings and values are the
    /// method calls to produce the values.
    /// </summary>
    public IReadOnlyDictionary<string, Func<T, string>> PropertyMap => _propertyMap;

    /// <summary>
    /// Gets a value indicating whether headings have been written out to the output stream.
    /// </summary>
    public bool HasWrittenHeadings { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the writer writes out the headings.
    /// </summary>
    public bool WritesHeadings { get; }

    /// <summary>
    /// Removes a mapping to the specified heading.
    /// </summary>
    /// <param name="headingName">The name of the heading to remove.</param>
    /// <returns>This instance, in order to enable fluent API.</returns>
    public CsvWriter<T> RemoveMapping(string headingName)
    {
        if (headingName is null)
            throw new ArgumentNullException(nameof(headingName));

        if (HasWrittenHeadings)
            throw new InvalidOperationException("Cannot change headings once they have been written.");

        _propertyMap.Remove(headingName);
        return this;
    }

    /// <summary>
    /// Removes all the mappings for the headings.
    /// </summary>
    /// <returns>This instance, in order to enable fluent API.</returns>
    /// <exception cref="InvalidOperationException">Operation is not permitted when headings have already been written.</exception>
    public CsvWriter<T> ClearMappings()
    {
        if (HasWrittenHeadings)
            throw new InvalidOperationException("Cannot change headings once they have been written.");

        _propertyMap.Clear();
        return this;
    }

    /// <summary>
    /// Adds or replaces a mapping between a heading and a source transform.
    /// </summary>
    /// <param name="headingName">The name of the heading to map to.</param>
    /// <param name="valueProvider">The optional value provider method to transform the source value into a string fro the given heading.</param>
    /// <returns>This instance, in order to enable fluent API.</returns>
    public CsvWriter<T> AddMapping(string headingName, Func<T, string> valueProvider)
    {
        if (headingName is null)
            throw new ArgumentNullException(nameof(headingName));

        if (HasWrittenHeadings)
            throw new InvalidOperationException("Cannot change headings once they have been written.");

        _propertyMap[headingName] = valueProvider;

        return this;
    }

    /// <summary>
    /// Writes an object as a set of CSV strings.
    /// </summary>
    /// <param name="item">The object to write.</param>
    public void WriteLine(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (!HasWrittenHeadings)
        {
            if (WritesHeadings)
                WriteLine(_propertyMap.Keys.ToArray());

            HasWrittenHeadings = true;
        }

        var values = _propertyMap.Select(kvp => kvp.Value.Invoke(item)).ToArray();
        WriteLine(values);
    }

    /// <summary>
    /// Writes an object as a set of CSV strings.
    /// </summary>
    /// <param name="item">The object to write.</param>
    public async ValueTask WriteLineAsync(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (!HasWrittenHeadings)
        {
            if (WritesHeadings)
                await WriteLineAsync(_propertyMap.Keys.ToArray()).ConfigureAwait(false);

            HasWrittenHeadings = true;
        }

        var values = _propertyMap.Select(kvp => kvp.Value.Invoke(item)).ToArray();
        await WriteLineAsync(values).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes multiple objects as a set of CSV strings.
    /// </summary>
    /// <param name="items">The objects to write.</param>
    public void WriteLines(IEnumerable<T> items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
            WriteLine(item);

        Flush();
    }

    /// <summary>
    /// Writes multiple objects as a set of CSV strings.
    /// </summary>
    /// <param name="items">The objects to write.</param>
    /// <param name="ct">The optional cancellation token.</param>
    public async ValueTask WriteLinesAsync(IEnumerable<T> items, CancellationToken ct = default)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
        {
            await WriteLineAsync(item).ConfigureAwait(false);
            if (ct.IsCancellationRequested)
                break;
        }

        await FlushAsync().ConfigureAwait(false);
    }
}
