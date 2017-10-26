#if !UWP
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
        
        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.ModifyResponse);
    }
}
#endif