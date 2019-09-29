using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Swan.Configuration;
using Swan.Reflection;

namespace Swan
{
    /// <summary>
    /// Provides various extension methods for Reflection and Types.
    /// </summary>
    public static class ReflectionExtensions
    {
        private static readonly Lazy<ConcurrentDictionary<Tuple<bool, PropertyInfo>, Func<object, object>>> CacheGetMethods =
            new Lazy<ConcurrentDictionary<Tuple<bool, PropertyInfo>, Func<object, object>>>(() => new ConcurrentDictionary<Tuple<bool, PropertyInfo>, Func<object, object>>(), true);

        private static readonly Lazy<ConcurrentDictionary<Tuple<bool, PropertyInfo>, Action<object, object[]>>> CacheSetMethods =
            new Lazy<ConcurrentDictionary<Tuple<bool, PropertyInfo>, Action<object, object[]>>>(() => new ConcurrentDictionary<Tuple<bool, PropertyInfo>, Action<object, object[]>>(), true);

        #region Assembly Extensions

        /// <summary>
        /// Gets all types within an assembly in a safe manner.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        /// Array of Type objects representing the types specified by an assembly.
        /// </returns>
        /// <exception cref="ArgumentNullException">assembly.</exception>
        public static IEnumerable<Type> GetAllTypes(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        #endregion

        #region Type Extensions

        /// <summary>
        /// The closest programmatic equivalent of default(T).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// Default value of this type.
        /// </returns>
        /// <exception cref="ArgumentNullException">type.</exception>
        public static object GetDefault(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.IsValueType ? Activator.CreateInstance(type) : default;
        }

        /// <summary>
        /// Determines whether this type is compatible with ICollection.
        /// </summary>
        /// <param name="sourceType">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified source type is collection; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">sourceType.</exception>
        public static bool IsCollection(this Type sourceType)
        {
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));

            return sourceType != typeof(string) &&
                   typeof(IEnumerable).IsAssignableFrom(sourceType);
        }

        /// <summary>
        /// Gets a method from a type given the method name, binding flags, generic types and parameter types.
        /// </summary>
        /// <param name="type">Type of the source.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="genericTypes">The generic types.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns>
        /// An object that represents the method with the specified name.
        /// </returns>
        /// <exception cref="System.Reflection.AmbiguousMatchException">
        /// The exception that is thrown when binding to a member results in more than one member matching the 
        /// binding criteria. This class cannot be inherited.
        /// </exception>
        public static MethodInfo GetMethod(
            this Type type,
            BindingFlags bindingFlags,
            string methodName,
            Type[] genericTypes,
            Type[] parameterTypes)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (methodName == null)
                throw new ArgumentNullException(nameof(methodName));

            if (genericTypes == null)
                throw new ArgumentNullException(nameof(genericTypes));

            if (parameterTypes == null)
                throw new ArgumentNullException(nameof(parameterTypes));

