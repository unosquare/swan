namespace Swan.Reflection;

/// <summary>
/// Represents a generic interface to store getters and setters for high speed access to properties.
/// </summary>
public interface IPropertyProxy
{
    /// <summary>
    /// Gets the type proxy that owns this property proxy.
    /// </summary>
    ITypeInfo ParentType { get; }

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Gets the associated reflection property info.
    /// </summary>
    PropertyInfo PropertyInfo { get; }

    /// <summary>
    /// Gets the property type metadata.
    /// </summary>
    ITypeInfo PropertyType { get; }

    /// <summary>
    /// Gets the default value for this type.
    /// Reference types return null while value types return their default equivalent.
    /// </summary>
    object? DefaultValue { get; }

    /// <summary>
    /// Gets all the custom attributes applied to this property declaration.
    /// </summary>
    IReadOnlyList<object> PropertyAttributes { get; }

    /// <summary>
    /// Gets whether the property getter is declared as public.
    /// </summary>
    bool HasPublicGetter { get; }

    /// <summary>
    /// Gets whether the property setter is declared as public.
    /// </summary>
    bool HasPublicSetter { get; }

    /// <summary>
    /// Gets a value indicating whether this property can be read from.
    /// </summary>
    public bool CanRead { get; }

    /// <summary>
    /// Gets a value indicating whether this property can be written to.
    /// </summary>
    public bool CanWrite { get; }

    /// <summary>
    /// Gets the property value via a stored delegate.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>The property value.</returns>
    object? Read(object instance);

    /// <summary>
    /// Tries to read the current property from the given object.
    /// </summary>
    /// <param name="instance">The target instance to read the property from.</param>
    /// <param name="value">The output value. Will be set to the default value of the property type if unsuccessful.</param>
    /// <returns>True if the operation succeeds. False otherwise.</returns>
    bool TryRead(object instance, out object? value);

    /// <summary>
    /// Sets the property value via a stored delegate.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="value">The value.</param>
    void Write(object instance, object? value);

    /// <summary>
    /// Tries to safely set the value of the property by changing types if necessary.
    /// </summary>
    /// <param name="instance">The target instance containing the property.</param>
    /// <param name="value">The value to pass into the property.</param>
    /// <returns>Returns true if the operation was successful. False otherwise.</returns>
    bool TryWrite(object instance, object? value);
}
