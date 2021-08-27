using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides access to a cached repository of <see cref="ExtendedTypeInfo"/>.
    /// </summary>
    public static partial class TypeManager
    {
        private static readonly ConcurrentDictionary<Type, ExtendedTypeInfo> TypeCache = new();

        /// <summary>
        /// Provides a callection of primitive, numeric types.
        /// </summary>
        public static IReadOnlyCollection<Type> NumericTypes { get; } = new[]
        {
            typeof(byte),
            typeof(sbyte),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
        };

        /// <summary>
        /// Provides a collection of basic value types including numeric types,
        /// string, guid, timespan, and datetime.
        /// </summary>
        public static IReadOnlyCollection<Type> BasicValueTypes { get; } = new[]
        {
                typeof(int),
                typeof(bool),
                typeof(string),
                typeof(DateTime),
                typeof(double),
                typeof(decimal),
                typeof(Guid),
                typeof(long),
                typeof(TimeSpan),
                typeof(uint),
                typeof(float),
                typeof(byte),
                typeof(short),
                typeof(sbyte),
                typeof(ushort),
                typeof(ulong),
                typeof(char),
        };

        /// <summary>
        /// Provides cached and extended type information for
        /// easy and efficient access to common reflection scenarios.
        /// </summary>
        /// <param name="t">The type to provide extended info for.</param>
        /// <returns>Returns an <see cref="ExtendedTypeInfo"/> for the given type.</returns>
        public static ExtendedTypeInfo TypeInfo(this Type t)
        {
            if (t is null)
                throw new ArgumentNullException(nameof(t));

            if (TypeCache.TryGetValue(t, out var typeInfo))
                return typeInfo;

            typeInfo = new ExtendedTypeInfo(t);
            TypeCache.TryAdd(t, typeInfo);

            return typeInfo;
        }

        /// <summary>
        /// The closest programmatic equivalent of default(T).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// Default value of this type.
        /// </returns>
        /// <exception cref="ArgumentNullException">type.</exception>
        public static object? GetDefault(this Type type) => type is not null
            ? type.TypeInfo().Default
            : throw new ArgumentNullException(nameof(type));

        /// <summary>
        /// Calls the parameterless constructor on this type returning an isntance.
        /// For value types it returns the default value.
        /// If no parameterless constructor is available a <see cref="MissingMethodException"/> is thrown.
        /// </summary>
        /// <param name="type">The type to create an instance of.</param>
        /// <returns>A new instance of this type or the default value for value types.</returns>
        public static object CreateInstance(this Type type) => type is not null
            ? type.TypeInfo().CreateInstance()
            : throw new ArgumentNullException(nameof(type));

        /// <summary>
        /// Calls the parameterless constructor on this type returning an isntance.
        /// For value types it returns the default value.
        /// If no parameterless constructor is available a <see cref="MissingMethodException"/> is thrown.
        /// </summary>
        /// <typeparam name="T">The type to create an instance of.</typeparam>
        /// <returns>A new instance of this type or the default value for value types.</returns>
        public static T CreateInstance<T>() => (T)typeof(T).CreateInstance();

        /// <summary>
        /// Gets the property proxies associated with a given type.
        /// </summary>
        /// <param name="t">The type to retrieve property proxies from.</param>
        /// <returns>The property proxies for the given type.</returns>
        public static IReadOnlyCollection<IPropertyProxy> Properties(this Type t) => t is not null
                ? t.TypeInfo().Properties.Values.ToArray()
                : throw new ArgumentNullException(nameof(t));

        /// <summary>
        /// Gets the property proxies associated with the provided instance type.
        /// </summary>
        /// <typeparam name="T">The instance type.</typeparam>
        /// <param name="obj">The instance.</param>
        /// <returns>A dictionary with property names as keys and <see cref="IPropertyProxy"/> objects as values.</returns>
        public static IReadOnlyCollection<IPropertyProxy> Properties<T>(this T obj) =>
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

        /// <summary>
        /// Searches for the first attribute of the given type.
        /// </summary>
        /// <typeparam name="T">The attribute type to search for.</typeparam>
        /// <param name="type">The type to be searched.</param>
        /// <returns>Returns a null if an attribute of the given type is not found.</returns>
        public static T? Attribute<T>(this Type type)
            where T : Attribute =>
            type?.TypeInfo().AllAttributes.FirstOrDefault(c => c.GetType() == typeof(T)) as T;

        /// <summary>
        /// Searches for the first attribute of the given type.
        /// </summary>
        /// <param name="type">The type to be searched.</param>
        /// <param name="attributeType">The attribute type to search for.</param>
        /// <returns>Returns a null if an attribute of the given type is not found.</returns>
        public static object? Attribute(this Type type, Type attributeType) =>
            attributeType is null
            ? throw new ArgumentNullException(nameof(attributeType))
            : type?.TypeInfo().AllAttributes.FirstOrDefault(c => c.GetType().IsAssignableTo(attributeType));

        /// <summary>
        /// Gets a value indicating whether an attribute of the given type has been applied.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to search for.</typeparam>
        /// <param name="type">The type to be searched.</param>
        /// <returns>True if the attribute is found. False otherwise.</returns>
        public static bool HasAttribute<T>(this Type type)
            where T : Attribute =>
            type?.Attribute<T>() is not null;

        /// <summary>
        /// Gets a value indicating whether an attribute of the given type has been applied.
        /// </summary>
        /// <param name="type">The type to be searched.</param>
        /// <param name="attributeType">The type of the attribute to search for.</param>
        /// <returns>True if the attribute is found. False otherwise.</returns>
        public static bool HasAttribute(this Type type, Type attributeType) =>
            type?.Attribute(attributeType) is not null;


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
