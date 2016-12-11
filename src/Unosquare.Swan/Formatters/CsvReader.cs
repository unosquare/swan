namespace Unosquare.Swan.Formatters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Represents a reader designed for CSV text.
    /// It is capable of deserializing objects from individual lines of CSV text,
    /// transforming CSV lines of text into Expando Objects,
    /// or simply reading the lines of CSV as an array of strings
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class CsvReader : IDisposable
    {
        #region Static Declarations

        static private readonly ConcurrentDictionary<Type, PropertyInfo[]> TypeCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

        #endregion

        #region Property Backing

        private ulong m_Count = 0;
        private char m_EscapeCharacter = '"';
        private char m_SeparatorCharacter = ',';

        #endregion

        #region State Variables

        private readonly object SyncLock = new object();
        private bool HasDisposed = false; // To detect redundant calls
        private string[] Headings = null;
        private Dictionary<string, string> DefaultMap = null;
        private Stream InputStream = null;
        private StreamReader Reader = null;
        private bool LeaveInputStreamOpen = false;

        #endregion

        #region Enumerations

        /// <summary>
        /// Defines the 3 different read states
        /// for the parsing state machine
        /// </summary>
        private enum ReadState
        {
            WaitingForNewField,
            PushingNormal,
            PushingQuoted,
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader" /> class.
        /// </summary>
        /// <param name="inputStream">The stream.</param>
        /// <param name="leaveOpen">if set to <c>true</c> leaves the input stream open</param>
        /// <param name="textEncoding">The text encoding.</param>
        public CsvReader(Stream inputStream, bool leaveOpen, Encoding textEncoding)
        {
            if (inputStream == null)
                throw new NullReferenceException(nameof(inputStream));

            if (textEncoding == null)
                throw new NullReferenceException(nameof(textEncoding));

            InputStream = inputStream;
            LeaveInputStreamOpen = leaveOpen;
            Reader = new StreamReader(inputStream, textEncoding, true, 2048, leaveOpen);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// It will automatically close the stream upn disposing
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
        /// and uses the Windows 1253 encoding
        /// </summary>
        /// <param name="stream">The stream.</param>
        public CsvReader(Stream stream)
            : this(stream, false, Constants.Windows1252Encoding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// It uses the Windows 1252 Encoding by default and it automatically closes the file
        /// when this reader is disposed of.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public CsvReader(string filename)
            : this(File.OpenRead(filename), false, Constants.Windows1252Encoding)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// It automatically closes the file when disposing this reader
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="encoding">The encoding.</param>
        public CsvReader(string filename, Encoding encoding)
            : this(File.OpenRead(filename), false, encoding)
        {
            // placehoder
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of lines that have been read, including the headings
        /// </summary>
        public ulong Count { get { lock (SyncLock) { return m_Count; } } }

        /// <summary>
        /// Gets or sets the escape character.
        /// By default it is the double quote '"'
        /// </summary>
        public char EscapeCharacter
        {
            get { return m_EscapeCharacter; }
            set
            {
                lock (SyncLock)
                {
                    m_EscapeCharacter = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the separator character.
        /// By default it is the comma character ','
        /// </summary>
        public char SeparatorCharacter
        {
            get { return m_SeparatorCharacter; }
            set
            {
                lock (SyncLock)
                {
                    m_SeparatorCharacter = value;
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
                lock (SyncLock)
                {
                    return Reader.EndOfStream;
                }
            }
        }

        #endregion

        #region Generic, Main ReadLine method

        /// <summary>
        /// Reads a line of CSV text into an array of strings
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream</exception>
        public string[] ReadLine()
        {
            lock (SyncLock)
            {
                if (Reader.EndOfStream)
                    throw new EndOfStreamException("Cannot read past the end of the stream");

                var values = ParseRecord(Reader, m_EscapeCharacter, m_SeparatorCharacter);
                m_Count++;
                return values;
            }
        }

        #endregion

        #region Read Methods

        /// <summary>
        /// Skips a line of CSV text.
        /// This operation does not increment the Count property and it is useful when you need to read the headings
        /// skipping over a few lines as Reading headings is only supported as the first read operation (i.e. while count is still 0)
        /// </summary>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream</exception>
        public void SkipRecord()
        {
            lock (SyncLock)
            {
                if (Reader.EndOfStream)
                    throw new EndOfStreamException("Cannot read past the end of the stream");

                var line = ParseRecord(Reader, m_EscapeCharacter, m_SeparatorCharacter);
                return;
            }
        }

        /// <summary>
        /// Reads a line of CSV text and stores the values read as a representation of the column names
        /// to be used for parsing objects. You have to call this method before calling ReadObject methods.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">
        /// Reading headings is only supported as the first read operation.
        /// or
        /// ReadHeadings
        /// </exception>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream</exception>
        public string[] ReadHeadings()
        {
            lock (SyncLock)
            {
                if (m_Count != 0)
                    throw new InvalidOperationException("Reading headings is only supported as the first read operation.");

                if (Headings != null)
                    throw new InvalidOperationException($"The {nameof(ReadHeadings)} method had already been called.");

                Headings = ReadLine();
                DefaultMap = new Dictionary<string, string>();
                foreach (var heading in Headings)
                {
                    DefaultMap[heading] = heading;
                }

                return Headings.ToArray();
            }
        }

        /// <summary>
        /// Reads a line of CSV text, converting it into a dynamic object in which properties correspond to the names of the headings
        /// </summary>
        /// <param name="map">The mapppings between CSV headings (keys) and object properties (values)</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">ReadHeadings</exception>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream</exception>
        /// <exception cref="System.ArgumentNullException">map</exception>
        public IDictionary<string, object> ReadObject(IDictionary<string, string> map)
        {
            lock (SyncLock)
            {
                if (Headings == null)
                    throw new InvalidOperationException($"Call the {nameof(ReadHeadings)} method before reading as an object.");

                if (map == null)
                    throw new ArgumentNullException(nameof(map));

                var result = new Dictionary<string, object>();
                var values = ReadLine();

                for (var i = 0; i < Headings.Length; i++)
                {
                    if (i > values.Length - 1)
                        break;

                    result[Headings[i]] = values[i];
                }

                return result;
            }
        }

        /// <summary>
        /// Reads a line of CSV text, converting it into a dynamic object
        /// The property names ocrrespond to the names of the CSV headings
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> ReadObject()
        {
            return ReadObject(DefaultMap);
        }

        /// <summary>
        /// Reads a line of CSV text converting it into an object of the given type, using a map (or Dictionary)
        /// where the keys are the names of the headings and the values are the names of the instance properties
        /// in the given Type. The result object must be already intantiated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map">The map.</param>
        /// <param name="result">The result.</param>
        /// <exception cref="System.ArgumentNullException">map
        /// or
        /// result</exception>
        /// <exception cref="System.InvalidOperationException">ReadHeadings</exception>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream</exception>
        public void ReadObject<T>(IDictionary<string, string> map, ref T result)
        {
            lock (SyncLock)
            {
                // Check arguments
                {
                    if (map == null)
                        throw new ArgumentNullException(nameof(map));

                    if (Headings == null)
                        throw new InvalidOperationException($"Call the {nameof(ReadHeadings)} method before reading as an object.");

                    if (Reader.EndOfStream)
                        throw new EndOfStreamException("Cannot read past the end of the stream");

                    if (result == null)
                        throw new ArgumentNullException(nameof(result));
                }

                // Read line and extract values
                var values = ReadLine();

                // Read target properties
                if (TypeCache.ContainsKey(typeof(T)) == false)
                {
                    var targetProperties = typeof(T).GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.CanWrite && Constants.BasicTypesInfo.ContainsKey(x.PropertyType));
                    TypeCache[typeof(T)] = targetProperties.ToArray();
                }

                // Extract properties from cache
                var properties = TypeCache[typeof(T)];

                // Assign property values for each heading
                for (var i = 0; i < Headings.Length; i++)
                {
                    // break if no more headings are matched
                    if (i > values.Length - 1)
                        break;

                    // skip if no heading is availabale or the heading is empty
                    if (map.ContainsKey(Headings[i]) == false &&
                        string.IsNullOrWhiteSpace(map[Headings[i]]) == false)
                        continue;

                    // Prepare the target property
                    var propertyName = map[Headings[i]];
                    var propertyStringValue = values[i];
                    var targetProperty = properties.FirstOrDefault(p => p.Name.Equals(propertyName));

                    // Skip if the property is not found
                    if (targetProperty == null)
                        continue;

                    // Parse and assign the basic type value to the property
                    try
                    {
                        object propertyValue = null;
                        if (Constants.BasicTypesInfo[targetProperty.PropertyType].TryParse(propertyStringValue, out propertyValue))
                            targetProperty.SetValue(result, propertyValue);
                    }
                    catch
                    {
                        // swallow
                    }
                }
            }
        }

        /// <summary>
        /// Reads a line of CSV text converting it into an object of the given type, using a map (or Dictionary)
        /// where the keys are the names of the headings and the values are the names of the instance properties
        /// in the given Type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map">The map of CSV headings (keys) and Type property names (values).</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">map</exception>
        /// <exception cref="System.InvalidOperationException">ReadHeadings</exception>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream</exception>
        public T ReadObject<T>(IDictionary<string, string> map)
            where T : new()
        {
            var result = Activator.CreateInstance<T>();
            ReadObject(map, ref result);
            return result;
        }

        /// <summary>
        /// Reads a line of CSV text converting it into an object of the given type, and assuming
        /// the property names of the target type match the heading names of the file.
        /// </summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReadObject<T>()
            where T : new()
        {
            return ReadObject<T>(DefaultMap);
        }

        #endregion

        #region Support Methods

        /// <summary>
        /// Parses a line of standard CSV text into an array of strings.
        /// Note that quoted values might have new line sequences in them. Field values will contain such sequences
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="escapeCharacter">The escape character.</param>
        /// <param name="separatorCharacter">The separator character.</param>
        /// <returns></returns>
        static private string[] ParseRecord(StreamReader reader, char escapeCharacter = '"', char separatorCharacter = ',')
        {
            var values = new List<string>();
            var currentValue = new StringBuilder(1024);
            char currentChar;
            char? nextChar = null;
            var currentState = ReadState.WaitingForNewField;
            string line = null;

            while ((line = reader.ReadLine()) != null)
            {
                for (var charIndex = 0; charIndex < line.Length; charIndex++)
                {
                    // Get the current and next character
                    currentChar = line[charIndex];
                    nextChar = charIndex < line.Length - 1 ? line[charIndex + 1] : new char?();

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
                                else if (currentChar == separatorCharacter)
                                {
                                    values.Add(currentValue.ToString());
                                    currentState = ReadState.WaitingForNewField;
                                    continue;
                                }
                                else
                                {
                                    currentValue.Append(currentChar);
                                    currentState = ReadState.PushingNormal;
                                    continue;
                                }
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
                                if (currentChar == escapeCharacter && nextChar.HasValue && nextChar != escapeCharacter)
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

            // If we ended up pushing quoted and no closing closing quotes we might
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
        /// Loads the records from the give file path.
        /// This method uses Windows 1252 encoding
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        static public IList<T> LoadRecords<T>(string filePath)
            where T : new()
        {
            var result = new List<T>();
            using (var reader = new Formatters.CsvReader(filePath))
            {
                reader.ReadHeadings();
                while (reader.EndOfStream == false)
                {
                    var record = reader.ReadObject<T>();
                    result.Add(record);
                }
            }

            return result;
        }

        #endregion

        #region IDisposable Support

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!HasDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        Reader.Dispose();
                    }
                    finally
                    {
                        Reader = null;
                    }
                }

                HasDisposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }
}
