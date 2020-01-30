using System;
using System.Runtime.Serialization;

/*
 * NOTE TO CONTRIBUTORS:
 *
 * Never use this exception directly.
 * Use the methods in the SelfCheck class instead.
 */

namespace Swan
{
#pragma warning disable CA1032 // Add standard exception constructors.
    /// <summary>
    /// <para>The exception that is thrown by methods of the <see cref="SelfCheck"/> class
    /// to signal a condition most probably caused by an internal error in a library
    /// or application.</para>
    /// <para>Do not use this class directly; use the methods of the <see cref="SelfCheck"/> class instead.</para>
    /// </summary>
    [Serializable]
    public sealed class InternalErrorException : Exception
    {
#pragma warning disable SA1642 // Constructor summary documentation should begin with standard text - the <para> tag confuses the analyzer.
        /// <summary>
        /// <para>Initializes a new instance of the <see cref="InternalErrorException"/> class.</para>
        /// <para>Do not call this constrcutor directly; use the methods of the <see cref="SelfCheck"/> class instead.</para>
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
#pragma warning disable SA1642
        internal InternalErrorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalErrorException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"></see> that contains contextual information about the source or destination.</param>
        private InternalErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
#pragma warning restore CA1032
}