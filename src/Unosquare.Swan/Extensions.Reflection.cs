namespace Unosquare.Swan
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;

    partial class Extensions
    {
        #region Support Classes and Declarations

        private static readonly ConcurrentDictionary<GenericMethodCacheKey, MethodInfo> GenericMethodCache =
            new ConcurrentDictionary<GenericMethodCacheKey, MethodInfo>();

        private sealed class GenericMethodCacheKey
        {
            private readonly Type _sourceType;

            private readonly string _methodName;

            private readonly Type[] _genericTypes;

            private readonly Type[] _parameterTypes;

            private readonly int _hashCode;

            public GenericMethodCacheKey(Type sourceType, string methodName, Type[] genericTypes, Type[] parameterTypes)
            {
                _sourceType = sourceType;
                _methodName = methodName;
                _genericTypes = genericTypes;
                _parameterTypes = parameterTypes;
                _hashCode = GenerateHashCode();
            }

            public override bool Equals(object obj)
            {
                var cacheKey = obj as GenericMethodCacheKey;
                if (cacheKey == null)
                    return false;

                if (_sourceType != cacheKey._sourceType)
                    return false;

                if (!string.Equals(_methodName, cacheKey._methodName, StringComparison.Ordinal))
                    return false;

                if (_genericTypes.Length != cacheKey._genericTypes.Length)
                    return false;

                if (_parameterTypes.Length != cacheKey._parameterTypes.Length)
                    return false;

                if (_genericTypes.Where((t, i) => t != cacheKey._genericTypes[i]).Any())
                {
                    return false;
                }

                return !_parameterTypes.Where((t, i) => t != cacheKey._parameterTypes[i]).Any();
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            private int GenerateHashCode()
            {
                unchecked
                {
                    var result = _sourceType.GetHashCode();

                    result = (result * 397) ^ _methodName.GetHashCode();

                    result = _genericTypes.Aggregate(result, (current, t) => (current * 397) ^ t.GetHashCode());

                    for (var i = 0; i < _parameterTypes.Length; ++i)
                    {
                        result = (result * 397) ^ i.GetHashCode();
                    }

                    return result;
                }
            }
        }

        #endregion

        #region Assembly Extensions

        /// <summary>
        /// Gets all types within an assembly in a safe manner.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static Type[] GetAllTypes(this Assembly assembly)
        {
            Type[] assemblyTypes;

            try
            {
                assemblyTypes = assembly.GetTypes();
            }
            catch (System.IO.FileNotFoundException)
            {
                assemblyTypes = new Type[] { };
            }
            catch (NotSupportedException)
            {
                assemblyTypes = new Type[] { };
            }
            catch (ReflectionTypeLoadException e)
            {
                assemblyTypes = e.Types.Where(t => t != null).ToArray();
            }

            return assemblyTypes;
        }

        #endregion

        #region Type Extensions

        /// <summary>
        /// The closest programmatic equivalent of default(T)
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static object GetDefault(this Type type)
        {
            return type.IsValueType() ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// Determines whether this type is compatible with ICollection.
        /// </summary>
        /// <param name="sourceType">The type.</param>
        public static bool IsCollection(this Type sourceType)
        {
            return sourceType != typeof(string) &&
                             typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(sourceType);
        }

        /// <summary>
        /// Gets a generic method from a type given the method name, binding flags, generic types and parameter types
        /// </summary>
        /// <param name="sourceType">Source type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <param name="methodName">Name of the method</param>
        /// <param name="genericTypes">Generic types to use to make the method generic</param>
        /// <param name="parameterTypes">Method parameters</param>
        /// <returns>MethodInfo or null if no matches found</returns>
        /// <exception cref="System.Reflection.AmbiguousMatchException"/>
        /// <exception cref="System.ArgumentException"/>
        public static MethodInfo GetGenericMethod(this Type sourceType, BindingFlags bindingFlags, string methodName, Type[] genericTypes, Type[] parameterTypes)
        {
            MethodInfo method;
            var cacheKey = new GenericMethodCacheKey(sourceType, methodName, genericTypes, parameterTypes);

            // Shouldn't need any additional locking
            // we don't care if we do the method info generation
            // more than once before it gets cached.
            if (!GenericMethodCache.TryGetValue(cacheKey, out method))
            {
                method = GetMethod(sourceType, bindingFlags, methodName, genericTypes, parameterTypes);
                GenericMethodCache[cacheKey] = method;
            }

            return method;
        }

        /// <summary>
        /// Gets a method from a type given the method name, binding flags, generic types and parameter types
        /// </summary>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="genericTypes">The generic types.</param>
        /// <param name="parameterTypes">The parameter types.</param>
        /// <returns></returns>
        /// <exception cref="System.Reflection.AmbiguousMatchException"></exception>
        private static MethodInfo GetMethod(Type sourceType, BindingFlags bindingFlags, string methodName, Type[] genericTypes, Type[] parameterTypes)
        {
            var methods =
                sourceType.GetTypeInfo().GetMethods(bindingFlags).Where(
                    mi => string.Equals(methodName, mi.Name, StringComparison.Ordinal)).Where(
                        mi => mi.ContainsGenericParameters).Where(mi => mi.GetGenericArguments().Length == genericTypes.Length).
                    Where(mi => mi.GetParameters().Length == parameterTypes.Length).Select(
                        mi => mi.MakeGenericMethod(genericTypes)).Where(
                            mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameterTypes)).ToList();

            if (methods.Count > 1)
            {
                throw new AmbiguousMatchException();
            }

            return methods.FirstOrDefault();
        }

        /// <summary>
        /// Determines whether this instance is class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is class; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }

        /// <summary>
        /// Determines whether this instance is abstract.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is abstract; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAbstract(this Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        /// <summary>
        /// Determines whether this instance is interface.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is interface; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        /// <summary>
        /// Determines whether this instance is primitive.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is primitive; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        /// <summary>
        /// Determines whether [is value type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is value type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValueType(this Type type) => type.GetTypeInfo().IsValueType;

        /// <summary>
        /// Determines whether [is generic type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is generic type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsGenericType(this Type type) => type.GetTypeInfo().IsGenericType;

        /// <summary>
        /// Determines whether [is generic parameter].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is generic parameter] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsGenericParameter(this Type type) => type.IsGenericParameter;

        /// <summary>
        /// Determines whether the specified attribute type is defined.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns>
        ///   <c>true</c> if the specified attribute type is defined; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDefined(this Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().IsDefined(attributeType, inherit);
        }

        /// <summary>
        /// Gets the custom attributes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns></returns>
        public static Attribute[] GetCustomAttributes(this Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).Cast<Attribute>().ToArray();
        }

        /// <summary>
        /// Determines whether [is generic type definition].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is generic type definition] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsGenericTypeDefinition(this Type type) => type.GetTypeInfo().IsGenericTypeDefinition;

        /// <summary>
        /// Bases the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Type BaseType(this Type type) => type.GetTypeInfo().BaseType;

        /// <summary>
        /// Assemblies the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Assembly Assembly(this Type type) => type.GetTypeInfo().Assembly;

        /// <summary>
        /// Determines whether [is i enumerable request].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is i enumerable request] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIEnumerable(this Type type)
        {
            if (!type.IsGenericType())
                return false;

            return type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        #endregion
    }
}
