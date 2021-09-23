using Swan.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
    }
}
