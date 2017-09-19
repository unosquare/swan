#if !UWP
using System.IO;

namespace Unosquare.Swan.Networking.Ldap
{
    /// <summary>
    /// Represents an Ldap Modify Request.
    /// <pre>
    /// ModifyRequest ::= [APPLICATION 6] SEQUENCE {
    /// object          LdapDN,
    /// modification    SEQUENCE OF SEQUENCE {
    /// operation       ENUMERATED {
    /// add     (0),
    /// delete  (1),
    /// replace (2) },
    /// modification    AttributeTypeAndValues } }
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.IRfcRequest" />
    internal class RfcModifyRquest : Asn1Sequence, IRfcRequest
    {
        public RfcModifyRquest(RfcLdapDN objectRenamed, Asn1SequenceOf modification)
            : base(2)
        {
            Add(objectRenamed);
            Add(modification);
        }

        internal RfcModifyRquest(Asn1Object[] origiRequest, string baseRenamed)
            : base(origiRequest, origiRequest.Length)
        {
            if ((object)baseRenamed != null)
            {
                SetRenamed(0, new RfcLdapDN(baseRenamed));
            }
        }

        public virtual Asn1SequenceOf Modifications
        {
            get { return (Asn1SequenceOf)GetRenamed(1); }
        }

        public IRfcRequest DupRequest(string requestBase, string filter, bool reference)
        {
            return new RfcModifyRquest(ToArray(), requestBase);
        }

        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.MODIFY_REQUEST);
        }

        public string GetRequestDN()
        {
            return ((RfcLdapDN)GetRenamed(0)).StringValue();
        }
    }

    internal class RfcModifyResponse : RfcLdapResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RfcModifyResponse"/> class.
        /// The only time a client will create a ModifyResponse is when it is
        /// decoding it from an InputStream
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="in_Renamed">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcModifyResponse(IAsn1Decoder dec, Stream in_Renamed, int len) 
            : base(dec, in_Renamed, len)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcModifyResponse"/> class.
        /// </summary>
        /// <param name="resultCode">the result code of the operation</param>
        /// <param name="matchedDN">the matched DN returned from the server</param>
        /// <param name="errorMessage">the diagnostic message returned from the server</param>
        /// <param name="referral">the referral(s) returned by the server</param>
        public RfcModifyResponse(Asn1Enumerated resultCode, RfcLdapDN matchedDN, RfcLdapString errorMessage)
            : base(resultCode, matchedDN, errorMessage)
        {
        }

        /// <summary>
        /// Override getIdentifier to return an application-wide id.
        /// </summary>
        /// <returns></returns>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.MODIFY_RESPONSE);
        }
    }
}
#endif