namespace Unosquare.Swan.Formatters
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a reader designed for CSV text.
    /// It is capable of deserializing objects from individual lines of CSV text,
    /// transforming CSV lines of text into Expando Objects,
    /// or simply reading the lines of CSV as an array of strings
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class CsvReader : IDisposable
    {
        static public readonly Encoding Windows1252Encoding = Encoding.GetEncoding(1252);
        static private readonly Dictionary<Type, PropertyInfo[]> CachedTypes = new Dictionary<Type, PropertyInfo[]>();

        private readonly object SyncLock = new object();
        private int m_ReadCount = 0;
        private char m_EscapeCharacter = '"';
        private char m_SeparatorCharacter = ',';
        private string[] Headers = null;
        private Dictionary<string, string> DefaultMap = null;
        private bool HasDisposed = false; // To detect redundant calls

        private Stream InputStream = null;
        private StreamReader Reader = null;

        /// <summary>
        /// Defines the 3 different read states
        /// </summary>
        private enum ReadState
        {
            WaitingForNewField,
            PushingNormal,
            PushingQuoted,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="textEncoding">The text encoding.</param>
        public CsvReader(Stream stream, Encoding textEncoding)
        {
            InputStream = stream;
            Reader = new StreamReader(stream, textEncoding);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// It uses the Windows 1252 Encoding by default
        /// </summary>
        /// <param name="filename">The filename.</param>
        public CsvReader(string filename)
            : this(File.OpenRead(filename), Windows1252Encoding)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="encoding">The encoding.</param>
        public CsvReader(string filename, Encoding encoding)
            : this(File.OpenRead(filename), encoding)
        {
            // placehoder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvReader"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public CsvReader(Stream stream)
            : this(stream, Windows1252Encoding)
        {
        }

        /// <summary>
        /// Gets number of lines that have been read, including the header
        /// </summary>
        public int ReadCount { get { return m_ReadCount; } }

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
        /// Skips a line of CSV text.
        /// This operation does not increment the ReadCount property
        /// </summary>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream</exception>
        public void SkipLine()
        {
            lock (SyncLock)
            {
                if (Reader.EndOfStream)
                    throw new EndOfStreamException("Cannot read past the end of the stream");

                var line = Reader.ReadLine();
                return;
            }
        }

        /// <summary>
        /// Reads a line of CSV text and stores the values read as a representation of the column names
        /// to be used for parsing objects. You have to call this method before calling ReadObject methods.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Reading headers is only supported as the first read operation.</exception>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream</exception>
        public string[] ReadHeader()
        {
            lock (SyncLock)
            {
                if (m_ReadCount != 0)
                    throw new InvalidOperationException("Reading headers is only supported as the first read operation.");

                if (Reader.EndOfStream)
                    throw new EndOfStreamException("Cannot read past the end of the stream");

                var line = Reader.ReadLine();
                Headers = ParseLine(line);
                DefaultMap = new Dictionary<string, string>();
                foreach (var header in Headers)
                {
                    DefaultMap[header] = header;
                }

                m_ReadCount++;
                return Headers.ToArray();
            }
        }

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

                var line = Reader.ReadLine();
                m_ReadCount++;
                return ParseLine(line);
            }
        }

        /// <summary>
        /// Reads a line of CSV text, converting it into a dynamic object in which properties correspond to the names of the headers
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">ReadHeaders</exception>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream</exception>
        public dynamic ReadObject()
        {
            lock (SyncLock)
            {
                if (Headers == null)
                    throw new InvalidOperationException($"Call the {nameof(ReadHeader)} method before reading as an object.");

                if (Reader.EndOfStream)
                    throw new EndOfStreamException("Cannot read past the end of the stream");


                var line = Reader.ReadLine();
                m_ReadCount++;
                dynamic resultObject = new ExpandoObject();
                var result = resultObject as IDictionary<string, object>;
                var values = ParseLine(line);

                for (var i = 0; i < Headers.Length; i++)
                {
                    if (i > values.Length - 1)
                        break;

                    result[Headers[i]] = values[i];
                }

                return resultObject;
            }
        }

        /// <summary>
        /// Reads a line of CSV text converting it into an object of the given type, using a map (or Dictionary)
        /// where the keys are the names of the headers and the values are the names of the instance properties
        /// in the given Type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map">The map of CSV headers (keys) and Type property names (values).</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">map</exception>
        /// <exception cref="System.InvalidOperationException">ReadHeaders</exception>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream</exception>
        public T ReadObject<T>(IDictionary<string, string> map)
        {
            lock (SyncLock)
            {
                if (map == null)
                    throw new ArgumentNullException(nameof(map));

                if (Headers == null)
                    throw new InvalidOperationException($"Call the {nameof(ReadHeader)} method before reading as an object.");

                if (Reader.EndOfStream)
                    throw new EndOfStreamException("Cannot read past the end of the stream");



                var line = Reader.ReadLine();
                m_ReadCount++;
                var values = ParseLine(line);


                var result = Activator.CreateInstance<T>();
                if (CachedTypes.ContainsKey(typeof(T)) == false)
                {
                    var targetProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.CanWrite && Constants.BasicTypesInfo.ContainsKey(x.PropertyType));
                    CachedTypes[typeof(T)] = targetProperties.ToArray();
                }

                var properties = CachedTypes[typeof(T)];

                for (var i = 0; i < Headers.Length; i++)
                {
                    if (i > values.Length - 1)
                        break;

                    if (map.ContainsKey(Headers[i]) == false &&
                        string.IsNullOrWhiteSpace(map[Headers[i]]) == false)
                        continue;

                    var propertyName = map[Headers[i]];
                    var propertyStringValue = values[i];
                    var targetProperty = properties.FirstOrDefault(p => p.Name.Equals(propertyName));
                    if (targetProperty == null)
                        continue;

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

                return result;
            }
        }

        /// <summary>
        /// Reads a line of CSV text converting it into an object of the given type, and assuming
        /// the property names of the target type match the header names of the file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">ReadHeaders</exception>
        /// <exception cref="System.IO.EndOfStreamException">Cannot read past the end of the stream</exception>
        public T ReadObject<T>()
        {
            lock (SyncLock)
            {
                if (Headers == null)
                    throw new InvalidOperationException($"Call the {nameof(ReadHeader)} method before reading as an object.");

                if (Reader.EndOfStream)
                    throw new EndOfStreamException("Cannot read past the end of the stream");

                return ReadObject<T>(DefaultMap);
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

        /// <summary>
        /// Parses a line of standard CSV text into an array of strings.
        /// </summary>
        /// <param name="line">The line of CSV text.</param>
        /// <returns></returns>
        static public string[] ParseLine(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder(1024);
            char currentChar;
            Nullable<char> nextChar = null;
            var currentState = ReadState.WaitingForNewField;

            for (var charIndex = 0; charIndex < line.Length; charIndex++)
            {
                // Get the current and next character
                currentChar = line[charIndex];
                nextChar = charIndex < line.Length - 1 ? line[charIndex + 1] : new Nullable<char>();

                // Perform logic based on state and decide on next state
                switch (currentState)
                {
                    case ReadState.WaitingForNewField:
                        {
                            currentValue.Clear();
                            if (currentChar == m_EscapeCharacter)
                            {
                                currentState = ReadState.PushingQuoted;
                                continue;
                            }
                            else if (currentChar == m_SeparatorCharacter)
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
                            if (currentChar == m_SeparatorCharacter)
                            {
                                currentState = ReadState.WaitingForNewField;
                                values.Add(currentValue.ToString().Trim());
                                currentValue.Clear();
                                continue;
                            }

                            // Handle double quote escaping
                            if (currentChar == m_EscapeCharacter && nextChar == m_EscapeCharacter)
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
                            if (currentChar == m_EscapeCharacter && nextChar != m_EscapeCharacter)
                            {
                                currentState = ReadState.PushingNormal;
                                continue;
                            }

                            // Handle double quote escaping
                            if (currentChar == m_EscapeCharacter && nextChar == m_EscapeCharacter)
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

            // push anything that has not been pushed (flush)
            values.Add(currentValue.ToString().Trim());
            return values.ToArray();
        }

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
                        InputStream.Dispose();
                    }
                    finally
                    {
                        Reader = null;
                        InputStream = null;
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
