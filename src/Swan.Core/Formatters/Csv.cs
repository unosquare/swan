using Swan.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Swan.Formatters
{
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
        /// <returns>A an awaitable task with a list of objects parsed from the underlying stream.</returns>
        public static async ValueTask<IList<TRecord>> LoadAsync<TRecord>(Stream stream, Encoding? encoding = default)
            where TRecord : class, new()
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            await using var reader = new CsvObjectReader<TRecord>(stream, encoding);
            var result = new List<TRecord>(1024);
            await foreach (var item in reader)
                result.Add(item);

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
        /// <returns>A list of objects parsed from the underlying stream.</returns>
        public static async ValueTask<IList<TRecord>> LoadAsync<TRecord>(string filePath, Encoding? encoding = default)
            where TRecord : class, new()
        {
            await using var stream = File.OpenRead(filePath);
            return await LoadAsync<TRecord>(stream, encoding);
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
        /// <returns>A list of objects parsed from the underlying stream.</returns>
        public static async ValueTask<IList<dynamic>> LoadAsync(Stream stream, Encoding? encoding = default)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            var result = new List<dynamic>(1024);
            await using var reader = new CsvDynamicReader(stream, encoding);
            await foreach (var item in reader)
                result.Add(item);

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

        public static long Save<T>(IEnumerable<T> items, Stream stream, Encoding? encoding = default)
        {
            using var writer = new CsvWriter<T>(stream, encoding);
            writer.WriteLines(items);
            return writer.Count;
        }

        public static async ValueTask<long> SaveAsync<T>(IEnumerable<T> items, Stream stream, Encoding? encoding = default)
        {
            await using var writer = new CsvWriter<T>(stream, encoding);
            await writer.WriteLinesAsync(items);
            return writer.Count;
        }

        public static long Save<T>(IEnumerable<T> items, string filePath, Encoding? encoding = default)
        {
            using var fileStream = File.OpenWrite(filePath);
            return Save(items, fileStream, encoding);
        }

        public static async ValueTask<long> SaveAsync<T>(IEnumerable<T> items, string filePath, Encoding? encoding = default)
        {
            await using var fileStream = File.OpenWrite(filePath);
            return await SaveAsync(items, fileStream, encoding);
        }
    }
}
