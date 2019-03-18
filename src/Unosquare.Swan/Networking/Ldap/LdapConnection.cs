namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
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
    /// port.
    /// 
    /// Based on https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard.
    /// </summary>
    /// <example>
    /// The following code describes how to use the LdapConnection class:
    /// 
    /// <code>
    /// class Example
    /// {
    ///     using Unosquare.Swan;
    ///     using Unosquare.Swan.Networking.Ldap;
    ///     using System.Threading.Tasks;
    ///     
    ///     static async Task Main()
    ///     {
    ///         // create a LdapConnection object
    ///         var connection = new LdapConnection();
    ///         
    ///         // connect to a server
    ///         await connection.Connect("ldap.forumsys.com", 389);
    ///         
    ///         // set up the credentials 
    ///         await connection.Bind("cn=read-only-admin,dc=example,dc=com", "password");
    ///         
    ///         // retrieve all entries that have the specified email using ScopeSub 
    ///         // which searches all entries at all levels under 
    ///         // and including the specified base DN
    ///         var searchResult = await connection
    ///         .Search("dc=example,dc=com", LdapConnection.ScopeSub, "(cn=Isaac Newton)");
    ///         
    ///         // if there are more entries remaining keep going
    ///         while (searchResult.HasMore())
    ///         {
    ///             // point to the next entry
    ///             var entry = searchResult.Next();
    ///             
    ///             // get all attributes 
    ///             var entryAttributes = entry.GetAttributeSet();
    ///             
    ///             // select its name and print it out
    ///             entryAttributes.GetAttribute("cn").StringValue.Info();
    ///         }
    ///         
    ///         // modify Tesla and sets its email as tesla@email.com
    ///         connection.Modify("uid=tesla,dc=example,dc=com", 
    ///         new[] { 
    ///             new LdapModification(LdapModificationOp.Replace,
    ///                 "mail", "tesla@email.com") 
    ///             });
    ///             
    ///         // delete the listed values from the given attribute
    ///         connection.Modify("uid=tesla,dc=example,dc=com", 
    ///         new[] { 
    ///             new LdapModification(LdapModificationOp.Delete,
    ///             "mail", "tesla@email.com") 
    ///             });
    ///         
    ///         // add back the recently deleted property
    ///         connection.Modify("uid=tesla,dc=example,dc=com", 
    ///             new[] { 
    ///                 new LdapModification(LdapModificationOp.Add,
    ///                 "mail", "tesla@email.com") 
    ///             });    
    /// 
    ///         // disconnect from the LDAP server
    ///         connection.Disconnect();
    ///         
    ///         Terminal.Flush();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class LdapConnection : IDisposable
    {
        private const int LdapV3 = 3;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        
        private Connection _conn;
        private bool _isDisposing;

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
        /// value is one of the following:.
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
        
        internal BindProperties BindProperties { get; set; }
        
        internal List<RfcLdapMessage> Messages { get; } = new List<RfcLdapMessage>();

        /// <inheritdoc />
        public void Dispose()
        {
            if (_isDisposing) return;

            _isDisposing = true;
            Disconnect();
            _cts?.Dispose();
        }

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
        /// <param name="password">If non-null and non-empty, specifies that the
        /// connection and all operations through it should
        /// be authenticated with dn as the distinguished
        /// name and password.
        /// Note: the application should use care in the use
        /// of String password objects.  These are long lived
        /// objects, and may expose a security risk, especially
        /// in objects that are serialized.  The LdapConnection
        /// keeps no long lived instances of these objects.</param>
        /// <returns>
        /// A <see cref="Task" /> representing the asynchronous operation.
        /// </returns>
        public Task Bind(string dn, string password) => Bind(LdapV3, dn, password);

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
        /// <param name="password">If non-null and non-empty, specifies that the
        /// connection and all operations through it should
        /// be authenticated with dn as the distinguished
        /// name and passwd as password.
        /// Note: the application should use care in the use
        /// of String password objects.  These are long lived
        /// objects, and may expose a security risk, especially
        /// in objects that are serialized.  The LdapConnection
        /// keeps no long lived instances of these objects.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Bind(int version, string dn, string password)
        {
            dn = string.IsNullOrEmpty(dn) ? string.Empty : dn.Trim();
            var passwordData = string.IsNullOrWhiteSpace(password) ? Array.Empty<sbyte>() : Encoding.UTF8.GetSBytes(password);

            var anonymous = false;

            if (passwordData.Length == 0)
            {
                anonymous = true; // anonymous, password length zero with simple bind
                dn = string.Empty; // set to null if anonymous
            }
            
            BindProperties = new BindProperties(version, dn, "simple", anonymous);

            return RequestLdapMessage(new LdapBindRequest(version, dn, passwordData));
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
            await tcpClient.ConnectAsync(host, port).ConfigureAwait(false);
            _conn = new Connection(tcpClient, Encoding.UTF8, "\r\n", true, 0);
            
#pragma warning disable 4014
            Task.Run(RetrieveMessages, _cts.Token);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// the LdapEntry read from the server.
        /// </returns>
        /// <exception cref="LdapException">Read response is ambiguous, multiple entries returned.</exception>
        public async Task<LdapEntry> Read(string dn, string[] attrs = null, CancellationToken cancellationToken = default)
        {
            var sr = await Search(dn, LdapScope.ScopeSub, null, attrs, false, cancellationToken).ConfigureAwait(false);
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
        /// <param name="scope">The scope of the entries to search.</param>
        /// <param name="filter">The search filter specifying the search criteria.</param>
        /// <param name="attrs">The names of attributes to retrieve.</param>
        /// <param name="typesOnly">If true, returns the names but not the values of
        /// the attributes found.  If false, returns the
        /// names and values for attributes found.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task" /> representing the asynchronous operation.
        /// </returns>
        public async Task<LdapSearchResults> Search(
            string @base, 
            LdapScope scope, 
            string filter = "objectClass=*", 
            string[] attrs = null,
            bool typesOnly = false, 
            CancellationToken cancellationToken = default)
        {
            // TODO: Add Search options
            var msg = new LdapSearchRequest(@base, scope, filter, attrs, 0, 1000, 0, typesOnly, null);

            await RequestLdapMessage(msg, cancellationToken).ConfigureAwait(false);
            
            return new LdapSearchResults(Messages, msg.MessageId);
        }

        /// <summary>
        /// Modifies the specified dn.
        /// </summary>
        /// <param name="distinguishedName">Name of the distinguished.</param>
        /// <param name="mods">The mods.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>
        /// A <see cref="Task" /> representing the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">distinguishedName.</exception>
        public Task Modify(string distinguishedName, LdapModification[] mods, CancellationToken ct = default)
        {
            if (distinguishedName == null)
            {
                throw new ArgumentNullException(nameof(distinguishedName));
            }

            return RequestLdapMessage(new LdapModifyRequest(distinguishedName, mods, null), ct);
        }

        internal async Task RequestLdapMessage(LdapMessage msg, CancellationToken ct = default)
        {
            using (var stream = new MemoryStream())
            {
                LberEncoder.Encode(msg.Asn1Object, stream);
                await _conn.WriteDataAsync(stream.ToArray(), true, ct).ConfigureAwait(false);

                try
                {
                    while (new List<RfcLdapMessage>(Messages).Any(x => x.MessageId == msg.MessageId) == false)
                        await Task.Delay(100, ct).ConfigureAwait(false);
                }
                catch (ArgumentException)
                {
                    // expected
                }

                var first = new List<RfcLdapMessage>(Messages).FirstOrDefault(x => x.MessageId == msg.MessageId);

                if (first != null)
                {
                    var response = new LdapResponse(first);
                    response.ChkResultCode();
                }
            }
        }

        internal void RetrieveMessages()
        {
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

                    Messages.Add(new RfcLdapMessage(_conn.ActiveStream, asn1Len.Length));
                }
                catch (IOException)
                {
                    // ignore
                }
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}