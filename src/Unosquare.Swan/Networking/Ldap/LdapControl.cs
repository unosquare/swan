﻿#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections;
    using System.Text;

    /// <summary>
    /// Encapsulates optional additional parameters or constraints to be applied to
    /// an Ldap operation.
    /// When included with LdapConstraints or LdapSearchConstraints
    /// on an LdapConnection or with a specific operation request, it is
    /// sent to the server along with operation requests.
    /// </summary>
    /// <seealso cref="LdapConnection.ResponseControls"></seealso>
    /// <seealso cref="LdapConstraints.GetControls"></seealso>
    public class LdapControl
    {
        private RfcControl _control; // An RFC 2251 Control

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapControl"/> class.
        /// Constructs a new LdapControl object using the specified values.
        /// </summary>
        /// <param name="oid">The OID of the control, as a dotted string.</param>
        /// <param name="critical">True if the Ldap operation should be discarded if
        /// the control is not supported. False if
        /// the operation can be processed without the control.</param>
        /// <param name="values">The control-specific data.</param>
        /// <exception cref="ArgumentException">An OID must be specified</exception>
        public LdapControl(string oid, bool critical, sbyte[] values)
        {
            if (oid == null)
            {
                throw new ArgumentException("An OID must be specified");
            }

            _control = values == null
                ? new RfcControl(new Asn1OctetString(oid), new Asn1Boolean(critical))
                : new RfcControl(new Asn1OctetString(oid), new Asn1Boolean(critical), new Asn1OctetString(values));
        }

        /// <summary>
        ///     Returns the identifier of the control.
        /// </summary>
        /// <returns>
        ///     The object ID of the control.
        /// </returns>
        public virtual string ID => _control.ControlType.StringValue();

        /// <summary>
        ///     Returns whether the control is critical for the operation.
        /// </summary>
        /// <returns>
        ///     Returns true if the control must be supported for an associated
        ///     operation to be executed, and false if the control is not required for
        ///     the operation.
        /// </returns>
        public virtual bool Critical => _control.Criticality.BooleanValue();

        internal static RespControlVector RegisteredControls { get; } = new RespControlVector(5);

        /// <summary>
        ///     Returns the RFC 2251 Control object.
        /// </summary>
        /// <returns>
        ///     An ASN.1 RFC 2251 Control.
        /// </returns>
        internal virtual RfcControl Asn1Object => _control;

        /// <summary>
        ///     Returns a copy of the current LdapControl object.
        /// </summary>
        /// <returns>
        ///     A copy of the current LdapControl object.
        /// </returns>
        public object Clone()
        {
            var cont = (LdapControl) MemberwiseClone();
            var vals = GetValue();

            if (vals != null)
            {
                // is this necessary?
                // Yes even though the contructor above allocates a
                // new Asn1OctetString, vals in that constuctor
                // is only copied by reference
                var twin = new sbyte[vals.Length];
                for (var i = 0; i < vals.Length; i++)
                {
                    twin[i] = vals[i];
                }

                cont._control = new RfcControl(new Asn1OctetString(ID), new Asn1Boolean(Critical),
                    new Asn1OctetString(twin));
            }

            return cont;
        }

        /// <summary>
        ///     Returns the control-specific data of the object.
        /// </summary>
        /// <returns>
        ///     The control-specific data of the object as a byte array,
        ///     or null if the control has no data.
        /// </returns>
        public virtual sbyte[] GetValue() => _control.ControlValue?.ByteValue();

        /// <summary>
        /// Sets the control-specific data of the object.  This method is for
        /// use by an extension of LdapControl.
        /// </summary>
        /// <param name="controlValue">The control value.</param>
        protected internal virtual void SetValue(sbyte[] controlValue)
        {
            _control.ControlValue = new Asn1OctetString(controlValue);
        }

        /// <summary>
        /// Registers a class to be instantiated on receipt of a control with the
        /// given OID.
        /// Any previous registration for the OID is overridden. The
        /// controlClass must be an extension of LdapControl.
        /// </summary>
        /// <param name="oid">The object identifier of the control.</param>
        /// <param name="controlClass">A class which can instantiate an LdapControl.</param>
        public static void Register(string oid, Type controlClass)
            => RegisteredControls.RegisterResponseControl(oid, controlClass);
    }

    /// <summary>
    /// Represents and Ldap Bind Request.
    /// <pre>
    /// BindRequest ::= [APPLICATION 0] SEQUENCE {
    /// version                 INTEGER (1 .. 127),
    /// name                    LdapDN,
    /// authentication          AuthenticationChoice }
    /// </pre></summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    /// <seealso cref="IRfcRequest" />
    internal class RfcBindRequest
        : Asn1Sequence, IRfcRequest
    {
        public virtual Asn1Integer Version
        {
            get => (Asn1Integer) Get(0);
            set => Set(0, value);
        }

        public virtual RfcLdapDN Name
        {
            get => (RfcLdapDN) Get(1);
            set => Set(1, value);
        }

        public virtual RfcAuthenticationChoice AuthenticationChoice
        {
            get => (RfcAuthenticationChoice) Get(2);
            set => Set(2, value);
        }

        /// <summary>
        /// ID is added for Optimization.
        /// ID needs only be one Value for every instance,
        /// thus we create it only once.
        /// </summary>
        private static readonly Asn1Identifier ID = new Asn1Identifier(LdapOperation.BindRequest);

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcBindRequest"/> class.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="name">The name.</param>
        /// <param name="auth">The authentication.</param>
        public RfcBindRequest(Asn1Integer version, RfcLdapDN name, RfcAuthenticationChoice auth)
            : base(3)
        {
            Add(version);
            Add(name);
            Add(auth);
        }

        /// <summary>
        /// Override getIdentifier to return an application-wide id.
        /// <pre>
        /// ID = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 0. (0x60)
        /// </pre>
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier() => ID;

        public string GetRequestDN() => ((RfcLdapDN) Get(1)).StringValue();
    }

    /// <summary>
    /// Represents a simple bind request.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.LdapMessage" />
    public sealed class LdapBindRequest : LdapMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapBindRequest"/> class.
        /// Constructs a simple bind request.
        /// </summary>
        /// <param name="version">The Ldap protocol version, use Ldap_V3.
        /// Ldap_V2 is not supported.</param>
        /// <param name="dn">If non-null and non-empty, specifies that the
        /// connection and all operations through it should
        /// be authenticated with dn as the distinguished
        /// name.</param>
        /// <param name="passwd">If non-null and non-empty, specifies that the
        /// connection and all operations through it should
        /// be authenticated with dn as the distinguished
        /// name and passwd as password.</param>
        /// <param name="cont">Any controls that apply to the simple bind request,
        /// or null if none.</param>
        public LdapBindRequest(int version, string dn, sbyte[] passwd)
            : base(LdapOperation.BindRequest,
                new RfcBindRequest(new Asn1Integer(version), new RfcLdapDN(dn),
                    new RfcAuthenticationChoice(new Asn1Tagged(new Asn1Identifier(0), new Asn1OctetString(passwd),
                        false))), 
                null)
        {
        }

        /// <summary>
        ///     Retrieves the Authentication DN for a bind request.
        /// </summary>
        /// <returns>
        ///     the Authentication DN for a bind request
        /// </returns>
        public string AuthenticationDN => Asn1Object.RequestDn;

        /// <summary>
        /// Return an Asn1 representation of this add request.
        /// #return an Asn1 representation of this object.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Asn1Object.ToString();
    }

    /// <summary>
    /// Encapsulates a continuation reference from an asynchronous search operation.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.LdapMessage" />
    internal sealed class LdapSearchResultReference : LdapMessage
    {
        private string[] srefs;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapSearchResultReference"/> class.
        /// Constructs an LdapSearchResultReference object.
        /// </summary>
        /// <param name="message">The LdapMessage with a search reference.</param>
        internal LdapSearchResultReference(RfcLdapMessage message)
            : base(message)
        {
        }

        /// <summary>
        /// Returns any URLs in the object.
        /// </summary>
        /// <value>
        /// The referrals.
        /// </value>
        public string[] Referrals
        {
            get
            {
                var references = ((RfcSearchResultReference) Message.Response).ToArray();
                srefs = new string[references.Length];
                for (var i = 0; i < references.Length; i++)
                {
                    srefs[i] = ((Asn1OctetString) references[i]).StringValue();
                }

                return srefs;
            }
        }
    }
    
    /// <summary>
    /// Encapsulates parameters of an Ldap URL query as defined in RFC2255.
    /// An LdapUrl object can be passed to LdapConnection.search to retrieve
    /// search results.
    /// </summary>
    /// <seealso cref="LdapConnection.Search"></seealso>
    public sealed class LdapUrl
    {
        // Broken out parts of the URL
        private readonly bool ipV6 = false; // TCP/IP V6

        private int _port; // Port
        private string _dn; // Base DN

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapUrl"/> class.
        /// Constructs a URL object with the specified string as the URL.
        /// </summary>
        /// <param name="url">An Ldap URL string, e.g.
        /// "ldap://ldap.example.com:80/dc=example,dc=com?cn,
        /// sn?sub?(objectclass=inetOrgPerson)".</param>
        public LdapUrl(string url)
        {
            ParseUrl(url);
        }

        /// <summary>
        /// Returns an array of attribute names specified in the URL.
        /// </summary>
        /// <value>
        /// The attribute array.
        /// </value>
        public string[] AttributeArray { get; private set; }

        /// <summary>
        ///     Returns an enumerator for the attribute names specified in the URL.
        /// </summary>
        /// <returns>
        ///     An enumeration of attribute names.
        /// </returns>
        /// <summary>
        ///     Returns any Ldap URL extensions specified, or null if none are
        ///     specified. Each extension is a type=value expression. The =value part
        ///     MAY be omitted. The expression MAY be prefixed with '!' if it is
        ///     mandatory for evaluation of the URL.
        /// </summary>
        /// <returns>
        ///     string array of extensions.
        /// </returns>
        public string[] Extensions { get; private set; }

        /// <summary>
        ///     Returns the search filter or <code>null</code> if none was specified.
        /// </summary>
        /// <returns>
        ///     The search filter.
        /// </returns>
        public string Filter { get; private set; }

        /// <summary>
        ///     Returns the name of the Ldap server in the URL.
        /// </summary>
        /// <returns>
        ///     The host name specified in the URL.
        /// </returns>
        public string Host { get; private set; }

        /// <summary>
        ///     Returns the port number of the Ldap server in the URL.
        /// </summary>
        /// <returns>
        ///     The port number in the URL.
        /// </returns>
        public int Port => _port == 0 ? LdapConnection.DefaultPort : _port;

        /// <summary>
        ///     Returns the depth of search. It returns one of the following from
        ///     LdapConnection: SCOPE_BASE, SCOPE_ONE, or SCOPE_SUB.
        /// </summary>
        /// <returns>
        ///     The search scope.
        /// </returns>
        public int Scope { get; private set; } = LdapConnection.ScopeBase;

        /// <summary>
        ///     Returns true if the URL is of the type ldaps (Ldap over SSL, a predecessor
        ///     to startTls)
        /// </summary>
        /// <returns>
        ///     whether this is a secure Ldap url or not.
        /// </returns>
        public bool Secure { get; private set; }

        /// <summary>
        ///     Returns a clone of this URL object.
        /// </summary>
        /// <returns>
        ///     clone of this URL object.
        /// </returns>
        public object Clone() => MemberwiseClone();

        /// <summary>
        ///     Returns the base distinguished name encapsulated in the URL.
        /// </summary>
        /// <returns>
        ///     The base distinguished name specified in the URL, or null if none.
        /// </returns>
        public string GetDN() => _dn;

        /// <summary>
        /// Sets the base distinguished name encapsulated in the URL.
        /// </summary>
        /// <param name="dn">The dn.</param>
        internal void SetDN(string dn) => _dn = dn;

        /// <summary>
        /// Returns a valid string representation of this Ldap URL.
        /// </summary>
        /// <returns>
        /// The string representation of the Ldap URL.
        /// </returns>
        public override string ToString()
        {
            var url = new StringBuilder(256);

            // Scheme
            url.Append(Secure ? "ldaps://" : "ldap://");

            // Host:port/dn
            url.Append(ipV6 ? $"[{Host}]" : Host);

            // Port not specified
            if (_port != 0)
            {
                url.Append(":" + _port);
            }

            if (_dn == null && AttributeArray == null && Scope == LdapConnection.ScopeBase && Filter == null &&
                Extensions == null)
            {
                return url.ToString();
            }

            url.Append("/");
            if (_dn != null)
            {
                url.Append(_dn);
            }

            if (AttributeArray == null && Scope == LdapConnection.ScopeBase && Filter == null && Extensions == null)
            {
                return url.ToString();
            }

            // attributes
            url.Append("?");
            if (AttributeArray != null)
            {
                // should we check also for attrs != "*"
                for (var i = 0; i < AttributeArray.Length; i++)
                {
                    url.Append(AttributeArray[i]);
                    if (i < AttributeArray.Length - 1)
                    {
                        url.Append(",");
                    }
                }
            }

            if (Scope == LdapConnection.ScopeBase && Filter == null && Extensions == null)
            {
                return url.ToString();
            }

            // scope
            url.Append("?");
            if (Scope != LdapConnection.ScopeBase)
            {
                url.Append(Scope == LdapConnection.ScopeOne ? "one" : "sub");
            }

            if (Filter == null && Extensions == null)
            {
                return url.ToString();
            }

            // filter
            url.Append(Filter == null ? "?" : $"?{Filter}");

            if (Extensions == null)
            {
                return url.ToString();
            }

            // extensions
            url.Append("?");
            if (Extensions != null)
            {
                for (var i = 0; i < Extensions.Length; i++)
                {
                    url.Append(Extensions[i]);
                    if (i < Extensions.Length - 1)
                    {
                        url.Append(",");
                    }
                }
            }

            return url.ToString();
        }

        private string[] ParseList(string listStr, char delimiter, int listStart, int listEnd)
        {
            // Check for and empty string
            if (listEnd - listStart < 1)
            {
                return null;
            }

            // First count how many items are specified
            var itemStart = listStart;
            int itemEnd;
            var itemCount = 0;
            while (itemStart > 0)
            {
                // itemStart == 0 if no delimiter found
                itemCount += 1;
                itemEnd = listStr.IndexOf(delimiter, itemStart);
                if (itemEnd <= 0 || itemEnd >= listEnd)
                    break;
                itemStart = itemEnd + 1;
            }

            // Now fill in the array with the attributes
            itemStart = listStart;
            var list = new string[itemCount];
            itemCount = 0;
            while (itemStart > 0)
            {
                itemEnd = listStr.IndexOf(delimiter, itemStart);
                if (itemStart > listEnd)
                {
                    break;
                }

                if (itemEnd < 0)
                    itemEnd = listEnd;
                if (itemEnd > listEnd)
                    itemEnd = listEnd;
                list[itemCount] = listStr.Substring(itemStart, itemEnd - itemStart);
                itemStart = itemEnd + 1;
                itemCount += 1;
            }

            return list;
        }

        private void ParseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new UriFormatException("LdapUrl: URL cannot be null");

            var scanStart = 0;
            var scanEnd = url.Length;

            // Check if URL is enclosed by < & >
            if (url[scanStart] == '<')
            {
                if (url[scanEnd - 1] != '>')
                    throw new UriFormatException("LdapUrl: URL bad enclosure");
                scanStart += 1;
                scanEnd -= 1;
            }

            // Determine the URL scheme and set appropriate default port
            if (url.Substring(scanStart, scanStart + 4 - scanStart).ToUpper().Equals("URL:".ToUpper()))
            {
                scanStart += 4;
            }

            if (url.Substring(scanStart, scanStart + 7 - scanStart).ToUpper().Equals("ldap://".ToUpper()))
            {
                scanStart += 7;
                _port = LdapConnection.DefaultPort;
            }
            else if (url.Substring(scanStart, scanStart + 8 - scanStart).ToUpper().Equals("ldaps://".ToUpper()))
            {
                Secure = true;
                scanStart += 8;
                _port = LdapConnection.DefaultSslPort;
            }
            else
            {
                throw new UriFormatException("LdapUrl: URL scheme is not ldap");
            }

            // Find where host:port ends and dn begins
            var dnStart = url.IndexOf("/", scanStart);
            var hostPortEnd = scanEnd;
            var novell = false;
            if (dnStart < 0)
            {
                dnStart = url.IndexOf("?", scanStart);
                if (dnStart > 0)
                {
                    if (url[dnStart + 1] == '?')
                    {
                        hostPortEnd = dnStart;
                        dnStart += 1;
                        novell = true;
                    }
                    else
                    {
                        dnStart = -1;
                    }
                }
            }
            else
            {
                hostPortEnd = dnStart;
            }

            // Check for IPV6 "[ipaddress]:port"
            int portStart;
            var hostEnd = hostPortEnd;
            if (url[scanStart] == '[')
            {
                hostEnd = url.IndexOf(']', scanStart + 1);
                if (hostEnd >= hostPortEnd || hostEnd == -1)
                {
                    throw new UriFormatException("LdapUrl: \"]\" is missing on IPV6 host name");
                }

                // Get host w/o the [ & ]
                Host = url.Substring(scanStart + 1, hostEnd - (scanStart + 1));
                portStart = url.IndexOf(":", hostEnd);
                if (portStart < hostPortEnd && portStart != -1)
                {
                    // port is specified
                    _port = int.Parse(url.Substring(portStart + 1, hostPortEnd - (portStart + 1)));
                }
            }
            else
            {
                portStart = url.IndexOf(":", scanStart);

                // Isolate the host and port
                if (portStart < 0 || portStart > hostPortEnd)
                {
                    // no port is specified, we keep the default
                    Host = url.Substring(scanStart, hostPortEnd - scanStart);
                }
                else
                {
                    // port specified in URL
                    Host = url.Substring(scanStart, portStart - scanStart);
                    _port = int.Parse(url.Substring(portStart + 1, hostPortEnd - (portStart + 1)));
                }
            }

            scanStart = hostPortEnd + 1;
            if (scanStart >= scanEnd || dnStart < 0)
                return;

            // Parse out the base dn
            scanStart = dnStart + 1;
            var attrsStart = url.IndexOf('?', scanStart);

            _dn = attrsStart < 0
                ? url.Substring(scanStart, scanEnd - scanStart)
                : url.Substring(scanStart, attrsStart - scanStart);

            scanStart = attrsStart + 1;

            // Wierd novell syntax can have nothing beyond the dn
            if (scanStart >= scanEnd || attrsStart < 0 || novell)
                return;

            // Parse out the attributes
            var scopeStart = url.IndexOf('?', scanStart);
            if (scopeStart < 0)
                scopeStart = scanEnd - 1;
            AttributeArray = ParseList(url, ',', attrsStart + 1, scopeStart);
            scanStart = scopeStart + 1;
            if (scanStart >= scanEnd)
                return;

            // Parse out the scope
            var filterStart = url.IndexOf('?', scanStart);
            var scopeStr = filterStart < 0
                ? url.Substring(scanStart, scanEnd - scanStart)
                : url.Substring(scanStart, filterStart - scanStart);

            if (scopeStr.ToUpper().Equals(string.Empty.ToUpper()))
            {
                Scope = LdapConnection.ScopeBase;
            }
            else if (scopeStr.ToUpper().Equals("base".ToUpper()))
            {
                Scope = LdapConnection.ScopeBase;
            }
            else if (scopeStr.ToUpper().Equals("one".ToUpper()))
            {
                Scope = LdapConnection.ScopeOne;
            }
            else if (scopeStr.ToUpper().Equals("sub".ToUpper()))
            {
                Scope = LdapConnection.ScopeSub;
            }
            else
            {
                throw new UriFormatException("LdapUrl: URL invalid scope");
            }

            scanStart = filterStart + 1;
            if (scanStart >= scanEnd || filterStart < 0)
                return;

            // Parse out the filter
            scanStart = filterStart + 1;
            var extStart = url.IndexOf('?', scanStart);

            var filterStr = extStart < 0
                ? url.Substring(scanStart, scanEnd - scanStart)
                : url.Substring(scanStart, extStart - scanStart);

            if (!filterStr.Equals(string.Empty))
            {
                Filter = filterStr; // Only modify if not the default filter
            }

            scanStart = extStart + 1;
            if (scanStart >= scanEnd || extStart < 0)
                return;

            // Parse out the extensions
            var end = url.IndexOf('?', scanStart);
            if (end > 0)
                throw new UriFormatException("LdapUrl: URL has too many ? fields");

            Extensions = ParseList(url, ',', scanStart, scanEnd);
        }
    }

    /// <summary>
    ///     A message received from an LdapServer
    ///     in response to an asynchronous request.
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    public sealed class LdapResponse : LdapMessage
    {
        private readonly LdapException exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapResponse"/> class.
        /// Creates a response LdapMessage when receiving an asynchronous
        /// response from a server.
        /// </summary>
        /// <param name="message">The RfcLdapMessage from a server.</param>
        internal LdapResponse(RfcLdapMessage message)
            : base(message)
        {
        }

        /// <summary>
        ///     Returns any error message in the response.
        /// </summary>
        /// <returns>
        ///     Any error message in the response.
        /// </returns>
        public string ErrorMessage => exception != null
            ? exception.LdapErrorMessage
            : ((IRfcResponse) Message.Response).GetErrorMessage().StringValue();

        /// <summary>
        ///     Returns the partially matched DN field from the server response,
        ///     if the response contains one.
        /// </summary>
        /// <returns>
        ///     The partially matched DN field, if the response contains one.
        /// </returns>
        public string MatchedDN => exception != null
            ? exception.MatchedDN
            : ((IRfcResponse) Message.Response).GetMatchedDN().StringValue();

        /// <summary>
        ///     Returns all referrals in a server response, if the response contains any.
        /// </summary>
        /// <returns>
        ///     All the referrals in the server response.
        /// </returns>
        public string[] Referrals
        {
            get
            {
                string[] referrals;
                var reference = ((IRfcResponse) Message.Response).GetReferral();

                if (reference == null)
                {
                    referrals = new string[0];
                }
                else
                {
                    // convert RFC 2251 Referral to String[]
                    var size = reference.Size();
                    referrals = new string[size];
                    for (var i = 0; i < size; i++)
                    {
                        var aRef = ((Asn1OctetString) reference.Get(i)).StringValue();
                        try
                        {
                            // get the referral URL
                            var urlRef = new LdapUrl(aRef);
                            if (urlRef.GetDN() == null)
                            {
                                var origMsg = Asn1Object.RequestingMessage.Asn1Object;
                                string dn;
                                if ((dn = origMsg.RequestDn) != null)
                                {
                                    urlRef.SetDN(dn);
                                    aRef = urlRef.ToString();
                                }
                            }
                        }
                        catch (UriFormatException)
                        {
                            // Ignore
                        }
                        finally
                        {
                            referrals[i] = aRef;
                        }
                    }
                }

                return referrals;
            }
        }

        /// <summary>
        ///     Returns the result code in a server response.
        ///     For a list of result codes, see the LdapException class.
        /// </summary>
        /// <returns>
        ///     The result code.
        /// </returns>
        public LdapStatusCode ResultCode
        {
            get
            {
                if (exception != null)
                {
                    return exception.ResultCode;
                }

                if (Message.Response is RfcSearchResultEntry)
                    return LdapStatusCode.Success;

                if ((IRfcResponse) Message.Response is RfcIntermediateResponse)
                    return LdapStatusCode.Success;

                return (LdapStatusCode) ((IRfcResponse) Message.Response).GetResultCode().IntValue();
            }
        }

        /// <summary>
        ///     Checks the resultCode and generates the appropriate exception or
        ///     null if success.
        /// </summary>
        internal LdapException ResultException
        {
            get
            {
                LdapException ex = null;
                switch (ResultCode)
                {
                    case LdapStatusCode.Success:
                    case LdapStatusCode.CompareTrue:
                    case LdapStatusCode.CompareFalse:
                        break;
                    case LdapStatusCode.Referral:
                        var refs = Referrals;
                        ex = new LdapReferralException("Automatic referral following not enabled",
                            LdapStatusCode.Referral, ErrorMessage);
                        ((LdapReferralException) ex).SetReferrals(refs);
                        break;
                    default:
                        ex = new LdapException(ResultCode.ToString().Humanize(), ResultCode, ErrorMessage, MatchedDN);
                        break;
                }

                return ex;
            }
        }

        /// <summary>
        /// Returns any controls in the message.
        /// </summary>
        /// <value>
        /// The controls.
        /// </value>
        public override LdapControl[] Controls => exception != null ? null : base.Controls;

        /// <summary>
        ///     Returns an embedded exception response
        /// </summary>
        /// <returns>
        ///     an embedded exception if any
        /// </returns>
        internal LdapException Exception => exception;

        internal bool HasException() => exception != null;

        internal void ChkResultCode()
        {
            if (exception != null)
            {
                throw exception;
            }

            var ex = ResultException;
            if (ex != null)
            {
                throw ex;
            }
        }
    }

    /// <summary>
    /// The RespControlVector class implements extends the
    /// existing Vector class so that it can be used to maintain a
    /// list of currently registered control responses.
    /// </summary>
    /// <seealso cref="System.Collections.ArrayList" />
    internal sealed class RespControlVector : ArrayList
    {
        /// <summary>
        ///     Inner class defined to create a temporary object to encapsulate
        ///     all registration information about a response control.  This class
        ///     cannot be used outside this class
        /// </summary>
        private class RegisteredControl
        {
            public RegisteredControl(RespControlVector enclosingInstance, string oid, Type controlClass)
            {
                EnclosingInstance = enclosingInstance;
                MyOid = oid;
                MyClass = controlClass;
            }

            internal Type MyClass { get; }

            internal string MyOid { get; }

            private RespControlVector EnclosingInstance { get; }

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RespControlVector"/> class.
        /// </summary>
        /// <param name="cap">The cap.</param>
        public RespControlVector(int cap)
            : base(cap)
        {
        }

        /// <summary>
        /// Registers the response control.
        /// </summary>
        /// <param name="oid">The oid.</param>
        /// <param name="controlClass">The control class.</param>
        public void RegisterResponseControl(string oid, Type controlClass)
        {
            lock (this)
            {
                Add(new RegisteredControl(this, oid, controlClass));
            }
        }

        /// <summary>
        /// Finds the response control.
        /// </summary>
        /// <param name="searchOID">The search oid.</param>
        /// <returns></returns>
        /// <exception cref="FieldAccessException"></exception>
        public Type FindResponseControl(string searchOID)
        {
            lock (this)
            {
                RegisteredControl ctl;

                // loop through the contents of the vector
                for (var i = 0; i < Count; i++)
                {
                    // Get next registered control
                    if ((ctl = (RegisteredControl) ToArray()[i]) == null)
                    {
                        throw new FieldAccessException();
                    }

                    // Does the stored OID match with whate we are looking for
                    if (ctl.MyOid.CompareTo(searchOID) == 0)
                    {
                        // Return the class name if we have match
                        return ctl.MyClass;
                    }
                }

                return null;
            }
        }
    }
}
#endif