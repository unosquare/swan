using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// A thread-safe cache of attributes belonging to a given key (MemberInfo or Type).
    /// 
    /// The Retrieve method is the most useful one in this class as it
    /// calls the retrieval process if the type is not contained
    /// in the cache.
    /// </summary>
    public class AttributeCache
    {
        private readonly Lazy<ConcurrentDictionary<Tuple<object, Type>, IEnumerable<object>>> _data =
            new Lazy<ConcurrentDictionary<Tuple<object, Type>, IEnumerable<object>>>(() =>
                new ConcurrentDictionary<Tuple<object, Type>, IEnumerable<object>>(), true);

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeCache"/> class.
        /// </summary>
        /// <param name="propertyCache">The property cache object.</param>
        public AttributeCache(PropertyTypeCache? propertyCache = null)
        {
            PropertyTypeCache = propertyCache ?? PropertyTypeCache.DefaultCache.Value;
        }

        /// <summary>
        /// Gets the default cache.
        /// </summary>
        /// <value>
        /// The default cache.
        /// </value>
        public static Lazy<AttributeCache> DefaultCache { get; } = new Lazy<AttributeCache>(() => new AttributeCache());

        /// <summary>
        /// A PropertyTypeCache object for caching properties and their attributes.
        /// </summary>
        public PropertyTypeCache PropertyTypeCache { get; }

        /// <summary>
        /// Determines whether [contains] [the specified member].
        /// </summary>
        /// <typeparam name="T">The type of the attribute to be retrieved.</typeparam>
        /// <param name="member">The member.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified member]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains<T>(MemberInfo member) => _data.Value.ContainsKey(new Tuple<object, Type>(member, typeof(T)));
        
        /// <summary>
        /// Gets specific attributes from a member constrained to an attribute.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to be retrieved.</typeparam>
        /// <param name="member">The member.</param>
        /// <param name="inherit"><c>true</c> to inspect the ancestors of element; otherwise, <c>false</c>.</param>
        /// <returns>An array of the attributes stored for the specified type.</returns>
        public IEnumerable<object> Retrieve<T>(MemberInfo member, bool inherit = false)
            where T : Attribute
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            return Retrieve(new Tuple<object, Type>(member, typeof(T)), t => member.GetCustomAttributes<T>(inherit));
        }

        /// <summary>
        /// Gets all attributes of a specific type from a member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="type">The attribute type.</param>
        /// <param name="inherit"><c>true</c> to inspect the ancestors of element; otherwise, <c>false</c>.</param>
        /// <returns>An array of the attributes stored for the specified type.</returns>
        public IEnumerable<object> Retrieve(MemberInfo member, Type type, bool inherit = false)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return Retrieve(
                new Tuple<object, Type>(member, type), 
                t => member.GetCustomAttributes(type, inherit));
        }

        /// <summary>
        /// Gets one attribute of a specific type from a member.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="member">The member.</param>
        /// <param name="inherit"><c>true</c> to inspect the ancestors of element; otherwise, <c>false</c>.</param>
        /// <returns>An attribute stored for the specified type.</returns>
        public T RetrieveOne<T>(MemberInfo member, bool inherit = false)
            where T : Attribute
        {
            if (member == null)
                return default;

            var attr = Retrieve(
                new Tuple<object, Type>(member, typeof(T)), 
                t => member.GetCustomAttributes(typeof(T), inherit));

            return ConvertToAttribute<T>(attr);
        }

        /// <summary>
        /// Gets one attribute of a specific type from a generic type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <typeparam name="T">The type to retrieve the attribute.</typeparam>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns>An attribute stored for the specified type.</returns>
        public TAttribute RetrieveOne<TAttribute, T>(bool inherit = false)
            where TAttribute : Attribute
        {
            var attr = Retrieve(
                new Tuple<object, Type>(typeof(T), typeof(TAttribute)), 
                t => typeof(T).GetCustomAttributes(typeof(TAttribute), inherit));
            
            return ConvertToAttribute<TAttribute>(attr);
        }

        /// <summary>
        /// Gets all properties an their attributes of a given type constrained to only attributes.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to retrieve.</typeparam>
        /// <param name="type">The type of the object.</param>
        /// <param name="inherit"><c>true</c> to inspect the ancestors of element; otherwise, <c>false</c>.</param>
        /// <returns>A dictionary of the properties and their attributes stored for the specified type.</returns>
        public Dictionary<PropertyInfo, IEnumerable<object>> Retrieve<T>(Type type, bool inherit = false)
            where T : Attribute =>
            PropertyTypeCache.RetrieveAllProperties(type, true)
                .ToDictionary(x => x, x => Retrieve<T>(x, inherit));

        /// <summary>
        /// Gets all properties and their attributes of a given type.
        /// </summary>
        /// <typeparam name="T">The object type used to extract the properties from.</typeparam>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="inherit"><c>true</c> to inspect the ancestors of element; otherwise, <c>false</c>.</param>
        /// <returns>
        /// A dictionary of the properties and their attributes stored for the specified type.
        /// </returns>
        public Dictionary<PropertyInfo, IEnumerable<object>> RetrieveFromType<T, TAttribute>(bool inherit = false)
            => RetrieveFromType<T>(typeof(TAttribute), inherit);

        /// <summary>
        /// Gets all properties and their attributes of a given type.
        /// </summary>
        /// <typeparam name="T">The object type used to extract the properties from.</typeparam>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="inherit"><c>true</c> to inspect the ancestors of element; otherwise, <c>false</c>.</param>
        /// <returns>
        /// A dictionary of the properties and their attributes stored for the specified type.
        /// </returns>
        public Dictionary<PropertyInfo, IEnumerable<object>> RetrieveFromType<T>(Type attributeType, bool inherit = false)
        {
            if (attributeType == null)
                throw new ArgumentNullException(nameof(attributeType));

            return PropertyTypeCache.RetrieveAllProperties<T>(true)
                .ToDictionary(x => x, x => Retrieve(x, attributeType, inherit));
        }

        private static T ConvertToAttribute<T>(IEnumerable<object> attr)
            where T : Attribute
        {
            if (attr?.Any() != true)
                return default;

            return attr.Count() == 1
                ? (T) Convert.ChangeType(attr.First(), typeof(T))
                : throw new AmbiguousMatchException("Multiple custom attributes of the same type found.");
        }

        private IEnumerable<object> Retrieve(Tuple<object, Type> key, Func<Tuple<object, Type>, IEnumerable<object>> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            return _data.Value.GetOrAdd(key, k => factory.Invoke(k).Where(item => item != null));
        }
    }
}
