
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
    /// A CSV writer useful for exporting a set of objects or a 
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
        /// Gets number of lines that have been written, including the headings line
        /// </summary>
        public ulong Count { get { lock (SyncLock) { return m_Count; } } }

        #endregion

        #region Generic, main Write Line Method

        /// <summary>
        /// Writes a line of CSV text. Items are converted to strings.
        /// If items are found to be null, empty strings are written out.
        /// If items are not string, the ToString() method is called on them
        /// </summary>
        /// <param name="items">The items.</param>
        public void WriteLine(params object[] items)
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

        #region Write Object Method

        /// <summary>
        /// Writes a row of CSV text. It handles the special cases where the object is
        /// a dynamic object or and array. It also handles non-collection objects fine.
        /// If you do not like the way the output is handled, you can simply write an extension
        /// method of this class and use the WriteLine method instead.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <exception cref="System.ArgumentNullException">item</exception>
        public void WriteObject(object item)
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

        /// <summary>
        /// Writes a row of CSV text. It handles the special cases where the object is
        /// a dynamic object or and array. It also handles non-collection objects fine.
        /// If you do not like the way the output is handled, you can simply write an extension
        /// method of this class and use the WriteLine method instead.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        public void WriteObject<T>(T item)
        {
            WriteObject(item as object);
        }

        /// <summary>
        /// Writes a set of items, one per line and atomically by repeatedly calling the
        /// WriteObject method. For more info check out the description of the WriteObject
        /// method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        public void WriteObjects<T>(IEnumerable<T> items)
        {
            lock (SyncLock)
            {
                foreach (var item in items)
                    WriteObject(item);
            }
        }

        /// <summary>
        /// Writes the object values.
        /// </summary>
        /// <param name="item">The item.</param>
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

            WriteLine(values.ToArray());
        }

        /// <summary>
        /// Writes the collection values.
        /// </summary>
        /// <param name="typedItem">The typed item.</param>
        private void WriteCollectionValues(ICollection typedItem)
        {
            var values = new List<object>();
            foreach (var item in typedItem)
            {
                values.Add(item);
            }

            WriteLine(values.ToArray());
        }

        /// <summary>
        /// Writes the dictionary values.
        /// </summary>
        /// <param name="typedItem">The typed item.</param>
        private void WriteDictionaryValues(IDictionary typedItem)
        {
            WriteLine(GetFilteredDictionaryValues(typedItem));
        }

        /// <summary>
        /// Writes the dynamic object values.
        /// </summary>
        /// <param name="typedItem">The typed item.</param>
        private void WriteDynamicObjectValues(IDictionary<string, object> typedItem)
        {
            WriteLine(GetFilteredDictionaryValues(typedItem));
        }

        #endregion

        #region Write Headings Methods

        public void WriteHeadings(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var properties = GetFilteredTypeProperties(type).Select(p => p.Name).Cast<object>().ToArray();
            WriteLine(properties);
        }

        public void WriteHeadings<T>()
        {
            WriteHeadings(typeof(T));
        }

        public void WriteHeadings(IDictionary dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            WriteLine(GetFilteredDictionaryKeys(dictionary));
        }

        public void WriteHeadings(IDictionary<string, object> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            WriteLine(GetFilteredDictionaryKeys(dictionary));
        }

#if NET452
        public void WriteHeadings(dynamic item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var dictionary = item as IDictionary<string, object>;
            if (dictionary == null)
                throw new ArgumentException("Unable to cast dynamic object to a suitable dictionary", nameof(item));

            WriteHeadings(dictionary);
        }
#endif

        #endregion

        #region Support Methods

        /// <summary>
        /// Gets the filtered dictionary keys using the IgnoreProperties list.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the filtered dictionary keys using the IgnoreProperties list.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the filtered dictionary values using the IgnoreProperties list.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the filtered dictionary values using the IgnoreProperties list.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
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


        /// <summary>
        /// Gets the filtered type properties using the IgnoreProperties list.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private PropertyInfo[] GetFilteredTypeProperties(Type type)
        {
            lock (SyncLock)
            {
                if (TypeCache.ContainsKey(type) == false)
                {
                    var properties = type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanRead)
                        .ToArray();

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
