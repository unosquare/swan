using Swan.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides a base class for efficiently exposing details about a given type.
    /// </summary>
    internal sealed class TypeProxy : ITypeProxy
    {
        /// <summary>
        /// Binding flags to retrieve instance, public and non-public members.
        /// </summary>
        private const BindingFlags PublicAndPrivate = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Lazy<IReadOnlyDictionary<string, IPropertyProxy>> PropertiesLazy;
        private readonly Lazy<IReadOnlyDictionary<string, IPropertyProxy>> PropertyThesaurusLazy;
        private readonly Lazy<object[]> TypeAttributesLazy;
        private readonly Lazy<ITypeProxy[]> GenericTypeArgumentsLazy;
        private readonly Lazy<FieldInfo[]> FieldsLazy;
        private readonly Lazy<TryParseMethodInfo> TryParseMethodLazy;
        private readonly Lazy<ToStringMethodInfo> ToStringMethodLazy;
        private readonly Lazy<ConstructorInfo?> DefaultConstructorLazy;
        private readonly Lazy<object?> DefaultLazy;
        private readonly Lazy<Func<object>> CreateInstanceLazy;
        private readonly Lazy<Type[]> InterfacesLazy;
        private readonly Lazy<bool> IsEnumerableLazy;
        private readonly Lazy<CollectionInfo?> CollectionLazy;

        /// <summary>
        /// Creates a new instance of the <see cref="TypeProxy"/> class.
        /// </summary>
        /// <param name="proxiedType">The type to create a proxy from.</param>
        public TypeProxy(Type proxiedType)
        {
            if (proxiedType.IsGenericType && !proxiedType.IsConstructedGenericType)
                throw new ArgumentException($"Generic type definitions cannot be proxied.");

            ProxiedType = proxiedType ?? throw new ArgumentNullException(nameof(proxiedType));
            IsValueType = proxiedType.IsValueType;

            if (Nullable.GetUnderlyingType(proxiedType) is { } nullableType)
            {
                IsValueType = false;
                IsNullableValueType = true;
                UnderlyingType = nullableType.TypeInfo();
            }
            else if (IsEnum && Enum.GetUnderlyingType(proxiedType) is { } enumBaseType)
            {
                UnderlyingType = enumBaseType.TypeInfo();
            }

            UnderlyingType ??= this;
            IsNumeric = TypeManager.NumericTypes.Contains(UnderlyingType.ProxiedType);
            IsBasicType = TypeManager.BasicValueTypes.Contains(UnderlyingType.ProxiedType);

            FieldsLazy = new(() => proxiedType.GetFields(PublicAndPrivate), true);
            TypeAttributesLazy = new(() => proxiedType.GetCustomAttributes(true), true);
            TryParseMethodLazy = new(() => new TryParseMethodInfo(this), true);
            ToStringMethodLazy = new(() => new ToStringMethodInfo(this), true);
            DefaultConstructorLazy = new(() => !IsValueType
                ? proxiedType.GetConstructor(PublicAndPrivate, null, Type.EmptyTypes, null)
                : null, true);

            DefaultLazy = new(() => IsValueType ? Activator.CreateInstance(proxiedType) : null, true);
            CreateInstanceLazy = new(() =>
            {
                if (IsValueType)
                    return new(() => DefaultValue!);

                var constructor = DefaultConstructorLazy.Value;
                return constructor is not null
                    ? (Func<object>)Expression.Lambda(Expression.New(constructor)).Compile()
                    : () => throw new MissingMethodException($"Type '{ProxiedType.Name}' does not have a parameterless constructor.");

            }, true);
            InterfacesLazy = new(() => ProxiedType.GetInterfaces(), true);
            IsEnumerableLazy = new(() => Interfaces.Any(c => c == typeof(IEnumerable)), true);
            GenericTypeArgumentsLazy = new(() =>
            {
                return ProxiedType.GenericTypeArguments.Select(c => c.TypeInfo()).ToArray();
            }, true);

            CollectionLazy = new(() => IsEnumerable ? CollectionInfo.Create(this) : default, true);

            PropertiesLazy = new(() =>
            {
                var properties = ProxiedType.GetProperties(PublicAndPrivate);
                var proxies = new Dictionary<string, IPropertyProxy>(properties.Length, StringComparer.Ordinal);

                foreach (var propertyInfo in properties)
                {
                    // skip indexers
                    if (propertyInfo.GetIndexParameters().Length > 0)
                        continue;

                    // skip properties from base classes, as this class might have declared a new property
                    if (proxies.ContainsKey(propertyInfo.Name) && ProxiedType != propertyInfo.DeclaringType)
                        continue;

                    proxies[propertyInfo.Name] = new PropertyProxy(ProxiedType, propertyInfo);
                }

                return proxies;
            }, true);

            PropertyThesaurusLazy = new(() =>
            {
                var thesaurus = new Dictionary<string, IPropertyProxy>(Properties.Count, StringComparer.Ordinal);
                foreach (var property in Properties.Values)
                {
                    thesaurus[property.PropertyName] = property;
                    thesaurus[property.PropertyName.ToUpperInvariant()] = property;

                    if (!property.PropertyName.Contains('.', StringComparison.Ordinal))
                        continue;

                    var parts = property.PropertyName.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    thesaurus[parts[^1]] = property;
                    thesaurus[parts[^1].ToUpperInvariant()] = property;
                }

                return thesaurus;
            }, true);
        }

        /// <inheritdoc />
        public Type ProxiedType { get; }

        /// <inheritdoc />
        public bool IsNullableValueType { get; }

        /// <inheritdoc />
        public bool IsNumeric { get; }

        /// <inheritdoc />
        public bool IsConstructedGenericType => ProxiedType.IsConstructedGenericType;

        /// <inheritdoc />
        public bool IsValueType { get; }

        /// <inheritdoc />
        public bool IsAbstract => ProxiedType.IsAbstract;

        /// <inheritdoc />
        public bool IsInterface => ProxiedType.IsInterface;

        /// <inheritdoc />
        public bool IsEnum => ProxiedType.IsEnum;

        /// <inheritdoc />
        public bool IsBasicType { get; }

        /// <inheritdoc />
        public CollectionInfo? Collection => CollectionLazy.Value;

        /// <inheritdoc />
        public ITypeProxy UnderlyingType { get; }

        /// <inheritdoc />
        public object? DefaultValue => DefaultLazy.Value;

        /// <inheritdoc />
        public bool CanParseNatively => ProxiedType == typeof(string) || TryParseMethodInfo != null;

        /// <inheritdoc />
        public bool CanCreateInstance => IsValueType || (!IsAbstract && !IsInterface && DefaultConstructorLazy.Value is not null);

        /// <inheritdoc />
        public bool IsEnumerable => IsEnumerableLazy.Value;

        /// <inheritdoc />
        public IReadOnlyList<ITypeProxy> GenericTypeArguments => GenericTypeArgumentsLazy.Value;

        /// <inheritdoc />
        public IReadOnlyList<Type> Interfaces => InterfacesLazy.Value;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IPropertyProxy> Properties => PropertiesLazy.Value;

        /// <inheritdoc />
        public IReadOnlyList<FieldInfo> Fields => FieldsLazy.Value;

        /// <inheritdoc />
        public IReadOnlyList<object> TypeAttributes => TypeAttributesLazy.Value;

        private MethodInfo? TryParseMethodInfo => TryParseMethodLazy.Value.Method;

        private MethodInfo? ToStringMethodInfo => ToStringMethodLazy.Value.Method;

        /// <inheritdoc />
        public object CreateInstance() => CreateInstanceLazy.Value.Invoke();

        /// <inheritdoc />
        public string ToStringInvariant(object? instance)
        {
            if (instance is null)
                return string.Empty;

            return ToStringMethodInfo is not null && ToStringMethodLazy.Value.Parameters.Count == 1
                ? ToStringMethodInfo.Invoke(instance, new object[] { CultureInfo.InvariantCulture }) as string ?? string.Empty
                : instance.ToString() ?? string.Empty;
        }

        /// <inheritdoc />
        public bool TryParse(string s, [MaybeNullWhen(false)] out object? result)
        {
            result = DefaultValue;

            if (TryParseMethodInfo is null)
                return false;

            if (ProxiedType == typeof(string))
            {
                result = s;
                return true;
            }

            if ((IsNullableValueType && string.IsNullOrWhiteSpace(s)))
            {
                return true;
            }

            try
            {
                // Build the arguments of the TryParse method
                var dynamicArguments = new List<object?>(8) { s };

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

        /// <inheritdoc />
        public bool TryFindProperty(string name, [MaybeNullWhen(false)] out IPropertyProxy value)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            if (Properties.TryGetValue(name, out value))
                return true;

            return PropertyThesaurusLazy.Value.TryGetValue(name.ToUpperInvariant(), out value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Type Proxy: {ProxiedType.Name}";
        }
    }
}
