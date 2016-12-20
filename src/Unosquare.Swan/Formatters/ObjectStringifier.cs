namespace Unosquare.Swan.Formatters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Provides mechanisms to easily represent the properties of the given object as a string.
    /// </summary>
    public class ObjectStringifier
    {
        #region Private Declarations

        private object innerObject;
        private Dictionary<string, string> innerPairs;

        #endregion

        #region Static Methods

        /// <summary>
        /// Creates a stringifier from the given object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static ObjectStringifier New(object obj)
        {
            return new ObjectStringifier(obj);
        }

        /// <summary>
        /// Stringifies the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string Stringify(object obj)
        {
            return StringifyObject(obj);
        }

        /// <summary>
        /// Stringifies the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private static string StringifyObject(object obj)
        {
            if (obj is string)
            {
                return obj as string;
            }
            else if (obj is IDictionary)
            {
                return StringifyDictionary(obj as IDictionary);
            }
            else if (obj is IEnumerable)
            {
                return StringifyList(obj as IEnumerable);
            }
            else
            {
                return obj == null ? "null" : obj.ToString();
            }
        }

        /// <summary>
        /// Stringifies the specified list.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns></returns>
        private static string StringifyList(IEnumerable enumerable)
        {
            return "[" + string.Join(", ", enumerable.Cast<object>().Select(o => StringifyObject(o)).ToArray()) + "]";
        }

        /// <summary>
        /// Stringifies the specified dictionary.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        /// <returns></returns>
        private static string StringifyDictionary(IDictionary dict)
        {
            var result = new StringBuilder();

            result.Append("{");

            foreach (DictionaryEntry pair in dict)
                result.Append($"{pair.Key}={StringifyObject(pair.Value)}, ");

            // remove the last comma, space
            if (result.Length > 1)
            {
                result.Remove(result.Length - 2, 2);
            }

            return result.Append("}").ToString();
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectStringifier"/> class.
        /// </summary>
        /// <param name="obj">The object.</param>
        public ObjectStringifier(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            innerObject = obj;
            innerPairs = new Dictionary<string, string>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Removes the specified properties from the stingifier
        /// </summary>
        /// <param name="names">The names.</param>
        /// <returns></returns>
        public ObjectStringifier Remove(params string[] names)
        {
            foreach (string name in names)
            {
                innerPairs.Remove(name);
            }

            return this;
        }

        /// <summary>
        /// Adds the specified properties to the stringifier.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <returns></returns>
        public ObjectStringifier Add(params string[] names)
        {
            Type type = innerObject.GetType();

            foreach (string name in names)
            {
                PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                object value = property.GetValue(innerObject, new object[] { });

                innerPairs.Add(name, StringifyObject(value));
            }

            return this;
        }

        /// <summary>
        /// Adds a property and value to the stringifier
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ObjectStringifier Add(string name, object value)
        {
            innerPairs.Add(name, StringifyObject(value));
            return this;
        }

        /// <summary>
        /// Adds all public, instance properties to the stingifier.
        /// </summary>
        /// <returns></returns>
        public ObjectStringifier AddAll()
        {
            PropertyInfo[] properties = innerObject.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(innerObject, new object[] { });
                innerPairs.Add(property.Name, StringifyObject(value));
            }

            return this;
        }

        /// <summary>
        /// Returns the stingified object.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return StringifyDictionary(innerPairs);
        }

        #endregion
    }
}
