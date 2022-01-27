namespace Swan.Reflection;

/// <summary>
/// Provides extended type information.
/// </summary>
public interface ITypeInfo
{
    /// <summary>
    /// Provides the unique type from which this type info was derived.
    /// </summary>
    Type NativeType { get; }

    /// <summary>
    /// Provides the type name without the full namespace.
    /// </summary>
    string ShortName { get; }

    /// <summary>
    /// Equivalent to calling the ToString method on the type which
    /// produces the type name with the full namespace.
    /// </summary>
    string FullName { get; }

    /// <summary>
    /// Gets a value indicating whether the type is a nullable value type.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is nullable value type; otherwise, <c>false</c>.
    /// </value>
    bool IsNullable { get; }

    /// <summary>
    /// Gets a value indicating whether the type or underlying type is numeric.
    /// Enums are considered numeric.
    /// </summary>
    /// <value>
    ///  <c>true</c> if this instance is numeric; otherwise, <c>false</c>.
    /// </value>
    bool IsNumeric { get; }

    /// <summary>
    /// Gets a value indicating whether the type is an array.
    /// </summary>
    /// <value>
    ///  <c>true</c> if this instance is an array; otherwise, <c>false</c>.
    /// </value>
    bool IsArray { get; }

    /// <summary>
    /// Gets a value indicating whether the type is value type.
    /// Nullable value types have this property set to false.
    /// </summary>
    bool IsValueType { get; }

    /// <summary>
    /// Gets a value indicating whether the type is abstract.
    /// </summary>
    bool IsAbstract { get; }

    /// <summary>
    /// Gets a value indicating whether the type is an interface.
    /// </summary>
    bool IsInterface { get; }

    /// <summary>
    /// Gets a value indicating whether the type is an enumeration regardless of nullability.
    /// </summary>
    bool IsEnum { get; }

    /// <summary>
    /// Gets a value indicating whether this type implements <see cref="IEnumerable"/>.
    /// </summary>
    bool IsEnumerable { get; }

    /// <summary>
    /// Gets a value indicating whether the type is basic.
    /// Basic types are all primitive types plus strings, GUIDs , TimeSpans, DateTimes
    /// including their nullable versions. Also, enums, since they are just integral types
    /// are considered basic types, even their nullable versions.
    /// </summary>
    bool IsBasicType { get; }

    /// <summary>
    /// Gets a value indicating whether this type was built from a generic one.
    /// </summary>
    bool IsConstructedGenericType { get; }

    /// <summary>
    /// Gets a list of generic type arguments. Might be empty if not available.
    /// </summary>
    IReadOnlyList<ITypeInfo> GenericTypeArguments { get; }

    /// <summary>
    /// When dealing with nullable value types, this property will
    /// return the underlying value type of the nullable. When dealing with
    /// enumerations, nullable or not, it will return the numeric type backing it.
    /// Otherwise it will return the same type as the <see cref="NativeType"/> property.
    /// </summary>
    ITypeInfo BackingType { get; }

    /// <summary>
    /// When dealing with enums (nullable or otherwise), it will return the enumeration's type.
    /// If not dealing with an enum type, then this property returns null.
    /// </summary>
    ITypeInfo? EnumType { get; }

    /// <summary>
    /// Gets the default value for this type.
    /// Reference types return null while value types return their default equivalent.
    /// </summary>
    object? DefaultValue { get; }

    /// <summary>
    /// Gets a value indicating whether the type contains a suitable TryParse method.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance can parse natively; otherwise, <c>false</c>.
    /// </value>
    public bool CanParseNatively { get; }

    /// <summary>
    /// Determines if a parameter-less constructor can be called on this type.
    /// This always returns <c>true</c> on value types.
    /// </summary>
    bool CanCreateInstance { get; }

    /// <summary>
    /// Provides collection metadata if available that enables
    /// a uniform API to manipulate collection objects.
    /// </summary>
    ICollectionInfo? Collection { get; }

    /// <summary>
    /// Retrieves a list of interfaces this type implements.
    /// </summary>
    IReadOnlyList<Type> Interfaces { get; }

    /// <summary>
    /// Gets the property proxies associated with this type.
    /// </summary>
    /// <returns>A dictionary with property names as keys and <see cref="IPropertyProxy"/>
    /// objects as values.</returns>
    IReadOnlyDictionary<string, IPropertyProxy> Properties { get; }

    /// <summary>
    /// Provides a collection of all instance fields (public and non public) for this type.
    /// </summary>
    IReadOnlyList<FieldInfo> Fields { get; }

