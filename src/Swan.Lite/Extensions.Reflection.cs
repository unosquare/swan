using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swan
{
    /// <summary>
    /// Provides various extension methods for Reflection and Types.
    /// </summary>
    public static class ReflectionExtensions
    {
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

        #region Type Extensions

        /// <summary>
        /// The closest programmatic equivalent of default(T).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// Default value of this type.
        /// </returns>
        /// <exception cref="ArgumentNullException">type.</exception>
        public static object? GetDefault(this Type type)
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

            if (type != typeof(bool))
                return TryParseBasicType(type, value.ToStringInvariant(), out result);

            result = value.ToBoolean();
            return true;
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
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="value">The value.</param>
        /// <param name="target">The object.</param>
        /// <returns>
        ///   <c>true</c> if parsing was successful; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">propertyInfo.</exception>
        public static bool TrySetBasicType(this PropertyInfo propertyInfo, object value, object target)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            try
            {
                if (propertyInfo.PropertyType.TryParseBasicType(value, out var propertyValue))
                {
                    propertyInfo.SetValue(target, propertyValue);
                    return true;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
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
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
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
        /// <exception cref="ArgumentNullException">propertyInfo.</exception>
        public static bool TrySetArray(this PropertyInfo propertyInfo, IEnumerable<object>? value, object obj)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            var elementType = propertyInfo.PropertyType.GetElementType();

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
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // ignored
            }

            return false;
        }

        /// <summary>
        /// Convert a object to a boolean.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the string represents a valid truly value, otherwise <c>false</c>.
        /// </returns>
        public static bool ToBoolean(this object value) => value.ToStringInvariant().ToBoolean();
    }
}
