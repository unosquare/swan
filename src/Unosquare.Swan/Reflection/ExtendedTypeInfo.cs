namespace Unosquare.Swan.Reflection
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    /// <summary>
    /// Provides extended information about a type
    /// This class is mainly used to define sets of types within the COnstants class
    /// and it is not meant for other than querying the VasicTypesInfo dictionary.
    /// </summary>
    public class ExtendedTypeInfo
    {

        private const string TryParseMethodName = "TryParse";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedTypeInfo"/> class.
        /// </summary>
        /// <param name="t">The t.</param>
        public ExtendedTypeInfo(Type t)
        {
            Type = t;
            IsNullableValueType = Type.IsGenericType
                && Type.GetGenericTypeDefinition() == typeof(Nullable<>);

            IsValueType = t.IsValueType;

            UnderlyingType = IsNullableValueType ?
                new NullableConverter(Type).UnderlyingType :
                Type;

            Type[] argumentTypes = { typeof(string), UnderlyingType.MakeByRefType() };
            TryParseMethodInfo = UnderlyingType.GetMethod(TryParseMethodName, argumentTypes);
        }

        /// <summary>
        /// Gets the type this extended info class provides for.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the type is a nullable value type.
        /// </summary>
        public bool IsNullableValueType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the type is value type.
        /// Nullable value types have this property set to False
        /// </summary>
        public bool IsValueType { get; private set; }

        /// <summary>
        /// When dealing with nullable value types, this property will
        /// return the underlying value type of the nullable,
        /// Otherwise it will return the same type as the Type property
        /// </summary>
        public Type UnderlyingType { get; private set; }

        /// <summary>
        /// Gets the try parse method information. If the type does not contain
        /// a suitable TryParse static method, it will return null.
        /// </summary>
        public MethodInfo TryParseMethodInfo { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the type contains a suitable TryParse method.
        /// </summary>
        public bool CanParseNatively { get { return TryParseMethodInfo != null; } }

        /// <summary>
        /// Gets the default value of this type. For reference types it return null.
        /// For value types it returns the default value.
        /// </summary>
        /// <returns></returns>
        public object GetDefault()
        {
            if (Type.IsValueType)
                return Activator.CreateInstance(Type);

            return null;
        }

        /// <summary>
        /// Tries to parse a string into the given type. Te T type argument HAS
        /// to be equal to Type. Otherwise it will return null or default and it will
        /// fail the parsing process automatically.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s">The s.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public bool TryParse<T>(string s, out T result)
        {
            result = default(T);

            if (typeof(T) != Type)
            {
                return false;
            }

            object innerResult = null;
            var success = TryParse(s, out innerResult);
            if (success && innerResult != null)
            {
                result = (T)innerResult;
            }

            return success;
        }

        /// <summary>
        /// Trieas to parse the string into an object of the type this instance represents.
        /// Returns false when no suitable TryParse methods exists for the type or when parsing fails
        /// for any reason.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public bool TryParse(string s, out object result)
        {
            result = GetDefault();

            try
            {
                if (Type == typeof(string))
                {
                    result = Convert.ChangeType(s, Type);
                    return true;
                }

                if (IsNullableValueType)
                {
                    if (string.IsNullOrEmpty(s))
                    {
                        result = GetDefault();
                        return true;
                    }
                }

                if (CanParseNatively == false)
                {
                    result = GetDefault();
                    return false;
                }

                object[] parseAeguments = { s, null };
                var parseResult = (bool)TryParseMethodInfo.Invoke(null, parseAeguments);

                if (parseResult == false)
                {
                    result = GetDefault();
                    return false;
                }

                result = parseAeguments[1];
                return true;
            }
            catch
            {
                return false;
            }

        }
    }

}
