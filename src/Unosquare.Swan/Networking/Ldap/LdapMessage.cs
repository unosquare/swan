#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Reflection;

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
        internal LdapMessage(LdapOperation type, IRfcRequest op, LdapControl[] controls)
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
        /// Returns any controls in the message.
        /// </summary>
        /// <value>
        /// The controls.
        /// </value>
        public virtual LdapControl[] Controls
        {
            get
            {
                var asn1Ctrls = Message.Controls;

                // convert from RFC 2251 Controls to LDAPControl[].
                if (asn1Ctrls == null) return null;

                var controls = new LdapControl[asn1Ctrls.Size()];

                for (var i = 0; i < asn1Ctrls.Size(); i++)
                {
                    var rfcCtl = (RfcControl) asn1Ctrls.Get(i);
                    var oid = rfcCtl.ControlType.StringValue();
                    var arrayValue = rfcCtl.ControlValue.ByteValue();
                    var critical = rfcCtl.Criticality.BooleanValue();

                    // Return from this call should return either an LDAPControl
                    // or a class extending LDAPControl that implements the
                    // appropriate registered response control
                    controls[i] = ControlFactory(oid, critical, arrayValue);
                }

                return controls;
            }
        }

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
        /// Indicates whether the message is a request or a response
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

        private static LdapControl ControlFactory(string oid, bool critical, sbyte[] values)
        {
            try
            {
                var respCtlClass = LdapControl.RegisteredControls.FindResponseControl(oid);

                // Did not find a match so return default LDAPControl
                if (respCtlClass == null)
                    return new LdapControl(oid, critical, values);

                Type[] argsClass = {typeof(string), typeof(bool), typeof(sbyte[])};

                var ctlConstructor = respCtlClass.GetConstructor(argsClass);

                return (LdapControl) ctlConstructor.Invoke(new object[] {oid, critical, values});
            }
            catch (Exception)
            {
                // No match with the OID
                // Do nothing. Fall through and construct a default LDAPControl object.
            }

            // If we get here we did not have a registered response control
            // for this oid.  Return a default LDAPControl object.
            return new LdapControl(oid, critical, values);
        }
    }
}
#endif