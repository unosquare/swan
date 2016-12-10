
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

        #region Generic, main Write Method

        public void Write(object[] items)
        {
            lock (SyncLock)
            {
                var length = items.Length;
                var separatorBytes = Encoding.GetBytes(new char[] { SeparatorCharacter });
                var endOfLineBytes = Encoding.GetBytes(NewLineSequence);
                var needsEnclosing = false;
                object value = null;
                string textValue = null;
                byte[] output = null;

                for (var i = 0; i < length; i++)
                {
                    // convert the value as a string value
                    value = items[i];
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
        }

        #endregion

        #region Write Line Method

        public void WriteItem(object item)
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
                        return;
                    }
                }

                { // Handling as array
                    var typedItem = item as ICollection;
                    if (typedItem != null)
                    {
                        WriteCollectionValues(typedItem);
                        return;
                    }
                }

                { // Handling as a regular type
                    WriteObjectValues(item);
                }
            }

        }

        private void WriteObjectValues(object item)
        {
            var properties = GetFilteredTypeProperties(item.GetType());
            var values = new List<object>();
            foreach (var property in properties)
            {
                try
                {
                    var value = property.GetValue(item);
                    values.Add(value);
                }
                catch
                {
                    values.Add(null);
                }
            }

            Write(values.ToArray());
        }

        private void WriteCollectionValues(ICollection typedItem)
        {
            var values = new List<object>();
            foreach (var item in typedItem)
            {
                values.Add(item);
            }

            Write(values.ToArray());
        }

        private void WriteDictionaryValues(IDictionary typedItem)
        {
            Write(GetFilteredDictionaryValues(typedItem));
        }

        private void WriteDynamicObjectValues(IDictionary<string, object> typedItem)
        {
            Write(GetFilteredDictionaryValues(typedItem));
        }

        #endregion

        #region Write Header Methods

        public void WriteHeader(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var properties = GetFilteredTypeProperties(type).Select(p => p.Name).Cast<object>().ToArray();
            Write(properties);
        }

        public void WriteHeader<T>()
        {
            WriteHeader(typeof(T));
        }

        public void WriteHeader(IDictionary dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            Write(GetFilteredDictionaryKeys(dictionary));
        }

        public void WriteHeader(IDictionary<string, object> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            Write(GetFilteredDictionaryKeys(dictionary));
        }

#if NET452
        public void WriteHeader(dynamic item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var dictionary = item as IDictionary<string, object>;
            if (dictionary == null)
                throw new ArgumentException("Unable to cast dynamic object to a suitable dictionary", nameof(item));

            WriteHeader(dictionary);
        }
#endif

        #endregion

        #region Support Methods

        private object[] GetFilteredDictionaryKeys(IDictionary dictionary)
        {
            var keys = new List<object>();
            foreach (var key in dictionary.Keys)
            {
                var stringKey = key == null ? string.Empty : key.ToString();
                if (IgnorePropertyNames.Contains(stringKey))
                    continue;

                keys.Add(stringKey);
            }

            return keys.ToArray();
        }

        private object[] GetFilteredDictionaryKeys(IDictionary<string, object> dictionary)
        {
            var keys = new List<object>();
            foreach (var key in dictionary.Keys)
            {
                var stringKey = key == null ? string.Empty : key;
                if (IgnorePropertyNames.Contains(stringKey))
                    continue;

                keys.Add(stringKey);
            }

            return keys.ToArray();
        }

        private object[] GetFilteredDictionaryValues(IDictionary dictionary)
        {
            var values = new List<object>();
            foreach (var key in dictionary.Keys)
            {
                var stringKey = key == null ? string.Empty : key.ToString();
                if (IgnorePropertyNames.Contains(stringKey))
                    continue;

                values.Add(dictionary[key]);
            }

            return values.ToArray();
        }

        private object[] GetFilteredDictionaryValues(IDictionary<string, object> dictionary)
        {
            var values = new List<object>();
            foreach (var key in dictionary.Keys)
            {
                var stringKey = key == null ? string.Empty : key;
                if (IgnorePropertyNames.Contains(stringKey))
                    continue;

                values.Add(dictionary[key]);
            }

            return values.ToArray();
        }


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
