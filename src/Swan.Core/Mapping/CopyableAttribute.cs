namespace Swan.Mapping
{
    using System;

    /// <summary>
    /// Represents an attribute to select which properties are copyable between objects.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CopyableAttribute : Attribute
    {
    }
}