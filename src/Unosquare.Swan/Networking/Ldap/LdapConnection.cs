#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Exceptions;

    /// <summary>
    /// The central class that encapsulates the connection
    /// to a directory server through the Ldap protocol.
    /// LdapConnection objects are used to perform common Ldap
    /// operations such as search, modify and add.
    /// In addition, LdapConnection objects allow you to bind to an
    /// Ldap server, set connection and search constraints, and perform
    /// several other tasks.
    /// An LdapConnection object is not connected on
    /// construction and can only be connected to one server at one
    /// port. Multiple threads may share this single connection, typically
    /// by cloning the connection object, one for each thread. An
    /// application may have more than one LdapConnection object, connected
    /// to the same or different directory servers.
    /// 
    /// Base on https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard
    /// </summary>
    public class LdapConnection
    {
        /// <summary>
        /// Used with search to specify that the scope of entrys to search is to
        /// search only the base object.
        /// SCOPE_BASE = 0
        /// </summary>
        public const int ScopeBase = 0;

        /// <summary>
        /// Used with search to specify that the scope of entrys to search is to
        /// search only the immediate subordinates of the base object.
        /// SCOPE_ONE = 1
        /// </summary>
        public const int ScopeOne = 1;

        /// <summary>
        /// Used with search to specify that the scope of entrys to search is to
        /// search the base object and all entries within its subtree.
        /// SCOPE_ONE = 2
        /// </summary>
        public const int ScopeSub = 2;

        /// <summary>
        /// Used with search instead of an attribute list to indicate that no
        /// attributes are to be returned.
        /// NO_ATTRS = "1.1"
        /// </summary>
        public const string NoAttrs = "1.1";

        /// <summary>
        /// Used with search instead of an attribute list to indicate that all
        /// attributes are to be returned.
        /// ALL_USER_ATTRS = "*"
        /// </summary>
        public const string AllUserAttrs = "*";

        /// <summary>
        /// Specifies the Ldapv3 protocol version when performing a bind operation.
        /// Specifies Ldap version V3 of the protocol, and is specified
        /// when performing bind operations.
        /// You can use this identifier in the version parameter
        /// of the bind method to specify an Ldapv3 bind.
        /// Ldap_V3 is the default protocol version
        /// Ldap_V3 = 3
        /// </summary>
        public const int LdapV3 = 3;

        /// <summary>
        ///     The default port number for Ldap servers.
        ///     You can use this identifier to specify the port when establishing
        ///     a clear text connection to a server.  This the default port.
        ///     DEFAULT_PORT = 389
        /// </summary>
        public const int DefaultPort = 389;

        /// <summary>
        ///     The default SSL port number for Ldap servers.
        ///     DEFAULT_SSL_PORT = 636
        ///     You can use this identifier to specify the port when establishing
        ///     a an SSL connection to a server..
        /// </summary>
        public const int DefaultSslPort = 636;

        /// <summary>
        ///     A string that can be passed in to the getProperty method.
        ///     Ldap_PROPERTY_SDK = "version.sdk"
        ///     You can use this string to request the version of the SDK.
        /// </summary>
        public const string LdapPropertySdk = "version.sdk";

        /// <summary>
        ///     A string that can be passed in to the getProperty method.
        ///     Ldap_PROPERTY_PROTOCOL = "version.protocol"
        ///     You can use this string to request the version of the
        ///     Ldap protocol.
        /// </summary>
        public const string LdapPropertyProtocol = "version.protocol";

        /// <summary>
        ///     A string that can be passed in to the getProperty method.
        ///     Ldap_PROPERTY_SECURITY = "version.security"
        ///     You can use this string to request the type of security
        ///     being used.
        /// </summary>
        public const string LdapPropertySecurity = "version.security";

        /// <summary>
        ///     A string that corresponds to the server shutdown notification OID.
        ///     This notification may be used by the server to advise the client that
        ///     the server is about to close the connection due to an error
        ///     condition.
        ///     SERVER_SHUTDOWN_OID = "1.3.6.1.4.1.1466.20036"
        /// </summary>
        public const string ServerShutdownOid = "1.3.6.1.4.1.1466.20036";

        /// <summary> The OID string that identifies a StartTLS request and response.</summary>
        public const string StartTlsOid = "1.3.6.1.4.1.1466.20037";

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly object _responseCtlSemaphore = new object();

        private LdapControl[] _responseCtls;
        private Connection _conn;
        
        /// <summary>
        /// Returns the protocol version uses to authenticate.
        /// 0 is returned if no authentication has been performed.
        /// </summary>
        /// <value>
        /// The protocol version.
        /// </value>
        public int ProtocolVersion => BindProperties?.ProtocolVersion ?? LdapV3;

        /// <summary>
        /// Returns the distinguished name (DN) used for as the bind name during
        /// the last successful bind operation.  null is returned
        /// if no authentication has been performed or if the bind resulted in
        /// an anonymous connection.
        /// </summary>
        /// <value>
        /// The authentication dn.
        /// </value>
        public string AuthenticationDn => BindProperties == null ? null : (BindProperties.Anonymous ? null : BindProperties.AuthenticationDN);

        /// <summary>
        /// Returns the method used to authenticate the connection. The return
        /// value is one of the following:
        /// <ul><li>"none" indicates the connection is not authenticated.</li><li>
        /// "simple" indicates simple authentication was used or that a null
        /// or empty authentication DN was specified.
        /// </li><li>"sasl" indicates that a SASL mechanism was used to authenticate</li></ul>
        /// </summary>
        /// <value>
        /// The authentication method.
        /// </value>
        public string AuthenticationMethod => BindProperties == null ? "simple" : BindProperties.AuthenticationMethod;
        
        /// <summary>
        ///     Indicates whether the connection represented by this object is open
        ///     at this time.
        /// </summary>
        /// <returns>
        ///     True if connection is open; false if the connection is closed.
        /// </returns>
        public bool Connected => _conn?.IsConnected == true;

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
        public LdapControl[] ResponseControls
        {
            get
            {
                if (_responseCtls == null)
                {
                    return null;
                }

                // We have to clone the control just in case
                // we have two client threads that end up retreiving the
                // same control.
                var clonedControl = new LdapControl[_responseCtls.Length];

                // Also note we synchronize access to the local response
                // control object just in case another message containing controls
                // comes in from the server while we are busy duplicating
                // this one.
                lock (_responseCtlSemaphore)
                {
                    for (var i = 0; i < _responseCtls.Length; i++)
                    {
                        clonedControl[i] = (LdapControl)_responseCtls[i].Clone();
                    }
                }

                // Return the cloned copy.  Note we have still left the
                // control in the local responseCtls variable just in case
                // somebody requests it again.
                return clonedControl;
            }
        }

        internal BindProperties BindProperties { get; set; }
        
        internal Connection Connection => _conn;

        internal List<RfcLdapMessage> Messages { get; } = new List<RfcLdapMessage>();

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Synchronously authenticates to the Ldap server (that the object is
        /// currently connected to) using the specified name, password, Ldap version,
        /// and constraints.
        /// If the object has been disconnected from an Ldap server,
        /// this method attempts to reconnect to the server. If the object
        /// has already authenticated, the old authentication is discarded.
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
        /// <returns>
        /// A <see cref="Task" /> representing the asynchronous operation.
        /// </returns>
        public Task Bind(string dn, string passwd) => Bind(LdapV3, dn, passwd);

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
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Bind(int version, string dn, string passwd)
        {
            dn = string.IsNullOrEmpty(dn) ? string.Empty : dn.Trim();
            var passwdData = string.IsNullOrWhiteSpace(passwd) ? new sbyte[] { } : Encoding.UTF8.GetSBytes(passwd);

            var anonymous = false;

            if (passwdData.Length == 0)
            {
                anonymous = true; // anonymous, passwd length zero with simple bind
                dn = string.Empty; // set to null if anonymous
            }
            
            BindProperties = new BindProperties(version, dn, "simple", anonymous);

            return RequestLdapMessage(new LdapBindRequest(version, dn, passwdData));
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
        public async Task Connect(string host, int port)
        {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port);
            _conn = new Connection(tcpClient, Encoding.UTF8, "\r\n", true, 0);
            
#pragma warning disable 4014
            Task.Factory.StartNew(RetrieveMessages, _cts.Token);
#pragma warning restore 4014
        }
        
        /// <summary>
        /// Synchronously disconnects from the Ldap server.
        /// Before the object can perform Ldap operations again, it must
        /// reconnect to the server by calling connect.
        /// The disconnect method abandons any outstanding requests, issues an
        /// unbind request to the server, and then closes the socket.
        /// </summary>
        public void Disconnect()
        {
            // disconnect from API call
            _cts.Cancel();
            _conn.Disconnect();
        }

        /// <summary>
        /// Synchronously reads the entry for the specified distinguished name (DN),
        /// using the specified constraints, and retrieves only the specified
        /// attributes from the entry.
        /// </summary>
        /// <param name="dn">The distinguished name of the entry to retrieve.</param>
        /// <param name="attrs">The names of the attributes to retrieve.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// the LdapEntry read from the server
        /// </returns>
        /// <exception cref="LdapException">Read response is ambiguous, multiple entries returned</exception>
        public async Task<LdapEntry> Read(string dn, string[] attrs = null, CancellationToken ct = default(CancellationToken))
        {
            var sr = await Search(dn, ScopeSub, null, attrs, false, ct);
            LdapEntry ret = null;

            if (sr.HasMore())
            {
                ret = sr.Next();
                if (sr.HasMore())
                {
                    throw new LdapException("Read response is ambiguous, multiple entries returned", LdapStatusCode.AmbiguousResponse);
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
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task" /> representing the asynchronous operation.
        /// </returns>
        public async Task<LdapSearchResults> Search(
            string @base, 
            int scope, 
            string filter = "objectClass=*", 
            string[] attrs = null,
            bool typesOnly = false, 
            CancellationToken ct = default(CancellationToken))
        {
            // TODO: Add Search options
            var msg = new LdapSearchRequest(@base, scope, filter, attrs, 0, 1000, 0, typesOnly, null);

            await RequestLdapMessage(msg, ct);
            
            return new LdapSearchResults(this, msg.MessageId);
        }

        /// <summary>
        /// Modifies the specified dn.
        /// </summary>
        /// <param name="dn">The dn.</param>
        /// <param name="mods">The mods.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task" /> representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">dn</exception>
        public Task Modify(string dn, LdapModification[] mods, CancellationToken ct = default(CancellationToken))
        {
            if (dn == null)
            {
                throw new ArgumentNullException(nameof(dn));
            }
            
            return RequestLdapMessage(new LdapModifyRequest(dn, mods, null), ct);
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
            await _conn.WriteDataAsync(ber.ToByteArray(), true, ct);

            while (new List<RfcLdapMessage>(Messages).Any(x => x.MessageId == msg.MessageId) == false)
                await Task.Delay(100, ct);

            var first = new List<RfcLdapMessage>(Messages).FirstOrDefault(x => x.MessageId == msg.MessageId);

            if (first != null)
            {
                var response = new LdapResponse(first);
                response.ChkResultCode();
            }
        }

        internal void RetrieveMessages()
        {
            var decoder = new LBERDecoder();

            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var asn1Id = new Asn1Identifier(_conn.ActiveStream);

                    if (asn1Id.Tag != Asn1Sequence.Tag)
                    {
                        continue; // loop looking for an RfcLdapMessage identifier
                    }

                    // Turn the message into an RfcMessage class
                    var asn1Len = new Asn1Length(_conn.ActiveStream);

                    Messages.Add(new RfcLdapMessage(decoder, _conn.ActiveStream, asn1Len.Length));
                }
                catch (System.IO.IOException)
                {
                    // ignore
                }
            }

            // ReSharper disable once FunctionNeverReturns
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="isDisposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                Disconnect();
            }
        }
    }
}

#endif