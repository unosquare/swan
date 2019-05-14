namespace Unosquare.Swan
{
    /// <summary>
    /// Enumeration of Operating Systems.
    /// </summary>
    public enum OperatingSystem
    {
        /// <summary>
        /// Unknown OS
        /// </summary>
        Unknown,
        
        /// <summary>
        /// Windows
        /// </summary>
        Windows,
        
        /// <summary>
        /// UNIX/Linux
        /// </summary>
        Unix,
        
        /// <summary>
        /// macOS (OSX)
        /// </summary>
        Osx,
    }

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

    /// <summary>
    /// Enumerates the JSON serializer cases to use: None (keeps the same case), PascalCase, or camelCase.
    /// </summary>
    public enum JsonSerializerCase
    {
        /// <summary>
        /// The none
        /// </summary>
        None,

        /// <summary>
        /// The pascal case (eg. PascalCase)
        /// </summary>
        PascalCase,

        /// <summary>
        /// The camel case (eg. camelCase)
        /// </summary>
        CamelCase,
    }
}
