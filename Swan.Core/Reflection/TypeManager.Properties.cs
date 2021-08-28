using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Swan.Reflection
{
    public static partial class TypeManager
    {

        /// <summary>
        /// Gets the property proxies associated with a given type.
        /// </summary>
        /// <param name="t">The type to retrieve property proxies from.</param>
        /// <returns>The property proxies for the given type.</returns>
        public static IReadOnlyList<IPropertyProxy> Properties(this Type t) => t is not null
                ? t.TypeInfo().Properties.Values.ToArray()
                : throw new ArgumentNullException(nameof(t));

        /// <summary>
        /// Gets the property proxies associated with the provided instance type.
        /// </summary>
        /// <typeparam name="T">The instance type.</typeparam>
        /// <param name="obj">The instance.</param>
        /// <returns>A dictionary with property names as keys and <see cref="IPropertyProxy"/> objects as values.</returns>
        public static IReadOnlyList<IPropertyProxy> Properties<T>(this T obj) =>
            (obj?.GetType() ?? typeof(T)).Properties();

        /// <summary>
        /// Gets the property proxy given the property name.
        /// If the property is not found, it returns a null property proxy.
        /// </summary>
        /// <param name="t">The associated type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The associated <see cref="IPropertyProxy"/> if found; otherwise returns null.</returns>
        public static IPropertyProxy? Property(this Type t, string propertyName)
        {
            if (t == null)
                throw new ArgumentNullException(nameof(t));

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            return t.TypeInfo().Properties.TryGetValue(propertyName, out var property)
                ? property
                : null;
        }

        /// <summary>
        /// Gets the property proxy given the property name.
        /// </summary>
        /// <typeparam name="T">The type of instance to extract proxies from.</typeparam>
        /// <param name="obj">The instance to extract proxies from.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The associated <see cref="IPropertyProxy"/></returns>
        public static IPropertyProxy? Property<T>(this T obj, string propertyName) =>
            (obj?.GetType() ?? typeof(T)).Property(propertyName);

        /// <summary>
        /// Gets the property proxy given the property name as an expression.
        /// </summary>
        /// <typeparam name="T">The instance type.</typeparam>
        /// <typeparam name="TProperty">The property value type.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns>The associated <see cref="IPropertyProxy"/></returns>
        public static IPropertyProxy? Property<T, TProperty>(this T obj, Expression<Func<T, TProperty>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            var propertyName = propertyExpression.PropertyName();
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Unable to parse member expression to obtain property name", nameof(propertyExpression));

            return (obj?.GetType() ?? typeof(T)).Property(propertyName);
        }

        /// <summary>
        /// Reads the property value.
        /// </summary>
        /// <typeparam name="T">The type to get property proxies from.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="obj">The instance.</param>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns>
        /// The value obtained from the associated <see cref="IPropertyProxy" />
        /// </returns>
        /// <exception cref="ArgumentNullException">obj.</exception>
        public static TProperty? ReadProperty<T, TProperty>(this T obj, Expression<Func<T, TProperty>> propertyExpression)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var proxy = obj.Property(propertyExpression)
                ?? throw new ArgumentException("Could not find a property with the given name.", nameof(propertyExpression)); ;

            return proxy.TryGetValue(obj, out var value) && value is not null
                ? (TProperty)value
                : default;
        }

        /// <summary>
        /// Reads the property value.
        /// </summary>
        /// <typeparam name="T">The type to get property proxies from.</typeparam>
        /// <param name="obj">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// The value obtained from the associated <see cref="IPropertyProxy" />
        /// </returns>
        /// <exception cref="ArgumentNullException">obj.</exception>
        public static object? ReadProperty<T>(this T obj, string propertyName)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var proxy = obj.Property(propertyName)
                ?? throw new ArgumentException("Could not find a property with the given name.", nameof(propertyName)); ;

            return proxy.TryGetValue(obj, out var value) && value is not null
                ? value
                : default;
        }

        /// <summary>
        /// Writes the property value.
        /// </summary>
        /// <typeparam name="T">The type to get property proxies from.</typeparam>
        /// <typeparam name="TV">The type of the property.</typeparam>
        /// <param name="obj">The instance.</param>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if the property write operation was successful.</returns>
        public static bool WriteProperty<T, TV>(this T obj, Expression<Func<T, TV>> propertyExpression, TV value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var proxy = obj.Property(propertyExpression)
                ?? throw new ArgumentException("Could not find a property with the given name.", nameof(propertyExpression)); ;

            return proxy.TrySetValue(obj, value);
        }

        /// <summary>
        /// Writes the property value using the property proxy.
        /// </summary>
        /// <typeparam name="T">The type to get property proxies from.</typeparam>
        /// <param name="obj">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if the property write operation was successful.</returns>
        public static bool WriteProperty<T>(this T obj, string propertyName, object? value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var proxy = obj.Property(propertyName)
                ?? throw new ArgumentException("Could not find a property with the given name.", nameof(propertyName)); ;

            return proxy.TrySetValue(obj, value);
        }

        private static string? PropertyName<T, TV>(this Expression<Func<T, TV>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body is MemberExpression body
                ? body
                : (propertyExpression.Body as UnaryExpression)?.Operand as MemberExpression;

            if (memberExpression is not null)
                return memberExpression.Member.Name;

            return null;
        }
    }
}
