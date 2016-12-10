#if UNCOMMENTME
namespace Unosquare.Swan.Formatters
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// A CSV writer for 
    /// </summary>
    public class CsvWriter
    {
#region Static Variables

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> TypeCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

#endregion

#region State Variables

        private readonly object SyncLock = new object();
        private Stream OutputStream = null;
        private Encoding Encoding = null;

        private ulong m_Count = 0;

#endregion

#region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvWriter"/> class.
        /// </summary>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="encoding">The encoding.</param>
        public CsvWriter(Stream outputStream, Encoding encoding)
        {
            OutputStream = outputStream;
            Encoding = encoding;
        }

#endregion

#region Properties

        /// <summary>
        /// Gets or sets the field separator character.
        /// </summary>
        public char SeparatorCharacter { get; set; } = ',';

        /// <summary>
        /// Gets or sets the escape character to use to escape field values
        /// </summary>
        public char EscapeCharacter { get; set; } = '"';

        /// <summary>
        /// Gets or sets the new line character sequence to use when writing a line.
        /// </summary>
        public string NewLineSequence { get; set; } = Environment.NewLine;

        /// <summary>
        /// Defines a list of properties to ignore when outputing CSV lines
        /// </summary>
        public List<string> IgnorePropertyNames { get; } = new List<string>();

        /// <summary>
        /// Gets number of lines that have been written, including the header
        /// </summary>
        public ulong Count { get { lock (SyncLock) { return m_Count; } } }

#endregion

        private void WriteValues(object[] values)
        {
            var length = values.Length;
            var separatorBytes = Encoding.GetBytes(new char[] { SeparatorCharacter });
            var endOfLineBytes = Encoding.GetBytes(NewLineSequence);
            var needsEnclosing = false;
            object value = null;
            string textValue = null;
            byte[] output = null;

            for (var i = 0; i < length; i++)
            {
                // convert the value as a string value
                value = values[i];
                textValue = value == null ? string.Empty : value.ToString();

                // Determine if we need the string to be enclosed 
                // (it either contains an escape or separator char)
                needsEnclosing = textValue.IndexOf(SeparatorCharacter) >= 0
                    || textValue.IndexOf(EscapeCharacter) >= 0;

                // Escape the escape characters by repeating them twice for every instance
                textValue = textValue.Replace($"{EscapeCharacter}",
                    $"{EscapeCharacter}{EscapeCharacter}");

                // Enclose the text value if we need to
                if (needsEnclosing)
                    textValue = string.Format($"{EscapeCharacter}{0}{EscapeCharacter}", textValue);

                // Get the bytes to write to the stream and write them
                output = Encoding.GetBytes(textValue);
                OutputStream.Write(output, 0, output.Length);

                // only write a separator if we are moving in between values.
                // the last value should not be written.
                if (i < length - 1)
                    OutputStream.Write(separatorBytes, 0, separatorBytes.Length);
            }

            // output the newline sequence
            OutputStream.Write(endOfLineBytes, 0, endOfLineBytes.Length);
            m_Count += 1;
        }


#region Write Line Methods

        private void WriteLine(object item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            lock (SyncLock)
            {
                { // Handling as Dynamic Object
                    var typedItem = item as IDictionary<string, object>;
                    if (typedItem != null)
                    {
                        WriteDynamicObjectValues(typedItem);
                        return;
                    }
                }

                { // Handling as Dictionary
                    var typedItem = item as IDictionary;
                    if (typedItem != null)
                    {
                        WriteDictionaryValues(typedItem);
                    }
                }

                { // Handling as array
                    var typedItem = item as Array;
                    if (typedItem != null)
                    {
                        WriteArrayValues(typedItem);
                    }
                }

                { // Handling as I
                    var typedItem = item as Array;
                    if (typedItem != null)
                    {
                        WriteArrayValues(typedItem);
                    }
                }

                { // Handling as object with regular poperties

                }


            }

        }

        public void WriteLine<T>(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // Ty to cast as dictionary
            var dictionary = item as IDictionary;
            if (dictionary != null)
            {
                WriteDictionary(dictionary);
                return;
            }

            // Try to cast as IEnumerable


            var properties = GetFilteredTypeProperties(typeof(T));
            var valuesList = new List<object>();

            foreach (var property in properties)
            {
                try
                {
                    var value = property.GetValue(item, null);
                    valuesList.Add(value);
                }
                catch
                {
                    valuesList.Add(null);
                }
            }

            WriteObjects(valuesList.ToArray());
        }

        private void WriteDictionary(IDictionary dictionary)
        {
            var values = new List<object>();
            foreach (var key in dictionary.Keys)
            {
                if (key == null)
                    continue;

                var stringKey = key.ToString();

                if (IgnorePropertyNames.Contains(stringKey))
                    continue;

                values.Add(dictionary[key]);
            }

            WriteObjects(values.ToArray());
        }

        public void WriteEnumerable(IEnumerable values)
        {
            WriteObjects(values.Cast<object>().ToArray());
        }

#endregion


#region Write Header Methods

        public void WriteHeader(Type type)
        {
            lock (SyncLock)
            {
                var properties = GetFilteredTypeProperties(type);
                var valuesList = properties.Select(p => p.Name).Cast<object>().ToArray();
                WriteObjects(valuesList);
            }
        }

        public void WriteHeader<T>()
        {
            WriteHeader(typeof(T));
        }

        public void WriteHeader(IDictionary dictionary)
        {
            lock (SyncLock)
            {
                if (dictionary == null)
                    throw new ArgumentNullException(nameof(dictionary));

                var stringKeys = new List<string>();
                foreach (var key in dictionary.Keys)
                {
                    if (key == null)
                        continue;

                    var stringKey = key.ToString();

                    if (IgnorePropertyNames.Contains(stringKey))
                        continue;

                    stringKeys.Add(stringKey);
                }

                WriteObjects(stringKeys
                    .Cast<object>().ToArray());

            }
        }

#if NET452

        public void WriteHeader(dynamic obj)
        {
            var dictionary = obj as IDictionary<string, object>;
            if (dictionary == null)
                throw new InvalidCastException("Could not convert dynamic object to dictionary");

            WriteObjects(dictionary.Keys
                .Where(d => IgnorePropertyNames.Contains(d) == false)
                .Cast<object>().ToArray());
        }

#endif

#endregion

#region Support Methods

        private PropertyInfo[] GetFilteredTypeProperties(Type type)
        {
            lock (SyncLock)
            {
                if (TypeCache.ContainsKey(type) == false)
                {
                    var properties = type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    TypeCache[type] = properties;
                }

                return TypeCache[type]
                    .Where(p => IgnorePropertyNames.Contains(p.Name) == false)
                    .ToArray();
            }
        }

#endregion

    }
}
#endif