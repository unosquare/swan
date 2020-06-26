using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Swan.Reflection;

namespace Swan.Formatters
{
    /// <summary>
    /// A CSV writer useful for exporting a set of objects.
    /// </summary>
    /// <example>
    /// The following code describes how to save a list of objects into a CSV file.
    /// <code>
    /// using System.Collections.Generic;
    /// using Swan.Formatters;
    ///  
    /// class Example
    /// {
    ///     class Person
    ///     {
    ///         public string Name { get; set; }
    ///         public int Age { get; set; }
    ///     }
    ///     
    ///     static void Main()
    ///     {
    ///         // create a list of people 
    ///         var people = new List&lt;Person&gt;
    ///         {
    ///             new Person { Name = "Artyom", Age = 20 },
    ///             new Person { Name = "Aloy", Age = 18 }
    ///         }
    ///         
    ///         // write items inside file.csv
    ///         CsvWriter.SaveRecords(people, "C:\\Users\\user\\Documents\\file.csv");
    ///         
    ///         // output
    ///         // | Name   | Age |
    ///         // | Artyom | 20  |
    ///         // | Aloy   | 18  |
    ///     }
    /// }
    /// </code>
    /// </example>
    public class CsvWriter : IDisposable
    {
        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();

        private readonly object _syncLock = new object();
        private readonly Stream _outputStream;
        private readonly Encoding _encoding;
        private readonly bool _leaveStreamOpen;
        private bool _isDisposing;
        private ulong _mCount;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvWriter" /> class.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="leaveOpen">if set to <c>true</c> [leave open].</param>
        /// <param name="encoding">The encoding.</param>
        public CsvWriter(Stream outputStream, bool leaveOpen, Encoding encoding)
        {
            _outputStream = outputStream;
            _encoding = encoding;
            _leaveStreamOpen = leaveOpen;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvWriter"/> class.
        /// It automatically closes the stream when disposing this writer.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="encoding">The encoding.</param>
        public CsvWriter(Stream outputStream, Encoding encoding)
            : this(outputStream, false, encoding)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvWriter"/> class.
        /// It uses the Windows 1252 encoding and automatically closes
        /// the stream upon disposing this writer.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        public CsvWriter(Stream outputStream)
            : this(outputStream, false, Definitions.Windows1252Encoding)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvWriter"/> class.
        /// It opens the file given file, automatically closes the stream upon 
        /// disposing of this writer, and uses the Windows 1252 encoding.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public CsvWriter(string filename)
            : this(File.OpenWrite(filename), false, Definitions.Windows1252Encoding)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvWriter"/> class.
        /// It opens the file given file, automatically closes the stream upon 
        /// disposing of this writer, and uses the given text encoding for output.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="encoding">The encoding.</param>
        public CsvWriter(string filename, Encoding encoding)
            : this(File.OpenWrite(filename), false, encoding)
        {
            // placeholder
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the field separator character.
        /// </summary>
        /// <value>
        /// The separator character.
        /// </value>
        public char SeparatorCharacter { get; set; } = ',';

        /// <summary>
        /// Gets or sets the escape character to use to escape field values.
        /// </summary>
        /// <value>
        /// The escape character.
        /// </value>
        public char EscapeCharacter { get; set; } = '"';

        /// <summary>
        /// Gets or sets the new line character sequence to use when writing a line.
        /// </summary>
        /// <value>
        /// The new line sequence.
        /// </value>
        public string NewLineSequence { get; set; } = Environment.NewLine;

        /// <summary>
        /// Defines a list of properties to ignore when outputting CSV lines.
        /// </summary>
        /// <value>
        /// The ignore property names.
        /// </value>
        public List<string> IgnorePropertyNames { get; } = new List<string>();

        /// <summary>
        /// Gets number of lines that have been written, including the headings line.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public ulong Count
        {
            get
            {
                lock (_syncLock)
                {
                    return _mCount;
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Saves the items to a stream.
        /// It uses the Windows 1252 text encoding for output.
        /// </summary>
        /// <typeparam name="T">The type of enumeration.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="truncateData"><c>true</c> if stream is truncated, default <c>false</c>.</param>
        /// <returns>Number of item saved.</returns>
        public static int SaveRecords<T>(IEnumerable<T> items, Stream stream, bool truncateData = false)
        {
            // truncate the file if it had data
            if (truncateData && stream.Length > 0)
                stream.SetLength(0);

            using var writer = new CsvWriter(stream);
            writer.WriteHeadings<T>();
            writer.WriteObjects(items);
            return (int)writer.Count;
        }

        /// <summary>
        /// Saves the items to a CSV file.
        /// If the file exits, it overwrites it. If it does not, it creates it.
        /// It uses the Windows 1252 text encoding for output.
        /// </summary>
        /// <typeparam name="T">The type of enumeration.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>Number of item saved.</returns>
        public static int SaveRecords<T>(IEnumerable<T> items, string filePath) => SaveRecords(items, File.OpenWrite(filePath), true);

        #endregion

        #region Generic, main Write Line Method

        /// <summary>
        /// Writes a line of CSV text. Items are converted to strings.
        /// If items are found to be null, empty strings are written out.
        /// If items are not string, the ToStringInvariant() method is called on them.
        /// </summary>
        /// <param name="items">The items.</param>
        public void WriteLine(params object[] items)
            => WriteLine(items.Select(x => x == null ? string.Empty : x.ToStringInvariant()));
        
        /// <summary>
        /// Writes a line of CSV text. Items are converted to strings.
        /// If items are found to be null, empty strings are written out.
        /// If items are not string, the ToStringInvariant() method is called on them.
        /// </summary>
        /// <param name="items">The items.</param>
        public void WriteLine(IEnumerable<object> items)
            => WriteLine(items.Select(x => x == null ? string.Empty : x.ToStringInvariant()));

        /// <summary>
        /// Writes a line of CSV text.
        /// If items are found to be null, empty strings are written out.
        /// </summary>
        /// <param name="items">The items.</param>
        public void WriteLine(params string[] items) => WriteLine((IEnumerable<string>) items);

        /// <summary>
        /// Writes a line of CSV text.
        /// If items are found to be null, empty strings are written out.
        /// </summary>
        /// <param name="items">The items.</param>
        public void WriteLine(IEnumerable<string> items)
        {
            lock (_syncLock)
            {   
                var length = items.Count();
                var separatorBytes = _encoding.GetBytes(new[] { SeparatorCharacter });
                var endOfLineBytes = _encoding.GetBytes(NewLineSequence);

                // Declare state variables here to avoid recreation, allocation and
                // reassignment in every loop
                bool needsEnclosing;
                string textValue;
                byte[] output;

                for (var i = 0; i < length; i++)
                {
                    textValue = items.ElementAt(i);

                    // Determine if we need the string to be enclosed 
                    // (it either contains an escape, new line, or separator char)
                    needsEnclosing = textValue.IndexOf(SeparatorCharacter) >= 0
                                     || textValue.IndexOf(EscapeCharacter) >= 0
                                     || textValue.IndexOf('\r') >= 0
                                     || textValue.IndexOf('\n') >= 0;

                    // Escape the escape characters by repeating them twice for every instance
                    textValue = textValue.Replace($"{EscapeCharacter}",
                        $"{EscapeCharacter}{EscapeCharacter}");

                    // Enclose the text value if we need to
                    if (needsEnclosing)
                        textValue = string.Format($"{EscapeCharacter}{textValue}{EscapeCharacter}", textValue);

                    // Get the bytes to write to the stream and write them
                    output = _encoding.GetBytes(textValue);
                    _outputStream.Write(output, 0, output.Length);

                    // only write a separator if we are moving in between values.
                    // the last value should not be written.
                    if (i < length - 1)
                        _outputStream.Write(separatorBytes, 0, separatorBytes.Length);
                }

                // output the newline sequence
                _outputStream.Write(endOfLineBytes, 0, endOfLineBytes.Length);
                _mCount += 1;
            }
        }

        #endregion

        #region Write Object Method

        /// <summary>
        /// Writes a row of CSV text. It handles the special cases where the object is
        /// a dynamic object or and array. It also handles non-collection objects fine.
        /// If you do not like the way the output is handled, you can simply write an extension
        /// method of this class and use the WriteLine method instead.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <exception cref="System.ArgumentNullException">item.</exception>
        public void WriteObject(object? item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            lock (_syncLock)
            {
                switch (item)
                {
                    case IDictionary typedItem:
                        WriteLine(GetFilteredDictionary(typedItem));
                        return;
                    case ICollection typedItem:
                        WriteLine(typedItem.Cast<object>());
                        return;
                    default:
                        WriteLine(GetFilteredTypeProperties(item.GetType()).Select(x => item.ReadProperty(x.Name)));
                        break;
                }
            }
        }

        /// <summary>
        /// Writes a row of CSV text. It handles the special cases where the object is
        /// a dynamic object or and array. It also handles non-collection objects fine.
        /// If you do not like the way the output is handled, you can simply write an extension
        /// method of this class and use the WriteLine method instead.
        /// </summary>
        /// <typeparam name="T">The type of object to write.</typeparam>
        /// <param name="item">The item.</param>
        public void WriteObject<T>(T item) => WriteObject(item as object);

        /// <summary>
        /// Writes a set of items, one per line and atomically by repeatedly calling the
        /// WriteObject method. For more info check out the description of the WriteObject
        /// method.
        /// </summary>
        /// <typeparam name="T">The type of object to write.</typeparam>
        /// <param name="items">The items.</param>
        /// <exception cref="ArgumentNullException">items.</exception>
        public void WriteObjects<T>(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            lock (_syncLock)
            {
                foreach (var item in items)
                    WriteObject(item);
            }
        }

        #endregion

        #region Write Headings Methods

        /// <summary>
        /// Writes the headings.
        /// </summary>
        /// <param name="type">The type of object to extract headings.</param>
        /// <exception cref="System.ArgumentNullException">type.</exception>
        public void WriteHeadings(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var properties = GetFilteredTypeProperties(type).Select(p => p.Name.Humanize()).Cast<object>();
            WriteLine(properties);
        }

        /// <summary>
        /// Writes the headings.
        /// </summary>
        /// <typeparam name="T">The type of object to extract headings.</typeparam>
        public void WriteHeadings<T>() => WriteHeadings(typeof(T));

        /// <summary>
        /// Writes the headings.
        /// </summary>
        /// <param name="dictionary">The dictionary to extract headings.</param>
        /// <exception cref="System.ArgumentNullException">dictionary.</exception>
        public void WriteHeadings(IDictionary dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            WriteLine(GetFilteredDictionary(dictionary, true));
        }
        
        /// <summary>
        /// Writes the headings.
        /// </summary>
        /// <param name="obj">The object to extract headings.</param>
        /// <exception cref="ArgumentNullException">obj.</exception>
        public void WriteHeadings(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            WriteHeadings(obj.GetType());
        }

        #endregion

        #region IDisposable Support

        /// <inheritdoc />
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposeAlsoManaged"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposeAlsoManaged)
        {
            if (_isDisposing) return;

            if (disposeAlsoManaged)
            {
                if (_leaveStreamOpen == false)
                {
                    _outputStream.Dispose();
                }
            }

            _isDisposing = true;
        }

        #endregion

        #region Support Methods
        
        private IEnumerable<string> GetFilteredDictionary(IDictionary dictionary, bool filterKeys = false)
            => dictionary
                .Keys
                .Cast<object>()
                .Select(key => key == null ? string.Empty : key.ToStringInvariant())
                .Where(stringKey => !IgnorePropertyNames.Contains(stringKey))
                .Select(stringKey =>
                    filterKeys
                        ? stringKey
                        : dictionary[stringKey] == null ? string.Empty : dictionary[stringKey].ToStringInvariant());

        private IEnumerable<PropertyInfo> GetFilteredTypeProperties(Type type)
            => TypeCache.Retrieve(type, t =>
                    t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanRead))
                .Where(p => !IgnorePropertyNames.Contains(p.Name));

        #endregion

    }
}