using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Swan.Reflection;

namespace Swan.Formatters
{
    /// <summary>
    /// Represents a reader designed for CSV text.
    /// It is capable of deserializing objects from individual lines of CSV text,
    /// transforming CSV lines of text into objects,
    /// or simply reading the lines of CSV as an array of strings.
    /// </summary>
    /// <seealso cref="System.IDisposable" /> 
    /// <example>
    /// The following example describes how to load a list of objects from a CSV file.
    /// <code>
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
    ///         // load records from a CSV file
    ///        var loadedRecords = 
    ///        CsvReader.LoadRecords&lt;Person&gt;("C:\\Users\\user\\Documents\\file.csv");
    ///         
    ///         // loadedRecords = 
    ///         //  [
    ///         //      { Age = 20, Name = "George" }
    ///         //      { Age = 18, Name = "Juan" }
    ///         //  ]
    ///     }
    /// }
    /// </code>
    /// The following code explains how to read a CSV formatted string.
    /// <code>
    /// using Swan.Formatters;
    /// using System.Text;
    /// using Swan.Formatters;
    ///  
    /// class Example
    /// {
    ///     static void Main()
    ///     {
    ///         // data to be read
    ///         var data = @"Company,OpenPositions,MainTechnology,Revenue
    ///         Co,2,""C#, MySQL, JavaScript, HTML5 and CSS3"",500
    ///         Ca,2,""C#, MySQL, JavaScript, HTML5 and CSS3"",600";
    ///         
    ///         using(var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
    ///         {
    ///             // create a CSV reader
    ///             var reader = new CsvReader(stream, false, Encoding.UTF8);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class CsvReader : IDisposable
    {
        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();

        private readonly object _syncLock = new object();
        
        private ulong _count;
        private char _escapeCharacter = '"';
        private char _separatorCharacter = ',';
        
        private bool _hasDisposed; // To detect redundant calls
        private string[]? _headings;
        private Dictionary<string, string>? _defaultMap;
        private StreamReader? _reader;
        
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader" /> class.
        /// </summary>
        /// <param name="inputStream">The stream.</param>
        /// <param name="leaveOpen">if set to <c>true</c> leaves the input stream open.</param>
        /// <param name="textEncoding">The text encoding.</param>
        public CsvReader(Stream inputStream, bool leaveOpen, Encoding textEncoding)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));

            if (textEncoding == null)
                throw new ArgumentNullException(nameof(textEncoding));
            
            _reader = new StreamReader(inputStream, textEncoding, true, 2048, leaveOpen);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// It will automatically close the stream upon disposing.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="textEncoding">The text encoding.</param>
        public CsvReader(Stream stream, Encoding textEncoding)
            : this(stream, false, textEncoding)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// It automatically closes the stream when disposing this reader
        /// and uses the Windows 1253 encoding.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public CsvReader(Stream stream)
            : this(stream, false, Definitions.Windows1252Encoding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// It uses the Windows 1252 Encoding by default and it automatically closes the file
        /// when this reader is disposed of.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public CsvReader(string filename)
            : this(File.OpenRead(filename), false, Definitions.Windows1252Encoding)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// It automatically closes the file when disposing this reader.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="encoding">The encoding.</param>
        public CsvReader(string filename, Encoding encoding)
            : this(File.OpenRead(filename), false, encoding)
        {
            // placeholder
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of lines that have been read, including the headings.
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
                    return _count;
                }
            }
        }

        /// <summary>
        /// Gets or sets the escape character.
        /// By default it is the double quote '"'.
        /// </summary>
        /// <value>
        /// The escape character.
        /// </value>
        public char EscapeCharacter
        {
            get => _escapeCharacter;
            set
            {
                lock (_syncLock)
                {
                    _escapeCharacter = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the separator character.
        /// By default it is the comma character ','.
        /// </summary>
        /// <value>
        /// The separator character.
        /// </value>
        public char SeparatorCharacter
        {
            get => _separatorCharacter;
            set
            {
                lock (_syncLock)
                {
                    _separatorCharacter = value;
                }
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether the stream reader is at the end of the stream
        /// In other words, if no more data can be read, this will be set to true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [end of stream]; otherwise, <c>false</c>.
        /// </value>
        public bool EndOfStream
        {
            get
            {
                lock (_syncLock)
                {
                    return _reader?.EndOfStream ?? true;
                }
            }
        }

        #endregion

        #region Generic, Main ReadLine method

        /// <summary>
        /// Reads a line of CSV text into an array of strings.
        /// </summary>
        /// <returns>An array of the specified element type containing copies of the elements of the ArrayList.</returns>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream.</exception>
        public string[] ReadLine()
        {
            if (EndOfStream)
                throw new EndOfStreamException("Cannot read past the end of the stream");

            lock (_syncLock)
            {
                var values = ParseRecord(_reader!, _escapeCharacter, _separatorCharacter);
                _count++;
                return values;
            }
        }

        #endregion

        #region Read Methods

        /// <summary>
        /// Skips a line of CSV text.
        /// This operation does not increment the Count property and it is useful when you need to read the headings
        /// skipping over a few lines as Reading headings is only supported
        /// as the first read operation (i.e. while count is still 0).
        /// </summary>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream.</exception>
        public void SkipRecord()
        {
            if (EndOfStream)
                throw new EndOfStreamException("Cannot read past the end of the stream");

            lock (_syncLock)
            {
                ParseRecord(_reader!, _escapeCharacter, _separatorCharacter);
            }
        }

        /// <summary>
        /// Reads a line of CSV text and stores the values read as a representation of the column names
        /// to be used for parsing objects. You have to call this method before calling ReadObject methods.
        /// </summary>
        /// <returns>An array of the specified element type containing copies of the elements of the ArrayList.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Reading headings is only supported as the first read operation.
        /// or
        /// ReadHeadings.
        /// </exception>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream.</exception>
        public string[] ReadHeadings()
        {
            lock (_syncLock)
            {
                if (_headings != null)
                    throw new InvalidOperationException($"The {nameof(ReadHeadings)} method had already been called.");

                if (_count != 0)
                    throw new InvalidOperationException("Reading headings is only supported as the first read operation.");
                
                _headings = ReadLine();
                _defaultMap = _headings.ToDictionary(x => x, x => x);

                return _headings.ToArray();
            }
        }

        /// <summary>
        /// Reads a line of CSV text, converting it into a dynamic object in which properties correspond to the names of the headings.
        /// </summary>
        /// <param name="map">The mappings between CSV headings (keys) and object properties (values).</param>
        /// <returns>Object of the type of the elements in the collection of key/value pairs.</returns>
        /// <exception cref="System.InvalidOperationException">ReadHeadings.</exception>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream.</exception>
        /// <exception cref="System.ArgumentNullException">map.</exception>
        public IDictionary<string, object> ReadObject(IDictionary<string, string> map)
        {
            lock (_syncLock)
            {
                if (_headings == null)
                    throw new InvalidOperationException($"Call the {nameof(ReadHeadings)} method before reading as an object.");

                if (map == null)
                    throw new ArgumentNullException(nameof(map));

                var result = new Dictionary<string, object>();
                var values = ReadLine();

                for (var i = 0; i < _headings.Length; i++)
                {
                    if (i > values.Length - 1)
                        break;

                    result[_headings[i]] = values[i];
                }

                return result;
            }
        }

        /// <summary>
        /// Reads a line of CSV text, converting it into a dynamic object
        /// The property names correspond to the names of the CSV headings.
        /// </summary>
        /// <returns>Object of the type of the elements in the collection of key/value pairs.</returns>
        public IDictionary<string, object> ReadObject() => ReadObject(_defaultMap);

        /// <summary>
        /// Reads a line of CSV text converting it into an object of the given type, using a map (or Dictionary)
        /// where the keys are the names of the headings and the values are the names of the instance properties
        /// in the given Type. The result object must be already instantiated.
        /// </summary>
        /// <typeparam name="T">The type of object to map.</typeparam>
        /// <param name="map">The map.</param>
        /// <param name="result">The result.</param>
        /// <exception cref="System.ArgumentNullException">map
        /// or
        /// result.</exception>
        /// <exception cref="System.InvalidOperationException">ReadHeadings.</exception>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream.</exception>
        public void ReadObject<T>(IDictionary<string, string> map, ref T result)
        {
            lock (_syncLock)
            {
                // Check arguments
                {
                    if (map == null)
                        throw new ArgumentNullException(nameof(map));

                    if (_reader.EndOfStream)
                        throw new EndOfStreamException("Cannot read past the end of the stream");

                    if (_headings == null)
                        throw new InvalidOperationException($"Call the {nameof(ReadHeadings)} method before reading as an object.");
                    
                    if (Equals(result, default(T)))
                        throw new ArgumentNullException(nameof(result));
                }

                // Read line and extract values
                var values = ReadLine();

                // Extract properties from cache
                var properties = TypeCache
                    .RetrieveFilteredProperties(typeof(T), true, x => x.CanWrite && Definitions.BasicTypesInfo.Value.ContainsKey(x.PropertyType));

                // Assign property values for each heading
                for (var i = 0; i < _headings.Length; i++)
                {
                    // break if no more headings are matched
                    if (i > values.Length - 1)
                        break;

                    // skip if no heading is available or the heading is empty
                    if (map.ContainsKey(_headings[i]) == false &&
                        string.IsNullOrWhiteSpace(map[_headings[i]]) == false)
                        continue;

                    // Prepare the target property
                    var propertyName = map[_headings[i]];
                    
                    // Parse and assign the basic type value to the property if exists
                    properties
                        .FirstOrDefault(p => p.Name == propertyName)?
                        .TrySetBasicType(values[i], result);
                }
            }
        }

        /// <summary>
        /// Reads a line of CSV text converting it into an object of the given type, using a map (or Dictionary)
        /// where the keys are the names of the headings and the values are the names of the instance properties
        /// in the given Type.
        /// </summary>
        /// <typeparam name="T">The type of object to map.</typeparam>
        /// <param name="map">The map of CSV headings (keys) and Type property names (values).</param>
        /// <returns>The conversion of specific type of object.</returns>
        /// <exception cref="System.ArgumentNullException">map.</exception>
        /// <exception cref="System.InvalidOperationException">ReadHeadings.</exception>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream.</exception>
        public T ReadObject<T>(IDictionary<string, string> map)
            where T : new()
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            var result = Activator.CreateInstance<T>();
            ReadObject(map, ref result);
            return result;
        }

        /// <summary>
        /// Reads a line of CSV text converting it into an object of the given type, and assuming
        /// the property names of the target type match the heading names of the file.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <returns>The conversion of specific type of object.</returns>
        public T ReadObject<T>()
            where T : new() =>
            ReadObject<T>(_defaultMap);

        #endregion

        #region Support Methods

        /// <summary>
        /// Parses a line of standard CSV text into an array of strings.
        /// Note that quoted values might have new line sequences in them. Field values will contain such sequences.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="escapeCharacter">The escape character.</param>
        /// <param name="separatorCharacter">The separator character.</param>
        /// <returns>An array of the specified element type containing copies of the elements of the ArrayList.</returns>
        private static string[] ParseRecord(StreamReader reader, char escapeCharacter = '"', char separatorCharacter = ',')
        {
            var values = new List<string>();
            var currentValue = new StringBuilder(1024);
            var currentState = ReadState.WaitingForNewField;
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                for (var charIndex = 0; charIndex < line.Length; charIndex++)
                {
                    // Get the current and next character
                    var currentChar = line[charIndex];
                    var nextChar = charIndex < line.Length - 1 ? line[charIndex + 1] : default(char?);

                    // Perform logic based on state and decide on next state
                    switch (currentState)
                    {
                        case ReadState.WaitingForNewField:
                            {
                                currentValue.Clear();

                                if (currentChar == escapeCharacter)
                                {
                                    currentState = ReadState.PushingQuoted;
                                    continue;
                                }

                                if (currentChar == separatorCharacter)
                                {
                                    values.Add(currentValue.ToString());
                                    currentState = ReadState.WaitingForNewField;
                                    continue;
                                }

                                currentValue.Append(currentChar);
                                currentState = ReadState.PushingNormal;
                                continue;
                            }

                        case ReadState.PushingNormal:
                            {
                                // Handle field content delimiter by comma
                                if (currentChar == separatorCharacter)
                                {
                                    currentState = ReadState.WaitingForNewField;
                                    values.Add(currentValue.ToString());
                                    currentValue.Clear();
                                    continue;
                                }

                                // Handle double quote escaping
                                if (currentChar == escapeCharacter && nextChar.HasValue && nextChar == escapeCharacter)
                                {
                                    // advance 1 character now. The loop will advance one more.
                                    currentValue.Append(currentChar);
                                    charIndex++;
                                    continue;
                                }

                                currentValue.Append(currentChar);
                                break;
                            }

                        case ReadState.PushingQuoted:
                            {
                                // Handle field content delimiter by ending double quotes
                                if (currentChar == escapeCharacter && (nextChar.HasValue == false || nextChar != escapeCharacter))
                                {
                                    currentState = ReadState.PushingNormal;
                                    continue;
                                }

                                // Handle double quote escaping
                                if (currentChar == escapeCharacter && nextChar.HasValue && nextChar == escapeCharacter)
                                {
                                    // advance 1 character now. The loop will advance one more.
                                    currentValue.Append(currentChar);
                                    charIndex++;
                                    continue;
                                }

                                currentValue.Append(currentChar);
                                break;
                            }
                    }
                }

                // determine if we need to continue reading a new line if it is part of the quoted
                // field value
                if (currentState == ReadState.PushingQuoted)
                {
                    // we need to add the new line sequence to the output of the field
                    // because we were pushing a quoted value
                    currentValue.Append(Environment.NewLine);
                }
                else
                {
                    // push anything that has not been pushed (flush) into a last value
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                    
                    // stop reading more lines we have reached the end of the CSV record
                    break;
                }
            }

            // If we ended up pushing quoted and no closing quotes we might
            // have additional text in yt 
            if (currentValue.Length > 0)
            {
                values.Add(currentValue.ToString());
            }

            return values.ToArray();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Loads the records from the stream
        /// This method uses Windows 1252 encoding.
        /// </summary>
        /// <typeparam name="T">The type of IList items to load.</typeparam>
        /// <param name="stream">The stream.</param>
        /// <returns>A generic collection of objects that can be individually accessed by index.</returns>
        public static IList<T> LoadRecords<T>(Stream stream)
            where T : new()
        {
            var result = new List<T>();

            using (var reader = new CsvReader(stream))
            {
                reader.ReadHeadings();
                while (!reader.EndOfStream)
                {
                    result.Add(reader.ReadObject<T>());
                }
            }

            return result;
        }

        /// <summary>
        /// Loads the records from the give file path.
        /// This method uses Windows 1252 encoding.
        /// </summary>
        /// <typeparam name="T">The type of IList items to load.</typeparam>
        /// <param name="filePath">The file path.</param>
        /// <returns>A generic collection of objects that can be individually accessed by index.</returns>
        public static IList<T> LoadRecords<T>(string filePath)
            where T : new() =>
            LoadRecords<T>(File.OpenRead(filePath));

        #endregion

        #region IDisposable Support

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_hasDisposed) return;

            if (disposing)
            {
                try
                {
                    _reader?.Dispose();
                }
                finally
                {
                    _reader = null;
                }
            }

            _hasDisposed = true;
        }
        
        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
        
        /// <summary>
        /// Defines the 3 different read states
        /// for the parsing state machine.
        /// </summary>
        private enum ReadState
        {
            WaitingForNewField,
            PushingNormal,
            PushingQuoted,
        }
    }
}
