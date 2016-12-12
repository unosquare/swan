namespace Unosquare.Swan.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides extended information about a type
    /// This class is mainly used to define sets of types within the Constants class
    /// and it is not meant for other than querying the VasicTypesInfo dictionary.
    /// </summary>
    public class ExtendedTypeInfo
    {
        #region Static Declarations

        private const string TryParseMethodName = nameof(byte.TryParse);
        private const string ToStringMethodName = nameof(object.ToString);

        private static readonly Type[] NumericTypes = new Type[]
        {
            typeof (byte),
            typeof (sbyte),
            typeof (decimal),
            typeof (double),
            typeof (float),
            typeof (int),
            typeof (uint),
            typeof (long),
            typeof (ulong),
            typeof (short),
            typeof (ushort),
        };

        #endregion

        #region State Management

        private ParameterInfo[] TryParseParameters = null;
        private int ToStringArgumentLength = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedTypeInfo"/> class.
        /// </summary>
        /// <param name="t">The t.</param>
        public ExtendedTypeInfo(Type t)
        {
            Type = t;
            IsNullableValueType = Type.GetTypeInfo().IsGenericType
                && Type.GetGenericTypeDefinition() == typeof(Nullable<>);

            IsValueType = t.GetTypeInfo().IsValueType;

            UnderlyingType = IsNullableValueType ?
                new NullableConverter(Type).UnderlyingType :
                Type;

            IsNumeric = NumericTypes.Contains(UnderlyingType);

            // Extract the TryParse method info
            try
            {
                TryParseMethodInfo = UnderlyingType.GetTypeInfo().GetMethod(TryParseMethodName,
                    new Type[] { typeof(string), typeof(NumberStyles), typeof(IFormatProvider), UnderlyingType.MakeByRefType() });

                if (TryParseMethodInfo == null)
                    TryParseMethodInfo = UnderlyingType.GetTypeInfo().GetMethod(TryParseMethodName,
                        new Type[] { typeof(string), UnderlyingType.MakeByRefType() });

                TryParseParameters = TryParseMethodInfo?.GetParameters();
            }
            catch { }


            // Extract the ToString method Info
            try
            {
                ToStringMethodInfo = UnderlyingType.GetTypeInfo().GetMethod(ToStringMethodName,
                    new Type[] { typeof(IFormatProvider) });

                if (ToStringMethodInfo == null)
                {
                    ToStringMethodInfo = UnderlyingType.GetTypeInfo().GetMethod(ToStringMethodName,
                        new Type[] { });

                }

                ToStringArgumentLength = ToStringMethodInfo?.GetParameters().Length ?? 0;
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type this extended info class provides for.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets a value indicating whether the type is a nullable value type.
        /// </summary>
        public bool IsNullableValueType { get; }

        /// <summary>
        /// Gets a value indicating whether the type or underlying type is numeric.
        /// </summary>
        public bool IsNumeric { get; private set; }

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
        public Type UnderlyingType { get; }

        /// <summary>
        /// Gets the try parse method information. If the type does not contain
        /// a suitable TryParse static method, it will return null.
        /// </summary>
        public MethodInfo TryParseMethodInfo { get; }

        /// <summary>
        /// Gets the ToString method info
        /// It will prefer the overload containing the IFormatProvider argument
        /// </summary>
        public MethodInfo ToStringMethodInfo { get; }

        /// <summary>
        /// Gets a value indicating whether the type contains a suitable TryParse method.
        /// </summary>
        public bool CanParseNatively => TryParseMethodInfo != null;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the default value of this type. For reference types it return null.
        /// For value types it returns the default value.
        /// </summary>
        /// <returns></returns>
        public object GetDefault()
        {
            if (Type.GetTypeInfo().IsValueType)
                return Activator.CreateInstance(Type);

            return null;
        }

        /// <summary>
        /// Tries to parse the string into an object of the type this instance represents.
        /// Returns false when no suitable TryParse methods exists for the type or when parsing fails
        /// for any reason. When possible, this method uses CultureInfo.InvariantCulture and NumberStyles.Any
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

                // Build the arguments of the TryParse method
                var dynamicArguments = new List<object> {s};

                for (var pi = 1; pi < TryParseParameters.Length - 1; pi++)
                {
                    var argInfo = TryParseParameters[pi];
                    if (argInfo.ParameterType == typeof(IFormatProvider))
                        dynamicArguments.Add(CultureInfo.InvariantCulture);
                    else if (argInfo.ParameterType == typeof(NumberStyles))
                        dynamicArguments.Add(NumberStyles.Any);
                    else
                        dynamicArguments.Add(null);
                }

                dynamicArguments.Add(null);
                var parseArguments = dynamicArguments.ToArray();

                var parseResult = (bool)TryParseMethodInfo.Invoke(null, parseArguments);
                if (parseResult == false)
                {
                    result = GetDefault();
                    return false;
                }

                result = parseArguments[parseArguments.Length - 1];
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Converts this instance to its string representation, 
        /// trying to use the CultureInfo.InvariantCulture
        /// IFormat provider if the overload is available
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public string ToStringInvariant(object instance)
        {
            if (instance == null)
                return string.Empty;

            if (ToStringArgumentLength == 1)
            {
                var arguments = new object[] { CultureInfo.InvariantCulture };
                return ToStringMethodInfo.Invoke(instance, arguments) as string;
            }

            return instance.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Provides extended information about a type
    /// This class is mainly used to define sets of types within the Constants class
    /// and it is not meant for other than querying the VasicTypesInfo dictionary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtendedTypeInfo<T> : ExtendedTypeInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedTypeInfo{T}"/> class.
        /// </summary>
        public ExtendedTypeInfo()
            : base(typeof(T))
        {
            // placeholder
        }

        /// <summary>
        /// Tries to parse the string into an object of the type this instance represents.
        /// Returns false when no suitable TryParse methods exists for the type or when parsing fails
        /// for any reason. When possible, this method uses CultureInfo.InvariantCulture and NumberStyles.Any
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s">The s.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public bool TryParse(string s, out T result)
        {
            result = default(T);

            object innerResult;
            var success = TryParse(s, out innerResult);
            if (success && innerResult != null)
            {
                result = (T)innerResult;
            }

            return success;
        }

        /// <summary>
        /// Converts this instance to its string representation, 
        /// trying to use the CultureInfo.InvariantCulture
        /// IFormat provider if the overload is available
        /// </summary>
        public string ToStringInvariant(T instance)
        {
            return base.ToStringInvariant(instance);
        }
    }
}