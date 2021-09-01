using System;
using System.Collections.Generic;
using System.Linq;

namespace Swan.Reflection
{
    public static partial class TypeManager
    {
        internal static IEnumerable<object> FilterAttributes(this IEnumerable<object> attributes, Type filterType)
        {
            if (filterType is null)
                throw new ArgumentNullException(nameof(filterType));

            if (attributes is null || !attributes.Any())
                return Array.Empty<object>();

            return attributes.Where(c => c != null && c.GetType().IsAssignableTo(filterType));
        }

        internal static object? FirstAttribute(this IEnumerable<object> attributes, Type filterType) =>
            FilterAttributes(attributes, filterType).FirstOrDefault();

        internal static T? FirstAttribute<T>(this IEnumerable<object> attributes)
            where T : Attribute =>
            FirstAttribute(attributes, typeof(T)) as T;

        #region Property Attributes

        /// <summary>
        /// Searches for the first attribute of the given type.
        /// </summary>
        /// <param name="member">The declaration to be searched.</param>
        /// <typeparam name="T">The attribute type to search for.</typeparam>
        /// <returns>Returns a null if an attribute of the given type is not found.</returns>
        public static T? Attribute<T>(this IPropertyProxy member) where T : Attribute =>
            member?.PropertyAttributes.FirstAttribute<T>();

        /// <summary>
        /// Searches for the first attribute of the given type.
        /// </summary>
        /// <param name="member">The declaration to be searched.</param>
        /// <param name="attributeType">The attribute type to search for.</param>
        /// <returns>Returns a null if an attribute of the given type is not found.</returns>
        public static object? Attribute(this IPropertyProxy member, Type attributeType) =>
            member?.PropertyAttributes.FirstAttribute(attributeType);

        /// <summary>
        /// Gets a value indicating whether an attribute of the given type has been applied.
        /// </summary>
        /// <param name="member">The declaration to be searched.</param>
        /// <typeparam name="T">The type of the attribute to search for.</typeparam>
        /// <returns>True if the attribute is found. False otherwise.</returns>
        public static bool HasAttribute<T>(this IPropertyProxy member) where T : Attribute =>
            member?.Attribute<T>() is not null;

        /// <summary>
        /// Gets a value indicating whether an attribute of the given type has been applied.
        /// </summary>
        /// <param name="member">The declaration to be searched.</param>
        /// <param name="attributeType">The type of the attribute to search for.</param>
        /// <returns>True if the attribute is found. False otherwise.</returns>
        public static bool HasAttribute(this IPropertyProxy member, Type attributeType) =>
            member?.Attribute(attributeType) is not null;

        #endregion

        #region Type Attributes

        /// <summary>
        /// Searches for the first attribute of the given type.
        /// </summary>
        /// <param name="member">The declaration to be searched.</param>
        /// <typeparam name="T">The attribute type to search for.</typeparam>
        /// <returns>Returns a null if an attribute of the given type is not found.</returns>
        public static T? Attribute<T>(this Type member) where T : Attribute =>
            member?.TypeInfo().TypeAttributes.FirstAttribute<T>();

        /// <summary>
        /// Searches for the first attribute of the given type.
        /// </summary>
        /// <param name="member">The declaration to be searched.</param>
        /// <param name="attributeType">The attribute type to search for.</param>
        /// <returns>Returns a null if an attribute of the given type is not found.</returns>
        public static object? Attribute(this Type member, Type attributeType) =>
            member?.TypeInfo().TypeAttributes.FirstAttribute(attributeType);

        /// <summary>
        /// Gets a value indicating whether an attribute of the given type has been applied.
        /// </summary>
        /// <param name="member">The declaration to be searched.</param>
        /// <typeparam name="T">The type of the attribute to search for.</typeparam>
        /// <returns>True if the attribute is found. False otherwise.</returns>
        public static bool HasAttribute<T>(this Type member) where T : Attribute =>
            member?.Attribute<T>() is not null;

        /// <summary>
        /// Gets a value indicating whether an attribute of the given type has been applied.
        /// </summary>
        /// <param name="member">The declaration to be searched.</param>
        /// <param name="attributeType">The type of the attribute to search for.</param>
        /// <returns>True if the attribute is found. False otherwise.</returns>
        public static bool HasAttribute(this Type member, Type attributeType) =>
            member?.Attribute(attributeType) is not null;

        #endregion
    }
}
