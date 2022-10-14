namespace Swan.Formatters;

using Platform;
using System.IO;

/// <summary>
/// Provides methods for CSV reading and writing.
/// </summary>
public static class Csv
{
    /// <summary>
    /// Provides a the default separator character.
    /// </summary>
    public const char DefaultSeparatorChar = ',';

    /// <summary>
    /// Provides the default escape character.
    /// </summary>
    public const char DefaultEscapeChar = '"';

    /// <summary>
    /// The MIME type according to the RFC 4180 spec.
    /// </summary>
    public const string MimeType = "text/csv";

    /// <summary>
    /// Gets the default encoding used by CSV readers
    /// whenever an encoding is not specified.
    /// </summary>
    public static Encoding DefaultEncoding { get; } = SwanRuntime.Windows1252Encoding;

    /// <summary>
    /// Reads all the records from the stream as a list of objects using matching headings
    /// to property names.
    /// </summary>
    /// <typeparam name="TRecord">The type of the target object.</typeparam>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="encoding">The optional encoding.</param>
    /// <returns>A list of objects parsed from the underlying stream.</returns>
    public static IList<TRecord> Load<TRecord>(Stream stream, Encoding? encoding = default)
        where TRecord : class, new()
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new CsvObjectReader<TRecord>(stream, encoding);
        var result = new List<TRecord>(1024);
        result.AddRange(reader);
        return result;
    }

    /// <summary>
    /// Reads all the records from the stream as a list of objects using matching headings
    /// to property names.
    /// </summary>
    /// <typeparam name="TRecord">The type of the target object.</typeparam>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="encoding">The optional encoding.</param>
    /// <param name="ct">The optional cancellation token.</param>
    /// <returns>A list of objects parsed from the underlying stream.</returns>
    public static async Task<IList<TRecord>> LoadAsync<TRecord>(Stream stream, Encoding? encoding = default, CancellationToken ct = default)
        where TRecord : class, new()
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new CsvObjectReader<TRecord>(stream, encoding);
        reader.WithCancellation(ct);
        var result = new List<TRecord>(1024);

        await foreach(var item in reader)
        {
            if (item is null)
                continue;

            result.Add(item);
        }
        
        return result;
    }

    /// <summary>
    /// Reads all the records from the stream as a list of objects using matching headings
    /// to property names.
    /// </summary>
    /// <typeparam name="TRecord">The type of the target object.</typeparam>
    /// <param name="filePath">The path to the file to read from.</param>
    /// <param name="encoding">The optional encoding.</param>
    /// <returns>A list of objects parsed from the underlying stream.</returns>
    public static IList<TRecord> Load<TRecord>(string filePath, Encoding? encoding = default)
        where TRecord : class, new()
    {
        using var stream = File.OpenRead(filePath);
        return Load<TRecord>(stream, encoding);
    }

    /// <summary>
    /// Reads all the records from the stream as a list of objects using matching headings
    /// to property names.
    /// </summary>
    /// <typeparam name="TRecord">The type of the target object.</typeparam>
    /// <param name="filePath">The path to the file to read from.</param>
    /// <param name="encoding">The optional encoding.</param>
    /// <param name="ct">The optional cancellation token.</param>
    /// <returns>A list of objects parsed from the underlying stream.</returns>
    public static async Task<IList<TRecord>> LoadAsync<TRecord>(string filePath, Encoding? encoding = default, CancellationToken ct = default)
        where TRecord : class, new()
    {
        using var stream = File.OpenRead(filePath);
        return await LoadAsync<TRecord>(stream, encoding, ct);
    }

    /// <summary>
    /// Reads all the records from the stream as a list of expando objects.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="encoding">The optional encoding.</param>
    /// <returns>A list of objects parsed from the underlying stream.</returns>
    public static IList<dynamic> Load(Stream stream, Encoding? encoding = default)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        var result = new List<dynamic>(1024);
        using var reader = new CsvDynamicReader(stream, encoding);
        result.AddRange(reader);

        return result;
    }

    /// <summary>
    /// Reads all the records from the stream as a list of expando objects.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="encoding">The optional encoding.</param>
    /// <param name="ct">The optional cancellation token.</param>
    /// <returns>A list of objects parsed from the underlying stream.</returns>
    public static async Task<IList<dynamic>> LoadAsync(Stream stream, Encoding? encoding = default, CancellationToken ct = default)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new CsvDynamicReader(stream, encoding);
        reader.WithCancellation(ct);
        var result = new List<dynamic>(1024);

        await foreach (var item in reader)
        {
            if (item is null)
                continue;

            result.Add(item);
        }

        return result;
    }

    /// <summary>
    /// Reads all the records from the stream as a list of objects using matching headings
    /// to property names.
    /// </summary>
    /// <param name="filePath">The path to the file to read from.</param>
    /// <param name="encoding">The optional encoding.</param>
    /// <returns>A list of objects parsed from the underlying stream.</returns>
    public static IList<dynamic> Load(string filePath, Encoding? encoding = default)
    {
        using var stream = File.OpenRead(filePath);
        return Load(stream, encoding);
    }

    /// <summary>
    /// Reads all the records from the stream as a list of objects using matching headings
    /// to property names.
    /// </summary>
    /// <param name="filePath">The path to the file to read from.</param>
    /// <param name="encoding">The optional encoding.</param>
    /// <param name="ct">The optional cancellation token.</param>
    /// <returns>A list of objects parsed from the underlying stream.</returns>
    public static async Task<IList<dynamic>> LoadAsync(string filePath, Encoding? encoding = default, CancellationToken ct = default)
    {
        using var stream = File.OpenRead(filePath);
        return await LoadAsync(stream, encoding, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Saves multiple records to the underlying stream.
    /// </summary>
    /// <typeparam name="T">The type of objects to be written.</typeparam>
    /// <param name="items">A collection of items to be written.</param>
    /// <param name="stream">The target stream to write items into.</param>
    /// <param name="encoding">The encoding to be used.</param>
    /// <param name="writeHeadings">Whether headings should be written out to the file.</param>
    /// <returns>The number of records written to the file, including headings.</returns>
    public static long Save<T>(IEnumerable<T> items, Stream stream, Encoding? encoding = default, bool writeHeadings = true)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var writer = new CsvWriter<T>(stream, encoding, writeHeadings);
        writer.WriteLines(items);
        writer.Flush();
        return writer.Count;
    }

    /// <summary>
    /// Saves multiple records to the underlying stream.
    /// </summary>
    /// <typeparam name="T">The type of objects to be written.</typeparam>
    /// <param name="items">A collection of items to be written.</param>
    /// <param name="stream">The target stream to write items into.</param>
    /// <param name="encoding">The encoding to be used.</param>
    /// <param name="writeHeadings">Whether headings should be written out to the file.</param>
    /// <param name="ct">The optional cancellation token.</param>
    /// <returns>The number of records written to the file, including headings.</returns>
    public static async Task<long> SaveAsync<T>(IEnumerable<T> items, Stream stream, Encoding? encoding = default, bool writeHeadings = true, CancellationToken ct = default)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));

        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        using var writer = new CsvWriter<T>(stream, encoding, writeHeadings);
        writer.ConfigureAwait(false);

        await writer.WriteLinesAsync(items, ct).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
        return writer.Count;
    }

    /// <summary>
    /// Saves multiple records to the specified file.
    /// </summary>
    /// <typeparam name="T">The type of objects to be written.</typeparam>
    /// <param name="items">A collection of items to be written.</param>
    /// <param name="filePath">The path to the file to write to.</param>
    /// <param name="encoding">The encoding to be used.</param>
    /// <param name="truncate">Whether the file contents should be overwritten.</param>
    /// <returns>The number of records written to the file, including headings.</returns>
    public static long Save<T>(IEnumerable<T> items, string filePath, Encoding? encoding = default, bool truncate = true)
    {
        using var fileStream = File.OpenWrite(filePath);
        if (truncate)
            fileStream.SetLength(0);

        return Save(items, fileStream, encoding, fileStream.Length <= 0);
    }

    /// <summary>
    /// Saves multiple records to the specified file.
    /// </summary>
    /// <typeparam name="T">The type of objects to be written.</typeparam>
    /// <param name="items">A collection of items to be written.</param>
    /// <param name="filePath">The path to the file to write to.</param>
    /// <param name="encoding">The encoding to be used.</param>
    /// <param name="truncate">Whether the file contents should be overwritten.</param>
    /// <param name="ct">The optional cancellation token.</param>
    /// <returns>The number of records written to the file, including headings.</returns>
    public static async Task<long> SaveAsync<T>(IEnumerable<T> items, string filePath, Encoding? encoding = default, bool truncate = true, CancellationToken ct = default)
    {
        using var fileStream = File.OpenWrite(filePath);
        if (truncate)
            fileStream.SetLength(0);

        return await SaveAsync(items, fileStream, encoding, fileStream.Length <= 0, ct);
    }
}
