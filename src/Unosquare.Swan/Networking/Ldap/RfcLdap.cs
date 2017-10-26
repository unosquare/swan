#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System.IO;

    /// <summary>
    ///     Encapsulates a single search result that is in response to an asynchronous
    ///     search operation.
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    internal class LdapSearchResult : LdapMessage
    {
        /// <summary>
        /// Returns the entry of a server's search response.
        /// </summary>
        /// <value>
        /// The entry.
        /// </value>
        public virtual LdapEntry Entry
        {
            get
            {
                if (entry != null) return entry;

                var attrs = new LdapAttributeSet();
                var attrList = ((RfcSearchResultEntry) Message.Response).Attributes;
                var seqArray = attrList.ToArray();

                foreach (Asn1Sequence seq in seqArray)
                {
                    var attr = new LdapAttribute(((Asn1OctetString) seq.Get(0)).StringValue());
                    var set = (Asn1Set) seq.Get(1);

                    foreach (var t in set.ToArray())
                    {
                        attr.AddValue(((Asn1OctetString) t).ByteValue());
                    }

                    attrs.Add(attr);
                }

                entry = new LdapEntry(((RfcSearchResultEntry) Message.Response).ObjectName.StringValue(), attrs);

                return entry;
            }
        }

        private LdapEntry entry;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapSearchResult"/> class.
        /// Constructs an LdapSearchResult object.
        /// </summary>
        /// <param name="message">The RfcLdapMessage with a search result.</param>
        internal LdapSearchResult(RfcLdapMessage message)
            : base(message)
        {
        }

        /// <summary>
        ///     Return a String representation of this object.
        /// </summary>
        /// <returns>
        ///     a String representing this object.
        /// </returns>
        public override string ToString() => entry?.ToString() ?? base.ToString();
    }

    /// <summary>
    ///     Represents an Ldap Search Result Reference.
    ///     <pre>
    ///         SearchResultReference ::= [APPLICATION 19] SEQUENCE OF LdapURL
    ///     </pre>
    /// </summary>
    internal class RfcSearchResultReference : Asn1SequenceOf
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RfcSearchResultReference"/> class.
        /// The only time a client will create a SearchResultReference is when it is
        /// decoding it from an InputStream
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="stream">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcSearchResultReference(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
        }

        /// <summary>
        /// Override getIdentifier to return an application-wide id.
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.SearchResultReference);
    }

    /// <summary> Represnts an Ldap String.</summary>
    internal class RfcLdapString : Asn1OctetString
    {
        public RfcLdapString(string s)
            : base(s)
        {
        }

        public RfcLdapString(sbyte[] ba)
            : base(ba)
        {
        }

        public RfcLdapString(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
        }
    }

    /// <summary>
    ///     Represents an Ldap DN.
    ///     <pre>
    ///         LdapDN ::= LdapString
    ///     </pre>
    /// </summary>
    internal class RfcLdapDN : RfcLdapString
    {
        public RfcLdapDN(string s)
            : base(s)
        {
        }

        public RfcLdapDN(sbyte[] s)
            : base(s)
        {
        }
    }

    /// <summary>
    /// Represents an Ldap Extended Response.
    /// <pre>
    /// ExtendedResponse ::= [APPLICATION 24] SEQUENCE {
    /// COMPONENTS OF LdapResult,
    /// responseName     [10] LdapOID OPTIONAL,
    /// response         [11] OCTET STRING OPTIONAL }
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.IRfcResponse" />
    internal sealed class RfcExtendedResponse : Asn1Sequence, IRfcResponse
    {
        public const int RESPONSE_NAME = 10;
        
        public const int RESPONSE = 11;

        private readonly int referralIndex;
        private readonly int responseNameIndex;
        private readonly int responseIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcExtendedResponse"/> class.
        /// The only time a client will create a ExtendedResponse is when it is
        /// decoding it from an InputStream
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        public RfcExtendedResponse(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
            if (Size() <= 3) return;

            for (var i = 3; i < Size(); i++)
            {
                var obj = (Asn1Tagged) Get(i);
                var id = obj.GetIdentifier();
                switch (id.Tag)
                {
                    case RfcLdapResult.REFERRAL:
                        var content = ((Asn1OctetString) obj.TaggedValue).ByteValue();
                        var bais = new MemoryStream(content.ToByteArray());
                        Set(i, new Asn1SequenceOf(dec, bais, content.Length));
                        referralIndex = i;
                        break;
                    case RESPONSE_NAME:
                        Set(i, new Asn1OctetString(((Asn1OctetString) obj.TaggedValue).ByteValue()));
                        responseNameIndex = i;
                        break;
                    case RESPONSE:
                        Set(i, obj.TaggedValue);
                        responseIndex = i;
                        break;
                }
            }
        }

        public Asn1OctetString ResponseName => responseNameIndex != 0 ? (Asn1OctetString) Get(responseNameIndex) : null;

        public Asn1OctetString Response => responseIndex != 0 ? (Asn1OctetString) Get(responseIndex) : null;

        // Accessors
        public Asn1Enumerated GetResultCode() => (Asn1Enumerated) Get(0);

        public RfcLdapDN GetMatchedDN() => new RfcLdapDN(((Asn1OctetString) Get(1)).ByteValue());

        public RfcLdapString GetErrorMessage() => new RfcLdapString(((Asn1OctetString) Get(2)).ByteValue());

        public Asn1SequenceOf GetReferral()
            => referralIndex != 0 ? (Asn1SequenceOf) Get(referralIndex) : null;

        /// <summary>
        /// Override getIdentifier to return an application-wide id.
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.ExtendedResponse);
    }

    /// <summary>
    /// Represents and Ldap Bind Response.
    /// <pre>
    /// BindResponse ::= [APPLICATION 1] SEQUENCE {
    /// COMPONENTS OF LdapResult,
    /// serverSaslCreds    [7] OCTET STRING OPTIONAL }
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.IRfcResponse" />
    internal class RfcBindResponse : Asn1Sequence, IRfcResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RfcBindResponse"/> class.
        /// The only time a client will create a BindResponse is when it is
        /// decoding it from an InputStream
        /// Note: If serverSaslCreds is included in the BindResponse, it does not
        /// need to be decoded since it is already an OCTET STRING.
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="stream">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcBindResponse(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
            // Decode optional referral from Asn1OctetString to Referral.
            if (Size() <= 3) return;

            var obj = (Asn1Tagged) Get(3);

            if (obj.GetIdentifier().Tag == RfcLdapResult.REFERRAL)
            {
                var content = ((Asn1OctetString) obj.TaggedValue).ByteValue();
                var bais = new MemoryStream(content.ToByteArray());
                Set(3, new Asn1SequenceOf(dec, bais, content.Length));
            }
        }

        // Accessors
        public Asn1Enumerated GetResultCode() => (Asn1Enumerated) Get(0);

        public RfcLdapDN GetMatchedDN() => new RfcLdapDN(((Asn1OctetString) Get(1)).ByteValue());

        public RfcLdapString GetErrorMessage() => new RfcLdapString(((Asn1OctetString) Get(2)).ByteValue());

        public Asn1SequenceOf GetReferral()
        {
            return Size() > 3 && Get(3) is Asn1SequenceOf ? (Asn1SequenceOf) Get(3) : null;
        }

        /// <summary>
        /// Override getIdentifier to return an application-wide id.
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.BindResponse);
    }

    /// <summary>
    /// Represents an LDAP Intermediate Response.
    /// IntermediateResponse ::= [APPLICATION 25] SEQUENCE {
    /// COMPONENTS OF LDAPResult, note: only present on incorrectly
    /// encoded response from pre Falcon-sp1 server
    /// responseName     [10] LDAPOID OPTIONAL,
    /// responseValue    [11] OCTET STRING OPTIONAL }
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.IRfcResponse" />
    internal class RfcIntermediateResponse : Asn1Sequence, IRfcResponse
    {
        public const int TagResponseName = 0;
        public const int TagResponse = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcIntermediateResponse"/> class.
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="stream">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcIntermediateResponse(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
            var i = Size() >= 3 ? 3 : 0;

            for (; i < Size(); i++)
            {
                var obj = (Asn1Tagged) Get(i);

                switch (obj.GetIdentifier().Tag)
                {
                    case TagResponseName:
                        Set(i, new Asn1OctetString(((Asn1OctetString) obj.TaggedValue).ByteValue()));
                        break;
                    case TagResponse:
                        Set(i, obj.TaggedValue);
                        break;
                }
            }
        }

        public Asn1Enumerated GetResultCode() => Size() > 3 ? (Asn1Enumerated) Get(0) : null;

        public RfcLdapDN GetMatchedDN() => Size() > 3 ? new RfcLdapDN(((Asn1OctetString) Get(1)).ByteValue()) : null;

        public RfcLdapString GetErrorMessage() =>
            Size() > 3 ? new RfcLdapString(((Asn1OctetString) Get(2)).ByteValue()) : null;

        public Asn1SequenceOf GetReferral() => Size() > 3 ? (Asn1SequenceOf) Get(3) : null;

        /// <summary>
        /// Returns the identifier for this Asn1Object as an Asn1Identifier.
        /// This Asn1Identifier object will include the CLASS, FORM and TAG
        /// for this Asn1Object.
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.IntermediateResponse);
    }
}
#endif