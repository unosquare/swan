using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Unosquare.Swan.Formatters
{
    public class CsvReader : IDisposable
    {
        private const char DoubleQuote = '"';
        private const char Comma = ',';

        static public readonly Encoding Windows1252Encoding = Encoding.GetEncoding(1252);
        static public readonly Dictionary<Type, PropertyInfo[]> CachedTypes = new Dictionary<Type, PropertyInfo[]>();

        private readonly object SyncLock = new object();
        private int ReadCount = 0;
        private string[] Headers = null;
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

        public CsvReader(Stream stream, Encoding textEncoding)
        {
            InputStream = stream;
            Reader = new StreamReader(stream, textEncoding);
        }

        public CsvReader(string filename)
            : this(File.OpenRead(filename), Windows1252Encoding)
        {

        }

        public CsvReader(string filename, Encoding encoding)
            : this(File.OpenRead(filename), encoding)
        {

        }

        public CsvReader(Stream stream)
            : this(stream, Windows1252Encoding)
        {
        }

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

        public string[] ReadHeaders()
        {
            lock (SyncLock)
            {
                if (ReadCount != 0)
                    throw new InvalidOperationException("Reading headers is only supported as the first read operation.");

                if (Reader.EndOfStream)
                    throw new EndOfStreamException("Cannot read past the end of the stream");

                var line = Reader.ReadLine();
                Headers = ParseLine(line);
                ReadCount++;
                return Headers.ToArray();
            }
        }

        public string[] ReadLine()
        {
            lock (SyncLock)
            {
                if (Reader.EndOfStream)
                    throw new EndOfStreamException("Cannot read past the end of the stream");

                var line = Reader.ReadLine();
                ReadCount++;
                return ParseLine(line);
            }
        }

        public dynamic ReadObject()
        {
            lock (SyncLock)
            {
                if (Headers == null)
                    throw new InvalidOperationException($"Call the {nameof(ReadHeaders)} method before reading as an object.");

                if (Reader.EndOfStream)
                    throw new EndOfStreamException("Cannot read past the end of the stream");


                var line = Reader.ReadLine();
                ReadCount++;
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

        public T ReadObject<T>(Dictionary<string, string> map)
        {
            // map headers to real properties
            throw new NotImplementedException();
        }

        public T ReadObject<T>()
        {
            lock (SyncLock)
            {
                if (Headers == null)
                    throw new InvalidOperationException($"Call the {nameof(ReadHeaders)} method before reading as an object.");

                if (Reader.EndOfStream)
                    throw new EndOfStreamException("Cannot read past the end of the stream");


                var line = Reader.ReadLine();
                ReadCount++;
                var values = ParseLine(line);


                var result = Activator.CreateInstance<T>();
                if (CachedTypes.ContainsKey(typeof(T)) == false)
                {
                    var targetProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.CanWrite); // TODO: real fundamental type selection here
                    CachedTypes[typeof(T)] = targetProperties.ToArray();
                }

                var properties = CachedTypes[typeof(T)];

                for (var i = 0; i < Headers.Length; i++)
                {
                    if (i > values.Length - 1)
                        break;

                    var propertyName = Headers[i];
                    var propertyStringValue = values[i];

                    var targetProperty = properties.FirstOrDefault(p => p.Name.Equals(propertyName));
                    if (targetProperty == null)
                        continue;

                    try
                    {
                        // TODO: convert the value froms tring to real property type
                        targetProperty.SetValue(result, propertyStringValue);
                        
                    }
                    catch
                    {
                        // swallow
                    }
                }

                return result;
            }
        }

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
        /// Parses the line into an array of strings.
        /// </summary>
        /// <param name="line">The line.</param>
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
                            if (currentChar == DoubleQuote)
                            {
                                currentState = ReadState.PushingQuoted;
                                continue;
                            }
                            else if (currentChar == Comma)
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
                            if (currentChar == Comma)
                            {
                                currentState = ReadState.WaitingForNewField;
                                values.Add(currentValue.ToString().Trim());
                                currentValue.Clear();
                                continue;
                            }

                            // Handle double quote escaping
                            if (currentChar == DoubleQuote && nextChar == DoubleQuote)
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
                            if (currentChar == DoubleQuote && nextChar != DoubleQuote)
                            {
                                currentState = ReadState.PushingNormal;
                                continue;
                            }

                            // Handle double quote escaping
                            if (currentChar == DoubleQuote && nextChar == DoubleQuote)
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

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


    }
}
