﻿namespace Unosquare.Swan.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Components;

    /// <summary>
    /// A thread-safe cache of attributes belonging to a given type
    /// The Retrieve method is the most useful one in this class as it
    /// calls the retrieval process if the type is not contained
    /// in the cache.
    /// </summary>
    public class AttributeCache : CacheRepository<MemberInfo, object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeCache"/> class.
        /// </summary>
        /// <param name="propertyCache">The property cache object</param>
        public AttributeCache(PropertyTypeCache propertyCache = null)
        {
            PropertyTypeCache = propertyCache ?? Runtime.PropertyTypeCache;
        }

        /// <summary>
        /// A PropertyTypeCache object for caching properties and their attributes
        /// </summary>
        public PropertyTypeCache PropertyTypeCache { get; }

        /// <summary>
        /// Gets specific attributes from a member constrained to an attribute
        /// </summary>
        /// <typeparam name="T">The type of the attribute to be retrieved</typeparam>
        /// <param name="member">The member</param>
        /// <param name="inherit">True to inspect the ancestors of element; otherwise, false.</param>
        /// <returns>An array of the attributes stored for the specified type</returns>
        public object[] Retrieve<T>(MemberInfo member, bool inherit = false)
            where T : Attribute
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            return Retrieve(member, () => member.GetCustomAttributes<T>(inherit));
        }

        /// <summary>
        /// Gets all attributes of a specific type from a member
        /// </summary>
        /// <param name="member">The member</param>
        /// <param name="type">The attribute type</param>
        /// <param name="inherit">True to inspect the ancestors of element; otherwise, false.</param>
        /// <returns>An array of the attributes stored for the specified type</returns>
        public object[] Retrieve(MemberInfo member, Type type, bool inherit = false)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return Retrieve(member, () => member.GetCustomAttributes(type, inherit));
        }

        /// <summary>
        /// Gets one attribute of a specific type from a member
        /// </summary>
        /// <typeparam name="T">The attribute type</typeparam>
        /// <param name="member">The member</param>
        /// <param name="inherit">True to inspect the ancestors of element; otherwise, false.</param>
        /// <returns>An attribute stored for the specified type</returns>
        public T RetrieveOne<T>(MemberInfo member, bool inherit = false)
            where T : Attribute
        {
            var attrib = Retrieve(member, () => member.GetCustomAttributes(typeof(T), inherit));

            if (attrib == null || attrib.Length == 0)
                return default;

            if (attrib.Length == 1)
                return (T) Convert.ChangeType(attrib[0], typeof(T));

            throw new AmbiguousMatchException("Multiple custom attributes of the same type found.");
        }

        /// <summary>
        /// Gets all properties an their attributes of a given type constrained to only attributes
        /// </summary>
        /// <typeparam name="T">The type of the attribute to retrieve</typeparam>
        /// <param name="type">The type of the object</param>
        /// <param name="inherit">True to inspect the ancestors of element; otherwise, false.</param>
        /// <returns>A dictionary of the properties and their attributes stored for the specified type</returns>
        public Dictionary<PropertyInfo, object[]> Retrieve<T>(Type type, bool inherit = false)
            where T : Attribute
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return PropertyTypeCache.Retrieve(type, PropertyTypeCache.GetAllPublicPropertiesFunc(type))
                .ToDictionary(x => x, x => Retrieve<T>(x, inherit));
        }

        /// <summary>
        /// Gets all properties and their attributes of a given type
        /// </summary>
        /// <typeparam name="T">The object type used to extract the properties from</typeparam>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="inherit">True to inspect the ancestors of element; otherwise, false.</param>
        /// <returns>
        /// A dictionary of the properties and their attributes stored for the specified type
        /// </returns>
        public Dictionary<PropertyInfo, object[]> RetrieveFromType<T>(Type attributeType, bool inherit = false)
        {
            if (attributeType == null)
                throw new ArgumentNullException(nameof(attributeType));

            return PropertyTypeCache.Retrieve(typeof(T), PropertyTypeCache.GetAllPublicPropertiesFunc(typeof(T)))
                .ToDictionary(x => x, x => Retrieve(x, attributeType, inherit));
        }
    }
}