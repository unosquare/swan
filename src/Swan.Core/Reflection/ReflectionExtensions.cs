using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides various extension methods for Reflection and Types.
    /// </summary>
    public static class ReflectionExtensions
    {
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
        public static MethodInfo? GetMethod(
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
        public static bool TrySetArrayBasicType(this Type type, object? value, Array? target, int index)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (target is null)
                return false;

            var typeInfo = type.TypeInfo();

            try
            {
                if (value == null)
                {
                    target.SetValue(null, index);
                    return true;
                }

                if (TypeManager.TryChangeType(value, typeInfo, out var propertyValue))
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
    }
}