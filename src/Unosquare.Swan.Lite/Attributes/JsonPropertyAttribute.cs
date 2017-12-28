namespace Unosquare.Swan.Attributes
{
    using System;

    /// <summary>
    /// An attribute used to help setup a property behavior when serialize/deserialize JSON.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class JsonPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyAttribute" /> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="ignored">if set to <c>true</c> [ignored].</param>
        public JsonPropertyAttribute(string propertyName, bool ignored = false)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            Ignored = ignored;
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>
        /// The name of the property.
        /// </value>
        public string PropertyName { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="JsonPropertyAttribute" /> is ignored.
        /// </summary>
        /// <value>
        ///   <c>true</c> if ignored; otherwise, <c>false</c>.
        /// </value>
        public bool Ignored { get; }
    }
}