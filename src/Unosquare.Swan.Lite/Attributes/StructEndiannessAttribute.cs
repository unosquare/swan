namespace Unosquare.Swan.Attributes
{
    using System;

    /// <summary>
    /// An attribute used to help conversion structs back and forth into arrays of bytes via
    /// extension methods included in this library ToStruct and ToBytes.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class StructEndiannessAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StructEndiannessAttribute"/> class.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        public StructEndiannessAttribute(Endianness endianness)
        {
            Endianness = endianness;
        }

        /// <summary>
        /// Gets the endianness.
        /// </summary>
        /// <value>
        /// The endianness.
        /// </value>
        public Endianness Endianness { get; }
    }
}