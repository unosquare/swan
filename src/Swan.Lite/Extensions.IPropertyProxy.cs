using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Swan.Reflection;

namespace Swan
{
    /// <summary>
    /// Provides functionality to access <see cref="IPropertyProxy"/> objects
    /// associated with types. Getters and setters are stored as delegates compiled
    /// from constructed lambda expressions for fast access.
    /// </summary>
    public static class PropertyProxyExtensions
    {
        private static readonly object SyncLock = new object();
        private static readonly Dictionary<Type, Dictionary<string, IPropertyProxy>> ProxyCache =
            new Dictionary<Type, Dictionary<string, IPropertyProxy>>(32);

        /// <summary>
        /// Gets the property proxies associated with a given type.
        /// </summary>
        /// <param name="t">The type to retrieve property proxies from.</param>
        /// <returns>A dictionary with property names as keys and <see cref="IPropertyProxy"/> objects as values.</returns>
        public static Dictionary<string, IPropertyProxy> PropertyProxies(this Type t)
        {
            if (t == null) 
                throw new ArgumentNullException(nameof(t));

            lock (SyncLock)
            {
                if (ProxyCache.ContainsKey(t))
                    return ProxyCache[t];

                var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                var result = new Dictionary<string, IPropertyProxy>(properties.Length, StringComparer.InvariantCultureIgnoreCase);
                foreach (var propertyInfo in properties)
                    result[propertyInfo.Name] = new PropertyInfoProxy(t, propertyInfo);

                ProxyCache[t] = result;
                return result;
            }
        }

        /// <summary>
        /// Gets the property proxies associated with the provided instance type.
        /// </summary>
        /// <typeparam name="T">The instance type.</typeparam>
        /// <param name="obj">The instance.</param>
        /// <returns>A dictionary with property names as keys and <see cref="IPropertyProxy"/> objects as values.</returns>
        public static Dictionary<string, IPropertyProxy> PropertyProxies<T>(this T obj) =>
            (obj?.GetType() ?? typeof(T)).PropertyProxies();

        /// <summary>
        /// Gets the property proxy given the property name.
        /// </summary>
        /// <param name="t">The associated type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The associated <see cref="IPropertyProxy"/></returns>
        public static IPropertyProxy PropertyProxy(this Type t, string propertyName)
        {
            var proxies = t.PropertyProxies();
            return proxies.ContainsKey(propertyName) ? proxies[propertyName] : null;
        }

        /// <summary>
        /// Gets the property proxy given the property name.
        /// </summary>
        /// <typeparam name="T">The type of instance to extract proxies from.</typeparam>
        /// <param name="obj">The instance to extract proxies from.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The associated <see cref="IPropertyProxy"/></returns>
        public static IPropertyProxy PropertyProxy<T>(this T obj, string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));
            var proxies = (obj?.GetType() ?? typeof(T)).PropertyProxies();

            return proxies?.ContainsKey(propertyName) == true ? proxies[propertyName] : null;
        }

        /// <summary>
        /// Gets the property proxy given the property name as an expression.
        /// </summary>
        /// <typeparam name="T">The instance type.</typeparam>
        /// <typeparam name="V">The property value type.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns>The associated <see cref="IPropertyProxy"/></returns>
        public static IPropertyProxy PropertyProxy<T, V>(this T obj, Expression<Func<T, V>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            var proxies = (obj?.GetType() ?? typeof(T)).PropertyProxies();
            var propertyName = propertyExpression.PropertyName();
            return proxies?.ContainsKey(propertyName) == true ? proxies[propertyName] : null;
        }

        /// <summary>
        /// Reads the property value.
        /// </summary>
        /// <typeparam name="T">The type to get property proxies from.</typeparam>
        /// <typeparam name="V">The type of the property.</typeparam>
        /// <param name="obj">The instance.</param>
        /// <param name="propertyExpression">The property expression.</param>
        /// <returns>
        /// The value obtained from the associated <see cref="IPropertyProxy" />
        /// </returns>
        /// <exception cref="ArgumentNullException">obj.</exception>
        public static V ReadProperty<T, V>(this T obj, Expression<Func<T, V>> propertyExpression)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var proxy = obj.PropertyProxy(propertyExpression);
            return (V)(proxy?.GetValue(obj));
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

            var proxy = obj.PropertyProxy(propertyName);
            return proxy?.GetValue(obj);
        }

        /// <summary>
        /// Writes the property value.
        /// </summary>
        /// <typeparam name="T">The type to get property proxies from.</typeparam>
        /// <typeparam name="TV">The type of the property.</typeparam>
        /// <param name="obj">The instance.</param>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="value">The value.</param>
        public static void WriteProperty<T, TV>(this T obj, Expression<Func<T, TV>> propertyExpression, TV value)
        {
            var proxy = obj.PropertyProxy(propertyExpression);
            proxy?.SetValue(obj, value);
        }

        /// <summary>
        /// Writes the property value using the property proxy.
        /// </summary>
        /// <typeparam name="T">The type to get property proxies from.</typeparam>
        /// <param name="obj">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        public static void WriteProperty<T>(this T obj, string propertyName, object? value)
        {
            var proxy = obj.PropertyProxy(propertyName);
            proxy?.SetValue(obj, value);
        }

        private static string PropertyName<T, TV>(this Expression<Func<T, TV>> propertyExpression)
        {
            var memberExpression = !(propertyExpression.Body is MemberExpression)
                ? (propertyExpression.Body as UnaryExpression).Operand as MemberExpression
                : propertyExpression.Body as MemberExpression;

            return memberExpression.Member.Name;
        }
    }
}