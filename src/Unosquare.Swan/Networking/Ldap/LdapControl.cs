#if !UWP
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
        /// Initializes the <see cref="LdapControl" /> class.
        /// </summary>
        static LdapControl()
        {
            RegisteredControls = new RespControlVector(5);
        }

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
                ? new RfcControl(new RfcLdapOID(oid), new Asn1Boolean(critical))
                : new RfcControl(new RfcLdapOID(oid), new Asn1Boolean(critical), new Asn1OctetString(values));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapControl"/> class.
        /// Create an LdapControl from an existing control.
        /// </summary>
        /// <param name="control">The control.</param>
        internal LdapControl(RfcControl control)
        {
            _control = control;
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

        internal static RespControlVector RegisteredControls { get; }

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
            LdapControl cont;
            try
            {
                cont = (LdapControl)MemberwiseClone();
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }

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

                cont._control = new RfcControl(new RfcLdapOID(ID), new Asn1Boolean(Critical), new Asn1OctetString(twin));
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
        public virtual sbyte[] GetValue()
        {
            sbyte[] result = null;
            var val = _control.ControlValue;
            if (val != null)
            {
                result = val.ByteValue();
            }

            return result;
        }

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
        /// <summary> Sets the protocol version</summary>
        public virtual Asn1Integer Version
        {
            get => (Asn1Integer)Get(0);
            set => Set(0, value);
        }

        public virtual RfcLdapDN Name
        {
            get => (RfcLdapDN)Get(1);
            set => Set(1, value);
        }

        public virtual RfcAuthenticationChoice AuthenticationChoice
        {
            get => (RfcAuthenticationChoice)Get(2);
            set => Set(2, value);
        }

        /// <summary>
        /// ID is added for Optimization.
        /// ID needs only be one Value for every instance,
        /// thus we create it only once.
        /// </summary>
        private static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.BIND_REQUEST);

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

        public RfcBindRequest(int version, string dn, string mechanism, sbyte[] credentials)
            : this(new Asn1Integer(version), new RfcLdapDN(dn), new RfcAuthenticationChoice(mechanism, credentials))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcBindRequest"/> class.
        /// Constructs a new Bind Request copying the original data from
        /// an existing request.
        /// </summary>
        /// <param name="origRequest">The original request.</param>
        /// <param name="base_Renamed">The base renamed.</param>
        internal RfcBindRequest(Asn1Object[] origRequest, string base_Renamed) 
            : base(origRequest, origRequest.Length)
        {
            // Replace the dn if specified, otherwise keep original base
            if (base_Renamed != null)
            {
                Set(1, new RfcLdapDN(base_Renamed));
            }
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
        
        public string GetRequestDN() => ((RfcLdapDN)Get(1)).StringValue();
    }

    /// <summary>
    /// Represents a simple bind request.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.LdapMessage" />
    public class LdapBindRequest : LdapMessage
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
        public LdapBindRequest(int version, string dn, sbyte[] passwd, LdapControl[] cont)
            : base(BIND_REQUEST, new RfcBindRequest(new Asn1Integer(version), new RfcLdapDN(dn), new RfcAuthenticationChoice(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, 0), new Asn1OctetString(passwd), false))), cont)
        {
        }

        /// <summary>
        ///     Retrieves the Authentication DN for a bind request.
        /// </summary>
        /// <returns>
        ///     the Authentication DN for a bind request
        /// </returns>
        public virtual string AuthenticationDN => Asn1Object.RequestDn;

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
    ///     Encapsulates a continuation reference from an asynchronous search operation.
    /// </summary>
    public class LdapSearchResultReference : LdapMessage
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
        public virtual string[] Referrals
        {
            get
            {
                var references = ((RfcSearchResultReference)message.Response).ToArray();
                srefs = new string[references.Length];
                for (var i = 0; i < references.Length; i++)
                {
                    srefs[i] = ((Asn1OctetString)references[i]).StringValue();
                }

                return srefs;
            }
        }
    }

    /// <summary>
    /// Defines the options controlling search operations.
    /// An LdapSearchConstraints object is always associated with an
    /// LdapConnection object; its values can be changed with the
    /// LdapConnection.setConstraints method, or overridden by passing
    /// an LdapSearchConstraints object to the search operation.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.LdapConstraints" />
    /// <seealso cref="LdapConstraints"></seealso>
    /// <seealso cref="LdapConnection.Constraints"></seealso>
    public class LdapSearchConstraints : LdapConstraints
    {
        private int dereference;
        private int serverTimeLimit;
        private int maxResults = 1000;
        private int batchSize = 1;

        /// <summary>
        ///     Indicates that aliases are never dereferenced.
        ///     DEREF_NEVER = 0
        /// </summary>
        /// <seealso cref="Dereference">
        /// </seealso>
        /// <seealso cref="Dereference">
        /// </seealso>
        public const int DEREF_NEVER = 0;

        /// <summary>
        ///     Indicates that aliases are are derefrenced when
        ///     searching the entries beneath the starting point of the search,
        ///     but not when finding the starting entry.
        ///     DEREF_SEARCHING = 1
        /// </summary>
        /// <seealso cref="Dereference">
        /// </seealso>
        /// <seealso cref="Dereference">
        /// </seealso>
        public const int DEREF_SEARCHING = 1;

        /// <summary>
        ///     Indicates that aliases are dereferenced when
        ///     finding the starting point for the search,
        ///     but not when searching under that starting entry.
        ///     DEREF_FINDING = 2
        /// </summary>
        /// <seealso cref="Dereference">
        /// </seealso>
        /// <seealso cref="Dereference">
        /// </seealso>
        public const int DEREF_FINDING = 2;

        /// <summary>
        ///     Indicates that aliases are always dereferenced, both when
        ///     finding the starting point for the search, and also when
        ///     searching the entries beneath the starting entry.
        ///     DEREF_ALWAYS = 3
        /// </summary>
        /// <seealso cref="Dereference">
        /// </seealso>
        /// <seealso cref="Dereference">
        /// </seealso>
        public const int DEREF_ALWAYS = 3;

        /// <summary>
        /// Constructs an LdapSearchConstraints object with a default set
        /// of search constraints.
        /// </summary>
        public LdapSearchConstraints()
        {
            InitBlock();
        }

        /// <summary>
        /// Constructs an LdapSearchConstraints object initialized with values
        /// from an existing constraints object (LdapConstraints
        /// or LdapSearchConstraints).
        /// </summary>
        /// <param name="cons">The cons.</param>
        public LdapSearchConstraints(LdapConstraints cons)
            : base(cons.TimeLimit, cons.ReferralFollowing, cons.GetReferralHandler(), cons.HopLimit)
        {
            InitBlock();
            var lsc = cons.GetControls();
            if (lsc != null)
            {
                var generated_var = new LdapControl[lsc.Length];
                lsc.CopyTo(generated_var, 0);
                SetControls(generated_var);
            }

            var lp = cons.Properties;
            if (lp != null)
            {
                Properties = (Hashtable) lp.Clone();
            }

            if (cons is LdapSearchConstraints scons)
            {
                serverTimeLimit = scons.ServerTimeLimit;
                dereference = scons.Dereference;
                maxResults = scons.MaxResults;
                batchSize = scons.BatchSize;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapSearchConstraints"/> class.
        /// Constructs a new LdapSearchConstraints object and allows the
        /// specification operational constraints in that object.
        /// </summary>
        /// <param name="msLimit">The maximum time in milliseconds to wait for results.
        /// The default is 0, which means that there is no
        /// maximum time limit. This limit is enforced for an
        /// operation by the API, not by the server.
        /// The operation will be abandoned and terminated by the
        /// API with an LdapException.Ldap_TIMEOUT if the
        /// operation exceeds the time limit.</param>
        /// <param name="serverTimeLimit">The maximum time in seconds that the server
        /// should spend returning search results. This is a
        /// server-enforced limit.  The default of 0 means
        /// no time limit.
        /// The operation will be terminated by the server with an
        /// LdapException.TIME_LIMIT_EXCEEDED if the search
        /// operation exceeds the time limit.</param>
        /// <param name="dereference">Specifies when aliases should be dereferenced.
        /// Must be either DEREF_NEVER, DEREF_FINDING,
        /// DEREF_SEARCHING, or DEREF_ALWAYS from this class.
        /// Default: DEREF_NEVER</param>
        /// <param name="maxResults">The maximum number of search results to return
        /// for a search request.
        /// The search operation will be terminated by the server
        /// with an LdapException.SIZE_LIMIT_EXCEEDED if the
        /// number of results exceed the maximum.
        /// Default: 1000</param>
        /// <param name="doReferrals">Determines whether to automatically follow
        /// referrals or not. Specify true to follow
        /// referrals automatically, and false to throw
        /// an LdapException.REFERRAL if the server responds
        /// with a referral.
        /// It is ignored for asynchronous operations.
        /// Default: false</param>
        /// <param name="batchSize">The number of results to return in a batch. Specifying
        /// 0 means to block until all results are received.
        /// Specifying 1 means to return results one result at a
        /// time.  Default: 1</param>
        /// <param name="handler">The custom authentication handler called when
        /// LdapConnection needs to authenticate, typically on
        /// following a referral.  A null may be specified to
        /// indicate default authentication processing, i.e.
        /// referrals are followed with anonymous authentication.
        /// ThE object may be an implemention of either the
        /// the LdapBindHandler or LdapAuthHandler interface.
        /// It is ignored for asynchronous operations.</param>
        /// <param name="hopLimit">The maximum number of referrals to follow in a
        /// sequence during automatic referral following.
        /// The default value is 10. A value of 0 means no limit.
        /// It is ignored for asynchronous operations.
        /// The operation will be abandoned and terminated by the
        /// API with an LdapException.REFERRAL_LIMIT_EXCEEDED if the
        /// number of referrals in a sequence exceeds the limit.</param>
        /// <seealso cref="LdapException.Ldap_TIMEOUT"></seealso>
        /// <seealso cref="LdapException.REFERRAL"></seealso>
        /// <seealso cref="LdapException.SIZE_LIMIT_EXCEEDED"></seealso>
        /// <seealso cref="LdapException.TIME_LIMIT_EXCEEDED"></seealso>
        public LdapSearchConstraints(int msLimit, int serverTimeLimit, int dereference, int maxResults, bool doReferrals, int batchSize, ILdapReferralHandler handler, int hopLimit) 
            : base(msLimit, doReferrals, handler, hopLimit)
        {
            InitBlock();
            this.serverTimeLimit = serverTimeLimit;
            this.dereference = dereference;
            this.maxResults = maxResults;
            this.batchSize = batchSize;
        }

        /// <summary>
        /// Returns the number of results to block on during receipt of search
        /// results.
        /// This should be 0 if intermediate reults are not needed,
        /// and 1 if results are to be processed as they come in. A value of
        /// indicates block until all results are received.  Default:
        /// </summary>
        /// <value>
        /// The size of the batch.
        /// </value>
        public virtual int BatchSize
        {
            get => batchSize;
            set => batchSize = value;
        }

        /// <summary>
        ///     Specifies when aliases should be dereferenced.
        ///     Returns one of the following:
        ///     <ul>
        ///         <li>DEREF_NEVER</li>
        ///         <li>DEREF_FINDING</li>
        ///         <li>DEREF_SEARCHING</li>
        ///         <li>DEREF_ALWAYS</li>
        ///     </ul>
        /// </summary>
        /// <returns>
        ///     The setting for dereferencing aliases.
        /// </returns>
        public virtual int Dereference
        {
            get => dereference;
            set => dereference = value;
        }

        /// <summary>
        ///     Returns the maximum number of search results to be returned for
        ///     a search operation. A value of 0 means no limit.  Default: 1000
        ///     The search operation will be terminated with an
        ///     LdapException.SIZE_LIMIT_EXCEEDED if the number of results
        ///     exceed the maximum.
        /// </summary>
        /// <returns>
        ///     Maximum number of search results to return.
        /// </returns>
        /// <seealso cref="LdapException.SIZE_LIMIT_EXCEEDED">
        /// </seealso>
        public virtual int MaxResults
        {
            get => maxResults;
            set => maxResults = value;
        }

        /// <summary>
        ///     Returns the maximum number of seconds that the server waits when
        ///     returning search results.
        ///     The search operation will be terminated with an
        ///     LdapException.TIME_LIMIT_EXCEEDED if the operation exceeds the time
        ///     limit.
        /// </summary>
        /// <returns>
        ///     The maximum number of seconds the server waits for search'
        ///     results.
        /// </returns>
        /// <seealso cref="LdapException.TIME_LIMIT_EXCEEDED">
        /// </seealso>
        public virtual int ServerTimeLimit
        {
            get => serverTimeLimit;
            set => serverTimeLimit = value;
        }

        private void InitBlock()
        {
            dereference = DEREF_NEVER;
        }
    }

    /// <summary>
    ///     This class encapsulates the combination of LdapReferral URL and
    ///     the connection opened to service this URL
    /// </summary>
    public class ReferralInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferralInfo"/> class.
        /// Construct the ReferralInfo class
        /// </summary>
        /// <param name="lc">The DirectoryEntry opened to process this referral</param>
        /// <param name="refList">The reference list.</param>
        /// <param name="refUrl">The URL string associated with this connection</param>
        public ReferralInfo(LdapConnection lc, string[] refList, LdapUrl refUrl)
        {
            ReferralConnection = lc;
            ReferralUrl = refUrl;
            ReferralList = refList;
        }

        /// <summary>
        ///     Returns the referral URL
        /// </summary>
        /// <returns>
        ///     the Referral URL
        /// </returns>
        public virtual LdapUrl ReferralUrl { get; }

        /// <summary>
        ///     Returns the referral Connection
        /// </summary>
        /// <returns>
        ///     the Referral Connection
        /// </returns>
        public virtual LdapConnection ReferralConnection { get; }

        /// <summary>
        ///     Returns the referral list
        /// </summary>
        /// <returns>
        ///     the Referral list
        /// </returns>
        public virtual string[] ReferralList { get; }
    }

    /// <summary>
    /// Encapsulates parameters of an Ldap URL query as defined in RFC2255.
    /// An LdapUrl object can be passed to LdapConnection.search to retrieve
    /// search results.
    /// </summary>
    /// <seealso cref="LdapConnection.Search"></seealso>
    public class LdapUrl
    {
        private void InitBlock()
        {
            scope = DEFAULT_SCOPE;
        }

        private static readonly int DEFAULT_SCOPE = LdapConnection.ScopeBase;

        // Broken out parts of the URL
        private bool secure; // URL scheme ldap/ldaps
        private readonly bool ipV6 = false; // TCP/IP V6
        private string host; // Host
        private int port; // Port
        private string dn; // Base DN
        private string[] attrs; // Attributes
        private string filter; // Filter
        private int scope; // Scope
        private string[] extensions; // Extensions

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapUrl"/> class.
        /// Constructs a URL object with the specified string as the URL.
        /// </summary>
        /// <param name="url">An Ldap URL string, e.g.
        /// "ldap://ldap.example.com:80/dc=example,dc=com?cn,
        /// sn?sub?(objectclass=inetOrgPerson)".</param>
        public LdapUrl(string url)
        {
            InitBlock();
            ParseUrl(url);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapUrl"/> class.
        /// Constructs a URL object with the specified host, port, and DN.
        /// This form is used to create URL references to a particular object
        /// in the directory.
        /// </summary>
        /// <param name="host">Host identifier of Ldap server, or null for
        /// "localhost".</param>
        /// <param name="port">The port number for Ldap server (use
        /// LdapConnection.DEFAULT_PORT for default port).</param>
        /// <param name="dn">Distinguished name of the base object of the search.</param>
        public LdapUrl(string host, int port, string dn)
        {
            InitBlock();
            this.host = host;
            this.port = port;
            this.dn = dn;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapUrl"/> class.
        /// Constructs an Ldap URL with all fields explicitly assigned, including
        /// isSecure, to specify an Ldap search operation.
        /// </summary>
        /// <param name="host">Host identifier of Ldap server, or null for
        /// "localhost".</param>
        /// <param name="port">The port number for Ldap server (use
        /// LdapConnection.DEFAULT_PORT for default port).</param>
        /// <param name="dn">Distinguished name of the base object of the search.</param>
        /// <param name="attrNames">Names or OIDs of attributes to retrieve.  Passing a
        /// null array signifies that all user attributes are to be
        /// retrieved. Passing a value of "*" allows you to specify
        /// that all user attributes as well as any specified
        /// operational attributes are to be retrieved.</param>
        /// <param name="scope">Depth of search (in DN namespace). Use one of
        /// SCOPE_BASE, SCOPE_ONE, SCOPE_SUB from LdapConnection.</param>
        /// <param name="filter">The search filter specifying the search criteria.
        /// from LdapConnection: SCOPE_BASE, SCOPE_ONE, SCOPE_SUB.</param>
        /// <param name="extensions">Extensions provide a mechanism to extend the
        /// functionality of Ldap URLs. Currently no
        /// Ldap URL extensions are defined. Each extension
        /// specification is a type=value expression, and  may
        /// be <code>null</code> or empty.  The =value part may be
        /// omitted. The expression may be prefixed with '!' if it
        /// is mandatory for the evaluation of the URL.</param>
        /// <param name="secure">If true creates an Ldap URL of the ldaps type</param>
        public LdapUrl(string host, int port, string dn, string[] attrNames, int scope, string filter, string[] extensions, bool secure = false)
        {
            InitBlock();
            this.host = host;
            this.port = port;
            this.dn = dn;
            attrs = attrNames;
            this.scope = scope;
            this.filter = filter;
            this.extensions = new string[extensions.Length];
            extensions.CopyTo(this.extensions, 0);
            this.secure = secure;
        }

        /// <summary>
        /// Returns an array of attribute names specified in the URL.
        /// </summary>
        /// <value>
        /// The attribute array.
        /// </value>
        public virtual string[] AttributeArray => attrs;

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
        public virtual string[] Extensions => extensions;

        /// <summary>
        ///     Returns the search filter or <code>null</code> if none was specified.
        /// </summary>
        /// <returns>
        ///     The search filter.
        /// </returns>
        public virtual string Filter => filter;

        /// <summary>
        ///     Returns the name of the Ldap server in the URL.
        /// </summary>
        /// <returns>
        ///     The host name specified in the URL.
        /// </returns>
        public virtual string Host => host;

        /// <summary>
        ///     Returns the port number of the Ldap server in the URL.
        /// </summary>
        /// <returns>
        ///     The port number in the URL.
        /// </returns>
        public virtual int Port => port == 0 ? LdapConnection.DefaultPort : port;

        /// <summary>
        ///     Returns the depth of search. It returns one of the following from
        ///     LdapConnection: SCOPE_BASE, SCOPE_ONE, or SCOPE_SUB.
        /// </summary>
        /// <returns>
        ///     The search scope.
        /// </returns>
        public virtual int Scope => scope;

        /// <summary>
        ///     Returns true if the URL is of the type ldaps (Ldap over SSL, a predecessor
        ///     to startTls)
        /// </summary>
        /// <returns>
        ///     whether this is a secure Ldap url or not.
        /// </returns>
        public virtual bool Secure => secure;

        /// <summary>
        ///     Returns a clone of this URL object.
        /// </summary>
        /// <returns>
        ///     clone of this URL object.
        /// </returns>
        public object Clone()
        {
            try
            {
                return MemberwiseClone();
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }

        /// <summary>
        /// Decodes a URL-encoded string.
        /// Any occurences of %HH are decoded to the hex value represented.
        /// However, this method does NOT decode "+" into " ".
        /// </summary>
        /// <param name="urlEncoded">String to decode.</param>
        /// <returns>
        /// The decoded string.
        /// </returns>
        /// <exception cref="UriFormatException">LdapUrl.decode: must be two hex characters following escape character '%'
        /// or
        /// LdapUrl.decode: error converting hex characters to integer \"" +
        /// ex.Message + "\"</exception>
        public static string Decode(string urlEncoded)
        {
            var searchStart = 0;
            var fieldStart = urlEncoded.IndexOf("%", searchStart);

            // Return now if no encoded data
            if (fieldStart < 0)
            {
                return urlEncoded;
            }

            // Decode the %HH value and copy to new string buffer
            var fieldEnd = 0; // end of previous field
            var dataLen = urlEncoded.Length;
            var decoded = new StringBuilder(dataLen);
            while (true)
            {
                if (fieldStart > dataLen - 3)
                {
                    throw new UriFormatException(
                        "LdapUrl.decode: must be two hex characters following escape character '%'");
                }

                if (fieldStart < 0)
                    fieldStart = dataLen;

                // Copy to string buffer from end of last field to start of next
                decoded.Append(urlEncoded.Substring(fieldEnd, fieldStart - fieldEnd));
                fieldStart += 1;
                if (fieldStart >= dataLen)
                    break;
                fieldEnd = fieldStart + 2;
                try
                {
                    decoded.Append((char) Convert.ToInt32(urlEncoded.Substring(fieldStart, fieldEnd - fieldStart), 16));
                }
                catch (FormatException ex)
                {
                    throw new UriFormatException("LdapUrl.decode: error converting hex characters to integer \"" +
                                                 ex.Message + "\"");
                }

                searchStart = fieldEnd;
                if (searchStart == dataLen)
                    break;
                fieldStart = urlEncoded.IndexOf("%", searchStart);
            }

            return decoded.ToString();
        }

        /// <summary>
        /// Encodes an arbitrary string using the URL encoding rules.
        /// Any illegal characters are encoded as %HH.
        /// </summary>
        /// <param name="toEncode">The string to encode.</param>
        /// <returns>
        /// The URL-encoded string.
        /// Comment: An illegal character consists of any non graphical US-ASCII character, Unsafe, or reserved characters.
        /// </returns>
        public static string Encode(string toEncode)
        {
            var buffer = new StringBuilder(toEncode.Length);
            string temp;
            char currChar;

            for (var i = 0; i < toEncode.Length; i++)
            {
                currChar = toEncode[i];
                if (currChar <= 0x1F || currChar == 0x7F || currChar >= 0x80 && currChar <= 0xFF || currChar == '<' ||
                    currChar == '>' || currChar == '\"' || currChar == '#' || currChar == '%' || currChar == '{' ||
                    currChar == '}' || currChar == '|' || currChar == '\\' || currChar == '^' || currChar == '~' ||
                    currChar == '[' || currChar == '\'' || currChar == ';' || currChar == '/' || currChar == '?' ||
                    currChar == ':' || currChar == '@' || currChar == '=' || currChar == '&')
                {
                    temp = Convert.ToString(currChar, 16);
                    if (temp.Length == 1)
                        buffer.Append("%0" + temp);
                    else
                        buffer.Append("%" + Convert.ToString(currChar, 16));
                }
                else
                {
                    buffer.Append(currChar);
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        ///     Returns the base distinguished name encapsulated in the URL.
        /// </summary>
        /// <returns>
        ///     The base distinguished name specified in the URL, or null if none.
        /// </returns>
        public virtual string GetDN() => dn;

        /// <summary> Sets the base distinguished name encapsulated in the URL.</summary>
        internal virtual void SetDN(string dn) => this.dn = dn;

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
            url.Append(secure ? "ldaps://" : "ldap://");

            // Host:port/dn
            url.Append(ipV6 ? $"[{host}]" : host);

            // Port not specified
            if (port != 0)
            {
                url.Append(":" + port);
            }

            if (dn == null && attrs == null && scope == DEFAULT_SCOPE && filter == null && extensions == null)
            {
                return url.ToString();
            }

            url.Append("/");
            if (dn != null)
            {
                url.Append(dn);
            }

            if (attrs == null && scope == DEFAULT_SCOPE && filter == null && extensions == null)
            {
                return url.ToString();
            }

            // attributes
            url.Append("?");
            if (attrs != null)
            {
                // should we check also for attrs != "*"
                for (var i = 0; i < attrs.Length; i++)
                {
                    url.Append(attrs[i]);
                    if (i < attrs.Length - 1)
                    {
                        url.Append(",");
                    }
                }
            }

            if (scope == DEFAULT_SCOPE && filter == null && extensions == null)
            {
                return url.ToString();
            }

            // scope
            url.Append("?");
            if (scope != DEFAULT_SCOPE)
            {
                url.Append(scope == LdapConnection.ScopeOne ? "one" : "sub");
            }

            if (filter == null && extensions == null)
            {
                return url.ToString();
            }

            // filter
            url.Append(filter == null ? "?" : $"?{Filter}");

            if (extensions == null)
            {
                return url.ToString();
            }

            // extensions
            url.Append("?");
            if (extensions != null)
            {
                for (var i = 0; i < extensions.Length; i++)
                {
                    url.Append(extensions[i]);
                    if (i < extensions.Length - 1)
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
                port = LdapConnection.DefaultPort;
            }
            else if (url.Substring(scanStart, scanStart + 8 - scanStart).ToUpper().Equals("ldaps://".ToUpper()))
            {
                secure = true;
                scanStart += 8;
                port = LdapConnection.DefaultSslPort;
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
                host = url.Substring(scanStart + 1, hostEnd - (scanStart + 1));
                portStart = url.IndexOf(":", hostEnd);
                if (portStart < hostPortEnd && portStart != -1)
                {
                    // port is specified
                    port = int.Parse(url.Substring(portStart + 1, hostPortEnd - (portStart + 1)));
                }
            }
            else
            {
                portStart = url.IndexOf(":", scanStart);

                // Isolate the host and port
                if (portStart < 0 || portStart > hostPortEnd)
                {
                    // no port is specified, we keep the default
                    host = url.Substring(scanStart, hostPortEnd - scanStart);
                }
                else
                {
                    // port specified in URL
                    host = url.Substring(scanStart, portStart - scanStart);
                    port = int.Parse(url.Substring(portStart + 1, hostPortEnd - (portStart + 1)));
                }
            }

            scanStart = hostPortEnd + 1;
            if (scanStart >= scanEnd || dnStart < 0)
                return;

            // Parse out the base dn
            scanStart = dnStart + 1;
            var attrsStart = url.IndexOf('?', scanStart);

            dn = attrsStart < 0 ? url.Substring(scanStart, scanEnd - scanStart) : url.Substring(scanStart, attrsStart - scanStart);

            scanStart = attrsStart + 1;

            // Wierd novell syntax can have nothing beyond the dn
            if (scanStart >= scanEnd || attrsStart < 0 || novell)
                return;

            // Parse out the attributes
            var scopeStart = url.IndexOf('?', scanStart);
            if (scopeStart < 0)
                scopeStart = scanEnd - 1;
            attrs = ParseList(url, ',', attrsStart + 1, scopeStart);
            scanStart = scopeStart + 1;
            if (scanStart >= scanEnd)
                return;

            // Parse out the scope
            var filterStart = url.IndexOf('?', scanStart);
            string scopeStr;
            scopeStr = filterStart < 0 ? url.Substring(scanStart, scanEnd - scanStart) : url.Substring(scanStart, filterStart - scanStart);

            if (scopeStr.ToUpper().Equals(string.Empty.ToUpper()))
            {
                scope = LdapConnection.ScopeBase;
            }
            else if (scopeStr.ToUpper().Equals("base".ToUpper()))
            {
                scope = LdapConnection.ScopeBase;
            }
            else if (scopeStr.ToUpper().Equals("one".ToUpper()))
            {
                scope = LdapConnection.ScopeOne;
            }
            else if (scopeStr.ToUpper().Equals("sub".ToUpper()))
            {
                scope = LdapConnection.ScopeSub;
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

            var filterStr = extStart < 0 ? url.Substring(scanStart, scanEnd - scanStart) : url.Substring(scanStart, extStart - scanStart);

            if (!filterStr.Equals(string.Empty))
            {
                filter = filterStr; // Only modify if not the default filter
            }

            scanStart = extStart + 1;
            if (scanStart >= scanEnd || extStart < 0)
                return;

            // Parse out the extensions
            var end = url.IndexOf('?', scanStart);
            if (end > 0)
                throw new UriFormatException("LdapUrl: URL has too many ? fields");

            extensions = ParseList(url, ',', scanStart, scanEnd);
        }
    }

    /// <summary>
    ///     A message received from an LdapServer
    ///     in response to an asynchronous request.
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    public class LdapResponse : LdapMessage
    {
        /// <summary>
        ///     Returns any error message in the response.
        /// </summary>
        /// <returns>
        ///     Any error message in the response.
        /// </returns>
        public virtual string ErrorMessage => exception != null ? exception.LdapErrorMessage : ((IRfcResponse) message.Response).GetErrorMessage().StringValue();

        /// <summary>
        ///     Returns the partially matched DN field from the server response,
        ///     if the response contains one.
        /// </summary>
        /// <returns>
        ///     The partially matched DN field, if the response contains one.
        /// </returns>
        public virtual string MatchedDN => exception != null ? exception.MatchedDN : ((IRfcResponse) message.Response).GetMatchedDN().StringValue();

        /// <summary>
        ///     Returns all referrals in a server response, if the response contains any.
        /// </summary>
        /// <returns>
        ///     All the referrals in the server response.
        /// </returns>
        public virtual string[] Referrals
        {
            get
            {
                string[] referrals;
                var reference = ((IRfcResponse) message.Response).GetReferral();

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
        public virtual int ResultCode
        {
            get
            {
                if (exception != null)
                {
                    return exception.ResultCode;
                }

                if ((IRfcResponse) message.Response is RfcIntermediateResponse)
                    return 0;
                return ((IRfcResponse) message.Response).GetResultCode().IntValue();
            }
        }

        /// <summary>
        ///     Checks the resultCode and generates the appropriate exception or
        ///     null if success.
        /// </summary>
        internal virtual LdapException ResultException
        {
            get
            {
                LdapException ex = null;
                switch (ResultCode)
                {
                    case LdapException.SUCCESS:
                    case LdapException.COMPARE_TRUE:
                    case LdapException.COMPARE_FALSE:
                        break;
                    case LdapException.REFERRAL:
                        var refs = Referrals;
                        ex = new LdapReferralException("Automatic referral following not enabled", LdapException.REFERRAL, ErrorMessage);
                        ((LdapReferralException) ex).SetReferrals(refs);
                        break;
                    default:
                        ex = new LdapException(LdapException.resultCodeToString(ResultCode), ResultCode, ErrorMessage, MatchedDN);
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
        internal virtual LdapException Exception => exception;

        /// <summary>
        ///     Indicates the referral instance being followed if the
        ///     connection created to follow referrals.
        /// </summary>
        /// <returns>
        ///     the referral being followed
        /// </returns>
        internal virtual ReferralInfo ActiveReferral => activeReferral;

        private readonly LdapException exception;
        private readonly ReferralInfo activeReferral;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapResponse"/> class.
        /// Creates an LdapResponse using an LdapException.
        /// Used to wake up the user following an abandon.
        /// Note: The abandon doesn't have to be user initiated
        /// but may be the result of error conditions.
        /// Referral information is available if this connection created solely
        /// to follow a referral.
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <param name="activeReferral">The referral actually used to create the
        /// connection</param>
        public LdapResponse(LdapException ex, ReferralInfo activeReferral)
        {
            exception = ex;
            this.activeReferral = activeReferral;
        }

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
        /// Initializes a new instance of the <see cref="LdapResponse"/> class.
        /// Creates a SUCCESS response LdapMessage. Typically the response
        /// comes from a source other than a BER encoded Ldap message,
        /// such as from DSML.  Other values which are allowed in a response
        /// are set to their empty values.
        /// </summary>
        /// <param name="type">The message type as defined in LdapMessage.</param>
        /// <seealso cref="LdapMessage"></seealso>
        public LdapResponse(int type) 
            : this(type, LdapException.SUCCESS)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapResponse"/> class.
        /// Creates a response LdapMessage from parameters. Typically the data
        /// comes from a source other than a BER encoded Ldap message,
        /// such as from DSML.
        /// </summary>
        /// <param name="type">The message type as defined in LdapMessage.</param>
        /// <param name="resultCode">The result code as defined in LdapException.</param>
        /// <param name="matchedDN">The name of the lowest entry that was matched
        /// for some error result codes, an empty string
        /// or <code>null</code> if none.</param>
        /// <param name="serverMessage">A diagnostic message returned by the server,
        /// an empty string or <code>null</code> if none.</param>
        /// <seealso cref="LdapMessage"></seealso>
        /// <seealso cref="LdapException"></seealso>
        public LdapResponse(int type, int resultCode, string matchedDN = null, string serverMessage = null)
            : base(new RfcLdapMessage(RfcResultFactory(type, resultCode, matchedDN, serverMessage)))
        {
        }

        private static Asn1Sequence RfcResultFactory(int type, int resultCode, string matchedDN, string serverMessage)
        {
            Asn1Sequence ret;
            if (matchedDN == null)
                matchedDN = string.Empty;
            if (serverMessage == null)
                serverMessage = string.Empty;
            switch (type)
            {
                case SEARCH_RESULT:
                    ret = new RfcSearchResultDone(new Asn1Enumerated(resultCode), new RfcLdapDN(matchedDN), new RfcLdapString(serverMessage), null);
                    break;
                case BIND_RESPONSE:
                    ret = null; // Not yet implemented
                    break;
                case SEARCH_RESPONSE:
                    ret = null; // Not yet implemented
                    break;
                case SEARCH_RESULT_REFERENCE:
                    ret = null; // Not yet implemented
                    break;
                case EXTENDED_RESPONSE:
                    ret = null; // Not yet implemented
                    break;
                default:
                    throw new Exception("Type " + type + " Not Supported");
            }

            return ret;
        }

        /// <summary>
        ///     Checks the resultCode and throws the appropriate exception.
        /// </summary>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        internal virtual void ChkResultCode()
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
        
        /// <summary>
        ///     Indicates if this response is an embedded exception response
        /// </summary>
        /// <returns>
        ///     true if contains an embedded Ldapexception
        /// </returns>
        internal virtual bool HasException() =>exception != null;
    }

    /// <summary>
    ///     The <code>MessageVector</code> class implements extends the
    ///     existing Vector class so that it can be used to maintain a
    ///     list of currently registered control responses.
    /// </summary>
    public class RespControlVector : ArrayList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RespControlVector"/> class.
        /// </summary>
        /// <param name="cap">The cap.</param>
        public RespControlVector(int cap) 
            : base(cap)
        {
        }

        /// <summary>
        ///     Inner class defined to create a temporary object to encapsulate
        ///     all registration information about a response control.  This class
        ///     cannot be used outside this class
        /// </summary>
        private class RegisteredControl
        {
            public readonly string myOID;
            public readonly Type myClass;

            public RegisteredControl(RespControlVector enclosingInstance, string oid, Type controlClass)
            {
                Enclosing_Instance = enclosingInstance;
                myOID = oid;
                myClass = controlClass;
            }

            private RespControlVector Enclosing_Instance { get; }

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
                    /* Get next registered control */
                    if ((ctl = (RegisteredControl) ToArray()[i]) == null)
                    {
                        throw new FieldAccessException();
                    }

                    /* Does the stored OID match with whate we are looking for */
                    if (ctl.myOID.CompareTo(searchOID) == 0)
                    {
                        /* Return the class name if we have match */
                        return ctl.myClass;
                    }
                }

                /* The requested control does not have a registered response class */
                return null;
            }
        }
    }
}

#endif