#if !UWP

namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;

    /// <summary>
    ///     An implementation of LdapAuthHandler must be able to provide an
    ///     LdapAuthProvider object at the time of a referral.  The class
    ///     encapsulates information that is used by the client for authentication
    ///     when following referrals automatically.
    /// </summary>
    /// <seealso cref="LdapAuthHandler">
    /// </seealso>
    /// <seealso cref="LdapBindHandler">
    /// </seealso>
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
            this.DN = dn;
            this.Password = password;
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
                if (entry == null)
                {
                    var attrs = new LdapAttributeSet();
                    var attrList = ((RfcSearchResultEntry) message.Response).Attributes;
                    var seqArray = attrList.ToArray();
                    for (var i = 0; i < seqArray.Length; i++)
                    {
                        var seq = (Asn1Sequence) seqArray[i];
                        var attr = new LdapAttribute(((Asn1OctetString) seq.Get(0)).StringValue());
                        var Set = (Asn1Set) seq.Get(1);
                        object[] setArray = Set.ToArray();
                        for (var j = 0; j < setArray.Length; j++)
                        {
                            attr.AddValue(((Asn1OctetString) setArray[j]).ByteValue());
                        }
                        attrs.Add(attr);
                    }
                    entry = new LdapEntry(((RfcSearchResultEntry) message.Response).ObjectName.StringValue(), attrs);
                }
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
        {
            this.entry = entry ?? throw new ArgumentNullException(nameof(entry));
        }

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
        public override string ToString()
        {
            return entry == null ? base.ToString() : entry.ToString();
        }
    }

    /// <summary>
    ///     Encapsulates an ID which uniquely identifies a particular extended
    ///     operation, known to a particular server, and the data associated
    ///     with that extended operation.
    /// </summary>
    /// <seealso cref="LdapConnection.ExtendedOperation">
    /// </seealso>
    internal class LdapExtendedOperation
    {
        private string oid;
        private sbyte[] vals;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapExtendedOperation"/> class.
        /// Constructs a new object with the specified object ID and data.
        /// </summary>
        /// <param name="oid">The unique identifier of the operation.</param>
        /// <param name="vals">The operation-specific data of the operation.</param>
        public LdapExtendedOperation(string oid, sbyte[] vals)
        {
            this.oid = oid;
            this.vals = vals;
        }

        /// <summary>
        ///     Returns a clone of this object.
        /// </summary>
        /// <returns>
        ///     clone of this object.
        /// </returns>
        public object Clone()
        {
            try
            {
                var newObj = MemberwiseClone();
                Array.Copy(vals, 0, ((LdapExtendedOperation) newObj).vals, 0, vals.Length);
                return newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }

        /// <summary>
        ///     Returns the unique identifier of the operation.
        /// </summary>
        /// <returns>
        ///     The OID (object ID) of the operation.
        /// </returns>
        public virtual string GetID()
        {
            return oid;
        }

        /// <summary>
        ///     Returns a reference to the operation-specific data.
        /// </summary>
        /// <returns>
        ///     The operation-specific data.
        /// </returns>
        public virtual sbyte[] GetValue()
        {
            return vals;
        }

        /// <summary>
        ///     Sets the value for the operation-specific data.
        /// </summary>
        /// <param name="newVals">
        ///     The byte array of operation-specific data.
        /// </param>
        protected internal virtual void SetValue(sbyte[] newVals)
        {
            vals = newVals;
        }

        /// <summary>
        ///     Resets the OID for the operation to a new value
        /// </summary>
        /// <param name="newoid">
        ///     The new OID for the operation
        /// </param>
        protected internal virtual void SetID(string newoid)
        {
            oid = newoid;
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
        /// <param name="in_Renamed">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcSearchResultReference(IAsn1Decoder dec, Stream in_Renamed, int len)
            : base(dec, in_Renamed, len)
        {
        }

        /// <summary>
        /// Override getIdentifier to return an application-wide id.
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.SEARCH_RESULT_REFERENCE);
        }
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

        public RfcLdapString(IAsn1Decoder dec, Stream in_Renamed, int len)
            : base(dec, in_Renamed, len)
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
    ///     Represents an Ldap Extended Response.
    ///     <pre>
    ///         ExtendedResponse ::= [APPLICATION 24] SEQUENCE {
    ///         COMPONENTS OF LdapResult,
    ///         responseName     [10] LdapOID OPTIONAL,
    ///         response         [11] OCTET STRING OPTIONAL }
    ///     </pre>
    /// </summary>
    internal class RfcExtendedResponse : Asn1Sequence, IRfcResponse
    {
        public virtual RfcLdapOID ResponseName
        {
            get { return responseNameIndex != 0 ? (RfcLdapOID) Get(responseNameIndex) : null; }
        }

        public virtual Asn1OctetString Response
        {
            get { return responseIndex != 0 ? (Asn1OctetString) Get(responseIndex) : null; }
        }

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
                            Set(i, new RfcLdapOID(((Asn1OctetString) obj.TaggedValue).ByteValue()));
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

        // Accessors
        public Asn1Enumerated GetResultCode()
        {
            return (Asn1Enumerated) Get(0);
        }

        public RfcLdapDN GetMatchedDN()
        {
            return new RfcLdapDN(((Asn1OctetString) Get(1)).ByteValue());
        }

        public RfcLdapString GetErrorMessage()
        {
            return new RfcLdapString(((Asn1OctetString) Get(2)).ByteValue());
        }

        public Asn1SequenceOf GetReferral()
        {
            return referralIndex != 0 ? (Asn1SequenceOf) Get(referralIndex) : null;
        }

        /// <summary>
        /// Override getIdentifier to return an application-wide id.
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.EXTENDED_RESPONSE);
        }
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
                    return (Asn1OctetString) ((Asn1Tagged) Get(4)).TaggedValue;

                if (Size() == 4)
                {
                    // could be referral or serverSaslCreds
                    var obj = Get(3);
                    if (obj is Asn1Tagged)
                        return (Asn1OctetString) ((Asn1Tagged) obj).TaggedValue;
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
        /// <param name="in_Renamed">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcBindResponse(IAsn1Decoder dec, Stream in_Renamed, int len)
            : base(dec, in_Renamed, len)
        {
            // Decode optional referral from Asn1OctetString to Referral.
            if (Size() > 3)
            {
                var obj = (Asn1Tagged) Get(3);
                var id = obj.GetIdentifier();
                if (id.Tag == RfcLdapResult.REFERRAL)
                {
                    var content = ((Asn1OctetString) obj.TaggedValue).ByteValue();
                    var bais = new MemoryStream(content.ToByteArray());
                    Set(3, new Asn1SequenceOf(dec, bais, content.Length));
                }
            }
        }

        // Accessors
        public Asn1Enumerated GetResultCode()
        {
            return (Asn1Enumerated) Get(0);
        }

        public RfcLdapDN GetMatchedDN()
        {
            return new RfcLdapDN(((Asn1OctetString) Get(1)).ByteValue());
        }

        public RfcLdapString GetErrorMessage()
        {
            return new RfcLdapString(((Asn1OctetString) Get(2)).ByteValue());
        }

        public Asn1SequenceOf GetReferral()
        {
            if (Size() > 3)
            {
                var obj = Get(3);
                if (obj is Asn1SequenceOf)
                    return (Asn1SequenceOf) obj;
            }

            return null;
        }

        /// <summary>
        /// Override getIdentifier to return an application-wide id.
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.BIND_RESPONSE);
        }
    }

    /// <summary>
    ///     Represents an LDAP Intermediate Response.
    ///     IntermediateResponse ::= [APPLICATION 25] SEQUENCE {
    ///     COMPONENTS OF LDAPResult, note: only present on incorrectly
    ///     encoded response from
    ///     pre Falcon-sp1 server
    ///     responseName     [10] LDAPOID OPTIONAL,
    ///     responseValue    [11] OCTET STRING OPTIONAL }
    /// </summary>
    internal class RfcIntermediateResponse : Asn1Sequence, IRfcResponse
    {
        public const int TAG_RESPONSE_NAME = 0;
        public const int TAG_RESPONSE = 1;
        private int m_referralIndex;
        private readonly int m_responseNameIndex;
        private readonly int m_responseValueIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcIntermediateResponse"/> class.
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="stream">The in renamed.</param>
        /// <param name="len">The length.</param>
        public RfcIntermediateResponse(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
            m_responseNameIndex = m_responseValueIndex = 0;
            var i = Size() >= 3 ? 3 : 0;

            for (; i < Size(); i++)
            {
                var obj = (Asn1Tagged) Get(i);
                var id = obj.GetIdentifier();
                switch (id.Tag)
                {
                    case TAG_RESPONSE_NAME:
                        Set(i, new RfcLdapOID(((Asn1OctetString) obj.TaggedValue).ByteValue()));
                        m_responseNameIndex = i;
                        break;
                    case TAG_RESPONSE:
                        Set(i, obj.TaggedValue);
                        m_responseValueIndex = i;
                        break;
                }
            }
        }

        public Asn1Enumerated GetResultCode()
        {
            if (Size() > 3)
                return (Asn1Enumerated) Get(0);
            return null;
        }

        public RfcLdapDN GetMatchedDN()
        {
            if (Size() > 3)
                return new RfcLdapDN(((Asn1OctetString) Get(1)).ByteValue());
            return null;
        }

        public RfcLdapString GetErrorMessage()
        {
            if (Size() > 3)
                return new RfcLdapString(((Asn1OctetString) Get(2)).ByteValue());

            return null;
        }

        public Asn1SequenceOf GetReferral()
        {
            return Size() > 3 ? (Asn1SequenceOf) Get(3) : null;
        }

        public RfcLdapOID getResponseName()
        {
            return m_responseNameIndex >= 0
                ? (RfcLdapOID) Get(m_responseNameIndex)
                : null;
        }

        public Asn1OctetString getResponse()
        {
            return m_responseValueIndex != 0
                ? (Asn1OctetString) Get(m_responseValueIndex)
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
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.INTERMEDIATE_RESPONSE);
        }
    }
}
#endif