namespace Unosquare.Swan.Networking.Ldap
{
    internal class RfcModifyRquest : IRfcRequest
    {
        private RfcLdapDN rfcLdapDN;
        private object p;

        public RfcModifyRquest(RfcLdapDN rfcLdapDN, object p)
        {
            this.rfcLdapDN = rfcLdapDN;
            this.p = p;
        }

        //public virtual Asn1SequenceOf Modifications
        //{
        //    get { return (Asn1SequenceOf) get}
        //}

        public IRfcRequest DupRequest(string requestBase, string filter, bool reference)
        {
            throw new System.NotImplementedException();
        }

        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.MODIFY_REQUEST);
        }

        public string GetRequestDN()
        {
            throw new System.NotImplementedException();
        }
    }        
}