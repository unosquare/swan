#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     An LdapSearchResults object is returned from a synchronous search
    ///     operation. It provides access to all results received during the
    ///     operation (entries and exceptions).
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    public class LdapSearchResults
    {
        private LdapConnection conn; // LdapConnection which started search
        private int _messageId;

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
        public virtual int Count => new List<RfcLdapMessage>(conn.Messages)
            .Count(x => x.MessageID == _messageId && GetResponse(x) is LdapSearchResult);

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapSearchResults"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="messageId">The message identifier.</param>
        internal LdapSearchResults(LdapConnection connection, int messageId)
        {
            conn = connection;
            _messageId = messageId;
        }

        /// <summary>
        ///     Reports if there are more search results.
        /// </summary>
        /// <returns>
        ///     true if there are more search results.
        /// </returns>
        public virtual bool hasMore() => new List<RfcLdapMessage>(conn.Messages)
            .Any(x => x.MessageID == _messageId && GetResponse(x) is LdapSearchResult);

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
            var list = new List<RfcLdapMessage>(conn.Messages)
                .Where(x => x.MessageID == _messageId);

            foreach (var item in list)
            {
                conn.Messages.Remove(item);
                var response = GetResponse(item);

                if (response is LdapSearchResult)
                {
                    return (response as LdapSearchResult).Entry;
                }
            }

            throw new ArgumentOutOfRangeException("LdapSearchResults.next() no more results");
        }
        
        private LdapMessage GetResponse(RfcLdapMessage item)
        {
            switch (item.Type)
            {
                case LdapMessage.SEARCH_RESPONSE:
                    return new LdapSearchResult(item);
                case LdapMessage.SEARCH_RESULT_REFERENCE:
                    return new LdapSearchResultReference(item);
                default:
                    return new LdapResponse(item);
            }
        }
    }
}

#endif