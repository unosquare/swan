#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    /// <summary>
    /// LDAP Operation
    /// </summary>
    internal enum LdapOperation
    {
        /// <summary>
        /// The unknown
        /// </summary>
        Unknown = -1,

        /// <summary>
        ///     A bind request operation.
        ///     BIND_REQUEST = 0
        /// </summary>
        BindRequest = 0,

        /// <summary>
        ///     A bind response operation.
        ///     BIND_RESPONSE = 1
        /// </summary>
        BindResponse = 1,

        /// <summary>
        ///     An unbind request operation.
        ///     UNBIND_REQUEST = 2
        /// </summary>
        UnbindRequest = 2,

        /// <summary>
        ///     A search request operation.
        ///     SEARCH_REQUEST = 3
        /// </summary>
        SearchRequest = 3,

        /// <summary>
        ///     A search response containing data.
        ///     SEARCH_RESPONSE = 4
        /// </summary>
        SearchResponse = 4,

        /// <summary>
        ///     A search result message - contains search status.
        ///     SEARCH_RESULT = 5
        /// </summary>
        SearchResult = 5,

        /// <summary>
        ///     A modify request operation.
        ///     MODIFY_REQUEST = 6
        /// </summary>
        ModifyRequest = 6,

        /// <summary>
        ///     A modify response operation.
        ///     MODIFY_RESPONSE = 7
        /// </summary>
        ModifyResponse = 7,

        /// <summary>
        ///     An abandon request operation.
        ///     ABANDON_REQUEST = 16
        /// </summary>
        AbandonRequest = 16,

        /// <summary>
        ///     A search result reference operation.
        ///     SEARCH_RESULT_REFERENCE = 19
        /// </summary>
        SearchResultReference = 19,

        /// <summary>
        ///     An extended request operation.
        ///     EXTENDED_REQUEST = 23
        /// </summary>
        ExtendedRequest = 23,

        /// <summary>
        ///     An extended response operation.
        ///     EXTENDED_RESONSE = 24
        /// </summary>
        ExtendedResponse = 24,

        /// <summary>
        ///     An intermediate response operation.
        ///     INTERMEDIATE_RESONSE = 25
        /// </summary>
        IntermediateResponse = 25
    }

    /// <summary>
    /// ASN1 tags
    /// </summary>
    internal enum Asn1IdentifierTag
    {
        /// <summary>
        /// Universal tag class.
        /// </summary>
        Universal = 0,

        /// <summary>
        ///     Application-wide tag class.
        /// </summary>
        Application = 1,

        /// <summary>
        ///     Context-specific tag class.
        /// </summary>
        Context = 2,

        /// <summary>
        ///     Private-use tag class.
        /// </summary>
        Private = 3
    }
}
#endif