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
    internal sealed class TypeInfo : ITypeInfo
    {
        /// <summary>
        /// Binding flags to retrieve instance, public and non-public members.
        /// </summary>
        private const BindingFlags PublicAndPrivate = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Lazy<IReadOnlyDictionary<string, IPropertyProxy>> PropertiesLazy;
        private readonly Lazy<IReadOnlyDictionary<string, IPropertyProxy>> PropertyThesaurusLazy;
        private readonly Lazy<object[]> TypeAttributesLazy;
        private readonly Lazy<ITypeInfo[]> GenericTypeArgumentsLazy;
        private readonly Lazy<FieldInfo[]> FieldsLazy;
        private readonly Lazy<TryParseMethodInfo> TryParseMethodLazy;
        private readonly Lazy<ToStringMethodInfo> ToStringMethodLazy;
        private readonly Lazy<ConstructorInfo?> DefaultConstructorLazy;
        private readonly Lazy<object?> DefaultLazy;
        private readonly Lazy<Func<object>> CreateInstanceLazy;
        private readonly Lazy<Type[]> InterfacesLazy;
        private readonly Lazy<bool> IsEnumerableLazy;
        private readonly Lazy<ICollectionInfo?> CollectionLazy;

        /// <summary>
        /// Creates a new instance of the <see cref="TypeInfo"/> class.
        /// </summary>
        /// <param name="nativeType">The type to create a proxy from.</param>
        public TypeInfo(Type nativeType)
        {
            if (nativeType.IsGenericType && !nativeType.IsConstructedGenericType)
                throw new ArgumentException($"Generic type definitions cannot be proxied.");

            NativeType = nativeType ?? throw new ArgumentNullException(nameof(nativeType));
            IsValueType = nativeType.IsValueType;

            if (Nullable.GetUnderlyingType(nativeType) is { } nullableType)
            {
                IsValueType = false;
                IsNullableValueType = true;
                UnderlyingType = nullableType.TypeInfo();
            }
            else if (IsEnum && Enum.GetUnderlyingType(nativeType) is { } enumBaseType)
            {
                UnderlyingType = enumBaseType.TypeInfo();
            }

            UnderlyingType ??= this;
            IsNumeric = TypeManager.NumericTypes.Contains(UnderlyingType.NativeType);
            IsBasicType = TypeManager.BasicValueTypes.Contains(UnderlyingType.NativeType);

            FieldsLazy = new(() => nativeType.GetFields(PublicAndPrivate), true);
            TypeAttributesLazy = new(() => nativeType.GetCustomAttributes(true), true);
            TryParseMethodLazy = new(() => new TryParseMethodInfo(this), true);
            ToStringMethodLazy = new(() => new ToStringMethodInfo(this), true);
            DefaultConstructorLazy = new(() => !IsValueType
                ? nativeType.GetConstructor(PublicAndPrivate, null, Type.EmptyTypes, null)
                : null, true);

            DefaultLazy = new(() => IsValueType ? Activator.CreateInstance(nativeType) : null, true);
            CreateInstanceLazy = new(() =>
            {
                if (IsValueType)
                    return new(() => DefaultValue!);

                var constructor = DefaultConstructorLazy.Value;
                return constructor is not null
                    ? (Func<object>)Expression.Lambda(Expression.New(constructor)).Compile()
                    : () => throw new MissingMethodException($"Type '{FullName}' does not have a parameter-less constructor.");

            }, true);
            InterfacesLazy = new(() => NativeType.GetInterfaces(), true);
            IsEnumerableLazy = new(() => Interfaces.Any(c => c == typeof(IEnumerable)), true);
            GenericTypeArgumentsLazy = new(() =>
            {
                return NativeType.GenericTypeArguments.Select(c => c.TypeInfo()).ToArray();
            }, true);

            CollectionLazy = new(() =>
            {
                if (!IsEnumerable)
                    return default;

                var collectionInfo = new CollectionInfo(this);
                return collectionInfo.CollectionKind is not CollectionKind.None
                    ? collectionInfo
                    : default;
            }, true);

            PropertiesLazy = new(() =>
            {
                var properties = NativeType.GetProperties(PublicAndPrivate);
                var proxies = new Dictionary<string, IPropertyProxy>(properties.Length, StringComparer.Ordinal);

                foreach (var propertyInfo in properties)
                {
                    // skip indexers
                    if (propertyInfo.GetIndexParameters().Length > 0)
                        continue;

                    // skip properties from base classes, as this class might have declared a new property
                    if (proxies.ContainsKey(propertyInfo.Name) && NativeType != propertyInfo.DeclaringType)
                        continue;

                    proxies[propertyInfo.Name] = new PropertyProxy(this, propertyInfo);
                }

                return proxies;
            }, true);

            PropertyThesaurusLazy = new(() =>
            {
                var thesaurus = new Dictionary<string, IPropertyProxy>(Properties.Count * 4, StringComparer.Ordinal);
                foreach (var property in Properties.Values)
                {
                    var propertyName = property.PropertyName;
                    thesaurus[property.PropertyName] = property;
                    thesaurus.TryAdd(propertyName.ToUpperInvariant(), property);

                    if (!propertyName.Contains('.', StringComparison.Ordinal))
                        continue;

                    var parts = property.PropertyName.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    propertyName = $".{parts[^1]}";
                    thesaurus.TryAdd(propertyName, property);
                    thesaurus.TryAdd(propertyName.ToUpperInvariant(), property);
                }

                return thesaurus;
            }, true);
        }

        /// <inheritdoc />
        public Type NativeType { get; }

        /// <inheritdoc />
        public string ShortName => NativeType.Name;

        /// <inheritdoc />
        public string FullName => NativeType.ToString();

        /// <inheritdoc />
        public bool IsNullableValueType { get; }

        /// <inheritdoc />
        public bool IsNumeric { get; }

        /// <inheritdoc />
        public bool IsConstructedGenericType => NativeType.IsConstructedGenericType;

        /// <inheritdoc />
        public bool IsValueType { get; }

        /// <inheritdoc />
        public bool IsAbstract => NativeType.IsAbstract;

        /// <inheritdoc />
        public bool IsInterface => NativeType.IsInterface;

        /// <inheritdoc />
        public bool IsEnum => NativeType.IsEnum;

        /// <inheritdoc />
        public bool IsBasicType { get; }

        /// <inheritdoc />
        public ICollectionInfo? Collection => CollectionLazy.Value;

        /// <inheritdoc />
        public ITypeInfo UnderlyingType { get; }

        /// <inheritdoc />
        public object? DefaultValue => DefaultLazy.Value;

        /// <inheritdoc />
        public bool CanParseNatively => NativeType == typeof(string) || TryParseMethodInfo != null;

        /// <inheritdoc />
        public bool CanCreateInstance => IsValueType || (!IsAbstract && !IsInterface && DefaultConstructorLazy.Value is not null);

        /// <inheritdoc />
        public bool IsEnumerable => IsEnumerableLazy.Value;

        /// <inheritdoc />
        public bool IsArray => NativeType.IsArray;

        /// <inheritdoc />
        public IReadOnlyList<ITypeInfo> GenericTypeArguments => GenericTypeArgumentsLazy.Value;

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

            if (NativeType == typeof(string))
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

            var thesaurus = PropertyThesaurusLazy.Value;

            return thesaurus.TryGetValue(name, out value) ||
                   thesaurus.TryGetValue(name.ToUpperInvariant(), out value) ||
                   thesaurus.TryGetValue($".{name}", out value) ||
                   thesaurus.TryGetValue($".{name.ToUpperInvariant()}", out value);
        }

        /// <inheritdoc />
        public bool TryReadProperty(object instance, string propertyName, [MaybeNullWhen(false)] out object? value)
        {
            value = default;
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            if (!TryFindProperty(propertyName, out var property))
                return false;

            return property.TryRead(instance, out value) is not false;
        }

        /// <inheritdoc />
        public bool TryReadProperty<T>(object instance, string propertyName, out T? value)
        {
            value = default;

            if (!TryReadProperty(instance, propertyName, out var propertyValue))
                return false;

            switch (propertyValue)
            {
                case null:
                    return true;
                case T originalValue:
                    value = originalValue;
                    return true;
            }

            if (!TypeManager.TryChangeType(propertyValue, typeof(T), out object? objectValue))
                return false;

            if (objectValue is not T convertedValue)
                return false;

            value = convertedValue;
            return true;
        }

        /// <inheritdoc />
        public bool TryWriteProperty(object instance, string propertyName, object? value)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            if (TryFindProperty(propertyName, out var property) is false)
                return false;

            if (property.TryWrite(instance, value) is false)
                return false;

            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Type Proxy: {NativeType.Name}";
        }
    }
}
