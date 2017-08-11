﻿#if !UWP
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unosquare.Swan.Networking.Ldap
{
    /// <summary>
    /// Represents Ldap Sasl Credentials.
    /// <pre>
    /// SaslCredentials ::= SEQUENCE {
    /// mechanism               LdapString,
    /// credentials             OCTET STRING OPTIONAL }
    /// </pre></summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    public class RfcSaslCredentials : Asn1Sequence
    {
        public RfcSaslCredentials(RfcLdapString mechanism) : this(mechanism, null)
        {
        }

        public RfcSaslCredentials(RfcLdapString mechanism, Asn1OctetString credentials) : base(2)
        {
            Add(mechanism);
            if (credentials != null)
                Add(credentials);
        }
    }

    /// <summary>
    /// Represents an Ldap Authentication Choice.
    /// <pre>
    /// AuthenticationChoice ::= CHOICE {
    /// simple                  [0] OCTET STRING,
    /// -- 1 and 2 reserved
    /// sasl                    [3] SaslCredentials }
    /// </pre></summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Choice" />
    public class RfcAuthenticationChoice : Asn1Choice
    {
        public RfcAuthenticationChoice(Asn1Tagged choice) : base(choice)
        {
        }

        public RfcAuthenticationChoice(string mechanism, sbyte[] credentials)
            : base(
                new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, 3),
                    new RfcSaslCredentials(new RfcLdapString(mechanism),
                        credentials != null ? new Asn1OctetString(credentials) : null), false))
        {
            // implicit tagging
        }
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
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.RfcRequest" />
    public class RfcBindRequest : Asn1Sequence, RfcRequest
    {
        /// <summary> Sets the protocol version</summary>
        public virtual Asn1Integer Version
        {
            get { return (Asn1Integer)Get(0); }
            set { Set(0, value); }
        }

        public virtual RfcLdapDN Name
        {
            get { return (RfcLdapDN)Get(1); }
            set { Set(1, value); }
        }

        public virtual RfcAuthenticationChoice AuthenticationChoice
        {
            get { return (RfcAuthenticationChoice)Get(2); }
            set { Set(2, value); }
        }

        /// <summary>
        ///     ID is added for Optimization.
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        private static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.APPLICATION, true,
            LdapMessage.BIND_REQUEST);
        
        public RfcBindRequest(Asn1Integer version, RfcLdapDN name, RfcAuthenticationChoice auth) : base(3)
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
        ///     Constructs a new Bind Request copying the original data from
        ///     an existing request.
        /// </summary>
        internal RfcBindRequest(Asn1Object[] origRequest, string base_Renamed) : base(origRequest, origRequest.Length)
        {
            // Replace the dn if specified, otherwise keep original base
            if ((object)base_Renamed != null)
            {
                Set(1, new RfcLdapDN(base_Renamed));
            }
        }

        /// <summary>
        ///     Override getIdentifier to return an application-wide id.
        ///     <pre>
        ///         ID = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 0. (0x60)
        ///     </pre>
        /// </summary>
        public override Asn1Identifier GetIdentifier()
        {
            return ID;
        }

        public RfcRequest dupRequest(string base_Renamed, string filter, bool request)
        {
            return new RfcBindRequest(ToArray(), base_Renamed);
        }

        public string getRequestDN()
        {
            return ((RfcLdapDN)Get(1)).StringValue();
        }
    }

    /// <summary>
    ///     Represents a simple bind request.
    /// </summary>
    /// <seealso cref="LdapConnection.SendRequest">
    /// </seealso>
    /*
     *       BindRequest ::= [APPLICATION 0] SEQUENCE {
     *               version                 INTEGER (1 .. 127),
     *               name                    LdapDN,
     *               authentication          AuthenticationChoice }
     */
    public class LdapBindRequest : LdapMessage
    {
        /// <summary>
        ///     Retrieves the Authentication DN for a bind request.
        /// </summary>
        /// <returns>
        ///     the Authentication DN for a bind request
        /// </returns>
        public virtual string AuthenticationDN
        {
            get { return Asn1Object.RequestDN; }
        }

        /// <summary>
        ///     Constructs a simple bind request.
        /// </summary>
        /// <param name="version">
        ///     The Ldap protocol version, use Ldap_V3.
        ///     Ldap_V2 is not supported.
        /// </param>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        /// </param>
        /// <param name="cont">
        ///     Any controls that apply to the simple bind request,
        ///     or null if none.
        /// </param>
        public LdapBindRequest(int version, string dn, sbyte[] passwd, LdapControl[] cont)
            : base(
                BIND_REQUEST,
                new RfcBindRequest(new Asn1Integer(version), new RfcLdapDN(dn),
                    new RfcAuthenticationChoice(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, 0),
                        new Asn1OctetString(passwd), false))), cont)
        {
        }

        /// <summary>
        ///     Return an Asn1 representation of this add request.
        ///     #return an Asn1 representation of this object.
        /// </summary>
        public override string ToString()
        {
            return Asn1Object.ToString();
        }
    }

    /// <summary>
    ///     Encapsulates a continuation reference from an asynchronous search operation.
    /// </summary>
    public class LdapSearchResultReference : LdapMessage
    {
        /// <summary>
        ///     Returns any URLs in the object.
        /// </summary>
        /// <returns>
        ///     The URLs.
        /// </returns>
        public virtual string[] Referrals
        {
            get
            {
                var references = ((RfcSearchResultReference) message.Response).ToArray();
                srefs = new string[references.Length];
                for (var i = 0; i < references.Length; i++)
                {
                    srefs[i] = ((Asn1OctetString) references[i]).StringValue();
                }
                return srefs;
            }
        }

        private string[] srefs;
        private static int refNum = 0; // Debug, LdapConnection number
        private string name; // String name for debug
        /*package*/

        /// <summary>
        ///     Constructs an LdapSearchResultReference object.
        /// </summary>
        /// <param name="message">
        ///     The LdapMessage with a search reference.
        /// </param>
        internal LdapSearchResultReference(RfcLdapMessage message) : base(message)
        {
        }
    }

    /// <summary>
    ///     An LdapSearchResults object is returned from a synchronous search
    ///     operation. It provides access to all results received during the
    ///     operation (entries and exceptions).
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    public class LdapSearchResults
    {
        /// <summary>
        ///     Returns a count of the items in the search result.
        ///     Returns a count of the entries and exceptions remaining in the object.
        ///     If the search was submitted with a batch size greater than zero,
        ///     getCount reports the number of results received so far but not enumerated
        ///     with next().  If batch size equals zero, getCount reports the number of
        ///     items received, since the application thread blocks until all results are
        ///     received.
        /// </summary>
        /// <returns>
        ///     The number of items received but not retrieved by the application
        /// </returns>
        public virtual int Count
        {
            get
            {
                // TODO: Pending
                return 0;
            }
        }

        /// <summary>
        ///     Returns the latest server controls returned by the server
        ///     in the context of this search request, or null
        ///     if no server controls were returned.
        /// </summary>
        /// <returns>
        ///     The server controls returned with the search request, or null
        ///     if none were returned.
        /// </returns>
        public virtual LdapControl[] ResponseControls
        {
            get { return controls; }
        }

        /// <summary>
        ///     Collects batchSize elements from an LdapSearchQueue message
        ///     queue and places them in a Vector.
        ///     If the last message from the server,
        ///     the result message, contains an error, it will be stored in the Vector
        ///     for nextElement to process. (although it does not increment the search
        ///     result count) All search result entries will be placed in the Vector.
        ///     If a null is returned from getResponse(), it is likely that the search
        ///     was abandoned.
        /// </summary>
        /// <returns>
        ///     true if all search results have been placed in the vector.
        /// </returns>
        //private bool BatchOfResults
        //{
        //    get
        //    {
        //        LdapMessage msg;
        //        // <=batchSize so that we can pick up the result-done message
        //        for (var i = 0; i < batchSize;)
        //        {
        //            try
        //            {
        //                if ((msg = queue.getResponse()) != null)
        //                {
        //                    // Only save controls if there are some
        //                    var ctls = msg.Controls;
        //                    if (ctls != null)
        //                    {
        //                        controls = ctls;
        //                    }
        //                    if (msg is LdapSearchResult)
        //                    {
        //                        // Search Entry
        //                        object entry = ((LdapSearchResult)msg).Entry;
        //                        entries.Add(entry);
        //                        i++;
        //                        entryCount++;
        //                    }
        //                    else if (msg is LdapSearchResultReference)
        //                    {
        //                        // Search Ref
        //                        var refs = ((LdapSearchResultReference)msg).Referrals;
        //                        if (cons.ReferralFollowing)
        //                        {
        //                            //									referralConn = conn.chaseReferral(queue, cons, msg, refs, 0, true, referralConn);
        //                        }
        //                        else
        //                        {
        //                            references.Add(refs);
        //                            referenceCount++;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // LdapResponse
        //                        var resp = (LdapResponse)msg;
        //                        var resultCode = resp.ResultCode;
        //                        // Check for an embedded exception
        //                        if (resp.hasException())
        //                        {
        //                            // Fake it, results in an exception when msg read
        //                            resultCode = LdapException.CONNECT_ERROR;
        //                        }
        //                        if (resultCode == LdapException.REFERRAL && cons.ReferralFollowing)
        //                        {
        //                            // Following referrals
        //                            //									referralConn = conn.chaseReferral(queue, cons, resp, resp.Referrals, 0, false, referralConn);
        //                        }
        //                        else if (resultCode != LdapException.SUCCESS)
        //                        {
        //                            // Results in an exception when message read
        //                            entries.Add(resp);
        //                            entryCount++;
        //                        }
        //                        // We are done only when we have read all messages
        //                        // including those received from following referrals
        //                        var msgIDs = queue.MessageIDs;
        //                        if (msgIDs.Length == 0)
        //                        {
        //                            // Release referral exceptions
        //                            //									conn.releaseReferralConnections(referralConn);
        //                            return true; // search completed
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    // We get here if the connection timed out
        //                    // we have no responses, no message IDs and no exceptions
        //                    var e = new LdapException(null, LdapException.Ldap_TIMEOUT, null);
        //                    entries.Add(e);
        //                    break;
        //                }
        //            }
        //            catch (LdapException e)
        //            {
        //                // Hand exception off to user
        //                entries.Add(e);
        //            }
        //        }
        //        return false; // search not completed
        //    }
        //}
        private readonly ArrayList entries; // Search entries

        private int entryCount; // # Search entries in vector
        private int entryIndex; // Current position in vector
        private readonly ArrayList references; // Search Result References
        private int referenceCount; // # Search Result Reference in vector
        private int referenceIndex; // Current position in vector
        private readonly int batchSize; // Application specified batch size
        private bool completed; // All entries received
        private LdapControl[] controls; // Last set of controls
        private static object nameLock; // protect resultsNum
        private static int resultsNum = 0; // used for debug
        private string name; // used for debug
        private LdapConnection conn; // LdapConnection which started search
        private readonly LdapSearchConstraints cons; // LdapSearchConstraints for search
        private ArrayList referralConn = null; // Referral Connections

        /// <summary>
        ///     Constructs a queue object for search results.
        /// </summary>
        /// <param name="conn">
        ///     The LdapConnection which initiated the search
        /// </param>
        /// <param name="queue">
        ///     The queue for the search results.
        /// </param>
        /// <param name="cons">
        ///     The LdapSearchConstraints associated with this search
        /// </param>
        internal LdapSearchResults(LdapConnection conn, LdapSearchConstraints cons)
        {
            // setup entry Vector
            this.conn = conn;
            this.cons = cons;
            var batchSize = cons.BatchSize;
            var vectorIncr = batchSize == 0 ? 64 : 0;
            entries = new ArrayList(batchSize == 0 ? 64 : batchSize);
            entryCount = 0;
            entryIndex = 0;
            // setup search reference Vector
            references = new ArrayList(5);
            referenceCount = 0;
            referenceIndex = 0;
            this.batchSize = batchSize == 0 ? int.MaxValue : batchSize;
        }

        /// <summary>
        ///     Reports if there are more search results.
        /// </summary>
        /// <returns>
        ///     true if there are more search results.
        /// </returns>
        public virtual bool hasMore()
        {
            var ret = false;
            if (entryIndex < entryCount || referenceIndex < referenceCount)
            {
                // we have data
                ret = true;
            }
            else if (completed == false)
            {
                // reload the Vector by getting more results
                resetVectors();
                ret = entryIndex < entryCount || referenceIndex < referenceCount;
            }
            return ret;
        }

        public static void SetSize(ArrayList arrayList, int newSize)
        {
            if (newSize < 0) throw new ArgumentException();
            if (newSize < arrayList.Count)
                arrayList.RemoveRange(newSize, arrayList.Count - newSize);
            else
                while (newSize > arrayList.Count)
                    arrayList.Add(null);
        }

        /*
                * If both of the vectors are empty, get more data for them.
                */

        private void resetVectors()
        {
            // If we're done, no further checking needed
            if (completed)
            {
                return;
            }
            // Checks if we have run out of references
            if (referenceIndex != 0 && referenceIndex >= referenceCount)
            {
                SetSize(references, 0);
                referenceCount = 0;
                referenceIndex = 0;
            }
            // Checks if we have run out of entries
            if (entryIndex != 0 && entryIndex >= entryCount)
            {
                SetSize(entries, 0);
                entryCount = 0;
                entryIndex = 0;
            }
            // If no data at all, must reload enumeration
            //if (referenceIndex == 0 && referenceCount == 0 && entryIndex == 0 && entryCount == 0)
            //{
            //    completed = BatchOfResults;
            //}
        }

        /// <summary>
        ///     Returns the next result as an LdapEntry.
        ///     If automatic referral following is disabled or if a referral
        ///     was not followed, next() will throw an LdapReferralException
        ///     when the referral is received.
        /// </summary>
        /// <returns>
        ///     The next search result as an LdapEntry.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        /// <exception>
        ///     LdapReferralException A referral was received and not
        ///     followed.
        /// </exception>
        public virtual LdapEntry next()
        {
            if (completed && entryIndex >= entryCount && referenceIndex >= referenceCount)
            {
                throw new ArgumentOutOfRangeException("LdapSearchResults.next() no more results");
            }
            // Check if the enumeration is empty and must be reloaded
            resetVectors();
            object element = null;
            // Check for Search References & deliver to app as they come in
            // We only get here if not following referrals/references
            if (referenceIndex < referenceCount)
            {
                var refs = (string[]) references[referenceIndex++];
                var rex = new LdapReferralException(ExceptionMessages.REFERENCE_NOFOLLOW);
                rex.setReferrals(refs);
                throw rex;
            }
            if (entryIndex < entryCount)
            {
                // Check for Search Entries and the Search Result
                element = entries[entryIndex++];
                if (element is LdapResponse)
                {
                    // Search done w/bad status
                    if (((LdapResponse) element).hasException())
                    {
                        var lr = (LdapResponse) element;
                        var ri = lr.ActiveReferral;
                        if (ri != null)
                        {
                            // Error attempting to follow a search continuation reference
                            var rex = new LdapReferralException(ExceptionMessages.REFERENCE_ERROR, lr.Exception);
                            rex.setReferrals(ri.ReferralList);
                            rex.FailedReferral = ri.ReferralUrl.ToString();
                            throw rex;
                        }
                    }
                    // Throw an exception if not success
                    ((LdapResponse) element).chkResultCode();
                }
                else if (element is LdapException)
                {
                    throw (LdapException) element;
                }
            }
            else
            {
                // If not a Search Entry, Search Result, or search continuation
                // we are very confused.
                // LdapSearchResults.next(): No entry found & request is not complete
                throw new LdapException(ExceptionMessages.REFERRAL_LOCAL, new object[] {"next"},
                    LdapException.LOCAL_ERROR, null);
            }
            return (LdapEntry) element;
        }

        static LdapSearchResults()
        {
            nameLock = new object();
        }
    }

    /// <summary>
    ///     Defines the options controlling search operations.
    ///     An LdapSearchConstraints object is always associated with an
    ///     LdapConnection object; its values can be changed with the
    ///     LdapConnection.setConstraints method, or overridden by passing
    ///     an LdapSearchConstraints object to the search operation.
    /// </summary>
    /// <seealso cref="LdapConstraints">
    /// </seealso>
    /// <seealso cref="LdapConnection.Constraints">
    /// </seealso>
    public class LdapSearchConstraints : LdapConstraints
    {
        private void InitBlock()
        {
            dereference = DEREF_NEVER;
        }

        /// <summary>
        ///     Returns the number of results to block on during receipt of search
        ///     results.
        ///     This should be 0 if intermediate reults are not needed,
        ///     and 1 if results are to be processed as they come in. A value of
        ///     indicates block until all results are received.  Default:
        /// </summary>
        /// <returns>
        ///     The the number of results to block on.
        /// </returns>
        /// <seealso cref="BatchSize">
        /// </seealso>
        /// <summary>
        ///     Specifies the number of results to return in a batch.
        ///     Specifying 0 means to block until all results are received.
        ///     Specifying 1 means to return results one result at a time.  Default: 1
        ///     This should be 0 if intermediate results are not needed,
        ///     and 1 if results are to be processed as they come in.  The
        ///     default is 1.
        /// </summary>
        /// <param name="batchSize">
        ///     The number of results to block on.
        /// </param>
        /// <seealso cref="BatchSize">
        /// </seealso>
        public virtual int BatchSize
        {
            get { return batchSize; }
            set { batchSize = value; }
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
        /// <seealso cref="Dereference">
        /// </seealso>
        /// <summary>
        ///     Sets a preference indicating whether or not aliases should be
        ///     dereferenced, and if so, when.
        /// </summary>
        /// <param name="dereference">
        ///     Specifies how aliases are dereference and can be set
        ///     to one of the following:
        ///     <ul>
        ///         <li>DEREF_NEVER - do not dereference aliases</li>
        ///         <li>
        ///             DEREF_FINDING - dereference aliases when finding
        ///             the base object to start the search
        ///         </li>
        ///         <li>
        ///             DEREF_SEARCHING - dereference aliases when
        ///             searching but not when finding the base
        ///             object to start the search
        ///         </li>
        ///         <li>
        ///             DEREF_ALWAYS - dereference aliases when finding
        ///             the base object and when searching
        ///         </li>
        ///     </ul>
        /// </param>
        /// <seealso cref="Dereference">
        /// </seealso>
        public virtual int Dereference
        {
            get { return dereference; }
            set { dereference = value; }
        }

        /// <summary>
        ///     Returns the maximum number of search results to be returned for
        ///     a search operation. A value of 0 means no limit.  Default: 1000
        ///     The search operation will be terminated with an
        ///     LdapException.SIZE_LIMIT_EXCEEDED if the number of results
        ///     exceed the maximum.
        /// </summary>
        /// <returns>
        ///     The value for the maximum number of results to return.
        /// </returns>
        /// <seealso cref="MaxResults">
        /// </seealso>
        /// <seealso cref="LdapException.SIZE_LIMIT_EXCEEDED">
        /// </seealso>
        /// <summary>
        ///     Sets the maximum number of search results to be returned from a
        ///     search operation. The value 0 means no limit.  The default is 1000.
        ///     The search operation will be terminated with an
        ///     LdapException.SIZE_LIMIT_EXCEEDED if the number of results
        ///     exceed the maximum.
        /// </summary>
        /// <param name="maxResults">
        ///     Maximum number of search results to return.
        /// </param>
        /// <seealso cref="MaxResults">
        /// </seealso>
        /// <seealso cref="LdapException.SIZE_LIMIT_EXCEEDED">
        /// </seealso>
        public virtual int MaxResults
        {
            get { return maxResults; }
            set { maxResults = value; }
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
        /// <seealso cref="ServerTimeLimit">
        /// </seealso>
        /// <seealso cref="LdapException.TIME_LIMIT_EXCEEDED">
        /// </seealso>
        /// <summary>
        ///     Sets the maximum number of seconds that the server is to wait when
        ///     returning search results.
        ///     The search operation will be terminated with an
        ///     LdapException.TIME_LIMIT_EXCEEDED if the operation exceeds the time
        ///     limit.
        ///     The parameter is only recognized on search operations.
        /// </summary>
        /// <param name="seconds">
        ///     The number of seconds to wait for search results.
        /// </param>
        /// <seealso cref="ServerTimeLimit">
        /// </seealso>
        /// <seealso cref="LdapException.TIME_LIMIT_EXCEEDED">
        /// </seealso>
        public virtual int ServerTimeLimit
        {
            get { return serverTimeLimit; }
            set { serverTimeLimit = value; }
        }

        private int dereference;
        private int serverTimeLimit;
        private int maxResults = 1000;
        private int batchSize = 1;
        private static object nameLock; // protect agentNum
        private static int lSConsNum = 0; // Debug, LdapConnection number
        private string name; // String name for debug

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
        ///     Constructs an LdapSearchConstraints object with a default set
        ///     of search constraints.
        /// </summary>
        public LdapSearchConstraints()
        {
            InitBlock();
            // Get a unique connection name for debug
        }

        /// <summary>
        ///     Constructs an LdapSearchConstraints object initialized with values
        ///     from an existing constraints object (LdapConstraints
        ///     or LdapSearchConstraints).
        /// </summary>
        public LdapSearchConstraints(LdapConstraints cons)
            : base(cons.TimeLimit, cons.ReferralFollowing, cons.getReferralHandler(), cons.HopLimit)
        {
            InitBlock();
            var lsc = cons.getControls();
            if (lsc != null)
            {
                var generated_var = new LdapControl[lsc.Length];
                lsc.CopyTo(generated_var, 0);
                setControls(generated_var);
            }
            var lp = cons.Properties;
            if (lp != null)
            {
                Properties = (Hashtable) lp.Clone();
            }
            if (cons is LdapSearchConstraints)
            {
                var scons = (LdapSearchConstraints) cons;
                serverTimeLimit = scons.ServerTimeLimit;
                dereference = scons.Dereference;
                maxResults = scons.MaxResults;
                batchSize = scons.BatchSize;
            }
            // Get a unique connection name for debug
        }

        /// <summary>
        ///     Constructs a new LdapSearchConstraints object and allows the
        ///     specification operational constraints in that object.
        /// </summary>
        /// <param name="msLimit">
        ///     The maximum time in milliseconds to wait for results.
        ///     The default is 0, which means that there is no
        ///     maximum time limit. This limit is enforced for an
        ///     operation by the API, not by the server.
        ///     The operation will be abandoned and terminated by the
        ///     API with an LdapException.Ldap_TIMEOUT if the
        ///     operation exceeds the time limit.
        /// </param>
        /// <param name="serverTimeLimit">
        ///     The maximum time in seconds that the server
        ///     should spend returning search results. This is a
        ///     server-enforced limit.  The default of 0 means
        ///     no time limit.
        ///     The operation will be terminated by the server with an
        ///     LdapException.TIME_LIMIT_EXCEEDED if the search
        ///     operation exceeds the time limit.
        /// </param>
        /// <param name="dereference">
        ///     Specifies when aliases should be dereferenced.
        ///     Must be either DEREF_NEVER, DEREF_FINDING,
        ///     DEREF_SEARCHING, or DEREF_ALWAYS from this class.
        ///     Default: DEREF_NEVER
        /// </param>
        /// <param name="maxResults">
        ///     The maximum number of search results to return
        ///     for a search request.
        ///     The search operation will be terminated by the server
        ///     with an LdapException.SIZE_LIMIT_EXCEEDED if the
        ///     number of results exceed the maximum.
        ///     Default: 1000
        /// </param>
        /// <param name="doReferrals">
        ///     Determines whether to automatically follow
        ///     referrals or not. Specify true to follow
        ///     referrals automatically, and false to throw
        ///     an LdapException.REFERRAL if the server responds
        ///     with a referral.
        ///     It is ignored for asynchronous operations.
        ///     Default: false
        /// </param>
        /// <param name="batchSize">
        ///     The number of results to return in a batch. Specifying
        ///     0 means to block until all results are received.
        ///     Specifying 1 means to return results one result at a
        ///     time.  Default: 1
        /// </param>
        /// <param name="handler">
        ///     The custom authentication handler called when
        ///     LdapConnection needs to authenticate, typically on
        ///     following a referral.  A null may be specified to
        ///     indicate default authentication processing, i.e.
        ///     referrals are followed with anonymous authentication.
        ///     ThE object may be an implemention of either the
        ///     the LdapBindHandler or LdapAuthHandler interface.
        ///     It is ignored for asynchronous operations.
        /// </param>
        /// <param name="hop_limit">
        ///     The maximum number of referrals to follow in a
        ///     sequence during automatic referral following.
        ///     The default value is 10. A value of 0 means no limit.
        ///     It is ignored for asynchronous operations.
        ///     The operation will be abandoned and terminated by the
        ///     API with an LdapException.REFERRAL_LIMIT_EXCEEDED if the
        ///     number of referrals in a sequence exceeds the limit.
        /// </param>
        /// <seealso cref="LdapException.Ldap_TIMEOUT">
        /// </seealso>
        /// <seealso cref="LdapException.REFERRAL">
        /// </seealso>
        /// <seealso cref="LdapException.SIZE_LIMIT_EXCEEDED">
        /// </seealso>
        /// <seealso cref="LdapException.TIME_LIMIT_EXCEEDED">
        /// </seealso>
        public LdapSearchConstraints(int msLimit, int serverTimeLimit, int dereference, int maxResults, bool doReferrals,
            int batchSize, LdapReferralHandler handler, int hop_limit) : base(msLimit, doReferrals, handler, hop_limit)
        {
            InitBlock();
            this.serverTimeLimit = serverTimeLimit;
            this.dereference = dereference;
            this.maxResults = maxResults;
            this.batchSize = batchSize;
            // Get a unique connection name for debug
        }

        static LdapSearchConstraints()
        {
            nameLock = new object();
        }
    }

    /// <summary>
    ///     This class encapsulates the combination of LdapReferral URL and
    ///     the connection opened to service this URL
    /// </summary>
    public class ReferralInfo
    {
        /// <summary>
        ///     Returns the referral URL
        /// </summary>
        /// <returns>
        ///     the Referral URL
        /// </returns>
        public virtual LdapUrl ReferralUrl => referralUrl;

        /// <summary>
        ///     Returns the referral Connection
        /// </summary>
        /// <returns>
        ///     the Referral Connection
        /// </returns>
        public virtual LdapConnection ReferralConnection => conn;

        /// <summary>
        ///     Returns the referral list
        /// </summary>
        /// <returns>
        ///     the Referral list
        /// </returns>
        public virtual string[] ReferralList => referralList;

        //		private DirectoryEntry conn;
        private readonly LdapConnection conn;
        private readonly LdapUrl referralUrl;
        private readonly string[] referralList;

        /// <summary>
        ///     Construct the ReferralInfo class
        /// </summary>
        /// <param name="lc">
        ///     The DirectoryEntry opened to process this referral
        /// </param>
        /// <param name="refUrl">
        ///     The URL string associated with this connection
        /// </param>
        public ReferralInfo(LdapConnection lc, string[] refList, LdapUrl refUrl)
        {
            conn = lc;
            referralUrl = refUrl;
            referralList = refList;
        }
    }

    /// <summary>
    ///     Encapsulates parameters of an Ldap URL query as defined in RFC2255.
    ///     An LdapUrl object can be passed to LdapConnection.search to retrieve
    ///     search results.
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    public class LdapUrl
    {
        private void InitBlock()
        {
            scope = DEFAULT_SCOPE;
        }

        /// <summary>
        ///     Returns an array of attribute names specified in the URL.
        /// </summary>
        /// <returns>
        ///     An array of attribute names in the URL.
        /// </returns>
        public virtual string[] AttributeArray => attrs;

        /// <summary>
        ///     Returns an enumerator for the attribute names specified in the URL.
        /// </summary>
        /// <returns>
        ///     An enumeration of attribute names.
        /// </returns>
        //public virtual IEnumerator Attributes
        //{
        //    get { return new ArrayEnumeration(attrs); }
        //}
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
        public virtual int Port
        {
            get
            {
                if (port == 0)
                {
                    return LdapConnection.DEFAULT_PORT;
                }
                return port;
            }
        }

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

        private static readonly int DEFAULT_SCOPE = LdapConnection.SCOPE_BASE;
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
        ///     Constructs a URL object with the specified string as the URL.
        /// </summary>
        /// <param name="url">
        ///     An Ldap URL string, e.g.
        ///     "ldap://ldap.example.com:80/dc=example,dc=com?cn,
        ///     sn?sub?(objectclass=inetOrgPerson)".
        /// </param>
        /// <exception>
        ///     MalformedURLException The specified URL cannot be parsed.
        /// </exception>
        public LdapUrl(string url)
        {
            InitBlock();
            parseURL(url);
        }

        /// <summary>
        ///     Constructs a URL object with the specified host, port, and DN.
        ///     This form is used to create URL references to a particular object
        ///     in the directory.
        /// </summary>
        /// <param name="host">
        ///     Host identifier of Ldap server, or null for
        ///     "localhost".
        /// </param>
        /// <param name="port">
        ///     The port number for Ldap server (use
        ///     LdapConnection.DEFAULT_PORT for default port).
        /// </param>
        /// <param name="dn">
        ///     Distinguished name of the base object of the search.
        /// </param>
        public LdapUrl(string host, int port, string dn)
        {
            InitBlock();
            this.host = host;
            this.port = port;
            this.dn = dn;
        }

        /// <summary>
        ///     Constructs an Ldap URL with all fields explicitly assigned, to
        ///     specify an Ldap search operation.
        /// </summary>
        /// <param name="host">
        ///     Host identifier of Ldap server, or null for
        ///     "localhost".
        /// </param>
        /// <param name="port">
        ///     The port number for Ldap server (use
        ///     LdapConnection.DEFAULT_PORT for default port).
        /// </param>
        /// <param name="dn">
        ///     Distinguished name of the base object of the search.
        /// </param>
        /// <param name="attrNames">
        ///     Names or OIDs of attributes to retrieve.  Passing a
        ///     null array signifies that all user attributes are to be
        ///     retrieved. Passing a value of "*" allows you to specify
        ///     that all user attributes as well as any specified
        ///     operational attributes are to be retrieved.
        /// </param>
        /// <param name="scope">
        ///     Depth of search (in DN namespace). Use one of
        ///     SCOPE_BASE, SCOPE_ONE, SCOPE_SUB from LdapConnection.
        /// </param>
        /// <param name="filter">
        ///     The search filter specifying the search criteria.
        /// </param>
        /// <param name="extensions">
        ///     Extensions provide a mechanism to extend the
        ///     functionality of Ldap URLs. Currently no
        ///     Ldap URL extensions are defined. Each extension
        ///     specification is a type=value expression, and  may
        ///     be <code>null</code> or empty.  The =value part may be
        ///     omitted. The expression may be prefixed with '!' if it
        ///     is mandatory for the evaluation of the URL.
        /// </param>
        public LdapUrl(string host, int port, string dn, string[] attrNames, int scope, string filter,
            string[] extensions)
        {
            InitBlock();
            this.host = host;
            this.port = port;
            this.dn = dn;
            attrs = new string[attrNames.Length];
            attrNames.CopyTo(attrs, 0);
            this.scope = scope;
            this.filter = filter;
            this.extensions = new string[extensions.Length];
            extensions.CopyTo(this.extensions, 0);
        }

        /// <summary>
        ///     Constructs an Ldap URL with all fields explicitly assigned, including
        ///     isSecure, to specify an Ldap search operation.
        /// </summary>
        /// <param name="host">
        ///     Host identifier of Ldap server, or null for
        ///     "localhost".
        /// </param>
        /// <param name="port">
        ///     The port number for Ldap server (use
        ///     LdapConnection.DEFAULT_PORT for default port).
        /// </param>
        /// <param name="dn">
        ///     Distinguished name of the base object of the search.
        /// </param>
        /// <param name="attrNames">
        ///     Names or OIDs of attributes to retrieve.  Passing a
        ///     null array signifies that all user attributes are to be
        ///     retrieved. Passing a value of "*" allows you to specify
        ///     that all user attributes as well as any specified
        ///     operational attributes are to be retrieved.
        /// </param>
        /// <param name="scope">
        ///     Depth of search (in DN namespace). Use one of
        ///     SCOPE_BASE, SCOPE_ONE, SCOPE_SUB from LdapConnection.
        /// </param>
        /// <param name="filter">
        ///     The search filter specifying the search criteria.
        ///     from LdapConnection: SCOPE_BASE, SCOPE_ONE, SCOPE_SUB.
        /// </param>
        /// <param name="extensions">
        ///     Extensions provide a mechanism to extend the
        ///     functionality of Ldap URLs. Currently no
        ///     Ldap URL extensions are defined. Each extension
        ///     specification is a type=value expression, and  may
        ///     be <code>null</code> or empty.  The =value part may be
        ///     omitted. The expression may be prefixed with '!' if it
        ///     is mandatory for the evaluation of the URL.
        /// </param>
        /// <param name="secure">
        ///     If true creates an Ldap URL of the ldaps type
        /// </param>
        public LdapUrl(string host, int port, string dn, string[] attrNames, int scope, string filter,
            string[] extensions, bool secure)
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
        ///     Decodes a URL-encoded string.
        ///     Any occurences of %HH are decoded to the hex value represented.
        ///     However, this method does NOT decode "+" into " ".
        /// </summary>
        /// <param name="URLEncoded">
        ///     String to decode.
        /// </param>
        /// <returns>
        ///     The decoded string.
        /// </returns>
        /// <exception>
        ///     MalformedURLException The URL could not be parsed.
        /// </exception>
        public static string decode(string URLEncoded)
        {
            var searchStart = 0;
            int fieldStart;
            fieldStart = URLEncoded.IndexOf("%", searchStart);
            // Return now if no encoded data
            if (fieldStart < 0)
            {
                return URLEncoded;
            }
            // Decode the %HH value and copy to new string buffer
            var fieldEnd = 0; // end of previous field
            var dataLen = URLEncoded.Length;
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
                decoded.Append(URLEncoded.Substring(fieldEnd, fieldStart - fieldEnd));
                fieldStart += 1;
                if (fieldStart >= dataLen)
                    break;
                fieldEnd = fieldStart + 2;
                try
                {
                    decoded.Append((char) Convert.ToInt32(URLEncoded.Substring(fieldStart, fieldEnd - fieldStart), 16));
                }
                catch (FormatException ex)
                {
                    throw new UriFormatException("LdapUrl.decode: error converting hex characters to integer \"" +
                                                 ex.Message + "\"");
                }
                searchStart = fieldEnd;
                if (searchStart == dataLen)
                    break;
                fieldStart = URLEncoded.IndexOf("%", searchStart);
            }
            return decoded.ToString();
        }

        /// <summary>
        ///     Encodes an arbitrary string using the URL encoding rules.
        ///     Any illegal characters are encoded as %HH.
        /// </summary>
        /// <param name="toEncode">
        ///     The string to encode.
        /// </param>
        /// <returns>
        ///     The URL-encoded string.
        ///     Comment: An illegal character consists of any non graphical US-ASCII character, Unsafe, or reserved characters.
        /// </returns>
        public static string encode(string toEncode)
        {
            var buffer = new StringBuilder(toEncode.Length); //empty but initial capicity of 'length'
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
                    //if(temp.length()==2) this can only be two or one digit long.
                    else
                        buffer.Append("%" + Convert.ToString(currChar, 16));
                }
                else
                    buffer.Append(currChar);
            }
            return buffer.ToString();
        }

        /// <summary>
        ///     Returns the base distinguished name encapsulated in the URL.
        /// </summary>
        /// <returns>
        ///     The base distinguished name specified in the URL, or null if none.
        /// </returns>
        public virtual string getDN()
        {
            return dn;
        }

        /// <summary> Sets the base distinguished name encapsulated in the URL.</summary>
        internal virtual void setDN(string dn)
        {
            this.dn = dn;
        }

        /// <summary>
        ///     Returns a valid string representation of this Ldap URL.
        /// </summary>
        /// <returns>
        ///     The string representation of the Ldap URL.
        /// </returns>
        public override string ToString()
        {
            var url = new StringBuilder(256);
            // Scheme
            if (secure)
            {
                url.Append("ldaps://");
            }
            else
            {
                url.Append("ldap://");
            }
            // Host:port/dn
            if (ipV6)
            {
                url.Append("[" + host + "]");
            }
            else
            {
                url.Append(host);
            }
            // Port not specified
            if (port != 0)
            {
                url.Append(":" + port);
            }
            if ((object) dn == null && attrs == null && scope == DEFAULT_SCOPE && (object) filter == null &&
                extensions == null)
            {
                return url.ToString();
            }
            url.Append("/");
            if ((object) dn != null)
            {
                url.Append(dn);
            }
            if (attrs == null && scope == DEFAULT_SCOPE && (object) filter == null && extensions == null)
            {
                return url.ToString();
            }
            // attributes
            url.Append("?");
            if (attrs != null)
            {
                //should we check also for attrs != "*"
                for (var i = 0; i < attrs.Length; i++)
                {
                    url.Append(attrs[i]);
                    if (i < attrs.Length - 1)
                    {
                        url.Append(",");
                    }
                }
            }
            if (scope == DEFAULT_SCOPE && (object) filter == null && extensions == null)
            {
                return url.ToString();
            }
            // scope
            url.Append("?");
            if (scope != DEFAULT_SCOPE)
            {
                if (scope == LdapConnection.SCOPE_ONE)
                {
                    url.Append("one");
                }
                else
                {
                    url.Append("sub");
                }
            }
            if ((object) filter == null && extensions == null)
            {
                return url.ToString();
            }
            // filter
            if ((object) filter == null)
            {
                url.Append("?");
            }
            else
            {
                url.Append("?" + Filter);
            }
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

        private string[] parseList(string listStr, char delimiter, int listStart, int listEnd)
            // end of list + 1
        {
            string[] list;
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
                if (itemEnd > 0 && itemEnd < listEnd)
                {
                    itemStart = itemEnd + 1;
                }
                else
                {
                    break;
                }
            }
            // Now fill in the array with the attributes
            itemStart = listStart;
            list = new string[itemCount];
            itemCount = 0;
            while (itemStart > 0)
            {
                itemEnd = listStr.IndexOf(delimiter, itemStart);
                if (itemStart <= listEnd)
                {
                    if (itemEnd < 0)
                        itemEnd = listEnd;
                    if (itemEnd > listEnd)
                        itemEnd = listEnd;
                    list[itemCount] = listStr.Substring(itemStart, itemEnd - itemStart);
                    itemStart = itemEnd + 1;
                    itemCount += 1;
                }
                else
                {
                    break;
                }
            }
            return list;
        }

        private void parseURL(string url)
        {
            var scanStart = 0;
            var scanEnd = url.Length;
            if ((object) url == null)
                throw new UriFormatException("LdapUrl: URL cannot be null");
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
                port = LdapConnection.DEFAULT_PORT;
            }
            else if (url.Substring(scanStart, scanStart + 8 - scanStart).ToUpper().Equals("ldaps://".ToUpper()))
            {
                secure = true;
                scanStart += 8;
                port = LdapConnection.DEFAULT_SSL_PORT;
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
                /*
                                * Kludge. check for ldap://111.222.333.444:389??cn=abc,o=company
                                *
                                * Check for broken Novell referral format.  The dn is in
                                * the scope position, but the required slash is missing.
                                * This is illegal syntax but we need to account for it.
                                * Fortunately it can't be confused with anything real.
                                */
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
            if (attrsStart < 0)
            {
                dn = url.Substring(scanStart, scanEnd - scanStart);
            }
            else
            {
                dn = url.Substring(scanStart, attrsStart - scanStart);
            }
            scanStart = attrsStart + 1;
            // Wierd novell syntax can have nothing beyond the dn
            if (scanStart >= scanEnd || attrsStart < 0 || novell)
                return;
            // Parse out the attributes
            var scopeStart = url.IndexOf('?', scanStart);
            if (scopeStart < 0)
                scopeStart = scanEnd - 1;
            attrs = parseList(url, ',', attrsStart + 1, scopeStart);
            scanStart = scopeStart + 1;
            if (scanStart >= scanEnd)
                return;
            // Parse out the scope
            var filterStart = url.IndexOf('?', scanStart);
            string scopeStr;
            if (filterStart < 0)
            {
                scopeStr = url.Substring(scanStart, scanEnd - scanStart);
            }
            else
            {
                scopeStr = url.Substring(scanStart, filterStart - scanStart);
            }
            if (scopeStr.ToUpper().Equals("".ToUpper()))
            {
                scope = LdapConnection.SCOPE_BASE;
            }
            else if (scopeStr.ToUpper().Equals("base".ToUpper()))
            {
                scope = LdapConnection.SCOPE_BASE;
            }
            else if (scopeStr.ToUpper().Equals("one".ToUpper()))
            {
                scope = LdapConnection.SCOPE_ONE;
            }
            else if (scopeStr.ToUpper().Equals("sub".ToUpper()))
            {
                scope = LdapConnection.SCOPE_SUB;
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
            string filterStr;
            var extStart = url.IndexOf('?', scanStart);
            if (extStart < 0)
            {
                filterStr = url.Substring(scanStart, scanEnd - scanStart);
            }
            else
            {
                filterStr = url.Substring(scanStart, extStart - scanStart);
            }
            if (!filterStr.Equals(""))
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
            extensions = parseList(url, ',', scanStart, scanEnd);
        }
    }

    /// <summary>
    ///     A message received from an LdapServer
    ///     in response to an asynchronous request.
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    /*
        * Note: Exceptions generated by the reader thread are returned
        * to the application as an exception in an LdapResponse.  Thus
        * if <code>exception</code> has a value, it is not a server response,
        * but instad an exception returned to the application from the API.
        */
    public class LdapResponse : LdapMessage
    {
        /// <summary>
        ///     Returns any error message in the response.
        /// </summary>
        /// <returns>
        ///     Any error message in the response.
        /// </returns>
        public virtual string ErrorMessage
        {
            get
            {
                if (exception != null)
                {
                    return exception.LdapErrorMessage;
                }
                /*				RfcResponse resp=(RfcResponse)( message.Response);
                                if(resp == null)
                                    Console.WriteLine(" Response is null");
                                else
                                    Console.WriteLine(" Response is non null");
                                string str=resp.getErrorMessage().stringValue();
                                if( str==null)
                                     Console.WriteLine("str is null..");
                                Console.WriteLine(" Response is non null" + str);
                                return str;
                */
                return ((RfcResponse) message.Response).getErrorMessage().StringValue();
            }
        }

        /// <summary>
        ///     Returns the partially matched DN field from the server response,
        ///     if the response contains one.
        /// </summary>
        /// <returns>
        ///     The partially matched DN field, if the response contains one.
        /// </returns>
        public virtual string MatchedDN
        {
            get
            {
                if (exception != null)
                {
                    return exception.MatchedDN;
                }
                return ((RfcResponse) message.Response).getMatchedDN().StringValue();
            }
        }

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
                string[] referrals = null;
                var ref_Renamed = ((RfcResponse) message.Response).getReferral();
                if (ref_Renamed == null)
                {
                    referrals = new string[0];
                }
                else
                {
                    // convert RFC 2251 Referral to String[]
                    var size = ref_Renamed.Size();
                    referrals = new string[size];
                    for (var i = 0; i < size; i++)
                    {
                        var aRef = ((Asn1OctetString) ref_Renamed.Get(i)).StringValue();
                        try
                        {
                            // get the referral URL
                            var urlRef = new LdapUrl(aRef);
                            if ((object) urlRef.getDN() == null)
                            {
                                var origMsg = Asn1Object.RequestingMessage.Asn1Object;
                                string dn;
                                if ((object) (dn = origMsg.RequestDN) != null)
                                {
                                    urlRef.setDN(dn);
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
                if ((RfcResponse) message.Response is RfcIntermediateResponse)
                    return 0;
                return ((RfcResponse) message.Response).getResultCode().IntValue();
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
                        ex = new LdapReferralException("Automatic referral following not enabled",
                            LdapException.REFERRAL, ErrorMessage);
                        ((LdapReferralException) ex).setReferrals(refs);
                        break;
                    default:
                        ex = new LdapException(LdapException.resultCodeToString(ResultCode), ResultCode, ErrorMessage,
                            MatchedDN);
                        break;
                }
                return ex;
            }
        }

        /// <summary>
        ///     Returns any controls in the message.
        /// </summary>
        /// <seealso cref="Novell.Directory.Ldap.LdapMessage.Controls">
        /// </seealso>
        public override LdapControl[] Controls
        {
            get
            {
                if (exception != null)
                {
                    return null;
                }
                return base.Controls;
            }
        }

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
        ///     Creates an LdapResponse using an LdapException.
        ///     Used to wake up the user following an abandon.
        ///     Note: The abandon doesn't have to be user initiated
        ///     but may be the result of error conditions.
        ///     Referral information is available if this connection created solely
        ///     to follow a referral.
        /// </summary>
        /// <param name="ex">
        ///     The exception
        /// </param>
        /// <param name="activeReferral">
        ///     The referral actually used to create the
        ///     connection
        /// </param>
        public LdapResponse(LdapException ex, ReferralInfo activeReferral)
        {
            exception = ex;
            this.activeReferral = activeReferral;
        }

        /// <summary>
        ///     Creates a response LdapMessage when receiving an asynchronous
        ///     response from a server.
        /// </summary>
        /// <param name="message">
        ///     The RfcLdapMessage from a server.
        /// </param>
        /*package*/
        internal LdapResponse(RfcLdapMessage message) : base(message)
        {
        }

        /// <summary>
        ///     Creates a SUCCESS response LdapMessage. Typically the response
        ///     comes from a source other than a BER encoded Ldap message,
        ///     such as from DSML.  Other values which are allowed in a response
        ///     are set to their empty values.
        /// </summary>
        /// <param name="type">
        ///     The message type as defined in LdapMessage.
        /// </param>
        /// <seealso cref="LdapMessage">
        /// </seealso>
        public LdapResponse(int type) : this(type, LdapException.SUCCESS, null, null)
        {
        }

        /// <summary>
        ///     Creates a response LdapMessage from parameters. Typically the data
        ///     comes from a source other than a BER encoded Ldap message,
        ///     such as from DSML.
        /// </summary>
        /// <param name="type">
        ///     The message type as defined in LdapMessage.
        /// </param>
        /// <param name="resultCode">
        ///     The result code as defined in LdapException.
        /// </param>
        /// <param name="matchedDN">
        ///     The name of the lowest entry that was matched
        ///     for some error result codes, an empty string
        ///     or <code>null</code> if none.
        /// </param>
        /// <param name="serverMessage">
        ///     A diagnostic message returned by the server,
        ///     an empty string or <code>null</code> if none.
        /// </param>
        /// <seealso cref="LdapMessage">
        /// </seealso>
        /// <seealso cref="LdapException">
        /// </seealso>
        public LdapResponse(int type, int resultCode, string matchedDN, string serverMessage)
            : base(new RfcLdapMessage(RfcResultFactory(type, resultCode, matchedDN, serverMessage)))
        {
        }

        private static Asn1Sequence RfcResultFactory(int type, int resultCode, string matchedDN, string serverMessage)
        {
            Asn1Sequence ret;
            if ((object) matchedDN == null)
                matchedDN = "";
            if ((object) serverMessage == null)
                serverMessage = "";
            switch (type)
            {
                case SEARCH_RESULT:
                    ret = new RfcSearchResultDone(new Asn1Enumerated(resultCode), new RfcLdapDN(matchedDN),
                        new RfcLdapString(serverMessage), null);
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
        internal virtual void chkResultCode()
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

        /* Methods from LdapMessage */

        /// <summary>
        ///     Indicates if this response is an embedded exception response
        /// </summary>
        /// <returns>
        ///     true if contains an embedded Ldapexception
        /// </returns>
        /*package*/
        internal virtual bool hasException()
        {
            return exception != null;
        }
    }

    /// <summary>
    ///     The <code>MessageVector</code> class implements extends the
    ///     existing Vector class so that it can be used to maintain a
    ///     list of currently registered control responses.
    /// </summary>
    public class RespControlVector : ArrayList
    {
        public RespControlVector(int cap, int incr) : base(cap)
        {
        }

        /// <summary>
        ///     Inner class defined to create a temporary object to encapsulate
        ///     all registration information about a response control.  This class
        ///     cannot be used outside this class
        /// </summary>
        private class RegisteredControl
        {
            private RespControlVector enclosingInstance;
            public RespControlVector Enclosing_Instance => enclosingInstance;
            public readonly string myOID;
            public readonly Type myClass;

            public RegisteredControl(RespControlVector enclosingInstance, string oid, Type controlClass)
            {
                this.enclosingInstance = enclosingInstance;
                myOID = oid;
                myClass = controlClass;
            }
        }

        /* Adds a control to the current list of registered response controls.
                *
                */

        public void registerResponseControl(string oid, Type controlClass)
        {
            lock (this)
            {
                Add(new RegisteredControl(this, oid, controlClass));
            }
        }

        /* Searches the list of registered controls for a mathcing control.  We
                * search using the OID string.  If a match is found we return the
                * Class name that was provided to us on registration.
                */

        public Type findResponseControl(string searchOID)
        {
            lock (this)
            {
                RegisteredControl ctl = null;
                /* loop through the contents of the vector */
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

    /*
        * Represents an LdapOID.
        */

    public class RfcLdapOID : Asn1OctetString
    {
        public RfcLdapOID(string s) : base(s)
        {
        }

        public RfcLdapOID(sbyte[] s) : base(s)
        {
        }
    }

    /// <summary>
    ///     Represents an Ldap Control.
    ///     <pre>
    ///         Control ::= SEQUENCE {
    ///         controlType             LdapOID,
    ///         criticality             BOOLEAN DEFAULT FALSE,
    ///         controlValue            OCTET STRING OPTIONAL }
    ///     </pre>
    /// </summary>
    public class RfcControl : Asn1Sequence
    {
        public virtual Asn1OctetString ControlType => (Asn1OctetString) Get(0);

        /// <summary>
        ///     Returns criticality.
        ///     If no value present, return the default value of FALSE.
        /// </summary>
        public virtual Asn1Boolean Criticality
        {
            get
            {
                if (Size() > 1)
                {
                    // MAY be a criticality
                    var obj = Get(1);
                    if (obj is Asn1Boolean)
                        return (Asn1Boolean) obj;
                }
                return new Asn1Boolean(false);
            }
        }

        /// <summary>
        ///     Since controlValue is an OPTIONAL component, we need to check
        ///     to see if one is available. Remember that if criticality is of default
        ///     value, it will not be present.
        /// </summary>
        /// <summary>
        ///     Called to set/replace the ControlValue.  Will normally be called by
        ///     the child classes after the parent has been instantiated.
        /// </summary>
        public virtual Asn1OctetString ControlValue
        {
            get
            {
                if (Size() > 2)
                {
                    // MUST be a control value
                    return (Asn1OctetString) Get(2);
                }
                if (Size() > 1)
                {
                    // MAY be a control value
                    var obj = Get(1);
                    if (obj is Asn1OctetString)
                        return (Asn1OctetString) obj;
                }
                return null;
            }
            set
            {
                if (value == null)
                    return;
                if (Size() == 3)
                {
                    // We already have a control value, replace it
                    Set(2, value);
                    return;
                }
                if (Size() == 2)
                {
                    // Get the second element
                    var obj = Get(1);
                    // Is this a control value
                    if (obj is Asn1OctetString)
                    {
                        // replace this one
                        Set(1, value);
                    }
                    else
                    {
                        // add a new one at the end
                        Add(value);
                    }
                }
            }
        }

        public RfcControl(RfcLdapOID controlType) : this(controlType, new Asn1Boolean(false), null)
        {
        }

        public RfcControl(RfcLdapOID controlType, Asn1Boolean criticality) : this(controlType, criticality, null)
        {
        }

        /// <summary>
        ///     Note: criticality is only added if true, as per RFC 2251 sec 5.1 part
        ///     (4): If a value of a type is its default value, it MUST be
        ///     absent.
        /// </summary>
        public RfcControl(RfcLdapOID controlType, Asn1Boolean criticality, Asn1OctetString controlValue) : base(3)
        {
            Add(controlType);
            if (criticality.BooleanValue())
                Add(criticality);
            if (controlValue != null)
                Add(controlValue);
        }

        /// <summary> Constructs a Control object by decoding it from an InputStream.</summary>
        public RfcControl(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
        }

        /// <summary> Constructs a Control object by decoding from an Asn1Sequence</summary>
        public RfcControl(Asn1Sequence seqObj) : base(3)
        {
            var len = seqObj.Size();
            for (var i = 0; i < len; i++)
                Add(seqObj.Get(i));
        }
    }

    /// <summary>
    ///     Encapsulates optional additional parameters or constraints to be applied to
    ///     an Ldap operation.
    ///     When included with LdapConstraints or LdapSearchConstraints
    ///     on an LdapConnection or with a specific operation request, it is
    ///     sent to the server along with operation requests.
    /// </summary>
    /// <seealso cref="LdapConnection.ResponseControls">
    /// </seealso>
    /// <seealso cref="LdapConstraints.getControls">
    /// </seealso>
    /// <seealso cref="LdapConstraints.setControls">
    /// </seealso>
    public class LdapControl
    {
        /// <summary>
        ///     Returns the identifier of the control.
        /// </summary>
        /// <returns>
        ///     The object ID of the control.
        /// </returns>
        public virtual string ID => new StringBuilder(control.ControlType.StringValue()).ToString();

        /// <summary>
        ///     Returns whether the control is critical for the operation.
        /// </summary>
        /// <returns>
        ///     Returns true if the control must be supported for an associated
        ///     operation to be executed, and false if the control is not required for
        ///     the operation.
        /// </returns>
        public virtual bool Critical => control.Criticality.BooleanValue();

        internal static RespControlVector RegisteredControls => registeredControls;

        /// <summary>
        ///     Returns the RFC 2251 Control object.
        /// </summary>
        /// <returns>
        ///     An ASN.1 RFC 2251 Control.
        /// </returns>
        internal virtual RfcControl Asn1Object => control;

        private static readonly RespControlVector registeredControls;
        private RfcControl control; // An RFC 2251 Control

        /// <summary>
        ///     Constructs a new LdapControl object using the specified values.
        /// </summary>
        /// <param name="oid">
        ///     The OID of the control, as a dotted string.
        /// </param>
        /// <param name="critical">
        ///     True if the Ldap operation should be discarded if
        ///     the control is not supported. False if
        ///     the operation can be processed without the control.
        /// </param>
        /// <param name="values">
        ///     The control-specific data.
        /// </param>
        public LdapControl(string oid, bool critical, sbyte[] values)
        {
            if ((object) oid == null)
            {
                throw new ArgumentException("An OID must be specified");
            }
            if (values == null)
            {
                control = new RfcControl(new RfcLdapOID(oid), new Asn1Boolean(critical));
            }
            else
            {
                control = new RfcControl(new RfcLdapOID(oid), new Asn1Boolean(critical), new Asn1OctetString(values));
            }
        }

        /// <summary> Create an LdapControl from an existing control.</summary>
        protected internal LdapControl(RfcControl control)
        {
            this.control = control;
        }

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
                cont = (LdapControl) MemberwiseClone();
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
            var vals = getValue();
            sbyte[] twin = null;
            if (vals != null)
            {
                //is this necessary?
                // Yes even though the contructor above allocates a
                // new Asn1OctetString, vals in that constuctor
                // is only copied by reference
                twin = new sbyte[vals.Length];
                for (var i = 0; i < vals.Length; i++)
                {
                    twin[i] = vals[i];
                }
                cont.control = new RfcControl(new RfcLdapOID(ID), new Asn1Boolean(Critical), new Asn1OctetString(twin));
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
        public virtual sbyte[] getValue()
        {
            sbyte[] result = null;
            var val = control.ControlValue;
            if (val != null)
            {
                result = val.ByteValue();
            }
            return result;
        }

        /// <summary>
        ///     Sets the control-specific data of the object.  This method is for
        ///     use by an extension of LdapControl.
        /// </summary>
        protected internal virtual void setValue(sbyte[] controlValue)
        {
            control.ControlValue = new Asn1OctetString(controlValue);
        }

        /// <summary>
        ///     Registers a class to be instantiated on receipt of a control with the
        ///     given OID.
        ///     Any previous registration for the OID is overridden. The
        ///     controlClass must be an extension of LdapControl.
        /// </summary>
        /// <param name="oid">
        ///     The object identifier of the control.
        /// </param>
        /// <param name="controlClass">
        ///     A class which can instantiate an LdapControl.
        /// </param>
        public static void register(string oid, Type controlClass)
        {
            registeredControls.registerResponseControl(oid, controlClass);
        }

        static LdapControl()
        {
            registeredControls = new RespControlVector(5, 5);
        }
    }
}

#endif