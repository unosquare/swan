#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System.IO;

    /// <summary>
    /// Encapsulates a single search result that is in response to an asynchronous
    /// search operation.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.LdapMessage" />
    internal class LdapSearchResult : LdapMessage
    {
        private LdapEntry _entry;
        
        internal LdapSearchResult(RfcLdapMessage message)
            : base(message)
        {
        }
        
        public LdapEntry Entry
        {
            get
            {
                if (_entry != null) return _entry;

                var attrs = new LdapAttributeSet();
                var attrList = ((RfcSearchResultEntry)Message.Response).Attributes;
                
                foreach (Asn1Sequence seq in attrList.ToArray())
                {
                    var attr = new LdapAttribute(((Asn1OctetString)seq.Get(0)).StringValue());
                    var set = (Asn1Set)seq.Get(1);

                    foreach (var t in set.ToArray())
                    {
                        attr.AddValue(((Asn1OctetString)t).ByteValue());
                    }

                    attrs.Add(attr);
                }

                _entry = new LdapEntry(((RfcSearchResultEntry)Message.Response).ObjectName.StringValue(), attrs);

                return _entry;
            }
        }
        
        public override string ToString() => _entry?.ToString() ?? base.ToString();
    }

    /// <summary>
    /// Represents an Ldap Search Result Reference.
    /// <pre>
    /// SearchResultReference ::= [APPLICATION 19] SEQUENCE OF LdapURL
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1SequenceOf" />
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
        
        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.SearchResultReference);
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
    internal class RfcExtendedResponse : Asn1Sequence, IRfcResponse
    {
        public const int RESPONSE_NAME = 10;
        
        public const int RESPONSE = 11;

        private readonly int _referralIndex;
        private readonly int _responseNameIndex;
        private readonly int _responseIndex;

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
                        _referralIndex = i;
                        break;
                    case RESPONSE_NAME:
                        Set(i, new Asn1OctetString(((Asn1OctetString) obj.TaggedValue).ByteValue()));
                        _responseNameIndex = i;
                        break;
                    case RESPONSE:
                        Set(i, obj.TaggedValue);
                        _responseIndex = i;
                        break;
                }
            }
        }

        public Asn1OctetString ResponseName => _responseNameIndex != 0 ? (Asn1OctetString) Get(_responseNameIndex) : null;

        public Asn1OctetString Response => _responseIndex != 0 ? (Asn1OctetString) Get(_responseIndex) : null;
        
        public Asn1Enumerated GetResultCode() => (Asn1Enumerated) Get(0);

        public Asn1OctetString GetMatchedDN() => new Asn1OctetString(((Asn1OctetString) Get(1)).ByteValue());

        public Asn1OctetString GetErrorMessage() => new Asn1OctetString(((Asn1OctetString) Get(2)).ByteValue());

        public Asn1SequenceOf GetReferral()
            => _referralIndex != 0 ? (Asn1SequenceOf) Get(_referralIndex) : null;
        
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

            if (obj.GetIdentifier().Tag != RfcLdapResult.REFERRAL) return;

            var content = ((Asn1OctetString) obj.TaggedValue).ByteValue();
            var bais = new MemoryStream(content.ToByteArray());
            Set(3, new Asn1SequenceOf(dec, bais, content.Length));
        }
        
        public Asn1Enumerated GetResultCode() => (Asn1Enumerated) Get(0);

        public Asn1OctetString GetMatchedDN() => new Asn1OctetString(((Asn1OctetString) Get(1)).ByteValue());

        public Asn1OctetString GetErrorMessage() => new Asn1OctetString(((Asn1OctetString) Get(2)).ByteValue());

        public Asn1SequenceOf GetReferral() => Size() > 3 && Get(3) is Asn1SequenceOf ? (Asn1SequenceOf) Get(3) : null;
        
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

        public Asn1OctetString GetMatchedDN() => Size() > 3 ? new Asn1OctetString(((Asn1OctetString) Get(1)).ByteValue()) : null;

        public Asn1OctetString GetErrorMessage() =>
            Size() > 3 ? new Asn1OctetString(((Asn1OctetString) Get(2)).ByteValue()) : null;

        public Asn1SequenceOf GetReferral() => Size() > 3 ? (Asn1SequenceOf) Get(3) : null;
        
        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.IntermediateResponse);
    }
}
#endif