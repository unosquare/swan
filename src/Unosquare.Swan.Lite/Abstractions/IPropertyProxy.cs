namespace Unosquare.Swan.Abstractions
{
    /// <summary>
    /// Represents a generic interface to store getters and setters.
    /// </summary>
    public interface IPropertyProxy
    {
        /// <summary>
        /// Gets the property value via a stored delegate.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>The property value.</returns>
        object GetValue(object instance);

        /// <summary>
        /// Sets the property value via a stored delegate.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="value">The value.</param>
        void SetValue(object instance, object value);
    }
}