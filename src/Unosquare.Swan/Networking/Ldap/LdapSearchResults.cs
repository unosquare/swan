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
        private static readonly object SyncRoot = new object();

        private readonly List<RfcLdapMessage> _messages;
        private readonly int _messageId;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapSearchResults" /> class.
        /// </summary>
        /// <param name="messages">The messages.</param>
        /// <param name="messageId">The message identifier.</param>
        internal LdapSearchResults(List<RfcLdapMessage> messages, int messageId)
        {
            lock (SyncRoot)
                _messages = messages;

            _messageId = messageId;
        }

        /// <summary>
        /// Returns a count of the items in the search result.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return _messages
                        .Count(x => x.MessageId == _messageId && GetResponse(x) is LdapSearchResult);
                }
            }
        }

        /// <summary>
        /// Reports if there are more search results.
        /// </summary>
        /// <returns>
        /// true if there are more search results.
        /// </returns>
        public bool HasMore()
        {
            lock (SyncRoot)
                return _messages.Any(x => x.MessageId == _messageId && GetResponse(x) is LdapSearchResult);
        }

        /// <summary>
        /// Returns the next result as an LdapEntry.
        /// </summary>
        /// <returns>
        /// The next search result as an LdapEntry.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Next - No more results.</exception>
        public LdapEntry Next()
        {
            lock (SyncRoot)
            {
                var list = _messages
                    .Where(x => x.MessageId == _messageId)
                    .ToList();

                foreach (var item in list)
                {
                    _messages.Remove(item);
                    var response = GetResponse(item);

                    if (response is LdapSearchResult result)
                    {
                        return result.Entry;
                    }
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