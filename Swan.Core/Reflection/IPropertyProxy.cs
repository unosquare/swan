using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// Represents a generic interface to store getters and setters for high speed access to properties.
    /// </summary>
    public interface IPropertyProxy
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// Gets the associated reflection property info.
        /// </summary>
        PropertyInfo Property { get; }

        /// <summary>
        /// Gets all the custom attributes applied to this property declaration.
        /// </summary>
        IReadOnlyCollection<object> Attributes { get; }

        /// <summary>
        /// Gets whether the property getter is declared as public.
        /// </summary>
        bool HasPublicGetter { get; }

        /// <summary>
        /// Gets whether the property setter is declared as public.
        /// </summary>
        bool HasPublicSetter { get; }

        /// <summary>
        /// Gets the type owning this property proxy.
        /// </summary>
        Type EnclosingType { get; }

        /// <summary>
        /// Gets a value indicating whether this property can be read from.
        /// </summary>
        public bool CanRead { get; }

        /// <summary>
        /// Gets a value indicating whether this property can be written to.
        /// </summary>
        public bool CanWrite { get; }

        /// <summary>
        /// Gets a value indicating whether the type or underlying type is numeric.
        /// </summary>
        /// <value>
        ///  <c>true</c> if this instance is numeric; otherwise, <c>false</c>.
        /// </value>
        bool IsNumeric => PropertyType.TypeInfo().IsNumeric;

        /// <summary>
        /// Gets a value indicating whether the type is basic.
        /// Basic types are all primitive types plus strings, GUIDs , TimeSpans, DateTimes
        /// including their nullable versions.
        /// </summary>
        bool IsBasicType => PropertyType.TypeInfo().IsBasicType;

        /// <summary>
        /// Gets a value indicating whether the type is a nullable value type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is nullable value type; otherwise, <c>false</c>.
        /// </value>
        bool IsNullableValueType => PropertyType.TypeInfo().IsNullableValueType;

        /// <summary>
        /// Gets a value indicating whether the type is value type.
        /// Nullable value types have this property set to False.
        /// </summary>
        bool IsValueType => PropertyType.TypeInfo().IsValueType;

        /// <summary>
        /// Gets the property value via a stored delegate.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>The property value.</returns>
        object? GetValue(object instance);

        /// <summary>
        /// Tries to read the current property from the given object.
        /// </summary>
        /// <param name="instance">The target instance to read the property from.</param>
        /// <param name="value">The output value. Will be set to the default value of the property type if unsuccessful.</param>
        /// <returns>True if the operation succeeds. False otherwise.</returns>
        bool TryGetValue(object instance, out object? value);

        /// <summary>
        /// Sets the property value via a stored delegate.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="value">The value.</param>
        void SetValue(object instance, object? value);

        /// <summary>
        /// Tries to safely set the value of the property by changing types if necessary.
        /// </summary>
        /// <param name="instance">The target instance containing the property.</param>
        /// <param name="value">The value to pass into the property.</param>
        /// <returns>Returns true if the operation was successful. False otherwise.</returns>
        bool TrySetValue(object instance, object? value);

        /// <summary>
        /// Searches for the first attribute of the given type.
        /// </summary>
        /// <typeparam name="T">The attribute type to search for.</typeparam>
        /// <returns>Returns a null if an attribute of the given type is not found.</returns>
        T? Attribute<T>() where T : Attribute => Attributes.FirstOrDefault(c => c.GetType() == typeof(T)) as T;

        /// <summary>
        /// Searches for the first attribute of the given type.
        /// </summary>
        /// <param name="attributeType">The attribute type to search for.</param>
        /// <returns>Returns a null if an attribute of the given type is not found.</returns>
        object? Attribute(Type attributeType) =>
            attributeType is null
                ? throw new ArgumentNullException(nameof(attributeType))
                : Attributes.FirstOrDefault(c => c.GetType().IsAssignableTo(attributeType));

        /// <summary>
        /// Gets a value indicating whether an attribute of the given type has been applied.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to search for.</typeparam>
        /// <returns>True if the attribute is found. False otherwise.</returns>
        bool HasAttribute<T>() where T : Attribute => Attribute<T>() is not null;

        /// <summary>
        /// Gets a value indicating whether an attribute of the given type has been applied.
        /// </summary>
        /// <param name="attributeType">The type of the attribute to search for.</param>
        /// <returns>True if the attribute is found. False otherwise.</returns>
        bool HasAttribute(Type attributeType) => Attribute(attributeType) is not null;
    }
}