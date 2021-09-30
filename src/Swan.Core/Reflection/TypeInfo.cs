namespace Swan.Reflection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Provides a base class for efficiently exposing details about a given type.
    /// </summary>
    internal sealed class TypeInfo : ITypeInfo
    {
        /// <summary>
        /// Binding flags to retrieve instance, public and non-public members.
        /// </summary>
        private const BindingFlags PublicAndPrivate = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Type[] ToStringMethodArgTypes = new[] { typeof(IFormatProvider) };

        private readonly Lazy<IReadOnlyDictionary<string, IPropertyProxy>> PropertiesLazy;
        private readonly Lazy<IReadOnlyDictionary<string, IPropertyProxy>> PropertyThesaurusLazy;
        private readonly Lazy<object[]> TypeAttributesLazy;
        private readonly Lazy<ITypeInfo[]> GenericTypeArgumentsLazy;
        private readonly Lazy<FieldInfo[]> FieldsLazy;
        private readonly Lazy<TryParseMethodInfo> TryParseMethodLazy;
        private readonly Lazy<Func<object, string>> ToStringMethodLazy;
        private readonly Lazy<ConstructorInfo?> DefaultConstructorLazy;
        private readonly Lazy<object?> DefaultLazy;
        private readonly Lazy<Func<object>> CreateInstanceLazy;
        private readonly Lazy<Type[]> InterfacesLazy;
        private readonly Lazy<bool> IsEnumerableLazy;
        private readonly Lazy<ICollectionInfo?> CollectionLazy;
        private readonly Lazy<MethodInfo[]> MethodsLazy;

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

            if (Nullable.GetUnderlyingType(NativeType) is { } nullableBackingType)
            {
                IsValueType = false;
                IsNullable = true;
                IsEnum = nullableBackingType.IsEnum;
                EnumType = IsEnum ? nullableBackingType.TypeInfo() : null;
                BackingType = IsEnum
                    ? nullableBackingType.GetEnumUnderlyingType().TypeInfo()
                    : nullableBackingType.TypeInfo();
            }
            else if (NativeType.IsEnum)
            {
                IsValueType = true;
                IsEnum = true;
                EnumType = this;
                BackingType = NativeType.GetEnumUnderlyingType().TypeInfo();
            }

            BackingType ??= this;
            IsNumeric = TypeManager.NumericTypes.Contains(BackingType.NativeType);
            IsBasicType = TypeManager.BasicValueTypes.Contains(BackingType.NativeType);

            FieldsLazy = new(() => nativeType.GetFields(PublicAndPrivate), true);
            TypeAttributesLazy = new(() => nativeType.GetCustomAttributes(true), true);
            TryParseMethodLazy = new(() => new(this), true);
            ToStringMethodLazy = new(() => HasToStringFormatMethod()
                ? ((instance) => (instance as dynamic).ToString(CultureInfo.InvariantCulture))
                : ((instance) => instance.ToString() ?? string.Empty), true);
            DefaultConstructorLazy = new(() => !IsValueType
                ? nativeType.GetConstructor(PublicAndPrivate, null, Type.EmptyTypes, null)
                : null, true);

            DefaultLazy = new(() => IsValueType ? Activator.CreateInstance(nativeType) : null, true);
            CreateInstanceLazy = new(() =>
            {
                if (IsValueType)
                    return () => DefaultValue!;

                var constructor = DefaultConstructorLazy.Value;
                return constructor is not null
                    ? (Func<object>)Expression.Lambda(Expression.New(constructor)).Compile()
                    : () => throw new MissingMethodException($"Type '{FullName}' does not have a parameter-less constructor.");

            }, true);
            InterfacesLazy = new(() => NativeType.GetInterfaces(), true);
            IsEnumerableLazy = new(() => Interfaces.Any(c => c == typeof(IEnumerable)), true);
            GenericTypeArgumentsLazy = new(() => NativeType.GenericTypeArguments.Select(c => c.TypeInfo()).ToArray(), true);

            CollectionLazy = new(() =>
            {
                if (!IsEnumerable)
                    return default;

                var collectionInfo = new CollectionInfo(this);
                return collectionInfo.CollectionKind is not CollectionKind.None
                    ? collectionInfo
                    : default;
            }, true);

            MethodsLazy = new(() => nativeType.GetMethods(PublicAndPrivate), true);

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
        public bool IsNullable { get; }

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
        public bool IsEnum { get; }

        /// <inheritdoc />
        public bool IsBasicType { get; }

        /// <inheritdoc />
        public ICollectionInfo? Collection => CollectionLazy.Value;

        /// <inheritdoc />
        public ITypeInfo BackingType { get; }

        /// <inheritdoc />
        public ITypeInfo? EnumType { get; }

        /// <inheritdoc />
        public object? DefaultValue => DefaultLazy.Value;

        /// <inheritdoc />
        public bool CanParseNatively => NativeType == typeof(string) || TryParseMethodLazy.Value.IsNative;

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
        public IReadOnlyList<MethodInfo> Methods => MethodsLazy.Value;

        /// <inheritdoc />
        public IReadOnlyList<FieldInfo> Fields => FieldsLazy.Value;

        /// <inheritdoc />
        public IReadOnlyList<object> TypeAttributes => TypeAttributesLazy.Value;

        /// <inheritdoc />
        public object CreateInstance() => CreateInstanceLazy.Value.Invoke();

        /// <inheritdoc />
        public string ToStringInvariant(object? instance)
        {
            return instance is null
                ? string.Empty
                : ToStringMethodLazy.Value(instance);
        }

        /// <inheritdoc />
        public bool TryParse(string? s, [MaybeNullWhen(false)] out object result)
        {
            if (NativeType != typeof(string))
                return TryParseMethodLazy.Value.Invoke(s, out result);

            result = s ?? string.Empty;
            return true;
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
        public bool TryReadProperty(object instance, string propertyName, out object? value)
        {
            value = default;
            return instance is null
                ? throw new ArgumentNullException(nameof(instance))
                : TryFindProperty(propertyName, out var property) &&
                  property.TryRead(instance, out value) is not false;
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
            return instance is null
                ? throw new ArgumentNullException(nameof(instance))
                : TryFindProperty(propertyName, out var property) is not false &&
                  property?.TryWrite(instance, value) is true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var stopIndex = NativeType.Name.IndexOf('`', StringComparison.Ordinal);
            return stopIndex >= 0 && GenericTypeArguments.Count > 0
                ? $"{NativeType.Name[..stopIndex]}<{string.Join(", ", GenericTypeArguments)}>"
                : NativeType.Name;
        }

        private bool HasToStringFormatMethod()
        {
            try
            {
                var method = NativeType.GetMethod(nameof(ToString), ToStringMethodArgTypes);
                return method is not null;
            }
            catch
            {
                return false;
            }
        }
    }
}
