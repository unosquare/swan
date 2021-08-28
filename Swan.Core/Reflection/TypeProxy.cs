using System;
using System.Collections.Generic;
using System.Linq;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides extended information about a type such as property proxies,
    /// fields, attributes and methods.
    /// </summary>
    public sealed class TypeProxy : TypeProxyBase
    {
        private readonly Lazy<object[]> DirectAttributesLazy;
        private readonly Lazy<object[]> AllAttributesLazy;
        private readonly Lazy<object[]> InheritedAttributesLazy;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeProxy"/> class.
        /// </summary>
        /// <param name="backingType">The t.</param>
        public TypeProxy(Type backingType)
            : base(backingType)
        {
            DirectAttributesLazy = new(() => backingType.GetCustomAttributes(false), true);
            AllAttributesLazy = new(() => backingType.GetCustomAttributes(true), true);
            InheritedAttributesLazy = new(() => AllAttributes.Where(c => !DirectAttributes.Contains(DirectAttributes)).ToArray(), true);
        }

        /// <summary>
        /// Provides a collection of all instances of attributes applied directly on this type
        /// and without inherited attributes.
        /// </summary>
        public IReadOnlyList<object> DirectAttributes => DirectAttributesLazy.Value;

        /// <summary>
        /// Provides a collection of all instances of attributes applied on parent types of this type
        /// and without directly applied attributes.
        /// </summary>
        public IReadOnlyList<object> InheritedAttributes => InheritedAttributesLazy.Value;

        /// <summary>
        /// Provides a collection of all instances of attributes applied on this type and its parent types.
        /// </summary>
        public IReadOnlyList<object> AllAttributes => AllAttributesLazy.Value;
    }
}