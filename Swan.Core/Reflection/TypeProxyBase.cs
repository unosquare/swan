using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides a base class for efficiently exposing details about a given type.
    /// </summary>
    public abstract class TypeProxyBase : ITypeProxy
    {
        /// <summary>
        /// Binding flags to retrieve instanc, public and non-public members.
        /// </summary>
        protected const BindingFlags PublicAndPrivate = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly object SyncLock = new();
        private static readonly Dictionary<Type, Dictionary<string, IPropertyProxy>> PropertyCache = new(32);

        private readonly Lazy<FieldInfo[]> FieldsLazy;
        private readonly Lazy<TryParseMethodInfo> TryParseMethodLazy;
        private readonly Lazy<ToStringMethodInfo> ToStringMethodLazy;
        private readonly Lazy<ConstructorInfo?> DefaultConstructorLazy;
        private readonly Lazy<object?> DefaultLazy;
        private readonly Lazy<Func<object>> CreateInstanceLazy;

        /// <summary>
        /// Creates a new instance of the <see cref="TypeProxyBase"/> class.
        /// </summary>
        /// <param name="backingType">The type to create a proxy from.</param>
        protected TypeProxyBase(Type backingType)
        {
            BackingType = backingType ?? throw new ArgumentNullException(nameof(backingType));

            var nullableType = Nullable.GetUnderlyingType(backingType);

            IsNullableValueType = nullableType != null;
            IsValueType = backingType.IsValueType;
            UnderlyingType = nullableType ?? BackingType;
            IsNumeric = TypeManager.NumericTypes.Contains(UnderlyingType);
            IsBasicType = TypeManager.BasicValueTypes.Contains(UnderlyingType);

            FieldsLazy = new(() => backingType.GetFields(PublicAndPrivate), true);

            TryParseMethodLazy = new(() => new TryParseMethodInfo(this), true);
            ToStringMethodLazy = new(() => new ToStringMethodInfo(this), true);
            DefaultConstructorLazy = new(() => !IsValueType
                ? backingType.GetConstructor(PublicAndPrivate, null, Type.EmptyTypes, null)
                : null, true);

            DefaultLazy = new(() => IsValueType ? Activator.CreateInstance(backingType) : null, true);
            CreateInstanceLazy = new(() =>
            {
                if (IsValueType)
                    return new(() => DefaultValue);

                var constructor = DefaultConstructorLazy.Value;
                return constructor is not null
                    ? (Func<object>)Expression.Lambda(Expression.New(constructor)).Compile()
                    : () => throw new MissingMethodException($"Type '{BackingType.Name}' does not have a parameterless constructor.");

            }, true);
        }

        /// <inheritdoc />
        public Type BackingType { get; }

        /// <inheritdoc />
        public bool IsNullableValueType { get; }

        /// <inheritdoc />
        public bool IsNumeric { get; }

        /// <inheritdoc />
        public bool IsValueType { get; }

        /// <inheritdoc />
        public bool IsAbstract => BackingType.IsAbstract;

        /// <inheritdoc />
        public bool IsInterface => BackingType.IsInterface;

        /// <inheritdoc />
        public bool IsEnum => BackingType.IsEnum;

        /// <inheritdoc />
        public bool IsArray => BackingType.IsArray;

        /// <inheritdoc />
        public bool IsBasicType { get; }

        /// <inheritdoc />
        public Type UnderlyingType { get; }

        /// <inheritdoc />
        public object? DefaultValue => DefaultLazy.Value;

        /// <inheritdoc />
        public bool CanParseNatively => TryParseMethodInfo != null;

        /// <inheritdoc />
        public bool CanCreateInstance => IsValueType || (!IsAbstract && !IsInterface && DefaultConstructorLazy.Value is not null);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IPropertyProxy> Properties
        {
            get
            {
                lock (SyncLock)
                {
                    if (PropertyCache.TryGetValue(BackingType, out var proxies))
                        return proxies;

                    var properties = BackingType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    proxies = new Dictionary<string, IPropertyProxy>(properties.Length, StringComparer.InvariantCulture);
                    foreach (var propertyInfo in properties)
                        proxies[propertyInfo.Name] = new PropertyProxy(BackingType, propertyInfo);

                    PropertyCache.TryAdd(BackingType, proxies);
                    return proxies;
                }
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<FieldInfo> Fields => FieldsLazy.Value;

        private MethodInfo? TryParseMethodInfo => TryParseMethodLazy.Value.Method;

        private MethodInfo? ToStringMethodInfo => ToStringMethodLazy.Value.Method;

        /// <inheritdoc />
        public object CreateInstance() => CreateInstanceLazy.Value.Invoke();

        /// <inheritdoc />
        public string ToStringInvariant(object? instance)
        {
            if (instance == null)
                return string.Empty;

            return ToStringMethodInfo is not null && ToStringMethodLazy.Value.Parameters.Count != 1
                ? instance.ToString() ?? string.Empty
                : ToStringMethodInfo?.Invoke(instance, new object[] { CultureInfo.InvariantCulture }) as string ?? string.Empty;
        }

        /// <inheritdoc />
        public bool TryParse(string s, out object? result)
        {
            result = DefaultValue;

            try
            {
                if (BackingType == typeof(string))
                {
                    result = Convert.ChangeType(s, BackingType, CultureInfo.InvariantCulture);
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
    }
}