            var methods = type
                .GetMethods(bindingFlags)
                .Where(mi => string.Equals(methodName, mi.Name, StringComparison.Ordinal))
                .Where(mi => mi.ContainsGenericParameters)
                .Where(mi => mi.GetGenericArguments().Length == genericTypes.Length)
                .Where(mi => mi.GetParameters().Length == parameterTypes.Length)
                .Select(mi => mi.MakeGenericMethod(genericTypes))
                .Where(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameterTypes))
                .ToList();

            return methods.Count > 1 ? throw new AmbiguousMatchException() : methods.FirstOrDefault();
        }

        /// <summary>
        /// Determines whether [is i enumerable request].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is i enumerable request] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">type.</exception>
        public static bool IsIEnumerable(this Type type)
            => type == null
                ? throw new ArgumentNullException(nameof(type))
                : type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);

        #endregion

        /// <summary>
        /// Tries to parse using the basic types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        ///   <c>true</c> if parsing was successful; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">type</exception>
        public static bool TryParseBasicType(this Type type, object value, out object? result)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type == typeof(bool))
            {
                result = value.ToBoolean();
                return true;
            }

            return TryParseBasicType(type, value.ToStringInvariant(), out result);
        }

        /// <summary>
        /// Tries to parse using the basic types.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="result">The result.</param>
        /// <returns>
        ///   <c>true</c> if parsing was successful; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">type</exception>
        public static bool TryParseBasicType(this Type type, string value, out object? result)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            result = null;

            return Definitions.BasicTypesInfo.Value.ContainsKey(type) && Definitions.BasicTypesInfo.Value[type].TryParse(value, out result);
        }

        /// <summary>
        /// Tries the type of the set basic value to a property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <param name="target">The object.</param>
        /// <returns>
        ///  <c>true</c> if parsing was successful; otherwise, <c>false</c>.
        /// </returns>
        public static bool TrySetBasicType(this PropertyInfo property, object value, object target)
        {
            try
            {
                if (property.PropertyType.TryParseBasicType(value, out var propertyValue))
                {
                    property.SetValue(target, propertyValue);
                    return true;
                }
            }
            catch
            {
                // swallow
            }

            return false;
        }

        /// <summary>
        /// Tries the type of the set to an array a basic type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="target">The array.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        ///   <c>true</c> if parsing was successful; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">type</exception>
        public static bool TrySetArrayBasicType(this Type type, object value, Array target, int index)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (target == null)
                return false;

            try
            {
                if (value == null)
                {
                    target.SetValue(null, index);
                    return true;
                }

                if (type.TryParseBasicType(value, out var propertyValue))
                {
                    target.SetValue(propertyValue, index);
                    return true;
                }

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    target.SetValue(null, index);
                    return true;
                }
            }
            catch
            {
                // swallow
            }

            return false;
        }

        /// <summary>
        /// Tries to set a property array with another array.
        /// </summary>
        /// <param name="propertyInfo">The property.</param>
        /// <param name="value">The value.</param>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <c>true</c> if parsing was successful; otherwise, <c>false</c>.
        /// </returns>
        public static bool TrySetArray(this PropertyInfo propertyInfo, IEnumerable<object> value, object obj)
        {
            var elementType = propertyInfo?.PropertyType.GetElementType();

            if (elementType == null || value == null)
                return false;

            var targetArray = Array.CreateInstance(elementType, value.Count());

            var i = 0;

            foreach (var sourceElement in value)
            {
                var result = elementType.TrySetArrayBasicType(sourceElement, targetArray, i++);

                if (!result) return false;
            }

            propertyInfo.SetValue(obj, targetArray);

            return true;
        }

        /// <summary>
        /// Gets property actual value or <c>PropertyDisplayAttribute.DefaultValue</c> if presented.
        ///
        /// If the <c>PropertyDisplayAttribute.Format</c> value is presented, the property value
        /// will be formatted accordingly.
        ///
        /// If the object contains a null value, a empty string will be returned.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="target">The object.</param>
        /// <returns>The property value or null.</returns>
        public static string? ToFormattedString(this PropertyInfo propertyInfo, object target)
        {
            try
            {
                var value = propertyInfo.GetValue(target);
                var attr = AttributeCache.DefaultCache.Value.RetrieveOne<PropertyDisplayAttribute>(propertyInfo);

                if (attr == null) return value?.ToString() ?? string.Empty;

                var valueToFormat = value ?? attr.DefaultValue;

                return string.IsNullOrEmpty(attr.Format)
                    ? (valueToFormat?.ToString() ?? string.Empty)
                    : ConvertObjectAndFormat(propertyInfo.PropertyType, valueToFormat, attr.Format);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a MethodInfo from a Property Get method.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="nonPublic">if set to <c>true</c> [non public].</param>
        /// <returns>
        /// The cached MethodInfo.
        /// </returns>
        public static Func<object, object> GetCacheGetMethod(this PropertyInfo propertyInfo, bool nonPublic = false)
        {
            var key = Tuple.Create(!nonPublic, propertyInfo);

            // TODO: Fix public logic
            return !nonPublic && !CacheGetMethods.Value.ContainsKey(key) && !propertyInfo.GetGetMethod(true).IsPublic
                ? null
                : CacheGetMethods.Value
                    .GetOrAdd(key,
                        x => y => x.Item2.GetGetMethod(nonPublic).Invoke(y, null));
            //y => x => y.Item2.CreatePropertyProxy().GetValue(x));
        }

        /// <summary>
        /// Gets a MethodInfo from a Property Set method.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="nonPublic">if set to <c>true</c> [non public].</param>
        /// <returns>
        /// The cached MethodInfo.
        /// </returns>
        public static Action<object, object[]> GetCacheSetMethod(this PropertyInfo propertyInfo, bool nonPublic = false)
        {
            var key = Tuple.Create(!nonPublic, propertyInfo);

            return !nonPublic && !CacheSetMethods.Value.ContainsKey(key) && !propertyInfo.GetSetMethod(true).IsPublic
                ? null
                : CacheSetMethods.Value
                    .GetOrAdd(key,
                        x => (obj, args) => x.Item2.GetSetMethod(nonPublic).Invoke(obj, args));
            //y => (obj, args) => y.Item2.CreatePropertyProxy().SetValue(obj, args));
        }

        /// <summary>
        /// Convert a string to a boolean.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>
        ///   <c>true</c> if the string represents a valid truly value, otherwise <c>false</c>.
        /// </returns>
        public static bool ToBoolean(this string str)
        {
            try
            {
                return Convert.ToBoolean(str);
            }
            catch (FormatException)
            {
                // ignored
            }

            try
            {
                return Convert.ToBoolean(Convert.ToInt32(str));
            }
            catch
            {
                // ignored
            }

            return false;
        }

        /// <summary>
        /// Creates a property proxy that stores getter and setter delegates.
        /// </summary>
        /// <param name="this">The property information.</param>
        /// <returns>
        /// The property proxy.
        /// </returns>
        /// <exception cref="ArgumentNullException">this.</exception>
        public static IPropertyProxy CreatePropertyProxy(this PropertyInfo @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            var genericType = typeof(PropertyProxy<,>)
                .MakeGenericType(@this.DeclaringType, @this.PropertyType);

            return Activator.CreateInstance(genericType, @this) as IPropertyProxy;
        }

        /// <summary>
        /// Convert a object to a boolean.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the string represents a valid truly value, otherwise <c>false</c>.
        /// </returns>
        public static bool ToBoolean(this object value) => value.ToStringInvariant().ToBoolean();

        private static string ConvertObjectAndFormat(Type propertyType, object value, string format)
        {
            if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                return Convert.ToDateTime(value).ToString(format);
            if (propertyType == typeof(int) || propertyType == typeof(int?))
                return Convert.ToInt32(value).ToString(format);
            if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
                return Convert.ToDecimal(value).ToString(format);
            if (propertyType == typeof(double) || propertyType == typeof(double?))
                return Convert.ToDouble(value).ToString(format);
            if (propertyType == typeof(byte) || propertyType == typeof(byte?))
                return Convert.ToByte(value).ToString(format);

            return value?.ToString() ?? string.Empty;
        }
    }
}
