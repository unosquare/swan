namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Networking.DnsClient;

    #region Dependency Container

    /// <summary>
    /// An exception for dependency resolutions
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerResolutionException : Exception
    {
        private const string ErrorText = "Unable to resolve type: {0}";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DependencyContainerResolutionException(Type type)
            : base(String.Format(ErrorText, type.FullName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerResolutionException(Type type, Exception innerException)
            : base(String.Format(ErrorText, type.FullName), innerException)
        {
        }
    }

    /// <summary>
    /// Registration Type Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerRegistrationTypeException : Exception
    {
        private const string RegisterErrorText = "Cannot register type {0} - abstract classes or interfaces are not valid implementation types for {1}.";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationTypeException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="factory">The factory.</param>
        public DependencyContainerRegistrationTypeException(Type type, string factory)
            : base(String.Format(RegisterErrorText, type.FullName, factory))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationTypeException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerRegistrationTypeException(Type type, string factory, Exception innerException)
            : base(String.Format(RegisterErrorText, type.FullName, factory), innerException)
        {
        }
    }

    /// <summary>
    /// Generic Constraint Registration Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerRegistrationException : Exception
    {
        private const string ConvertErrorText = "Cannot convert current registration of {0} to {1}";
        private const string GenericConstraintErrorText = "Type {1} is not valid for a registration of type {0}";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        public DependencyContainerRegistrationException(Type type, string method)
            : base(String.Format(ConvertErrorText, type.FullName, method))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerRegistrationException(Type type, string method, Exception innerException)
            : base(String.Format(ConvertErrorText, type.FullName, method), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        public DependencyContainerRegistrationException(Type registerType, Type implementationType)
            : base(String.Format(GenericConstraintErrorText, registerType.FullName, implementationType.FullName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerRegistrationException(Type registerType, Type implementationType, Exception innerException)
            : base(String.Format(GenericConstraintErrorText, registerType.FullName, implementationType.FullName), innerException)
        {
        }
    }

    /// <summary>
    /// Weak Reference Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerWeakReferenceException : Exception
    {
        private const string ErrorText = "Unable to instantiate {0} - referenced object has been reclaimed";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerWeakReferenceException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DependencyContainerWeakReferenceException(Type type)
            : base(String.Format(ErrorText, type.FullName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerWeakReferenceException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerWeakReferenceException(Type type, Exception innerException)
            : base(String.Format(ErrorText, type.FullName), innerException)
        {
        }
    }

    /// <summary>
    /// Constructor Resolution Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerConstructorResolutionException : Exception
    {
        private const string ErrorText = "Unable to resolve constructor for {0} using provided Expression.";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerConstructorResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DependencyContainerConstructorResolutionException(Type type)
            : base(String.Format(ErrorText, type.FullName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerConstructorResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerConstructorResolutionException(Type type, Exception innerException)
            : base(String.Format(ErrorText, type.FullName), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerConstructorResolutionException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DependencyContainerConstructorResolutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerConstructorResolutionException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DependencyContainerConstructorResolutionException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Auto-registration Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerAutoRegistrationException : Exception
    {
        private const string ErrorText = "Duplicate implementation of type {0} found ({1}).";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerAutoRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="types">The types.</param>
        public DependencyContainerAutoRegistrationException(Type registerType, IEnumerable<Type> types)
            : base(String.Format(ErrorText, registerType, GetTypesString(types)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerAutoRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="types">The types.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerAutoRegistrationException(Type registerType, IEnumerable<Type> types, Exception innerException)
            : base(String.Format(ErrorText, registerType, GetTypesString(types)), innerException)
        {
        }

        /// <summary>
        /// Gets the types string.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <returns></returns>
        private static string GetTypesString(IEnumerable<Type> types)
        {
            var typeNames = from type in types
                            select type.FullName;

            return string.Join(",", typeNames.ToArray());
        }
    }

    #endregion

    #region Message Hub

    /// <summary>
    /// Thrown when an exceptions occurs while subscribing to a message type
    /// </summary>
    public class MessageHubSubscriptionException : Exception
    {
        private const string ErrorText = "Unable to add subscription for {0} : {1}";

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHubSubscriptionException"/> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="reason">The reason.</param>
        public MessageHubSubscriptionException(Type messageType, string reason)
            : base(String.Format(ErrorText, messageType, reason))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHubSubscriptionException"/> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="innerException">The inner exception.</param>
        public MessageHubSubscriptionException(Type messageType, string reason, Exception innerException)
            : base(string.Format(ErrorText, messageType, reason), innerException)
        {

        }
    }

    #endregion

    #region DNS Client

    /// <summary>
    /// An exception thrown when the DNS query fails.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DnsQueryException : Exception
    {
        #region Constructors

        internal DnsQueryException() { }
        internal DnsQueryException(string message) : base(message) { }
        internal DnsQueryException(string message, Exception e) : base(message, e) { }

        internal DnsQueryException(IDnsResponse response) : this(response, Format(response)) { }

        internal DnsQueryException(IDnsResponse response, Exception e)
            : base(Format(response), e)
        {
            Response = response;
        }

        internal DnsQueryException(IDnsResponse response, string message)
            : base(message)
        {
            Response = response;
        }

        #endregion

        #region Properties and Methods

        private static string Format(IDnsResponse response)
        {
            return $"Invalid response received with code {response.ResponseCode}";
        }

        internal IDnsResponse Response
        {
            get;
            private set;
        }

        #endregion
    }

    #endregion

    #region SMTP Client

#if !NET452

    /// <summary>
    /// Defines an SMTP Exceptions class
    /// </summary>
    public class SmtpException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpException"/> class with a message.
        /// </summary>
        public SmtpException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpException"/> class.
        /// </summary>
        public SmtpException(SmtpStatusCode replyCode, string message) : base($"{message} ReplyCode: {replyCode}")
        {
        }
    }

#endif

    #endregion
}
