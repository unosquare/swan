﻿#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.IO;

    /// <summary>
    /// Modification Request
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.LdapMessage" />
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

        /// <summary>
        /// Gets the dn.
        /// </summary>
        /// <value>
        /// The dn.
        /// </value>
        public virtual string DN => Asn1Object.RequestDN;

        /// <summary>
        /// Gets the modifications.
        /// </summary>
        /// <value>
        /// The modifications.
        /// </value>
        /// <exception cref="Exception">Modification Exception</exception>
        public virtual LdapModification[] Modifications
        {
            get
            {
                var req = (RfcModifyRquest)Asn1Object.GetRequest();

                var seqof = req.Modifications;
                var mods = seqof.ToArray();
                var modifications = new LdapModification[mods.Length];
                for (var m = 0; m < mods.Length; m++)
                {
                    var opSeq = (Asn1Sequence)mods[m];
                    if (opSeq.Size() != 2)
                    {
                        throw new Exception($"LdapModifyRequest: modification {m} is wrong size:{opSeq.Size()}");
                    }

                    // Contains operation and sequence for the attribute

                    var opArray = opSeq.ToArray();
                    var asn1op = (Asn1Enumerated)opArray[0];
                    // get the operation

                    var op = asn1op.IntValue();
                    var attrSeq = (Asn1Sequence)opArray[1];
                    var attrArray = attrSeq.ToArray();
                    var aname = (RfcAttributeDescription)attrArray[0];
                    var name = aname.StringValue();
                    var avalue = (Asn1SetOf)attrArray[1];
                    var valueArray = avalue.ToArray();
                    var attr = new LdapAttribute(name);

                    for (var v = 0; v < valueArray.Length; v++)
                    {
                        var rfcV = (RfcAttributeValue)valueArray[v];
                        attr.AddValue(rfcV.ByteValue());
                    }

                    modifications[m] = new LdapModification(op, attr);
                }

                return modifications;
            }
        }

        /// <summary>
        /// Return an Asn1 representation of this modify request
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Asn1Object.ToString();

        private static Asn1SequenceOf EncodeModifications(LdapModification[] mods)
        {
            var rfcMods = new Asn1SequenceOf(mods.Length);
            for (var i = 0; i < mods.Length; i++)
            {
                var attr = mods[i].Attribute;

                var vals = new Asn1SetOf(attr.Size());
                if (attr.Size() > 0)
                {
                    var attrEnum = attr.ByteValues;
                    while (attrEnum.MoveNext())
                    {
                        vals.Add(new RfcAttributeValue((sbyte[])attrEnum.Current));
                    }
                }

                var rfcMod = new Asn1Sequence(2);
                rfcMod.Add(new Asn1Enumerated(mods[i].Op));
                rfcMod.Add(new RfcAttributeTypeAndValues(new RfcAttributeDescription(attr.Name), vals));

                rfcMods.Add(rfcMod);
            }

            return rfcMods;
        }

        internal class RfcAttributeTypeAndValues : Asn1Sequence
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RfcAttributeTypeAndValues"/> class.
            /// </summary>
            /// <param name="type">The type.</param>
            /// <param name="vals">The vals.</param>
            public RfcAttributeTypeAndValues(RfcAttributeDescription type, Asn1SetOf vals)
                : base(2)
            {
                Add(type);
                Add(vals);
            }
        }

        internal class RfcAttributeDescription : RfcLdapString
        {
            public RfcAttributeDescription(string s)
                : base(s)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RfcAttributeDescription"/> class.
            /// </summary>
            /// <param name="dec">The decimal.</param>
            /// <param name="inRenamed">The in renamed.</param>
            /// <param name="len">The length.</param>
            public RfcAttributeDescription(IAsn1Decoder dec, Stream inRenamed, int len)
                : base(dec, inRenamed, len)
            {
            }
        }

        internal class RfcAttributeValue : Asn1OctetString
        {
            public RfcAttributeValue(string valueRenamed)
                : base(valueRenamed)
            {
            }

            public RfcAttributeValue(sbyte[] valueRenamed)
                : base(valueRenamed)
            {
            }
        }
    }
}
#endif