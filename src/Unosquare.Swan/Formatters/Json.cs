namespace Unosquare.Swan.Formatters
{
    using Reflection;
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A very simple JSON library written by Mario
    /// to teach Geo how things are done
    /// </summary>
    static public partial class Json
    {
        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();

        #region Constants 

        private const char OpenObjectChar = '{';
        private const char CloseObjectChar = '}';

        private const char OpenArrayChar = '[';
        private const char CloseArrayChar = ']';

        private const char FieldSeparatorChar = ',';
        private const char ValueSeparatorChar = ':';

        private const char StringEscapeChar = '\\';
        private const char StringQuotedChar = '"';

        private const string EmptyObjectLiteral = "{ }";
        private const string EmtpyArrayLiteral = "[ ]";
        private const string TrueLiteral = "true";
        private const string FalseLiteral = "false";
        private const string NullLiteral = "null";

        #endregion

        #region Private API

        /// <summary>
        /// Retrieves PropertyInfo[] (both public and non-public).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static PropertyInfo[] RetrieveProperties(Type type)
        {
            return TypeCache.Retrieve(type, () =>
            {
                return
                type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.CanRead || p.CanWrite).ToArray();
            });
        }

        #endregion

        #region Public API

        /// <summary>
        /// Serializes the specified object. All properties are serialized
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <returns></returns>
        public static string Serialize(object obj, bool format = false)
        {
            return Serializer.Serialize(obj, 0, format, null, null);
        }

        /// <summary>
        /// Serializes the specified object only including the specified property names.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <param name="includeNames">The include names.</param>
        /// <returns></returns>
        public static string SerializeOnly(object obj, bool format, params string[] includeNames)
        {
            return Serializer.Serialize(obj, 0, format, includeNames, null);
        }

        /// <summary>
        /// Serializes the specified object excluding the specified property names.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <param name="excludeNames">The exclude names.</param>
        /// <returns></returns>
        public static string SerializeExcluding(object obj, bool format, params string[] excludeNames)
        {
            return Serializer.Serialize(obj, 0, format, null, excludeNames);
        }

        /// <summary>
        /// Deserializes the specified json string as either a Dictionary of string and object or as a List of object
        /// depending on the syntax of the JSON string
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static object Deserialize(string json)
        {
            return Deserializer.Deserialize(json);
        }
        
        #endregion

    }
}