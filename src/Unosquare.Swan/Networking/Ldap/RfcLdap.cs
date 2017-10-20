#if !UWP

namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.IO;

    /// <summary>
    ///     An implementation of LdapAuthHandler must be able to provide an
    ///     LdapAuthProvider object at the time of a referral.  The class
    ///     encapsulates information that is used by the client for authentication
    ///     when following referrals automatically.
    /// </summary>
    internal class LdapAuthProvider
    {
        /// <summary>
        ///     Returns the distinguished name to be used for authentication on
        ///     automatic referral following.
        /// </summary>
        /// <returns>
        ///     The distinguished name from the object.
        /// </returns>
        public virtual string DN { get; }

        /// <summary>
        ///     Returns the password to be used for authentication on automatic
        ///     referral following.
        /// </summary>
        /// <returns>
        ///     The byte[] value (UTF-8) of the password from the object.
        /// </returns>
        public virtual sbyte[] Password { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapAuthProvider"/> class.
        ///     Constructs information that is used by the client for authentication
        ///     when following referrals automatically.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name to use when authenticating to
        ///     a server.
        /// </param>
        /// <param name="password">
        ///     The password to use when authenticating to a server.
        /// </param>
        public LdapAuthProvider(string dn, sbyte[] password)
        {
            DN = dn;
            Password = password;
        }
    }

    /// <summary>
    ///     Encapsulates a single search result that is in response to an asynchronous
    ///     search operation.
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    internal class LdapSearchResult : LdapMessage
    {
        /// <summary>
        ///     Returns the entry of a server's search response.
        /// </summary>
        /// <returns>
        ///     The LdapEntry associated with this LdapSearchResult
        /// </returns>
        public virtual LdapEntry Entry
        {
            get
            {
                if (entry != null) return entry;

                var attrs = new LdapAttributeSet();
                var attrList = ((RfcSearchResultEntry)Message.Response).Attributes;
                var seqArray = attrList.ToArray();

                foreach (Asn1Sequence seq in seqArray)
                {
                    var attr = new LdapAttribute(((Asn1OctetString)seq.Get(0)).StringValue());
                    var set = (Asn1Set)seq.Get(1);

                    foreach (var t in set.ToArray())
                    {
                        attr.AddValue(((Asn1OctetString)t).ByteValue());
                    }

                    attrs.Add(attr);
                }

                entry = new LdapEntry(((RfcSearchResultEntry)Message.Response).ObjectName.StringValue(), attrs);

                return entry;
            }
        }

        private LdapEntry entry;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapSearchResult"/> class.
        /// Constructs an LdapSearchResult object from an LdapEntry.
        /// </summary>
        /// <param name="entry">the LdapEntry represented by this search result.</param>
        /// <exception cref="ArgumentException">Argument \"entry\" cannot be null</exception>
        public LdapSearchResult(LdapEntry entry)
            => this.entry = entry ?? throw new ArgumentNullException(nameof(entry));

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
    ///     Encapsulates an ID which uniquely identifies a particular extended
    ///     operation, known to a particular server, and the data associated
    ///     with that extended operation.
    /// </summary>
    internal class LdapExtendedOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LdapExtendedOperation"/> class.
        /// Constructs a new object with the specified object ID and data.
        /// </summary>
        /// <param name="oid">The unique identifier of the operation.</param>
        /// <param name="vals">The operation-specific data of the operation.</param>
        public LdapExtendedOperation(string oid, sbyte[] vals)
        {
            Id = oid;
            Value = vals;
        }

        public string Id { get; set; }

        public sbyte[] Value { get; set; }

        /// <summary>
        ///     Returns a clone of this object.
        /// </summary>
        /// <returns>
        ///     clone of this object.
        /// </returns>
        public object Clone()
        {
            var newObj = MemberwiseClone();
            Array.Copy(Value, 0, ((LdapExtendedOperation)newObj).Value, 0, Value.Length);
            return newObj;
        }
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
        /// <summary> Context-specific TAG for optional responseName.</summary>
        public const int RESPONSE_NAME = 10;

        /// <summary> Context-specific TAG for optional response.</summary>
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
            if (Size() > 3)
            {
                for (var i = 3; i < Size(); i++)
                {
                    var obj = (Asn1Tagged)Get(i);
                    var id = obj.GetIdentifier();
                    switch (id.Tag)
                    {
                        case RfcLdapResult.REFERRAL:
                            var content = ((Asn1OctetString)obj.TaggedValue).ByteValue();
                            var bais = new MemoryStream(content.ToByteArray());
                            Set(i, new Asn1SequenceOf(dec, bais, content.Length));
                            referralIndex = i;
                            break;
                        case RESPONSE_NAME:
                            Set(i, new RfcLdapOID(((Asn1OctetString)obj.TaggedValue).ByteValue()));
                            responseNameIndex = i;
                            break;
                        case RESPONSE:
                            Set(i, obj.TaggedValue);
                            responseIndex = i;
                            break;
                    }
                }
            }
        }

        public RfcLdapOID ResponseName => responseNameIndex != 0 ? (RfcLdapOID)Get(responseNameIndex) : null;

        public Asn1OctetString Response => responseIndex != 0 ? (Asn1OctetString)Get(responseIndex) : null;

        // Accessors
        public Asn1Enumerated GetResultCode() => (Asn1Enumerated)Get(0);

        public RfcLdapDN GetMatchedDN() => new RfcLdapDN(((Asn1OctetString)Get(1)).ByteValue());

        public RfcLdapString GetErrorMessage() => new RfcLdapString(((Asn1OctetString)Get(2)).ByteValue());

        public Asn1SequenceOf GetReferral()
            => referralIndex != 0 ? (Asn1SequenceOf)Get(referralIndex) : null;

        /// <summary>
        /// Override getIdentifier to return an application-wide id.
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.ExtendedResponse);
    }

    /// <summary>
    ///     Represents and Ldap Bind Response.
    ///     <pre>
    ///         BindResponse ::= [APPLICATION 1] SEQUENCE {
    ///         COMPONENTS OF LdapResult,
    ///         serverSaslCreds    [7] OCTET STRING OPTIONAL }
    ///     </pre>
    /// </summary>
    internal class RfcBindResponse : Asn1Sequence, IRfcResponse
    {
        /// <summary>
        ///     Returns the OPTIONAL serverSaslCreds of a BindResponse if it exists
        ///     otherwise null.
        /// </summary>
        public virtual Asn1OctetString ServerSaslCreds
        {
            get
            {
                if (Size() == 5)
                    return (Asn1OctetString)((Asn1Tagged)Get(4)).TaggedValue;

                if (Size() == 4)
                {
                    // could be referral or serverSaslCreds
                    if (Get(3) is Asn1Tagged)
                        return (Asn1OctetString)((Asn1Tagged)Get(3)).TaggedValue;
                }

                return null;
            }
        }

        // Constructors for BindResponse
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
            if (Size() > 3)
            {
                var obj = (Asn1Tagged)Get(3);

                if (obj.GetIdentifier().Tag == RfcLdapResult.REFERRAL)
                {
                    var content = ((Asn1OctetString)obj.TaggedValue).ByteValue();
                    var bais = new MemoryStream(content.ToByteArray());
                    Set(3, new Asn1SequenceOf(dec, bais, content.Length));
                }
            }
        }

        // Accessors
        public Asn1Enumerated GetResultCode() => (Asn1Enumerated)Get(0);

        public RfcLdapDN GetMatchedDN() => new RfcLdapDN(((Asn1OctetString)Get(1)).ByteValue());

        public RfcLdapString GetErrorMessage() => new RfcLdapString(((Asn1OctetString)Get(2)).ByteValue());

        public Asn1SequenceOf GetReferral()
        {
            if (Size() > 3)
            {
                if (Get(3) is Asn1SequenceOf)
                    return (Asn1SequenceOf)Get(3);
            }

            return null;
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
        private readonly int _mResponseNameIndex;
        private readonly int _mResponseValueIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcIntermediateResponse"/> class.
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="stream">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcIntermediateResponse(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
            _mResponseNameIndex = _mResponseValueIndex = 0;
            var i = Size() >= 3 ? 3 : 0;

            for (; i < Size(); i++)
            {
                var obj = (Asn1Tagged)Get(i);

                switch (obj.GetIdentifier().Tag)
                {
                    case TagResponseName:
                        Set(i, new RfcLdapOID(((Asn1OctetString)obj.TaggedValue).ByteValue()));
                        _mResponseNameIndex = i;
                        break;
                    case TagResponse:
                        Set(i, obj.TaggedValue);
                        _mResponseValueIndex = i;
                        break;
                }
            }
        }

        public Asn1Enumerated GetResultCode() => Size() > 3 ? (Asn1Enumerated)Get(0) : null;

        public RfcLdapDN GetMatchedDN() => Size() > 3 ? new RfcLdapDN(((Asn1OctetString)Get(1)).ByteValue()) : null;

        public RfcLdapString GetErrorMessage() => Size() > 3 ? new RfcLdapString(((Asn1OctetString)Get(2)).ByteValue()) : null;

        public Asn1SequenceOf GetReferral() => Size() > 3 ? (Asn1SequenceOf)Get(3) : null;

        public RfcLdapOID GetResponseName()
        {
            return _mResponseNameIndex >= 0
                ? (RfcLdapOID)Get(_mResponseNameIndex)
                : null;
        }

        public Asn1OctetString GetResponse()
        {
            return _mResponseValueIndex != 0
                ? (Asn1OctetString)Get(_mResponseValueIndex)
                : null;
        }

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