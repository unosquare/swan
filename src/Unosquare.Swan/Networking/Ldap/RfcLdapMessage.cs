#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.IO;

    /// <summary>
    /// Represents an Ldap Message.
    /// <pre>
    /// LdapMessage ::= SEQUENCE {
    /// messageID       MessageID,
    /// protocolOp      CHOICE {
    /// bindRequest     BindRequest,
    /// bindResponse    BindResponse,
    /// unbindRequest   UnbindRequest,
    /// searchRequest   SearchRequest,
    /// searchResEntry  SearchResultEntry,
    /// searchResDone   SearchResultDone,
    /// searchResRef    SearchResultReference,
    /// modifyRequest   ModifyRequest,
    /// modifyResponse  ModifyResponse,
    /// addRequest      AddRequest,
    /// addResponse     AddResponse,
    /// delRequest      DelRequest,
    /// delResponse     DelResponse,
    /// modDNRequest    ModifyDNRequest,
    /// modDNResponse   ModifyDNResponse,
    /// compareRequest  CompareRequest,
    /// compareResponse CompareResponse,
    /// abandonRequest  AbandonRequest,
    /// extendedReq     ExtendedRequest,
    /// extendedResp    ExtendedResponse },
    /// controls       [0] Controls OPTIONAL }
    /// </pre>
    /// Note: The creation of a MessageID should be hidden within the creation of
    /// an RfcLdapMessage. The MessageID needs to be in sequence, and has an
    /// upper and lower limit. There is never a case when a user should be
    /// able to specify the MessageID for an RfcLdapMessage. The MessageID()
    /// constructor should be package protected. (So the MessageID value
    /// isn't arbitrarily run up.)
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    internal sealed class RfcLdapMessage : Asn1Sequence
    {
        private readonly Asn1Object _op;

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcLdapMessage"/> class.
        /// Create an RfcLdapMessage request from input parameters.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <param name="controls">The controls.</param>
        public RfcLdapMessage(IRfcRequest op, RfcControls controls)
            : base(3)
        {
            _op = (Asn1Object) op;

            Add(new RfcMessageID()); // MessageID has static counter
            Add((Asn1Object) op);
            if (controls != null)
            {
                Add(controls);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcLdapMessage"/> class.
        /// Will decode an RfcLdapMessage directly from an InputStream.
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        /// <exception cref="Exception">RfcLdapMessage: Invalid tag: " + protocolOpId.Tag</exception>
        public RfcLdapMessage(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
            // Decode implicitly tagged protocol operation from an Asn1Tagged type
            // to its appropriate application type.
            var protocolOp = (Asn1Tagged) Get(1);
            var protocolOpId = protocolOp.GetIdentifier();
            var content = ((Asn1OctetString) protocolOp.TaggedValue).ByteValue();
            var bais = new MemoryStream(content.ToByteArray());

            switch ((LdapOperation) protocolOpId.Tag)
            {
                case LdapOperation.SearchResponse:
                    Set(1, new RfcSearchResultEntry(dec, bais, content.Length));
                    break;

                case LdapOperation.SearchResult:
                    Set(1, new RfcSearchResultDone(dec, bais, content.Length));
                    break;

                case LdapOperation.SearchResultReference:
                    Set(1, new RfcSearchResultReference(dec, bais, content.Length));
                    break;

                case LdapOperation.BindResponse:
                    Set(1, new RfcBindResponse(dec, bais, content.Length));
                    break;

                case LdapOperation.ExtendedResponse:
                    Set(1, new RfcExtendedResponse(dec, bais, content.Length));
                    break;

                case LdapOperation.IntermediateResponse:
                    Set(1, new RfcIntermediateResponse(dec, bais, content.Length));
                    break;
                case LdapOperation.ModifyResponse:
                    Set(1, new RfcModifyResponse(dec, bais, content.Length));
                    break;

                default:
                    throw new Exception("RfcLdapMessage: Invalid tag: " + protocolOpId.Tag);
            }

            // decode optional implicitly tagged controls from Asn1Tagged type to
            // to RFC 2251 types.
            if (Size() > 2)
            {
                var controls = (Asn1Tagged) Get(2);
                content = ((Asn1OctetString) controls.TaggedValue).ByteValue();
                bais = new MemoryStream(content.ToByteArray());
                Set(2, new RfcControls(dec, bais, content.Length));
            }
        }

        public int MessageId => ((Asn1Integer) Get(0)).IntValue();

        /// <summary> Returns this RfcLdapMessage's message type</summary>
        public LdapOperation Type => (LdapOperation) Get(1).GetIdentifier().Tag;

        /// <summary>
        /// Returns the response associated with this RfcLdapMessage.
        /// Can be one of RfcLdapResult, RfcBindResponse, RfcExtendedResponse
        /// all which extend RfcResponse. It can also be
        /// RfcSearchResultEntry, or RfcSearchResultReference
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public Asn1Object Response => Get(1);

        /// <summary> Returns the optional Controls for this RfcLdapMessage.</summary>
        public RfcControls Controls => Size() > 2 ? (RfcControls) Get(2) : null;

        /// <summary> Returns the dn of the request, may be null</summary>
        public string RequestDn => ((IRfcRequest) _op).GetRequestDN();

        /// <summary>
        /// returns the original request in this message
        /// </summary>
        /// <value>
        /// The requesting message.
        /// </value>
        public LdapMessage RequestingMessage { get; set; }

        /// <summary>
        /// Returns the request associated with this RfcLdapMessage.
        /// Throws a class cast exception if the RfcLdapMessage is not a request.
        /// </summary>
        /// <returns>The RFC request</returns>
        public IRfcRequest GetRequest() => (IRfcRequest) Get(1);

        /// <summary>
        /// Determines whether this instance is request.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is request; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRequest() => Get(1) is IRfcRequest;
    }

    /// <summary>
    /// Represents Ldap Controls.
    /// <pre>
    /// Controls ::= SEQUENCE OF Control
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1SequenceOf" />
    internal class RfcControls : Asn1SequenceOf
    {
        public const int CONTROLS = 0;

        public RfcControls()
            : base(5)
        {
        }

        public RfcControls(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
            // Convert each SEQUENCE element to a Control
            for (var i = 0; i < Size(); i++)
            {
                var tempControl = new RfcControl((Asn1Sequence) Get(i));
                Set(i, tempControl);
            }
        }

        public void Add(RfcControl control) => base.Add(control);

        public void Set(int index, RfcControl control) => base.Set(index, control);

        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(CONTROLS, true);
    }

    /// <summary>
    /// This interface represents RfcLdapMessages that contain a response from a
    /// server.
    /// If the protocol operation of the RfcLdapMessage is of this type,
    /// it contains at least an RfcLdapResult.
    /// </summary>
    internal interface IRfcResponse
    {
        /// <summary>
        /// Gets the result code.
        /// </summary>
        /// <returns>Asn1Enumerated</returns>
        Asn1Enumerated GetResultCode();

        /// <summary>
        /// Gets the matched dn.
        /// </summary>
        /// <returns>RfcLdapDN</returns>
        Asn1OctetString GetMatchedDN();

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <returns>RfcLdapString</returns>
        Asn1OctetString GetErrorMessage();

        /// <summary>
        /// Gets the referral.
        /// </summary>
        /// <returns>Asn1SequenceOf</returns>
        Asn1SequenceOf GetReferral();
    }

    /// <summary>
    /// This interface represents Protocol Operations that are requests from a
    /// client.
    /// </summary>
    internal interface IRfcRequest
    {
        /// <summary>
        /// Builds a new request using the data from the this object.
        /// </summary>
        /// <returns>A <see cref="System.String" /></returns>
        string GetRequestDN();
    }

    /// <summary>
    /// Represents an LdapResult.
    /// <pre>
    /// LdapResult ::= SEQUENCE {
    /// resultCode      ENUMERATED {
    /// success                      (0),
    /// operationsError              (1),
    /// protocolError                (2),
    /// timeLimitExceeded            (3),
    /// sizeLimitExceeded            (4),
    /// compareFalse                 (5),
    /// compareTrue                  (6),
    /// authMethodNotSupported       (7),
    /// strongAuthRequired           (8),
    /// -- 9 reserved --
    /// referral                     (10),  -- new
    /// adminLimitExceeded           (11),  -- new
    /// unavailableCriticalExtension (12),  -- new
    /// confidentialityRequired      (13),  -- new
    /// saslBindInProgress           (14),  -- new
    /// noSuchAttribute              (16),
    /// undefinedAttributeType       (17),
    /// inappropriateMatching        (18),
    /// constraintViolation          (19),
    /// attributeOrValueExists       (20),
    /// invalidAttributeSyntax       (21),
    /// -- 22-31 unused --
    /// noSuchObject                 (32),
    /// aliasProblem                 (33),
    /// invalidDNSyntax              (34),
    /// -- 35 reserved for undefined isLeaf --
    /// aliasDereferencingProblem    (36),
    /// -- 37-47 unused --
    /// inappropriateAuthentication  (48),
    /// invalidCredentials           (49),
    /// insufficientAccessRights     (50),
    /// busy                         (51),
    /// unavailable                  (52),
    /// unwillingToPerform           (53),
    /// loopDetect                   (54),
    /// -- 55-63 unused --
    /// namingViolation              (64),
    /// objectClassViolation         (65),
    /// notAllowedOnNonLeaf          (66),
    /// notAllowedOnRDN              (67),
    /// entryAlreadyExists           (68),
    /// objectClassModsProhibited    (69),
    /// -- 70 reserved for CLdap --
    /// affectsMultipleDSAs          (71), -- new
    /// -- 72-79 unused --
    /// other                        (80) },
    /// -- 81-90 reserved for APIs --
    /// matchedDN       LdapDN,
    /// errorMessage    LdapString,
    /// referral        [3] Referral OPTIONAL }
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.IRfcResponse" />
    internal class RfcLdapResult : Asn1Sequence, IRfcResponse
    {
        public const int REFERRAL = 3;

        public RfcLdapResult(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
            // Decode optional referral from Asn1OctetString to Referral.
            if (Size() <= 3) return;

            var obj = (Asn1Tagged) Get(3);
            var id = obj.GetIdentifier();

            if (id.Tag != REFERRAL) return;

            var content = ((Asn1OctetString) obj.TaggedValue).ByteValue();
            var bais = new MemoryStream(content.ToByteArray());
            Set(3, new Asn1SequenceOf(dec, bais, content.Length));
        }

        public Asn1Enumerated GetResultCode() => (Asn1Enumerated) Get(0);

        public Asn1OctetString GetMatchedDN() => new Asn1OctetString(((Asn1OctetString) Get(1)).ByteValue());

        public Asn1OctetString GetErrorMessage() => new Asn1OctetString(((Asn1OctetString) Get(2)).ByteValue());

        public Asn1SequenceOf GetReferral() => Size() > 3 ? (Asn1SequenceOf) Get(3) : null;
    }

    /// <summary>
    /// Represents an Ldap Search Result Done Response.
    /// <pre>
    /// SearchResultDone ::= [APPLICATION 5] LdapResult
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.RfcLdapResult" />
    internal class RfcSearchResultDone : RfcLdapResult
    {
        public RfcSearchResultDone(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
        }

        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.SearchResult);
    }

    /// <summary>
    /// Represents an Ldap Search Result Entry.
    /// <pre>
    /// SearchResultEntry ::= [APPLICATION 4] SEQUENCE {
    /// objectName      LdapDN,
    /// attributes      PartialAttributeList }
    /// </pre>
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Sequence" />
    internal sealed class RfcSearchResultEntry : Asn1Sequence
    {
        public RfcSearchResultEntry(IAsn1Decoder dec, Stream stream, int len)
            : base(dec, stream, len)
        {
        }

        public Asn1OctetString ObjectName => (Asn1OctetString) Get(0);

        public Asn1Sequence Attributes => (Asn1Sequence) Get(1);

        public override Asn1Identifier GetIdentifier() => new Asn1Identifier(LdapOperation.SearchResponse);
    }

    /// <summary>
    /// Represents an Ldap Message ID.
    /// <pre>
    /// MessageID ::= INTEGER (0 .. maxInt)
    /// maxInt INTEGER ::= 2147483647 -- (2^^31 - 1) --
    /// Note: The creation of a MessageID should be hidden within the creation of
    /// an RfcLdapMessage. The MessageID needs to be in sequence, and has an
    /// upper and lower limit. There is never a case when a user should be
    /// able to specify the MessageID for an RfcLdapMessage. The MessageID()
    /// class should be package protected. (So the MessageID value isn't
    /// arbitrarily run up.)
    /// </pre></summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Integer" />
    internal class RfcMessageID : Asn1Integer
    {
        private static int _messageId;
        private static readonly object LockObj = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="RfcMessageID"/> class.
        /// Creates a MessageID with an auto incremented Asn1Integer value.
        /// Bounds: (0 .. 2,147,483,647) (2^^31 - 1 or Integer.MAX_VALUE)
        /// MessageID zero is never used in this implementation.  Always
        /// start the messages with one.
        /// </summary>
        protected internal RfcMessageID()
            : base(MessageId)
        {
        }

        private static int MessageId
        {
            get
            {
                lock (LockObj)
                {
                    return _messageId < int.MaxValue ? ++_messageId : (_messageId = 1);
                }
            }
        }
    }
}
#endif