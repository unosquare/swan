#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections;
    using System.Reflection;

    /// <summary>
    /// The base class for Ldap request and response messages.
    /// Subclassed by response messages used in asynchronous operations.
    /// </summary>
    public class LdapMessage
    {
        /// <summary> Returns the LdapMessage request associated with this response</summary>
        internal virtual LdapMessage RequestingMessage => message.RequestingMessage;

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
                var asn1Ctrls = message.Controls;

                // convert from RFC 2251 Controls to LDAPControl[].
                if (asn1Ctrls == null) return null;

                var controls = new LdapControl[asn1Ctrls.Size()];

                for (var i = 0; i < asn1Ctrls.Size(); i++)
                {
                    var rfcCtl = (RfcControl)asn1Ctrls.Get(i);
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
        ///     Returns the message ID.  The message ID is an integer value
        ///     identifying the Ldap request and its response.
        /// </summary>
        public virtual int MessageID
        {
            get
            {
                if (imsgNum == -1)
                {
                    imsgNum = message.MessageID;
                }

                return imsgNum;
            }
        }

        /// <summary>
        /// Returns the Ldap operation type of the message.
        /// The type is one of the following:
        /// <ul><li>BIND_REQUEST            = 0;</li><li>BIND_RESPONSE           = 1;</li><li>UNBIND_REQUEST          = 2;</li><li>SEARCH_REQUEST          = 3;</li><li>SEARCH_RESPONSE         = 4;</li><li>SEARCH_RESULT           = 5;</li><li>MODIFY_REQUEST          = 6;</li><li>MODIFY_RESPONSE         = 7;</li><li>ADD_REQUEST             = 8;</li><li>ADD_RESPONSE            = 9;</li><li>DEL_REQUEST             = 10;</li><li>DEL_RESPONSE            = 11;</li><li>MODIFY_RDN_REQUEST      = 12;</li><li>MODIFY_RDN_RESPONSE     = 13;</li><li>COMPARE_REQUEST         = 14;</li><li>COMPARE_RESPONSE        = 15;</li><li>ABANDON_REQUEST         = 16;</li><li>SEARCH_RESULT_REFERENCE = 19;</li><li>EXTENDED_REQUEST        = 23;</li><li>EXTENDED_RESPONSE       = 24;</li><li>INTERMEDIATE_RESPONSE   = 25;</li></ul>
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public virtual LdapOperation Type
        {
            get
            {
                if (messageType == LdapOperation.Unknown)
                {
                    messageType = message.Type;
                }

                return messageType;
            }
        }

        /// <summary>
        ///     Indicates whether the message is a request or a response
        /// </summary>
        /// <returns>
        ///     true if the message is a request, false if it is a response,
        ///     a search result, or a search result reference.
        /// </returns>
        public virtual bool Request => message.IsRequest();

        /// <summary> Returns the RFC 2251 LdapMessage composed in this object.</summary>
        internal virtual RfcLdapMessage Asn1Object => message;

        private string Name => Type.ToString();

        /// <summary>
        ///     Retrieves the identifier tag for this message.
        ///     An identifier can be associated with a message with the
        ///     <code>setTag</code> method.
        ///     Tags are set by the application and not by the API or the server.
        ///     If a server response <code>isRequest() == false</code> has no tag,
        ///     the tag associated with the corresponding server request is used.
        /// </summary>
        /// <returns>
        ///     the identifier associated with this message or <code>null</code>
        ///     if none.
        /// </returns>
        /// <summary>
        ///     Sets a string identifier tag for this message.
        ///     This method allows an API to set a tag and later identify messages
        ///     by retrieving the tag associated with the message.
        ///     Tags are set by the application and not by the API or the server.
        ///     Message tags are not included with any message sent to or received
        ///     from the server.
        ///     Tags set on a request to the server
        ///     are automatically associated with the response messages when they are
        ///     received by the API and transferred to the application.
        ///     The application can explicitly set a different value in a
        ///     response message.
        ///     To set a value in a server request, for example an
        ///     {@link LdapSearchRequest}, you must create the object,
        ///     set the tag, and use the
        ///     {@link LdapConnection.SendRequest LdapConnection.sendRequest()}
        ///     method to send it to the server.
        /// </summary>
        public virtual string Tag
        {
            get
            {
                if (stringTag != null)
                {
                    return stringTag;
                }

                return Request ? null : RequestingMessage?.stringTag;
            }

            set => stringTag = value;
        }

        /// <summary> A request or response message for an asynchronous Ldap operation.</summary>
        internal RfcLdapMessage message;

        private int imsgNum = -1; // This instance LdapMessage number

        private LdapOperation messageType = LdapOperation.Unknown;

        private string stringTag;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapMessage"/> class.
        /// Dummy constuctor
        /// </summary>
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
            messageType = type;
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
            message = new RfcLdapMessage(op, asn1Ctrls);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapMessage"/> class.
        /// Creates an Rfc 2251 LdapMessage when the libraries receive a response
        /// from a command.
        /// </summary>
        /// <param name="message">A response message.</param>
        internal LdapMessage(RfcLdapMessage message) => this.message = message;
        
        /// <summary>
        /// Instantiates an LdapControl.  We search through our list of
        /// registered controls.  If we find a matchiing OID we instantiate
        /// that control by calling its contructor.  Otherwise we default to
        /// returning a regular LdapControl object
        /// </summary>
        /// <param name="oid">The oid.</param>
        /// <param name="critical">if set to <c>true</c> [critical].</param>
        /// <param name="values">The value renamed.</param>
        /// <returns>LdapControl</returns>
        private LdapControl ControlFactory(string oid, bool critical, sbyte[] values)
        {
            var regControls = LdapControl.RegisteredControls;

            try
            {
                var respCtlClass = regControls.FindResponseControl(oid);

                // Did not find a match so return default LDAPControl
                if (respCtlClass == null)
                    return new LdapControl(oid, critical, values);

                /* If found, get LDAPControl constructor */
                Type[] argsClass = { typeof(string), typeof(bool), typeof(sbyte[]) };
                object[] args = { oid, critical, values };
                try
                {
                    var ctlConstructor = respCtlClass.GetConstructor(argsClass);

                    try
                    {
                        return (LdapControl)ctlConstructor.Invoke(args);
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (TargetInvocationException)
                    {
                    }
                    catch (Exception)
                    {
                        // Could not create the ResponseControl object
                        // All possible exceptions are ignored. We fall through
                        // and create a default LDAPControl object
                    }
                }
                catch (MethodAccessException)
                {
                    // bad class was specified, fall through and return a
                    // default LDAPControl object
                }
            }
            catch (FieldAccessException)
            {
                // No match with the OID
                // Do nothing. Fall through and construct a default LDAPControl object.
            }

            // If we get here we did not have a registered response control
            // for this oid.  Return a default LDAPControl object.
            return new LdapControl(oid, critical, values);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Name + "(" + MessageID + "): " + message;
    }
}
#endif