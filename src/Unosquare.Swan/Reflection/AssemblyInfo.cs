#if !NET452
namespace Unosquare.Swan.Reflection
{
    /// <summary>
    /// Represents an Assembly information object
    /// </summary>
    internal class AssemblyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInfo"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">value</exception>
        public AssemblyInfo(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new System.ArgumentNullException(nameof(value));

            var parts = value.Split('/');
            Name = parts[0];

            if (parts.Length == 2)
            {
                Version = parts[1];
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public string Version { get; set; }
    }
}
#endif