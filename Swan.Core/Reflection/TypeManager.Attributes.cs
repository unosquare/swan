using System;
using System.Linq;

namespace Swan.Reflection
{
    public static partial class TypeManager
    {
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
    }
}
