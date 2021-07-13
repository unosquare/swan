namespace Swan
{
    /// <summary>
    /// Defines Endianness, big or little.
    /// </summary>
    public enum Endianness
    {
        /// <summary>
        /// In big endian, you store the most significant byte in the smallest address. 
        /// </summary>
        Big,

        /// <summary>
        /// In little endian, you store the least significant byte in the smallest address.
        /// </summary>
        Little,
    }
}