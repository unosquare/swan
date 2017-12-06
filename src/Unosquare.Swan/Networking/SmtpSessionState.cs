namespace Unosquare.Swan.Networking
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the state of an SMTP session associated with a client
    /// </summary>
    public class SmtpSessionState
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpSessionState"/> class.
        /// </summary>
        public SmtpSessionState()
        {
            DataBuffer = new List<byte>();
            Reset(true);
            ResetAuthentication();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the contents of the data buffer.
        /// </summary>
        public List<byte> DataBuffer { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has initiated.
        /// </summary>
        public bool HasInitiated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current session supports extensions.
        /// </summary>
        public bool SupportsExtensions { get; set; }

        /// <summary>
        /// Gets or sets the client hostname.
        /// </summary>
        public string ClientHostname { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the session is currently receiving DATA
        /// </summary>
        public bool IsInDataMode { get; set; }

        /// <summary>
        /// Gets or sets the sender address.
        /// </summary>
        public string SenderAddress { get; set; }

        /// <summary>
        /// Gets the recipients.
        /// </summary>
        public List<string> Recipients { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the extended data supporting any additional field for storage by a responder implementation.
        /// </summary>
        public object ExtendedData { get; set; }

        #endregion

        #region AUTH State

        /// <summary>
        /// Gets or sets a value indicating whether this instance is in authentication mode.
        /// </summary>
        public bool IsInAuthMode { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has provided username.
        /// </summary>
        public bool HasProvidedUsername => string.IsNullOrWhiteSpace(Username) == false;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is authenticated.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the authentication mode.
        /// </summary>
        public string AuthMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is channel secure.
        /// </summary>
        public bool IsChannelSecure { get; set; }

        /// <summary>
        /// Resets the authentication state.
        /// </summary>
        public void ResetAuthentication()
        {
            Username = string.Empty;
            Password = string.Empty;
            AuthMode = string.Empty;
            IsInAuthMode = false;
            IsAuthenticated = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resets the data mode to false, clears the recipients, the sender address and the data buffer
        /// </summary>
        public void ResetEmail()
        {
            IsInDataMode = false;
            Recipients.Clear();
            SenderAddress = string.Empty;
            DataBuffer.Clear();
        }

        /// <summary>
        /// Resets the state table entirely
        /// </summary>
        /// <param name="clearExtensionData">if set to <c>true</c> [clear extension data].</param>
        public void Reset(bool clearExtensionData)
        {
            HasInitiated = false;
            SupportsExtensions = false;
            ClientHostname = string.Empty;
            ResetEmail();

            if (clearExtensionData)
                ExtendedData = null;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A clone</returns>
        public virtual SmtpSessionState Clone()
        {
            var clonedState = this.CopyPropertiesToNew<SmtpSessionState>(new[] {nameof(DataBuffer)});
            clonedState.DataBuffer.AddRange(DataBuffer);
            clonedState.Recipients.AddRange(Recipients);

            return clonedState;
        }

        #endregion
    }
}