    /// <summary>
    /// Provides a collection of all instance methods (public and non public) for this type.
    /// </summary>
    IReadOnlyList<MethodInfo> Methods { get; }

    /// <summary>
    /// Provides a collection of all instances of attributes
    /// applied on this type and its parent types.
    /// </summary>
    IReadOnlyList<object> TypeAttributes { get; }

    /// <summary>
    /// Calls the parameter-less constructor on this type returning an instance.
    /// For value types it returns the default value.
    /// If no parameter-less constructor is available a <see cref="MissingMethodException"/> is thrown.
    /// </summary>
    /// <returns>A new instance of this type or the default value for value types.</returns>
    object CreateInstance();

    /// <summary>
    /// Converts this instance to its string representation, 
    /// trying to use the CultureInfo.InvariantCulture
    /// IFormat provider if the overload is available.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>A <see cref="System.String" /> that represents the current object.</returns>
    string ToStringInvariant(object? instance);

    /// <summary>
    /// Tries to parse the string into an object of the type this instance represents.
    /// Returns false when no suitable TryParse methods exists for the type or when parsing fails
    /// for any reason. When possible, this method uses CultureInfo.InvariantCulture and NumberStyles.Any.
    /// </summary>
    /// <param name="s">The s.</param>
    /// <param name="result">The result.</param>
    /// <returns><c>true</c> if parse was converted successfully; otherwise, <c>false</c>.</returns>
    bool TryParse(string? s, [MaybeNullWhen(false)] out object result);

    /// <summary>
    /// Tries to find a property by its name, first by exact match, then by various
    /// other methods in a best-effort fashion.
    /// </summary>
    /// <param name="name">The property name to match.</param>
    /// <param name="value">The property proxy that was found.</param>
    /// <returns>True if successful. False otherwise.</returns>
    bool TryFindProperty(string name, [MaybeNullWhen(false)] out IPropertyProxy value);

    /// <summary>
    /// Tries to find a specific method.
    /// </summary>
    /// <param name="flags">The binding flags.</param>
    /// <param name="methodName">The method name.</param>
    /// <param name="argumentTypes">The argument types.</param>
    /// <param name="method">The output method.</param>
    /// <returns>True if the method search succeeds.</returns>
    bool TryFindMethod(BindingFlags flags, string methodName, Type[]? argumentTypes,
        [MaybeNullWhen(false)] out MethodInfo method);

    /// <summary>
    /// Tries to find a specific public instance method.
    /// </summary>
    /// <param name="methodName">The method name.</param>
    /// <param name="argumentTypes">The argument types.</param>
    /// <param name="method">The output method.</param>
    /// <returns>True if the method search succeeds.</returns>
    bool TryFindPublicMethod(string methodName, Type[]? argumentTypes, [MaybeNullWhen(false)] out MethodInfo method);

    /// <summary>
    /// Tries to find a specific public static method.
    /// </summary>
    /// <param name="methodName">The method name.</param>
    /// <param name="argumentTypes">The argument types.</param>
    /// <param name="method">The output method.</param>
    /// <returns>True if the method search succeeds.</returns>
    public bool TryFindStaticMethod(string methodName, Type[]? argumentTypes,
        [MaybeNullWhen(false)] out MethodInfo method);

    /// <summary>
    /// Tries to find a property by name and tries to read its value.
    /// </summary>
    /// <param name="instance">The object instance</param>
    /// <param name="propertyName">The property name to search for.</param>
    /// <param name="value">The output value.</param>
    /// <returns>True when the operation succeeds; false otherwise.</returns>
    bool TryReadProperty(object instance, string propertyName, out object? value);

    /// <summary>
    /// Tries to find a property by name and tries to read its value and applying a conversion
    /// to the target type.
    /// </summary>
    /// <param name="instance">The object instance</param>
    /// <param name="propertyName">The property name to search for.</param>
    /// <param name="value">The output value.</param>
    /// <returns>True when the operation succeeds; false otherwise.</returns>
    bool TryReadProperty<T>(object instance, string propertyName, out T? value);

    /// <summary>
    /// Tries to find a property by name and tries to write its value performing conversion if necessary.
    /// </summary>
    /// <param name="instance">The object instance</param>
    /// <param name="propertyName">The property name to search for.</param>
    /// <param name="value">The value to write.</param>
    /// <returns>True when the operation succeeds; false otherwise.</returns>
    bool TryWriteProperty(object instance, string propertyName, object? value);
}
