using System;
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
        /// Gets the property value via a stored delegate.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>The property value.</returns>
        object GetValue(object instance);

        /// <summary>
        /// Tries to read the current property from the given object.
        /// </summary>
        /// <param name="instance">The target instance to read the property from.</param>
        /// <param name="value">The output value. Will be set to the default value of the property type if unsuccessful.</param>
        /// <returns>True if the operation succeeds. False otherwise.</returns>
        bool TryGetValue(object instance, out object value);

        /// <summary>
        /// Sets the property value via a stored delegate.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="value">The value.</param>
        void SetValue(object instance, object value);

        /// <summary>
        /// Tries to safely set the value of the property by changing types if necessary.
        /// </summary>
        /// <param name="instance">The target instance containing the property.</param>
        /// <param name="value">The value to pass into the property.</param>
        /// <returns>Returns true if the operation was successful. False otherwise.</returns>
        bool TrySetValue(object instance, object value);
    }
}