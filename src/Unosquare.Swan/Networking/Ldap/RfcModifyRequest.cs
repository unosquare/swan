﻿#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System.IO;

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
    internal class RfcModifyRequest 
        : Asn1Sequence, IRfcRequest
    {
        public RfcModifyRequest(RfcLdapDN obj, Asn1SequenceOf modification)
            : base(2)
        {
            Add(obj);
            Add(modification);
        }

        internal RfcModifyRequest(Asn1Object[] origiRequest, string baseRenamed)
            : base(origiRequest, origiRequest.Length)
        {
            if (baseRenamed != null)
            {
                Set(0, new RfcLdapDN(baseRenamed));
            }
        }

        public virtual Asn1SequenceOf Modifications => (Asn1SequenceOf)Get(1);
        
        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.ModifyRequest);

        public string GetRequestDN() => ((RfcLdapDN)Get(0)).StringValue();
    }

    internal class RfcModifyResponse : RfcLdapResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RfcModifyResponse"/> class.
        /// The only time a client will create a ModifyResponse is when it is
        /// decoding it from an InputStream
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="stream">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcModifyResponse(IAsn1Decoder dec, Stream stream, int len) 
            : base(dec, stream, len)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcModifyResponse"/> class.
        /// </summary>
        /// <param name="resultCode">the result code of the operation</param>
        /// <param name="matchedDN">the matched DN returned from the server</param>
        /// <param name="errorMessage">the diagnostic message returned from the server</param>
        public RfcModifyResponse(Asn1Enumerated resultCode, RfcLdapDN matchedDN, RfcLdapString errorMessage)
            : base(resultCode, matchedDN, errorMessage)
        {
        }

        /// <summary>
        /// Override getIdentifier to return an application-wide id.
        /// </summary>
        /// <returns></returns>
        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.ModifyResponse);
    }
}
#endif