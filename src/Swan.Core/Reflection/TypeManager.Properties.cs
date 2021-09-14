using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swan.Reflection
{
    public static partial class TypeManager
    {
        /// <summary>
        /// Converts a PropertyInfo object to an IPropertyProxy.
        /// </summary>
        /// <param name="propertyInfo">The source property info.</param>
        /// <returns>A corresponding property proxy.</returns>
        public static IPropertyProxy ToPropertyProxy(this PropertyInfo propertyInfo)
        {
            if (propertyInfo is null)
                throw new ArgumentNullException(nameof(propertyInfo));

            if (propertyInfo.ReflectedType is null)
                throw new ArgumentException($"Unable to obtain enclosing type for property '{propertyInfo.Name}'.", nameof(propertyInfo));

            return propertyInfo.ReflectedType.Property(propertyInfo.Name)
                ?? throw new KeyNotFoundException($"Could not find property '{propertyInfo.Name}' for type '{propertyInfo.ReflectedType.Name}'");
        }

        /// <summary>
        /// Gets the property proxies associated with a given type.
        /// </summary>
        /// <param name="t">The type to retrieve property proxies from.</param>
        /// <returns>The property proxies for the given type.</returns>
        public static IReadOnlyList<IPropertyProxy> Properties(this Type t) => t is not null
                ? t.TypeInfo().Properties.Values.ToArray()
                : throw new ArgumentNullException(nameof(t));

        /// <summary>
        /// Gets the property proxies associated with a given type.
        /// </summary>
        /// <param name="t">The type to retrieve property proxies from.</param>
        /// <returns>The property proxies for the given type.</returns>
        public static IReadOnlyList<IPropertyProxy> Properties(this ITypeInfo t) => t is not null
                ? t.Properties.Values.ToArray()
                : throw new ArgumentNullException(nameof(t));

        /// <summary>
        /// Gets the property proxy given the property name.
        /// If the property is not found, it returns a null property proxy.
        /// </summary>
        /// <param name="t">The associated type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The associated <see cref="IPropertyProxy"/> if found; otherwise returns null.</returns>
        public static IPropertyProxy? Property(this ITypeInfo t, string propertyName)
        {
            if (t == null)
                throw new ArgumentNullException(nameof(t));

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            return t.TryFindProperty(propertyName, out var property)
                ? property
                : null;
        }

        /// <summary>
        /// Gets the property proxy given the property name.
        /// If the property is not found, it returns a null property proxy.
        /// </summary>
        /// <param name="t">The associated type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The associated <see cref="IPropertyProxy"/> if found; otherwise returns null.</returns>
        public static IPropertyProxy? Property(this Type t, string propertyName) =>
            t is not null
                ? t.TypeInfo().Property(propertyName)
                : throw new ArgumentNullException(nameof(t));

        /// <summary>
        /// Reads the property value using a <see cref="IPropertyProxy"/>.
        /// </summary>
        /// <typeparam name="T">The type to get property proxies from.</typeparam>
        /// <param name="obj">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// The value obtained from the associated <see cref="IPropertyProxy" />
        /// </returns>
        /// <exception cref="ArgumentNullException">obj.</exception>
        public static dynamic? ReadProperty<T>(this T obj, string propertyName)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            var proxy = obj.GetType().Property(propertyName)
                ?? throw new ArgumentException($"Could not find a property with the name '{propertyName}'.", nameof(propertyName));

            return proxy.TryRead(obj, out var value)
                ? value
                : default;
        }

        /// <summary>
        /// Writes the property value using a <see cref="IPropertyProxy"/>.
        /// </summary>
        /// <typeparam name="T">The type to get property proxies from.</typeparam>
        /// <param name="obj">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if the property write operation was successful.</returns>
        public static bool WriteProperty<T>(this T obj, string propertyName, object? value)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            var proxy = obj.GetType().Property(propertyName)
                ?? throw new ArgumentException($"Could not find a property with name '{propertyName}'.", nameof(propertyName));

            return proxy.TryWrite(obj, value);
        }
    }
}
