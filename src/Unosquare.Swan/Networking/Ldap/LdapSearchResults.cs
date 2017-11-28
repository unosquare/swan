#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An LdapSearchResults object is returned from a synchronous search
    /// operation. It provides access to all results received during the
    /// operation (entries and exceptions).
    /// </summary>
    /// <seealso cref="LdapConnection.Search"></seealso>
    public sealed class LdapSearchResults
    {
        private readonly LdapConnection _conn; // LdapConnection which started search
        private readonly int _messageId;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapSearchResults"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="messageId">The message identifier.</param>
        internal LdapSearchResults(LdapConnection connection, int messageId)
        {
            _conn = connection;
            _messageId = messageId;
        }

        /// <summary>
        /// Returns a count of the items in the search result.
        /// Returns a count of the entries and exceptions remaining in the object.
        /// If the search was submitted with a batch size greater than zero,
        /// getCount reports the number of results received so far but not enumerated
        /// with next().  If batch size equals zero, getCount reports the number of
        /// items received, since the application thread blocks until all results are
        /// received.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => new List<RfcLdapMessage>(_conn.Messages)
            .Count(x => x.MessageId == _messageId && GetResponse(x) is LdapSearchResult);

        /// <summary>
        /// Reports if there are more search results.
        /// </summary>
        /// <returns>
        /// true if there are more search results.
        /// </returns>
        public bool HasMore() => new List<RfcLdapMessage>(_conn.Messages)
            .Any(x => x.MessageId == _messageId && GetResponse(x) is LdapSearchResult);

        /// <summary>
        /// Returns the next result as an LdapEntry.
        /// If automatic referral following is disabled or if a referral
        /// was not followed, next() will throw an LdapReferralException
        /// when the referral is received.
        /// </summary>
        /// <returns>
        /// The next search result as an LdapEntry.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Next - No more results</exception>
        public LdapEntry Next()
        {
            var list = new List<RfcLdapMessage>(_conn.Messages)
                .Where(x => x.MessageId == _messageId);

            foreach (var item in list)
            {
                _conn.Messages.Remove(item);
                var response = GetResponse(item);

                if (response is LdapSearchResult)
                {
                    return (response as LdapSearchResult).Entry;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(Next), "No more results");
        }
        
        private static LdapMessage GetResponse(RfcLdapMessage item)
        {
            switch (item.Type)
            {
                case LdapOperation.SearchResponse:
                    return new LdapSearchResult(item);
                case LdapOperation.SearchResultReference:
                    return new LdapSearchResultReference(item);
                default:
                    return new LdapResponse(item);
            }
        }
    }
}

#endif