namespace Unosquare.Swan
{
    using System;

    /// <summary>
    /// An attibute used to help conversion structs back and forth into arrays of bytes via
    /// extension methods included in this library ToStruct and ToBytes.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class EndianAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndianAttribute"/> class.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        public EndianAttribute(Endianness endianness)
        {
            Endianness = endianness;
        }

        /// <summary>
        /// Gets the endianness.
        /// </summary>
        public Endianness Endianness
        {
            get;
            private set;
        }
    }
}
