namespace Unosquare.Swan.Networking.Ldap
{
    /// <summary>
    /// The base class for Ldap request and response messages.
    /// Subclassed by response messages used in asynchronous operations.
    /// </summary>
    public class LdapMessage
    {
        internal RfcLdapMessage Message;

        private int _imsgNum = -1; // This instance LdapMessage number

        private LdapOperation _messageType = LdapOperation.Unknown;

        private string _stringTag;

        internal LdapMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapMessage"/> class.
        /// Creates an LdapMessage when sending a protocol operation and sends
        /// some optional controls with the message.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="op">The operation type of message.</param>
        /// <param name="controls">The controls to use with the operation.</param>
        /// <seealso cref="Type"></seealso>
        internal LdapMessage(LdapOperation type, IRfcRequest op, LdapControl[] controls = null)
        {
            // Get a unique number for this request message
            _messageType = type;
            RfcControls asn1Ctrls = null;

            if (controls != null)
            {
                // Move LdapControls into an RFC 2251 Controls object.
                asn1Ctrls = new RfcControls();

                foreach (var t in controls)
                {
                    asn1Ctrls.Add(t.Asn1Object);
                }
            }

            // create RFC 2251 LdapMessage
            Message = new RfcLdapMessage(op, asn1Ctrls);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapMessage"/> class.
        /// Creates an Rfc 2251 LdapMessage when the libraries receive a response
        /// from a command.
        /// </summary>
        /// <param name="message">A response message.</param>
        internal LdapMessage(RfcLdapMessage message) => Message = message;
        
        /// <summary>
        /// Returns the message ID.  The message ID is an integer value
        /// identifying the Ldap request and its response.
        /// </summary>
        /// <value>
        /// The message identifier.
        /// </value>
        public virtual int MessageId
        {
            get
            {
                if (_imsgNum == -1)
                {
                    _imsgNum = Message.MessageId;
                }

                return _imsgNum;
            }
        }

        /// <summary>
        /// Indicates whether the message is a request or a response.
        /// </summary>
        /// <value>
        ///   <c>true</c> if request; otherwise, <c>false</c>.
        /// </value>
        public virtual bool Request => Message.IsRequest();

        internal LdapOperation Type
        {
            get
            {
                if (_messageType == LdapOperation.Unknown)
                {
                    _messageType = Message.Type;
                }

                return _messageType;
            }
        }

        internal virtual RfcLdapMessage Asn1Object => Message;
        
        internal virtual LdapMessage RequestingMessage => Message.RequestingMessage;

        /// <summary>
        /// Retrieves the identifier tag for this message.
        /// An identifier can be associated with a message with the
        /// <c>setTag</c> method.
        /// Tags are set by the application and not by the API or the server.
        /// If a server response <c>isRequest() == false</c> has no tag,
        /// the tag associated with the corresponding server request is used.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public virtual string Tag
        {
            get
            {
                if (_stringTag != null)
                {
                    return _stringTag;
                }

                return Request ? null : RequestingMessage?._stringTag;
            }

            set => _stringTag = value;
        }

        private string Name => Type.ToString();

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"{Name}({MessageId}): {Message}";
    }
}