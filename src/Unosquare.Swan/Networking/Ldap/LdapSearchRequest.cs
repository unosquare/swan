#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System.Collections;

    /// <summary>
    /// Represents an Ldap Search request.
    /// </summary>
    /// <seealso cref="LdapMessage" />
    internal sealed class LdapSearchRequest : LdapMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapSearchRequest"/> class.
        /// </summary>
        /// <param name="ldapBase">The base distinguished name to search from.</param>
        /// <param name="scope">The scope of the entries to search. The following
        /// are the valid options:
        /// <ul><li>SCOPE_BASE - searches only the base DN</li><li>SCOPE_ONE - searches only entries under the base DN</li><li>
        /// SCOPE_SUB - searches the base DN and all entries
        /// within its subtree
        /// </li></ul></param>
        /// <param name="filter">The search filter specifying the search criteria.</param>
        /// <param name="attrs">The names of attributes to retrieve.
        /// operation exceeds the time limit.</param>
        /// <param name="dereference">Specifies when aliases should be dereferenced.
        /// Must be one of the constants defined in
        /// LdapConstraints, which are DEREF_NEVER,
        /// DEREF_FINDING, DEREF_SEARCHING, or DEREF_ALWAYS.</param>
        /// <param name="maxResults">The maximum number of search results to return
        /// for a search request.
        /// The search operation will be terminated by the server
        /// with an LdapException.SIZE_LIMIT_EXCEEDED if the
        /// number of results exceed the maximum.</param>
        /// <param name="serverTimeLimit">The maximum time in seconds that the server
        /// should spend returning search results. This is a
        /// server-enforced limit.  A value of 0 means
        /// no time limit.</param>
        /// <param name="typesOnly">If true, returns the names but not the values of
        /// the attributes found.  If false, returns the
        /// names and values for attributes found.</param>
        /// <param name="cont">Any controls that apply to the search request.
        /// or null if none.</param>
        /// <seealso cref="LdapConnection.Search"></seealso>
        public LdapSearchRequest(
            string ldapBase,
            int scope,
            string filter,
            string[] attrs,
            int dereference,
            int maxResults,
            int serverTimeLimit,
            bool typesOnly,
            LdapControl[] cont)
            : base(
                LdapOperation.SearchRequest,
                new RfcSearchRequest(ldapBase, scope, dereference, maxResults, serverTimeLimit, typesOnly,  filter, attrs),
                cont)
        {
        }

        /// <summary>
        /// Retrieves an Iterator object representing the parsed filter for
        /// this search request.
        /// The first object returned from the Iterator is an Integer indicating
        /// the type of filter component. One or more values follow the component
        /// type as subsequent items in the Iterator. The pattern of Integer
        /// component type followed by values continues until the end of the
        /// filter.
        /// Values returned as a byte array may represent UTF-8 characters or may
        /// be binary values. The possible Integer components of a search filter
        /// and the associated values that follow are:
        /// <ul><li>AND - followed by an Iterator value</li><li>OR - followed by an Iterator value</li><li>NOT - followed by an Iterator value</li><li>
        /// EQUALITY_MATCH - followed by the attribute name represented as a
        /// String, and by the attribute value represented as a byte array
        /// </li><li>
        /// GREATER_OR_EQUAL - followed by the attribute name represented as a
        /// String, and by the attribute value represented as a byte array
        /// </li><li>
        /// LESS_OR_EQUAL - followed by the attribute name represented as a
        /// String, and by the attribute value represented as a byte array
        /// </li><li>
        /// APPROX_MATCH - followed by the attribute name represented as a
        /// String, and by the attribute value represented as a byte array
        /// </li><li>PRESENT - followed by a attribute name respresented as a String</li><li>
        /// EXTENSIBLE_MATCH - followed by the name of the matching rule
        /// represented as a String, by the attribute name represented
        /// as a String, and by the attribute value represented as a
        /// byte array.
        /// </li><li>
        /// SUBSTRINGS - followed by the attribute name represented as a
        /// String, by one or more SUBSTRING components (INITIAL, ANY,
        /// or FINAL) followed by the SUBSTRING value.
        /// </li></ul>
        /// </summary>
        /// <value>
        /// The search filter.
        /// </value>
        public IEnumerator SearchFilter => RfcFilter.GetFilterIterator();

        /// <summary>
        ///     Retrieves the Base DN for a search request.
        /// </summary>
        /// <returns>
        ///     the base DN for a search request
        /// </returns>
        public string DN => Asn1Object.RequestDn;

        /// <summary>
        /// Retrieves the scope of a search request.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        public int Scope => ((Asn1Enumerated)((RfcSearchRequest)Asn1Object.Get(1)).Get(1)).IntValue();

        /// <summary>
        /// Retrieves the behaviour of dereferencing aliases on a search request.
        /// </summary>
        /// <value>
        /// The dereference.
        /// </value>
        public int Dereference => ((Asn1Enumerated)((RfcSearchRequest)Asn1Object.Get(1)).Get(2)).IntValue();

        /// <summary>
        /// Retrieves the maximum number of entries to be returned on a search.
        /// </summary>
        /// <value>
        /// The maximum results.
        /// </value>
        public int MaxResults => ((Asn1Integer)((RfcSearchRequest)Asn1Object.Get(1)).Get(3)).IntValue();

        /// <summary>
        /// Retrieves the server time limit for a search request.
        /// </summary>
        /// <value>
        /// The server time limit.
        /// </value>
        public int ServerTimeLimit => ((Asn1Integer)((RfcSearchRequest)Asn1Object.Get(1)).Get(4)).IntValue();

        /// <summary>
        /// Retrieves whether attribute values or only attribute types(names) should
        /// be returned in a search request.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [types only]; otherwise, <c>false</c>.
        /// </value>
        public bool TypesOnly => ((Asn1Boolean)((RfcSearchRequest)Asn1Object.Get(1)).Get(5)).BooleanValue();

        /// <summary>
        /// Retrieves an array of attribute names to request for in a search.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public string[] Attributes
        {
            get
            {
                var attrs = (RfcAttributeDescriptionList)((RfcSearchRequest)Asn1Object.Get(1)).Get(7);
                var values = new string[attrs.Size()];
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = ((Asn1OctetString)attrs.Get(i)).StringValue();
                }

                return values;
            }
        }

        /// <summary>
        /// Creates a string representation of the filter in this search request.
        /// </summary>
        /// <value>
        /// The string filter.
        /// </value>
        public string StringFilter => RfcFilter.FilterToString();

        /// <summary>
        /// Retrieves an SearchFilter object representing a filter for a search request
        /// </summary>
        /// <value>
        /// The RFC filter.
        /// </value>
        private RfcFilter RfcFilter => (RfcFilter)((RfcSearchRequest)Asn1Object.Get(1)).Get(6);
    }
}

#endif