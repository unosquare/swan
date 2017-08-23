namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class LdapModifyRequest : LdapMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapModifyRequest"/> class.
        /// </summary>
        /// <param name="dn">The dn.</param>
        /// <param name="modifications">The modifications.</param>
        /// <param name="control">The control.</param>
        public LdapModifyRequest(string dn, LdapModification[] modifications, LdapControl[] control) 
            : base(MODIFY_REQUEST, new RfcModifyRquest(new RfcLdapDN(dn), EncodeModifications(modifications)), control)
        {
        }

        private static Asn1SequenceOf EncodeModifications(LdapModification[] mods)
        {
            // Convert Java-API LdapModification[] to RFC2251 SEQUENCE OF SEQUENCE
            var rfcMods = new Asn1SequenceOf(mods.Length);
            for (var i = 0; i < mods.Length; i++)
            {
                var attr = mods[i].Attribute;

                //// place modification attribute values in Asn1SetOf
                //var vals = new Asn1SetOf(attr.Size());
                //if (attr.Size() > 0)
                //{
                //    var attrEnum = attr.ByteValues;
                //    while (attrEnum.MoveNext())
                //    {
                //        vals.add(new RfcAttributeValue((sbyte[])attrEnum.Current));
                //    }
                //}

                //// create SEQUENCE containing mod operation and attr type and vals
                //var rfcMod = new Asn1Sequence(2);
                //rfcMod.add(new Asn1Enumerated(mods[i].Op));
                //rfcMod.add(new RfcAttributeTypeAndValues(new RfcAttributeDescription(attr.Name), vals));

                //// place SEQUENCE into SEQUENCE OF
                //rfcMods.add(rfcMod);
            }
            return rfcMods;
        }
    }
}
