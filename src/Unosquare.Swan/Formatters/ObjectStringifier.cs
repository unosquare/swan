namespace Unosquare.Swan.Formatters
{
    using Reflection;
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
        public static ObjectStringifier FromObject(object obj)
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
            if (obj is IDictionary)
            {
                return StringifyDictionary(obj as IDictionary);
            }
            if (obj is IEnumerable)
            {
                return StringifyList(obj as IEnumerable);
            }

            return obj == null ?
                "null" :
                FromObject(obj).AddAll().ToString();
        }

        /// <summary>
        /// Stringifies the specified list.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns></returns>
        private static string StringifyList(IEnumerable enumerable)
        {
            return "[" + string.Join(", ", enumerable.Cast<object>().Select(StringifyObject).ToArray()) + "]";
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
                result.Append($"{pair.Key}: {StringifyObject(pair.Value)}, ");

            // remove the last comma, space
            if (result.Length > 1)
                result.Remove(result.Length - 2, 2);

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
            foreach (var name in names)
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
            var type = innerObject.GetType();

            foreach (var name in names)
            {
                var property = type.GetTypeInfo().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                var value = property.GetValue(innerObject, new object[] { });

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
            var properties = innerObject.GetType().GetTypeInfo().GetProperties(
                BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead).ToArray();

            foreach (var property in properties)
            {
                try
                {
                    var value = property.GetValue(innerObject, new object[] { });
                    innerPairs.Add(property.Name, StringifyObject(value));
                }
                catch
                {
                    // swallow
                }
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
            if (innerPairs.Count > 0 && innerObject is string == false)
                return StringifyDictionary(innerPairs);

            return innerObject == null ? "null" : $"{innerObject.ToStringInvariant()}";
        }

        #endregion
    }


    public class Stringifier
    {
        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();

        private int Indentation = 0;
        private bool OutputIndented = true;

        private readonly StringBuilder Output = new StringBuilder();
        private object Target = null;
        private Type TargetType = null;

        private Stringifier(object target, int indentation)
        {
            Indentation = indentation;
            OutputIndented = indentation >= 0;

            Target = target;
            if (Target != null)
                TargetType = target.GetType();
            BuildOutput();
        }

        private string IndentationString
        {
            get
            {
                if (Indentation <= 0 || OutputIndented == false) return string.Empty;
                return new string(' ', Indentation * 4);
            }
        }

        private void Append(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var line = $"{IndentationString}{text}";
            //line = line.TrimEnd('\r', 'n', ' ').TrimStart('\r','\n');
            Output.Append(line);
        }

        private void AppendLine()
        {
            if (Indentation < 0 || OutputIndented == false)
                return;

            Output.AppendLine();
        }

        private void BuildOutput()
        {
            // handle nulls
            if (Target == null)
            {
                Indentation = 0;
                OutputIndented = false;
                Append("null");
                return;
            }

            // handle simple types
            if (Constants.BasicTypesInfo.ContainsKey(TargetType))
            {
                Indentation = 0;
                OutputIndented = false;
                Append($"\"{Constants.BasicTypesInfo[TargetType].ToStringInvariant(Target).RemoveControlChars()}\"");
                return;
            }

            // handle dictionaries
            if (Target is IDictionary)
            {
                var dictionary = Target as IDictionary;
                if (dictionary.Count == 0)
                {
                    OutputIndented = false;
                    Append("{}");
                    return;
                }

                AppendLine();
                Append("{");
                AppendLine();

                Indentation++;
                var index = 0;
                foreach (DictionaryEntry entry in dictionary)
                {

                    Append($"\"{entry.Key.ToString().RemoveControlChars()}\": {Stringify(entry.Value, Indentation)}");

                    if (index < dictionary.Count - 1)
                    {
                        Output.Append(",");
                        AppendLine();
                    }


                    index++;
                }

                Indentation--;

                AppendLine();
                Append("}");
                return;
            }

            // for IEnumerables
            if (Target is IEnumerable)
            {
                var enumerable = Target as IEnumerable;
                var items = enumerable.Cast<object>().ToArray();

                if (items.Length == 0)
                {
                    OutputIndented = false;
                    Append("[]");
                    return;
                }


                AppendLine();
                Append("[");
                AppendLine();

                Indentation++;
                var index = 0;
                foreach (var item in items)
                {
                    Append($"{Stringify(item, Indentation)}");

                    if (index < items.Length - 1)
                    {
                        Output.Append(",");
                        AppendLine();
                    }

                    index++;

                }
                Indentation--;

                AppendLine();
                Append("]");
                return;
            }

            // Handle all other object types
            var objectDictionary = new Dictionary<string, object>();
            var properties = TypeCache.Retrieve(TargetType, () =>
            {
                return
                TargetType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead).ToArray();
            });

            foreach (var property in properties)
            {
                objectDictionary[property.Name] = property.GetValue(Target);
            }

            if (objectDictionary.Count > 0)
            {
                Append($"{Stringify(objectDictionary, Indentation)}");
            }
            else
            {
                Indentation = -1;
                OutputIndented = false;
                Append($"{Stringify(Target.ToString(), Indentation)}");
            }


        }

        static private string Stringify(object obj, int indentation)
        {
            var stringifier = new Stringifier(obj, indentation);
            return stringifier.Output.ToString();
        }

        /// <summary>
        /// Stringifies the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="indent">if set to <c>true</c> [indent].</param>
        /// <returns></returns>
        static public string Stringify(object obj, bool indent = true)
        {
            var stringifier = new Stringifier(obj, indent ? 0 : -1);
            return stringifier.Output.ToString();
        }

    }
}
