#if !UWP
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
}
#endif