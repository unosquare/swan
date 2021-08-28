using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides extended information about a type such as property proxies,
    /// fields, attributes and methods.
    /// </summary>
    public sealed class ExtendedTypeInfo
    {
        private const BindingFlags PublicAndPrivate = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly object SyncLock = new();
        private static readonly Dictionary<Type, Dictionary<string, IPropertyProxy>> ProxyCache = new(32);

        private readonly Lazy<object[]> DirectAttributesLazy;
        private readonly Lazy<object[]> AllAttributesLazy;
        private readonly Lazy<object[]> InheritedAttributesLazy;
        private readonly Lazy<FieldInfo[]> FieldsLazy;
        private readonly Lazy<TryParseMethodInfo> TryParseMethodLazy;
        private readonly Lazy<ToStringMethodInfo> ToStringMethodLazy;
        private readonly Lazy<ConstructorInfo?> DefaultConstructorLazy;
        private readonly Lazy<object?> DefaultLazy;
        private readonly Lazy<Func<object>> CreateInstanceLazy;

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
            IsNumeric = TypeManager.NumericTypes.Contains(UnderlyingType);
            IsBasicType = TypeManager.BasicValueTypes.Contains(UnderlyingType);

            DirectAttributesLazy = new(() => Type.GetCustomAttributes(false), true);
            AllAttributesLazy = new(() => t.GetCustomAttributes(true), true);
            InheritedAttributesLazy = new(() => AllAttributes.Where(c => !DirectAttributes.Contains(DirectAttributes)).ToArray(), true);
            FieldsLazy = new(() => t.GetFields(PublicAndPrivate), true);

            TryParseMethodLazy = new(() => new TryParseMethodInfo(this), true);
            ToStringMethodLazy = new(() => new ToStringMethodInfo(this), true);
            DefaultConstructorLazy = new(() => !IsValueType
                ? Type.GetConstructor(PublicAndPrivate, null, Type.EmptyTypes, null)
                : null, true);

            DefaultLazy = new(() => IsValueType ? Activator.CreateInstance(Type) : null, true);
            CreateInstanceLazy = new(() => {
                if (IsValueType)
                    return new(() => Default);

                var constructor = DefaultConstructorLazy.Value;
                return constructor is not null
                    ? (Func<object>)Expression.Lambda(Expression.New(constructor)).Compile()
                    : () => throw new MissingMethodException($"Type '{Type.Name}' does not have a parameterless constructor.");

            }, true);
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

        /// <summary>
        /// Provides a collection of all instances of attributes applied directly on this type
        /// and without inherited attributes.
        /// </summary>
        public IReadOnlyCollection<object> DirectAttributes => DirectAttributesLazy.Value;

        /// <summary>
        /// Provides a collection of all instances of attributes applied on parent types of this type
        /// and without directly applied attributes.
        /// </summary>
        public IReadOnlyCollection<object> InheritedAttributes => InheritedAttributesLazy.Value;

        /// <summary>
        /// Provides a collection of all instances of attributes applied on this type and its parent types.
        /// </summary>
        public IReadOnlyCollection<object> AllAttributes => AllAttributesLazy.Value;

        /// <summary>
        /// Provides a collection of all instance fields (public and non public) for this type.
        /// </summary>
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
        /// Gets the default value for this type.
        /// Reference types return null while value types return their default equivalent.
        /// </summary>
        public object? Default => DefaultLazy.Value;

        /// <summary>
        /// Gets a value indicating whether the type contains a suitable TryParse method.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can parse natively; otherwise, <c>false</c>.
        /// </value>
        public bool CanParseNatively => TryParseMethodInfo != null;

        /// <summary>
        /// Determines if a parameterless constructor can be called on this type.
        /// This always returns true on value types.
        /// </summary>
        public bool CanCreateInstance => IsValueType || DefaultConstructorLazy.Value is not null;

        /// <summary>
        /// Gets the try parse method information. If the type does not contain
        /// a suitable TryParse static method, it will return null.
        /// </summary>
        /// <value>
        /// The try parse method information.
        /// </value>
        private MethodInfo? TryParseMethodInfo => TryParseMethodLazy.Value.Method;

        /// <summary>
        /// Gets the ToString method info
        /// It will prefer the overload containing the IFormatProvider argument.
        /// </summary>
        /// <value>
        /// To string method information.
        /// </value>
        private MethodInfo? ToStringMethodInfo => ToStringMethodLazy.Value.Method;

        #endregion

        #region Methods

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
            result = Default;

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

                for (var pi = 1; pi < TryParseMethodLazy.Value.Parameters.Count - 1; pi++)
                {
                    var argInfo = TryParseMethodLazy.Value.Parameters[pi];
                    if (argInfo.ParameterType == typeof(IFormatProvider))
                        dynamicArguments.Add(CultureInfo.InvariantCulture);
                    else if (argInfo.ParameterType == typeof(NumberStyles))
                        dynamicArguments.Add(NumberStyles.Any);
                    else
                        dynamicArguments.Add(null);
                }

                dynamicArguments.Add(null);
                var parseArguments = dynamicArguments.ToArray();

                if ((bool)(TryParseMethodInfo?.Invoke(null, parseArguments) ?? false))
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

            return ToStringMethodInfo is not null && ToStringMethodLazy.Value.Parameters.Count != 1
                ? instance.ToString() ?? string.Empty
                : ToStringMethodInfo?.Invoke(instance, new object[] { CultureInfo.InvariantCulture }) as string ?? string.Empty;
        }

        /// <summary>
        /// Calls the parameterless constructor on this type returning an isntance.
        /// For value types it returns the default value.
        /// If no parameterless constructor is available a <see cref="MissingMethodException"/> is thrown.
        /// </summary>
        /// <returns>A new instance of this type or the default value for value types.</returns>
        public object CreateInstance() => CreateInstanceLazy.Value.Invoke();

        #endregion
    }
}