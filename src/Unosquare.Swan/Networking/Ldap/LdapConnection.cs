#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     The central class that encapsulates the connection
    ///     to a directory server through the Ldap protocol.
    ///     LdapConnection objects are used to perform common Ldap
    ///     operations such as search, modify and add.
    ///     In addition, LdapConnection objects allow you to bind to an
    ///     Ldap server, set connection and search constraints, and perform
    ///     several other tasks.
    ///     An LdapConnection object is not connected on
    ///     construction and can only be connected to one server at one
    ///     port. Multiple threads may share this single connection, typically
    ///     by cloning the connection object, one for each thread. An
    ///     application may have more than one LdapConnection object, connected
    ///     to the same or different directory servers.
    ///     Base on https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard
    /// </summary>
    public class LdapConnection
    {

        private CancellationTokenSource cts = new CancellationTokenSource();

        internal BindProperties BindProperties { get; set; }

        /// <summary>
        /// Returns the protocol version uses to authenticate.
        /// 0 is returned if no authentication has been performed.
        /// </summary>
        /// <value>
        /// The protocol version.
        /// </value>
        public virtual int ProtocolVersion => BindProperties?.ProtocolVersion ?? Ldap_V3;

        /// <summary>
        ///     Returns the distinguished name (DN) used for as the bind name during
        ///     the last successful bind operation.  <code>null</code> is returned
        ///     if no authentication has been performed or if the bind resulted in
        ///     an aonymous connection.
        /// </summary>
        /// <returns>
        ///     The distinguished name if authenticated; otherwise, null.
        /// </returns>
        public virtual string AuthenticationDN => BindProperties == null ? null : (BindProperties.Anonymous ? null : BindProperties.AuthenticationDN);

        /// <summary>
        ///     Returns the method used to authenticate the connection. The return
        ///     value is one of the following:
        ///     <ul>
        ///         <li>"none" indicates the connection is not authenticated.</li>
        ///         <li>
        ///             "simple" indicates simple authentication was used or that a null
        ///             or empty authentication DN was specified.
        ///         </li>
        ///         <li>"sasl" indicates that a SASL mechanism was used to authenticate</li>
        ///     </ul>
        /// </summary>
        /// <returns>
        ///     The method used to authenticate the connection.
        /// </returns>
        public virtual string AuthenticationMethod
            => BindProperties == null ? "simple" : BindProperties.AuthenticationMethod;

        /// <summary>
        ///     Returns the properties if any specified on binding with a
        ///     SASL mechanism.
        ///     Null is returned if no authentication has been performed
        ///     or no authentication Map is present.
        /// </summary>
        /// <returns>
        ///     The bind properties Map Object used for SASL bind or null if
        ///     the connection is not present or not authenticated.
        /// </returns>
        public virtual IDictionary SaslBindProperties => BindProperties?.SaslBindProperties;

        /// <summary>
        ///     Returns the call back handler if any specified on binding with a
        ///     SASL mechanism.
        ///     Null is returned if no authentication has been performed
        ///     or no authentication call back handler is present.
        /// </summary>
        /// <returns>
        ///     The call back handler used for SASL bind or null if the
        ///     object is not present or not authenticated.
        /// </returns>
        public virtual object SaslBindCallbackHandler => BindProperties?.SaslCallbackHandler;

        /// <summary>
        /// Returns a copy of the set of constraints associated with this
        /// connection. These constraints apply to all operations performed
        /// through this connection (unless a different set of constraints is
        /// specified when calling an operation method).
        /// </summary>
        /// <value>
        /// The constraints.
        /// </value>
        /// 
        /// <seealso cref="Constraints()"></seealso>
        /// <seealso cref="SearchConstraints()"></seealso>
        public virtual LdapConstraints Constraints
        {
            get
            {
                return (LdapConstraints) defSearchCons.Clone();
            }

            set
            {
                // Set all constraints, replace the object with a new one
                if (value is LdapSearchConstraints)
                {
                    defSearchCons = (LdapSearchConstraints) value.Clone();
                    return;
                }

                // We set the constraints this way, so a thread doesn't get an
                // conconsistant view of the referrals.
                var newCons = (LdapSearchConstraints) defSearchCons.Clone();
                newCons.HopLimit = value.HopLimit;
                newCons.TimeLimit = value.TimeLimit;
                newCons.SetReferralHandler(value.GetReferralHandler());
                newCons.ReferralFollowing = value.ReferralFollowing;
                var lsc = value.GetControls();
                if (lsc != null)
                {
                    newCons.SetControls(lsc);
                }

                var lp = newCons.Properties;
                if (lp != null)
                {
                    newCons.Properties = lp;
                }

                defSearchCons = newCons;
            }
        }
        
        /// <summary>
        /// Returns a copy of the set of search constraints associated with this
        /// connection. These constraints apply to search operations performed
        /// through this connection (unless a different set of
        /// constraints is specified when calling the search operation method).
        /// </summary>
        /// <value>
        /// The search constraints.
        /// </value>
        /// <seealso cref="Constraints"></seealso>
        /// <seealso cref="LdapSearchConstraints"></seealso>
        public virtual LdapSearchConstraints SearchConstraints => (LdapSearchConstraints) defSearchCons.Clone();
        
        /// <summary>
        ///     Indicates whether the connection represented by this object is open
        ///     at this time.
        /// </summary>
        /// <returns>
        ///     True if connection is open; false if the connection is closed.
        /// </returns>
        public virtual bool Connected => conn?.IsConnected == true;
        
        /// <summary>
        ///     Returns the Server Controls associated with the most recent response
        ///     to a synchronous request on this connection object, or null
        ///     if the latest response contained no Server Controls. The method
        ///     always returns null for asynchronous requests. For asynchronous
        ///     requests, the response controls are available in LdapMessage.
        /// </summary>
        /// <returns>
        ///     The server controls associated with the most recent response
        ///     to a synchronous request or null if the response contains no server
        ///     controls.
        /// </returns>
        /// <seealso cref="LdapMessage.Controls">
        /// </seealso>
        public virtual LdapControl[] ResponseControls
        {
            get
            {
                if (responseCtls == null)
                {
                    return null;
                }

                // We have to clone the control just in case
                // we have two client threads that end up retreiving the
                // same control.
                var clonedControl = new LdapControl[responseCtls.Length];

                // Also note we synchronize access to the local response
                // control object just in case another message containing controls
                // comes in from the server while we are busy duplicating
                // this one.
                lock (responseCtlSemaphore)
                {
                    for (var i = 0; i < responseCtls.Length; i++)
                    {
                        clonedControl[i] = (LdapControl) responseCtls[i].Clone();
                    }
                }

                // Return the cloned copy.  Note we have still left the
                // control in the local responseCtls variable just in case
                // somebody requests it again.
                return clonedControl;
            }
        }

        /// <summary>
        ///     Return the Connection object associated with this LdapConnection
        /// </summary>
        /// <returns>
        ///     the Connection object
        /// </returns>
        internal virtual Connection Connection => conn;

        /// <summary>
        ///     Return the Connection object name associated with this LdapConnection
        /// </summary>
        /// <returns>
        ///     the Connection object name
        /// </returns>
        internal virtual string ConnectionName => name;

        private LdapSearchConstraints defSearchCons;
        private LdapControl[] responseCtls;
        private object responseCtlSemaphore;
        private Connection conn;
        private static object nameLock; // protect agentNum
        private static int lConnNum = 0; // Debug, LdapConnection number
        private string name; // String name for debug

        /// <summary>
        ///     Used with search to specify that the scope of entrys to search is to
        ///     search only the base obect.
        ///     SCOPE_BASE = 0
        /// </summary>
        public const int SCOPE_BASE = 0;

        /// <summary>
        ///     Used with search to specify that the scope of entrys to search is to
        ///     search only the immediate subordinates of the base obect.
        ///     SCOPE_ONE = 1
        /// </summary>
        public const int SCOPE_ONE = 1;

        /// <summary>
        ///     Used with search to specify that the scope of entrys to search is to
        ///     search the base object and all entries within its subtree.
        ///     SCOPE_ONE = 2
        /// </summary>
        public const int SCOPE_SUB = 2;

        /// <summary>
        ///     Used with search instead of an attribute list to indicate that no
        ///     attributes are to be returned.
        ///     NO_ATTRS = "1.1"
        /// </summary>
        public const string NO_ATTRS = "1.1";

        /// <summary>
        ///     Used with search instead of an attribute list to indicate that all
        ///     attributes are to be returned.
        ///     ALL_USER_ATTRS = "*"
        /// </summary>
        public const string ALL_USER_ATTRS = "*";

        /// <summary>
        ///     Specifies the Ldapv3 protocol version when performing a bind operation.
        ///     Specifies Ldap version V3 of the protocol, and is specified
        ///     when performing bind operations.
        ///     You can use this identifier in the version parameter
        ///     of the bind method to specify an Ldapv3 bind.
        ///     Ldap_V3 is the default protocol version
        ///     Ldap_V3 = 3
        /// </summary>
        public const int Ldap_V3 = 3;

        /// <summary>
        ///     The default port number for Ldap servers.
        ///     You can use this identifier to specify the port when establishing
        ///     a clear text connection to a server.  This the default port.
        ///     DEFAULT_PORT = 389
        /// </summary>
        public const int DEFAULT_PORT = 389;

        /// <summary>
        ///     The default SSL port number for Ldap servers.
        ///     DEFAULT_SSL_PORT = 636
        ///     You can use this identifier to specify the port when establishing
        ///     a an SSL connection to a server..
        /// </summary>
        public const int DEFAULT_SSL_PORT = 636;

        /// <summary>
        ///     A string that can be passed in to the getProperty method.
        ///     Ldap_PROPERTY_SDK = "version.sdk"
        ///     You can use this string to request the version of the SDK.
        /// </summary>
        public const string Ldap_PROPERTY_SDK = "version.sdk";

        /// <summary>
        ///     A string that can be passed in to the getProperty method.
        ///     Ldap_PROPERTY_PROTOCOL = "version.protocol"
        ///     You can use this string to request the version of the
        ///     Ldap protocol.
        /// </summary>
        public const string Ldap_PROPERTY_PROTOCOL = "version.protocol";

        /// <summary>
        ///     A string that can be passed in to the getProperty method.
        ///     Ldap_PROPERTY_SECURITY = "version.security"
        ///     You can use this string to request the type of security
        ///     being used.
        /// </summary>
        public const string Ldap_PROPERTY_SECURITY = "version.security";

        /// <summary>
        ///     A string that corresponds to the server shutdown notification OID.
        ///     This notification may be used by the server to advise the client that
        ///     the server is about to close the connection due to an error
        ///     condition.
        ///     SERVER_SHUTDOWN_OID = "1.3.6.1.4.1.1466.20036"
        /// </summary>
        public const string SERVER_SHUTDOWN_OID = "1.3.6.1.4.1.1466.20036";

        /// <summary> The OID string that identifies a StartTLS request and response.</summary>
        private const string START_TLS_OID = "1.3.6.1.4.1.1466.20037";

        /// <summary>
        /// Constructs a new LdapConnection object, which will use the supplied
        /// class factory to construct a socket connection during
        /// LdapConnection.connect method.
        /// </summary>
        public LdapConnection()
        {
            defSearchCons = new LdapSearchConstraints();
            responseCtlSemaphore = new object();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="isDisposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Returns a property of a connection object.
        /// </summary>
        /// <param name="name">Name of the property to be returned.
        /// The following read-only properties are available
        /// for any given connection:
        /// <ul><li>
        /// Ldap_PROPERTY_SDK returns the version of this SDK,
        /// as a Float data type.
        /// </li><li>
        /// Ldap_PROPERTY_PROTOCOL returns the highest supported version of
        /// the Ldap protocol, as a Float data type.
        /// </li><li>
        /// Ldap_PROPERTY_SECURITY returns a comma-separated list of the
        /// types of authentication supported, as a
        /// string.
        /// </li></ul>
        /// A deep copy of the property is provided where applicable; a
        /// client does not need to clone the object received.</param>
        /// <returns>
        /// The object associated with the requested property,
        /// or null if the property is not defined.
        /// </returns>
        /// <seealso cref="LdapConstraints.GetProperty"></seealso>
        /// <seealso cref="object"></seealso>
        public virtual object GetProperty(string name)
        {
            if (name.ToUpper().Equals(Ldap_PROPERTY_SDK.ToUpper()))
                return "2.2.1";
            if (name.ToUpper().Equals(Ldap_PROPERTY_PROTOCOL.ToUpper()))
                return 3;
            if (name.ToUpper().Equals(Ldap_PROPERTY_SECURITY.ToUpper()))
                return "simple";
            return null;
        }

        /// <summary>
        /// Starts Transport Layer Security (TLS) protocol on this connection
        /// to enable session privacy.
        /// This affects the LdapConnection object and all cloned objects. A
        /// socket factory that implements LdapTLSSocketFactory must be set on the
        /// connection.
        /// </summary>
        /// <param name="dn">If non-null and non-empty, specifies that the
        /// connection and all operations through it should
        /// be authenticated with dn as the distinguished
        /// name.</param>
        /// <param name="passwd">If non-null and non-empty, specifies that the
        /// connection and all operations through it should
        /// be authenticated with dn as the distinguished
        /// name and passwd as password.
        /// Note: the application should use care in the use
        /// of String password objects.  These are long lived
        /// objects, and may expose a security risk, especially
        /// in objects that are serialized.  The LdapConnection
        /// keeps no long lived instances of these objects.</param>
        /// <param name="cons">Constraints specific to the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task Bind(string dn, string passwd, LdapConstraints cons = null)
        {
            return Bind(Ldap_V3, dn, passwd, cons);
        }

        /// <summary>
        /// Synchronously authenticates to the Ldap server (that the object is
        /// currently connected to) using the specified name, password, Ldap version,
        /// and constraints.
        /// If the object has been disconnected from an Ldap server,
        /// this method attempts to reconnect to the server. If the object
        /// has already authenticated, the old authentication is discarded.
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
        /// name and passwd as password.
        /// Note: the application should use care in the use
        /// of String password objects.  These are long lived
        /// objects, and may expose a security risk, especially
        /// in objects that are serialized.  The LdapConnection
        /// keeps no long lived instances of these objects.</param>
        /// <param name="cons">The constraints specific to the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task Bind(int version, string dn, string passwd, LdapConstraints cons = null)
        {
            sbyte[] pw = null;
            if (string.IsNullOrWhiteSpace(passwd) == false)
            {
                pw = Encoding.UTF8.GetBytes(passwd).ToSByteArray();
            }

            return Bind(version, dn, pw, cons);
        }

        /// <summary>
        /// Asynchronously authenticates to the Ldap server (that the object is
        /// currently connected to) using the specified name, password, Ldap
        /// version, queue, and constraints.
        /// If the object has been disconnected from an Ldap server,
        /// this method attempts to reconnect to the server. If the object
        /// had already authenticated, the old authentication is discarded.
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
        /// <param name="cons">Constraints specific to the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task Bind(int version, string dn, sbyte[] passwd, LdapConstraints cons = null)
        {
            if (cons == null)
                cons = defSearchCons;

            dn = string.IsNullOrEmpty(dn) ? string.Empty : dn.Trim();

            if (passwd == null)
                passwd = new sbyte[] {};

            var anonymous = false;

            if (passwd.Length == 0)
            {
                anonymous = true; // anonymous, passwd length zero with simple bind
                dn = string.Empty; // set to null if anonymous
            }

            var msg = new LdapBindRequest(version, dn, passwd, (cons ?? defSearchCons).GetControls());

            BindProperties = new BindProperties(version, dn, "simple", anonymous, null, null);

            return RequestLdapMessage(msg);
        }
        
        /// <summary>
        /// Connects to the specified host and port.
        /// If this LdapConnection object represents an open connection, the
        /// connection is closed first before the new connection is opened.
        /// At this point, there is no authentication, and any operations are
        /// conducted as an anonymous client.
        /// </summary>
        /// <param name="host">A host name or a dotted string representing the IP address
        /// of a host running an Ldap server.</param>
        /// <param name="port">The TCP or UDP port number to connect to or contact.
        /// The default Ldap port is 389.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task Connect(string host, int port)
        {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port);
            conn = new Connection(tcpClient, Encoding.UTF8, "\r\n", true, 0);

            // TODO: how to stop?
            await Task.Factory.StartNew(RetrieveMessages, cts.Token);
        }
        
        /// <summary>
        /// Synchronously disconnects from the Ldap server.
        /// Before the object can perform Ldap operations again, it must
        /// reconnect to the server by calling connect.
        /// The disconnect method abandons any outstanding requests, issues an
        /// unbind request to the server, and then closes the socket.
        /// </summary>
        public virtual void Disconnect()
        {
            // disconnect from API call
            cts.Cancel();
            conn.Disconnect();
        }
        
        /// <summary>
        /// Requests the LDAP message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="ct">The ct.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task RequestLdapMessage(LdapMessage msg,
            CancellationToken ct = default(CancellationToken))
        {
            var encoder = new LBEREncoder();
            var ber = msg.Asn1Object.GetEncoding(encoder);
            await conn.WriteDataAsync(ber.ToByteArray(), true, ct);

            while (new List<RfcLdapMessage>(Messages).Any(x => x.MessageID == msg.MessageID) == false)
                await Task.Delay(100, ct);
        }

        internal List<RfcLdapMessage> Messages { get; } = new List<RfcLdapMessage>();

        internal void RetrieveMessages()
        {
            var decoder = new LBERDecoder();

            while (true)
            {
                var asn1Id = new Asn1Identifier(conn.ActiveStream);

                if (asn1Id.Tag != Asn1Sequence.TAG)
                {
                    continue; // loop looking for an RfcLdapMessage identifier
                }

                // Turn the message into an RfcMessage class
                var asn1Len = new Asn1Length(conn.ActiveStream);

                Messages.Add(new RfcLdapMessage(decoder, conn.ActiveStream, asn1Len.Length));
            }
        }

        /// <summary>
        /// Synchronously reads the entry for the specified distinguished name (DN),
        /// using the specified constraints, and retrieves only the specified
        /// attributes from the entry.
        /// </summary>
        /// <param name="dn">The distinguished name of the entry to retrieve.</param>
        /// <param name="attrs">The names of the attributes to retrieve.</param>
        /// <param name="cons">The constraints specific to the operation.</param>
        /// <returns>
        /// the LdapEntry read from the server
        /// </returns>
        public async Task<LdapEntry> Read(string dn, string[] attrs = null, LdapSearchConstraints cons = null)
        {
            var sr = await Search(dn, SCOPE_BASE, null, attrs, false, cons ?? defSearchCons);
            LdapEntry ret = null;

            if (sr.hasMore())
            {
                ret = sr.next();
                if (sr.hasMore())
                {
                    // "Read response is ambiguous, multiple entries returned"
                    throw new LdapLocalException(ExceptionMessages.READ_MULTIPLE, LdapException.AMBIGUOUS_RESPONSE);
                }
            }

            return ret;
        }

        /// <summary>
        /// Performs the search specified by the parameters,
        /// also allowing specification of constraints for the search (such
        /// as the maximum number of entries to find or the maximum time to
        /// wait for search results).
        /// </summary>
        /// <param name="base">The base distinguished name to search from.</param>
        /// <param name="scope">The scope of the entries to search. The following
        /// are the valid options:
        /// <ul><li>SCOPE_BASE - searches only the base DN</li><li>SCOPE_ONE - searches only entries under the base DN</li><li>
        /// SCOPE_SUB - searches the base DN and all entries
        /// within its subtree
        /// </li></ul></param>
        /// <param name="filter">The search filter specifying the search criteria.</param>
        /// <param name="attrs">The names of attributes to retrieve.</param>
        /// <param name="typesOnly">If true, returns the names but not the values of
        /// the attributes found.  If false, returns the
        /// names and values for attributes found.</param>
        /// <param name="cons">The constraints specific to the search.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<LdapSearchResults> Search(
            string @base, 
            int scope, 
            string filter = null, 
            string[] attrs = null,
            bool typesOnly = false, 
            LdapSearchConstraints cons = null)
        {
            if (string.IsNullOrWhiteSpace(filter))
                filter = "objectclass=*";
            if (cons == null)
                cons = defSearchCons;
            var msg = new LdapSearchRequest(@base, scope, filter, attrs, cons.Dereference, cons.MaxResults, cons.ServerTimeLimit, typesOnly, cons.GetControls());

            await RequestLdapMessage(msg);
            
            return new LdapSearchResults(this, msg.MessageID);
        }
    }
}

#endif