using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// PRovides extended type information.
    /// </summary>
    public interface ITypeProxy
    {
        /// <summary>
        /// Provides the type this proxy represents.
        /// </summary>
        Type ProxiedType { get; }

        /// <summary>
        /// Gets a value indicating whether the type is a nullable value type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is nullable value type; otherwise, <c>false</c>.
        /// </value>
        bool IsNullableValueType { get; }

        /// <summary>
        /// Gets a value indicating whether the type or underlying type is numeric.
        /// </summary>
        /// <value>
        ///  <c>true</c> if this instance is numeric; otherwise, <c>false</c>.
        /// </value>
        bool IsNumeric { get; }

        /// <summary>
        /// Gets a value indicating whether the type is value type.
        /// Nullable value types have this property set to False.
        /// </summary>
        bool IsValueType { get; }

        /// <summary>
        /// Gets a value indicating whether the type is abstract.
        /// </summary>
        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the type is an interface.
        /// </summary>
        bool IsInterface { get; }

        /// <summary>
        /// Gets a value indicating whether the type is a non-nullable enumeration.
        /// </summary>
        bool IsEnum { get; }

        /// <summary>
        /// Gets a value indicating whether this type is an array.
        /// </summary>
        bool IsArray { get; }

        /// <summary>
        /// Gets a value indicating whether this type was built from a generic one.
        /// </summary>
        bool IsConstructedGenericType { get; }

        /// <summary>
        /// Searches this type's interfaces for a constructed, generic <see cref="IDictionary{TKey, TValue}"/>
        /// implementation and provides the constructed type proxy.
        /// </summary>
        ITypeProxy? GenericDictionaryType { get; }

        /// <summary>
        /// Searches this type's interfaces for a constructed, generic <see cref="ICollection{T}"/>
        /// implementation and provides the constructed type proxy.
        /// </summary>
        ITypeProxy? GenericCollectionType { get; }

        /// <summary>
        /// Gets a list of generic type arguments. Might be empty if not available.
        /// </summary>
        IReadOnlyList<ITypeProxy> GenericTypeArguments { get; }

        /// <summary>
        /// For arrays and generic enumerables returns the element type.
        /// For non generic enumerables just returns the proxy for the object type.
        /// Returns null otherwise.
        /// </summary>
        ITypeProxy? ElementType { get; }

        /// <summary>
        /// Returns true for types that implement <see cref="IEnumerable"/>
        /// </summary>
        bool IsEnumerable { get; }

        /// <summary>
        /// Returns true for types that implement <see cref="IList"/>
        /// </summary>
        bool IsList { get; }

        /// <summary>
        /// Gets a value indicating whether the type is basic.
        /// Basic types are all primitive types plus strings, GUIDs , TimeSpans, DateTimes
        /// including their nullable versions.
        /// </summary>
        bool IsBasicType { get; }

        /// <summary>
        /// When dealing with nullable value types, this property will
        /// return the underlying value type of the nullable; when dealing with
        /// enums, it will return the enumeration's underlying type;
        /// Otherwise it will return the same type as the <see cref="ProxiedType"/> property.
        /// </summary>
        ITypeProxy UnderlyingType { get; }

        /// <summary>
        /// Gets the default value for this type.
        /// Reference types return null while value types return their default equivalent.
        /// </summary>
        object? DefaultValue { get; }

        /// <summary>
        /// Gets a value indicating whether the type contains a suitable TryParse method.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can parse natively; otherwise, <c>false</c>.
        /// </value>
        public bool CanParseNatively { get; }

        /// <summary>
        /// Determines if a parameterless constructor can be called on this type.
        /// This always returns true on value types.
        /// </summary>
        bool CanCreateInstance { get; }

        /// <summary>
        /// Retrieves a list of interfaces this type implements.
        /// </summary>
        IReadOnlyList<Type> Interfaces { get; }

        /// <summary>
        /// Gets the property proxies associated with this type.
        /// </summary>
        /// <returns>A dictionary with property names as keys and <see cref="IPropertyProxy"/> objects as values.</returns>
        IReadOnlyDictionary<string, IPropertyProxy> Properties { get; }

        /// <summary>
        /// Provides a collection of all instance fields (public and non public) for this type.
        /// </summary>
        IReadOnlyList<FieldInfo> Fields { get; }

        /// <summary>
        /// Provides a collection of all instances of attributes applied on this type and its parent types.
        /// </summary>
        IReadOnlyList<object> TypeAttributes { get; }

        /// <summary>
        /// Calls the parameterless constructor on this type returning an isntance.
        /// For value types it returns the default value.
        /// If no parameterless constructor is available a <see cref="MissingMethodException"/> is thrown.
        /// </summary>
        /// <returns>A new instance of this type or the default value for value types.</returns>
        object CreateInstance();

        /// <summary>
        /// Converts this instance to its string representation, 
        /// trying to use the CultureInfo.InvariantCulture
        /// IFormat provider if the overload is available.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>A <see cref="System.String" /> that represents the current object.</returns>
        string ToStringInvariant(object? instance);

        /// <summary>
        /// Tries to parse the string into an object of the type this instance represents.
        /// Returns false when no suitable TryParse methods exists for the type or when parsing fails
        /// for any reason. When possible, this method uses CultureInfo.InvariantCulture and NumberStyles.Any.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if parse was converted successfully; otherwise, <c>false</c>.</returns>
        bool TryParse(string s, out object? result);
    }
}
