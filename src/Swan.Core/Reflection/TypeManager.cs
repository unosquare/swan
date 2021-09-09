using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides efficient access to a cached repository of <see cref="TypeProxy"/>
    /// and various type utilities.
    /// </summary>
    public static partial class TypeManager
    {
        private static readonly ConcurrentDictionary<Type, ITypeProxy> TypeCache = new();

        /// <summary>
        /// Provides cached and extended type information for
        /// easy and efficient access to common reflection scenarios.
        /// </summary>
        /// <param name="t">The type to provide extended info for.</param>
        /// <returns>Returns an <see cref="TypeProxy"/> for the given type.</returns>
        public static ITypeProxy TypeInfo(this Type t)
        {
            if (t is null)
                throw new ArgumentNullException(nameof(t));

            if (TypeCache.TryGetValue(t, out var typeInfo))
                return typeInfo;

            typeInfo = new TypeProxy(t);
            TypeCache.TryAdd(t, typeInfo);

            return typeInfo;
        }

        /// <summary>
        /// Gets all types within an assembly in a safe manner.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        /// Array of Type objects representing the types specified by an assembly.
        /// </returns>
        /// <exception cref="ArgumentNullException">assembly.</exception>
        public static IReadOnlyList<Type> GetAllTypes(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t is not null).ToArray()!;
            }
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
            ? type.TypeInfo().DefaultValue
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
        /// Creates an instance of a <see cref="Array"/> from the given
        /// element type and count.
        /// </summary>
        /// <param name="elementType">The type of array elements.</param>
        /// <param name="elementCount">The array length.</param>
        /// <returns></returns>
        public static Array CreateArray(Type elementType, int elementCount)
        {
            return Array.CreateInstance(elementType, elementCount);
        }

        /// <summary>
        /// Creates a <see cref="List{T}"/> from type arguments.
        /// </summary>
        /// <param name="elementType">The generic type argument.</param>
        /// <returns>An instance of a <see cref="List{T}"/></returns>
        public static IEnumerable CreateGenericList(Type elementType)
        {
            var resultType = typeof(List<>).MakeGenericType(elementType);
            return (resultType.TypeInfo().CreateInstance() as IEnumerable)!;
        }

        /// <summary>
        /// Creates an <see cref="Dictionary{TKey, TValue}"/> from type arguments.
        /// </summary>
        /// <param name="keyType">The type for keys.</param>
        /// <param name="valueType">The type for values.</param>
        /// <returns>An instance of a <see cref="Dictionary{TKey, TValue}"/></returns>
        public static IEnumerable CreateGenericDictionary(Type keyType, Type valueType)
        {
            var resultType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            return (resultType.TypeInfo().CreateInstance() as IEnumerable)!;
        }

        /// <summary>
        /// Tries to convert a type of the source value to a type of the target value.
        /// </summary>
        /// <param name="sourceValue">The value to be converted.</param>
        /// <param name="targetType">The target type to turn the source value into.</param>
        /// <param name="targetValue">The resulting value.</param>
        /// <returns>Returns true inf the conversion succeeds.</returns>
        public static bool TryChangeType(object? sourceValue, ITypeProxy targetType, out object? targetValue)
        {
            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            // start with the default value of the target type
            // and if the input value is null simply return the default
            // value of the target type.
            targetValue = targetType.DefaultValue;
            if (sourceValue is null || sourceValue == targetType.DefaultValue)
                return true;

            // Normalize target removing nullable semantics
            if (targetType.IsNullableValueType)
            {
                targetType = targetType.UnderlyingType;
                targetValue = targetType.DefaultValue;
            }

            // Normalize source removing nullable semantics
            var sourceType = sourceValue.GetType().TypeInfo();
            if (sourceType.IsNullableValueType)
            {
                sourceType = sourceType.UnderlyingType;
                sourceValue = Convert.ChangeType(sourceValue,
                    sourceType.UnderlyingType.ProxiedType,
                    CultureInfo.InvariantCulture);
            }

            // Case 0: Direct assignment if types are the same or compatible.
            if (targetType.ProxiedType == sourceType.ProxiedType ||
                targetType.ProxiedType.IsAssignableFrom(sourceType.ProxiedType))
            {
                targetValue = sourceValue;
                return true;
            }

            // Case 1: Target type is a string and conversion is performed invariant of culture.
            if (targetType.ProxiedType == typeof(string))
            {
                targetValue = sourceType.ToStringInvariant(sourceValue);
                return true;
            }

            // Case 2: Target type is an enum and using convert cannot produce enumeration values.
            if (targetType.IsEnum)
            {
                // Convert source enum value to an integral type.
                if (sourceType.IsEnum)
                {
                    sourceType = sourceType.UnderlyingType;
                    sourceValue = Convert.ChangeType(sourceValue, sourceType.ProxiedType, CultureInfo.InvariantCulture);
                }

                // Parse the source value converted to a string
                var sourceEnumString = sourceValue is string stringValue
                    ? stringValue
                    : sourceType.ToStringInvariant(sourceValue);

                if (Enum.TryParse(targetType.ProxiedType, sourceEnumString, true, out var enumValue))
                {
                    targetValue = enumValue;
                    return true;
                }
                else
                {
                    targetValue = targetType.DefaultValue;
                    return false;
                }
            }

            // Case 3: Change type works by parsing the value as a string
            if (targetType.CanParseNatively)
            {
                var sourceString = sourceType.ToStringInvariant(sourceValue);
                if (targetType.TryParse(sourceString, out targetValue))
                    return true;
            }

            // Case 4: Change type works directly
            if (targetType.IsValueType)
            {
                try
                {
                    if (!sourceType.IsValueType)
                    {
                        var sourceString = sourceType.ToStringInvariant(sourceValue);
                        targetValue = Convert.ChangeType(sourceString, targetType.ProxiedType, CultureInfo.InvariantCulture);
                        return true;
                    }

                    targetValue = Convert.ChangeType(sourceValue, targetType.ProxiedType, CultureInfo.InvariantCulture);
                    return true;
                }
                catch
                {
                    // placeholder
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to convert a type of the source value to a type of the target value.
        /// </summary>
        /// <param name="sourceValue">The value to be converted.</param>
        /// <param name="targetType">The target type to turn the source value into.</param>
        /// <param name="targetValue">The resulting value.</param>
        /// <returns>Returns true inf the conversion succeeds.</returns>
        public static bool TryChangeType(object? sourceValue, Type targetType, out object? targetValue) =>
            TryChangeType(sourceValue, targetType.TypeInfo(), out targetValue);

        /// <summary>
        /// Tries to convert a type of the source value to a type of the target value.
        /// </summary>
        /// <typeparam name="T">The target type to turn the source value into.</typeparam>
        /// <param name="sourceValue">The value to be converted.</param>
        /// <param name="targetValue">The resulting value.</param>
        /// <returns>Returns true inf the conversion succeeds.</returns>
        public static bool TryChangeType<T>(object? sourceValue, out T? targetValue)
        {
            var result = TryChangeType(sourceValue, typeof(T).TypeInfo(), out var target);
            targetValue = (T)target;
            return result;
        }
    }
}
