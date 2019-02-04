namespace Unosquare.Swan.Networking.Ldap
{
    /// <summary>
    /// Represents a LDAP Modification Request Message.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.LdapMessage" />
    public sealed class LdapModifyRequest : LdapMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapModifyRequest"/> class.
        /// </summary>
        /// <param name="dn">The dn.</param>
        /// <param name="modifications">The modifications.</param>
        /// <param name="control">The control.</param>
        public LdapModifyRequest(string dn, LdapModification[] modifications, LdapControl[] control)
            : base(LdapOperation.ModifyRequest, new RfcModifyRequest(dn, EncodeModifications(modifications)), control)
        {
        }

        /// <summary>
        /// Gets the dn.
        /// </summary>
        /// <value>
        /// The dn.
        /// </value>
        public string DN => Asn1Object.RequestDn;

        /// <inheritdoc />
        public override string ToString() => Asn1Object.ToString();

        private static Asn1SequenceOf EncodeModifications(LdapModification[] mods)
        {
            var rfcMods = new Asn1SequenceOf(mods.Length);

            foreach (var t in mods)
            {
                var attr = t.Attribute;

                var vals = new Asn1SetOf(attr.Size());
                if (attr.Size() > 0)
                {
                    foreach (var val in attr.ByteValueArray)
                    {
                        vals.Add(new Asn1OctetString(val));
                    }
                }

                var rfcMod = new Asn1Sequence(2);
                rfcMod.Add(new Asn1Enumerated((int) t.Op));
                rfcMod.Add(new RfcAttributeTypeAndValues(attr.Name, vals));

                rfcMods.Add(rfcMod);
            }

            return rfcMods;
        }

        internal class RfcAttributeTypeAndValues : Asn1Sequence
        {
            public RfcAttributeTypeAndValues(string type, Asn1Object vals)
                : base(2)
            {
                Add(type);
                Add(vals);
            }
        }
    }
}