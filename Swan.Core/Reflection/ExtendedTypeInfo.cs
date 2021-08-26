using Swan.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides extended information about a type.
    /// 
    /// This class is mainly used to define sets of types within the Definition class
    /// and it is not meant for other than querying the BasicTypesInfo dictionary.
    /// </summary>
    public class ExtendedTypeInfo
    {
        private const string TryParseMethodName = nameof(byte.TryParse);
        private const string ToStringMethodName = nameof(ToString);

        private static readonly object SyncLock = new();
        private static readonly Dictionary<Type, Dictionary<string, IPropertyProxy>> ProxyCache =
            new(32);

        private static readonly Type[] NumericTypes =
        {
            typeof(byte),
            typeof(sbyte),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
        };

        private static readonly Type[] BasicTypes =
        {
                typeof(int),
                typeof(bool),
                typeof(string),
                typeof(DateTime),
                typeof(double),
                typeof(decimal),
                typeof(Guid),
                typeof(long),
                typeof(TimeSpan),
                typeof(uint),
                typeof(float),
                typeof(byte),
                typeof(short),
                typeof(sbyte),
                typeof(ushort),
                typeof(ulong),
                typeof(char),
        };

        private readonly ParameterInfo[]? _tryParseParameters;
        private readonly int _toStringArgumentLength;

        private readonly Lazy<object[]> DirectAttributesLazy;
        private readonly Lazy<object[]> AllAttributesLazy;
        private readonly Lazy<object[]> InheritedAttributesLazy;
        private readonly Lazy<FieldInfo[]> FieldsLazy;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedTypeInfo"/> class.
        /// </summary>
        /// <param name="t">The t.</param>
        public ExtendedTypeInfo(Type t)
        {
            Type = t ?? throw new ArgumentNullException(nameof(t));

            var nullableType = Nullable.GetUnderlyingType(t);

            IsNullableValueType = nullableType != null;
            IsValueType = t.IsValueType;
            UnderlyingType = nullableType ?? Type;
            IsNumeric = NumericTypes.Contains(UnderlyingType);
            IsBasicType = BasicTypes.Contains(UnderlyingType);

            DirectAttributesLazy = new(() => Type.GetCustomAttributes(false), true);
            AllAttributesLazy = new(() => t.GetCustomAttributes(true), true);
            InheritedAttributesLazy = new(() => AllAttributes.Where(c => !DirectAttributes.Contains(DirectAttributes)).ToArray(), true);
            FieldsLazy = new(() => t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), true);

            // Extract the TryParse method info
            try
            {
                TryParseMethodInfo =
                    UnderlyingType.GetMethod(TryParseMethodName,
                        new[] { typeof(string), typeof(NumberStyles), typeof(IFormatProvider), UnderlyingType.MakeByRefType() }) ??
                    UnderlyingType.GetMethod(TryParseMethodName,
                        new[] { typeof(string), UnderlyingType.MakeByRefType() });

                _tryParseParameters = TryParseMethodInfo?.GetParameters() ?? Array.Empty<ParameterInfo>();
            }
            catch
            {
                // ignored
            }

            // Extract the ToString method Info
            try
            {
                ToStringMethodInfo = UnderlyingType.GetMethod(ToStringMethodName, new[] { typeof(IFormatProvider) }) ??
                                     UnderlyingType.GetMethod(ToStringMethodName, Array.Empty<Type>());

                _toStringArgumentLength = ToStringMethodInfo?.GetParameters().Length ?? 0;
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the property proxies associated with a given type.
        /// </summary>
        /// <param name="t">The type to retrieve property proxies from.</param>
        /// <returns>A dictionary with property names as keys and <see cref="IPropertyProxy"/> objects as values.</returns>
        public IReadOnlyDictionary<string, IPropertyProxy> Properties
        {
            get
            {
                lock (SyncLock)
                {
                    if (ProxyCache.TryGetValue(Type, out var proxies))
                        return proxies;

                    var properties = Type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    proxies = new Dictionary<string, IPropertyProxy>(properties.Length, StringComparer.InvariantCultureIgnoreCase);
                    foreach (var propertyInfo in properties)
                        proxies[propertyInfo.Name] = new PropertyProxy(Type, propertyInfo);

                    ProxyCache.TryAdd(Type, proxies);
                    return proxies;
                }
            }
        }

        public IReadOnlyCollection<object> DirectAttributes => DirectAttributesLazy.Value;

        public IReadOnlyCollection<object> InheritedAttributes => InheritedAttributesLazy.Value;

        public IReadOnlyCollection<object> AllAttributes => AllAttributesLazy.Value;

        public IReadOnlyCollection<FieldInfo> Fields => FieldsLazy.Value;

        /// <summary>
        /// Gets the type this extended info class provides for.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type Type { get; }

        /// <summary>
        /// Gets a value indicating whether the type is a nullable value type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is nullable value type; otherwise, <c>false</c>.
        /// </value>
        public bool IsNullableValueType { get; }

        /// <summary>
        /// Gets a value indicating whether the type or underlying type is numeric.
        /// </summary>
        /// <value>
        ///  <c>true</c> if this instance is numeric; otherwise, <c>false</c>.
        /// </value>
        public bool IsNumeric { get; }

        /// <summary>
        /// Gets a value indicating whether the type is value type.
        /// Nullable value types have this property set to False.
        /// </summary>
        public bool IsValueType { get; }

        /// <summary>
        /// Gets a value indicating whether the type is basic.
        /// Basic types are all primitive types plus strings, GUIDs , TimeSpans, DateTimes
        /// including their nullable versions.
        /// </summary>
        public bool IsBasicType { get; }

        /// <summary>
        /// When dealing with nullable value types, this property will
        /// return the underlying value type of the nullable,
        /// Otherwise it will return the same type as the Type property.
        /// </summary>
        /// <value>
        /// The type of the underlying.
        /// </value>
        public Type UnderlyingType { get; }

        /// <summary>
        /// Gets the try parse method information. If the type does not contain
        /// a suitable TryParse static method, it will return null.
        /// </summary>
        /// <value>
        /// The try parse method information.
        /// </value>
        public MethodInfo? TryParseMethodInfo { get; }

        /// <summary>
        /// Gets the ToString method info
        /// It will prefer the overload containing the IFormatProvider argument.
        /// </summary>
        /// <value>
        /// To string method information.
        /// </value>
        public MethodInfo? ToStringMethodInfo { get; }

        /// <summary>
        /// Gets a value indicating whether the type contains a suitable TryParse method.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can parse natively; otherwise, <c>false</c>.
        /// </value>
        public bool CanParseNatively => TryParseMethodInfo != null;

        #endregion

        #region Methods

        /// <summary>
        /// Searches for the first attribute of the given type.
        /// </summary>
        /// <typeparam name="T">The attribute type to search for.</typeparam>
        /// <returns>Returns a null if an attribute of the given type is not found.</returns>
        public T? Attribute<T>()
            where T : Attribute =>
            AllAttributes.FirstOrDefault(c => c.GetType() == typeof(T)) as T;

        /// <summary>
        /// Searches for the first attribute of the given type.
        /// </summary>
        /// <param name="attributeType">The attribute type to search for.</param>
        /// <returns>Returns a null if an attribute of the given type is not found.</returns>
        public object? Attribute(Type attributeType) =>
            attributeType is null
            ? throw new ArgumentNullException(nameof(attributeType))
            : AllAttributes.FirstOrDefault(c => c.GetType().IsAssignableTo(attributeType));

        /// <summary>
        /// Gets a value indicating whether an attribute of the given type has been applied.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to search for.</typeparam>
        /// <returns>True if the attribute is found. False otherwise.</returns>
        public bool HasAttribute<T>()
            where T : Attribute =>
            Attribute<T>() is not null;

        /// <summary>
        /// Gets a value indicating whether an attribute of the given type has been applied.
        /// </summary>
        /// <param name="attributeType">The type of the attribute to search for.</param>
        /// <returns>True if the attribute is found. False otherwise.</returns>
        public bool HasAttribute(Type attributeType) =>
            Attribute(attributeType) is not null;

        /// <summary>
        /// Tries to parse the string into an object of the type this instance represents.
        /// Returns false when no suitable TryParse methods exists for the type or when parsing fails
        /// for any reason. When possible, this method uses CultureInfo.InvariantCulture and NumberStyles.Any.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if parse was converted successfully; otherwise, <c>false</c>.</returns>
        public bool TryParse(string s, out object? result)
        {
            result = Type.GetDefault();

            try
            {
                if (Type == typeof(string))
                {
                    result = Convert.ChangeType(s, Type, CultureInfo.InvariantCulture);
                    return true;
                }

                if ((IsNullableValueType && string.IsNullOrEmpty(s)) || !CanParseNatively)
                {
                    return true;
                }

                // Build the arguments of the TryParse method
                var dynamicArguments = new List<object?> { s };

                for (var pi = 1; pi < _tryParseParameters.Length - 1; pi++)
                {
                    var argInfo = _tryParseParameters[pi];
                    if (argInfo.ParameterType == typeof(IFormatProvider))
                        dynamicArguments.Add(CultureInfo.InvariantCulture);
                    else if (argInfo.ParameterType == typeof(NumberStyles))
                        dynamicArguments.Add(NumberStyles.Any);
                    else
                        dynamicArguments.Add(null);
                }

                dynamicArguments.Add(null);
                var parseArguments = dynamicArguments.ToArray();

                if ((bool)(TryParseMethodInfo.Invoke(null, parseArguments) ?? false))
                {
                    result = parseArguments[^1];
                    return true;
                }
            }
            catch
            {
                // Ignore
            }

            return false;
        }

        /// <summary>
        /// Converts this instance to its string representation, 
        /// trying to use the CultureInfo.InvariantCulture
        /// IFormat provider if the overload is available.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>A <see cref="System.String" /> that represents the current object.</returns>
        public string ToStringInvariant(object? instance)
        {
            if (instance == null)
                return string.Empty;

            return ToStringMethodInfo is not null && _toStringArgumentLength != 1
                ? instance.ToString() ?? string.Empty
                : ToStringMethodInfo.Invoke(instance, new object[] { CultureInfo.InvariantCulture }) as string ?? string.Empty;
        }

        #endregion
    }

    /// <summary>
    /// Provides extended information about a type.
    /// 
    /// This class is mainly used to define sets of types within the Constants class
    /// and it is not meant for other than querying the BasicTypesInfo dictionary.
    /// </summary>
    /// <typeparam name="T">The type of extended type information.</typeparam>
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
        /// Converts this instance to its string representation,
        /// trying to use the CultureInfo.InvariantCulture
        /// IFormat provider if the overload is available.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>A <see cref="System.String" /> that represents the current object.</returns>
        public string ToStringInvariant(T instance) => base.ToStringInvariant(instance);
    }
}