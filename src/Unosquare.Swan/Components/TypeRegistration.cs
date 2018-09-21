namespace Unosquare.Swan.Components
{
    using System;

    public partial class DependencyContainer
    {
        /// <summary>
        /// Represents a Type Registration within the IoC Container.
        /// </summary>
        public sealed class TypeRegistration
        {
            private readonly int _hashCode;

            /// <summary>
            /// Initializes a new instance of the <see cref="TypeRegistration"/> class.
            /// </summary>
            /// <param name="type">The type.</param>
            /// <param name="name">The name.</param>
            public TypeRegistration(Type type, string name = null)
            {
                Type = type;
                Name = name ?? string.Empty;

                _hashCode = string.Concat(Type.FullName, "|", Name).GetHashCode();
            }

            /// <summary>
            /// Gets the type.
            /// </summary>
            /// <value>
            /// The type.
            /// </value>
            public Type Type { get; }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
            /// <returns>
            ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (!(obj is TypeRegistration typeRegistration) || typeRegistration.Type != Type)
                    return false;

                return string.Compare(Name, typeRegistration.Name, StringComparison.Ordinal) == 0;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode() => _hashCode;
        }
    }
}