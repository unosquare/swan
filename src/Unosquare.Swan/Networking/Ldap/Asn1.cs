#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// The Asn1Set class can hold an unordered collection of components with
    /// identical type. This class inherits from the Asn1Structured class
    /// which already provides functionality to hold multiple Asn1 components.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Structured" />
    internal class Asn1SetOf
        : Asn1Structured
    {
        /// <summary> ASN.1 SET OF tag definition.</summary>
        public const int TAG = 0x11;

        /// <summary>
        ///     ID is added for Optimization.
        ///     Id needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, true, TAG);

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1SetOf"/> class.
        /// Constructs an Asn1SetOf object with no actual Asn1Objects in it. Assumes a default size of 5 elements.
        /// </summary>
        public Asn1SetOf()
            : base(ID)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1SetOf"/> class.
        ///     Constructs an Asn1SetOf object with the specified
        ///     number of placeholders for Asn1Objects. However there
        ///     are no actual Asn1Objects in this SequenceOf object.
        /// </summary>
        /// <param name="size">
        ///     Specifies the initial size of the collection.
        /// </param>
        public Asn1SetOf(int size)
            : base(ID, size)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1SetOf"/> class.
        /// A copy constructor that creates an Asn1SetOf from an instance of Asn1Set.
        /// Since SET and SET_OF have the same identifier, the decoder
        /// will always return a SET object when it detects that identifier.
        /// In order to take advantage of the Asn1SetOf type, we need to be
        /// able to construct this object when knowingly receiving an
        /// Asn1Set.
        /// </summary>
        /// <param name="setRenamed">The set renamed.</param>
        public Asn1SetOf(Asn1Set setRenamed)
            : base(ID, setRenamed.ToArray(), setRenamed.Size())
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => ToString("SET OF: { ");
    }

    /// <summary>
    ///     The Asn1Choice object represents the choice of any Asn1Object. All
    ///     Asn1Object methods are delegated to the object this Asn1Choice contains.
    /// </summary>
    internal class Asn1Choice : Asn1Object
    {
        /// <summary>
        /// Sets the CHOICE value stored in this Asn1Choice.
        /// </summary>
        /// <value>
        /// The choice value.
        /// </value>
        protected internal virtual Asn1Object ChoiceValue
        {
            set { _content = value; }
        }

        private Asn1Object _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Choice"/> class.
        ///     Constructs an Asn1Choice object using an Asn1Object value.
        /// </summary>
        /// <param name="content">
        ///     The Asn1Object that this Asn1Choice will
        ///     encode.  Since all Asn1 objects are derived from Asn1Object
        ///     any basic type can be passed in.
        /// </param>
        public Asn1Choice(Asn1Object content)
            : base(null)
        {
            _content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Choice"/> class.
        ///     No arg Constructor. This is used by Filter, who subsequently sets the
        ///     content after parsing the RFC 2254 Search Filter String.
        /// </summary>
        protected internal Asn1Choice()
            : base(null)
        {
            _content = null;
        }

        /// <summary>
        /// Call this method to encode the contents of this Asn1Choice
        /// instance into the specified output stream using the
        /// specified encoder object.
        /// </summary>
        /// <param name="enc">Encoder object to use when encoding self.</param>
        /// <param name="stream">The stream.</param>
        public override void Encode(Asn1Encoder enc, Stream stream)
        {
            _content.Encode(enc, stream);
        }

        /// <summary>
        /// Returns the CHOICE value stored in this Asn1Choice
        /// as an Asn1Object.
        /// </summary>
        /// <returns></returns>
        public Asn1Object choiceValue()
        {
            return _content;
        }

        /// <summary>
        /// This method will return the Asn1Identifier of the
        /// encoded Asn1Object.We  override the parent method
        /// as the identifier of an Asn1Choice depends on the
        /// type of the object encoded by this Asn1Choice.
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier()
        {
            return _content.GetIdentifier();
        }

        /// <summary>
        ///     Sets the identifier of the contained Asn1Object. We
        ///     override the parent method as the identifier of
        ///     an Asn1Choice depends on the type of the object
        ///     encoded by this Asn1Choice.
        /// </summary>
        public override void SetIdentifier(Asn1Identifier id)
        {
            _content.SetIdentifier(id);
        }

        /// <summary>
        /// Return a String representation of this Asn1Object.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _content.ToString();
        }
    }

    /// <summary>
    /// This class is used to encapsulate an ASN.1 Identifier.
    /// An Asn1Identifier is composed of three parts:
    /// <li> a class type,</li><li> a form, and</li><li> a tag.</li>
    /// The class type is defined as:
    /// <pre>
    /// bit 8 7 TAG CLASS
    /// ------- -----------
    /// 0 0 UNIVERSAL
    /// 0 1 APPLICATION
    /// 1 0 CONTEXT
    /// 1 1 PRIVATE
    /// </pre>
    /// The form is defined as:
    /// <pre>
    /// bit 6 FORM
    /// ----- --------
    /// 0 PRIMITIVE
    /// 1 CONSTRUCTED
    /// </pre>
    /// Note: CONSTRUCTED types are made up of other CONSTRUCTED or PRIMITIVE
    /// types.
    /// The tag is defined as:
    /// <pre>
    /// bit 5 4 3 2 1 TAG
    /// ------------- ---------------------------------------------
    /// 0 0 0 0 0
    /// . . . . .
    /// 1 1 1 1 0 (0-30) single octet tag
    /// 1 1 1 1 1 (&gt; 30) multiple octet tag, more octets follow
    /// </pre></summary>
    internal class Asn1Identifier : object
    {
        /// <summary>
        ///     Returns the CLASS of this Asn1Identifier as an int value.
        /// </summary>
        /// <seealso cref="UNIVERSAL">
        /// </seealso>
        /// <seealso cref="APPLICATION">
        /// </seealso>
        /// <seealso cref="CONTEXT">
        /// </seealso>
        /// <seealso cref="PRIVATE">
        /// </seealso>
        public virtual int Asn1Class => _tagClass;

        /// <summary>
        ///     Return a boolean indicating if the constructed bit is set.
        /// </summary>
        /// <returns>
        ///     true if constructed and false if primitive.
        /// </returns>
        public virtual bool Constructed => _constructed;

        /// <summary> Returns the TAG of this Asn1Identifier.</summary>
        public virtual int Tag => tag;

        /// <summary> Returns the encoded length of this Asn1Identifier.</summary>
        public virtual int EncodedLength => _encodedLength;

        /// <summary>
        ///     Returns a boolean value indicating whether or not this Asn1Identifier
        ///     has a TAG CLASS of UNIVERSAL.
        /// </summary>
        /// <seealso cref="UNIVERSAL">
        /// </seealso>
        public virtual bool Universal => _tagClass == UNIVERSAL;

        /// <summary>
        ///     Returns a boolean value indicating whether or not this Asn1Identifier
        ///     has a TAG CLASS of APPLICATION.
        /// </summary>
        /// <seealso cref="APPLICATION">
        /// </seealso>
        public virtual bool Application => _tagClass == APPLICATION;

        /// <summary>
        ///     Returns a boolean value indicating whether or not this Asn1Identifier
        ///     has a TAG CLASS of CONTEXT-SPECIFIC.
        /// </summary>
        /// <seealso cref="CONTEXT">
        /// </seealso>
        public virtual bool Context => _tagClass == CONTEXT;

        /// <summary>
        ///     Returns a boolean value indicating whether or not this Asn1Identifier
        ///     has a TAG CLASS of PRIVATE.
        /// </summary>
        /// <seealso cref="PRIVATE"></seealso>
        public virtual bool Private => _tagClass == PRIVATE;

        /// <summary>
        ///     Universal tag class.
        ///     UNIVERSAL = 0
        /// </summary>
        public const int UNIVERSAL = 0;

        /// <summary>
        ///     Application-wide tag class.
        ///     APPLICATION = 1
        /// </summary>
        public const int APPLICATION = 1;

        /// <summary>
        ///     Context-specific tag class.
        ///     CONTEXT = 2
        /// </summary>
        public const int CONTEXT = 2;

        /// <summary>
        ///     Private-use tag class.
        ///     PRIVATE = 3
        /// </summary>
        public const int PRIVATE = 3;

        private int _tagClass;
        private bool _constructed;
        private int tag;
        private int _encodedLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Identifier"/> class.
        ///     Constructs an Asn1Identifier using the classtype, form and tag.
        /// </summary>
        /// <param name="tagClass">
        ///     As defined above.
        /// </param>
        /// <param name="constructed">
        ///     Set to true if constructed and false if primitive.
        /// </param>
        /// <param name="tag">
        ///     The tag of this identifier
        /// </param>
        public Asn1Identifier(int tagClass, bool constructed, int tag)
        {
            _tagClass = tagClass;
            _constructed = constructed;
            this.tag = tag;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Identifier"/> class.
        /// Decode an Asn1Identifier directly from an InputStream and
        /// save the encoded length of the Asn1Identifier.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public Asn1Identifier(Stream stream)
        {
            var r = stream.ReadByte();
            _encodedLength++;
            if (r < 0)
                throw new EndOfStreamException("BERDecoder: decode: EOF in Identifier");
            _tagClass = r >> 6;
            _constructed = (r & 0x20) != 0;
            tag = r & 0x1F; // if tag < 30 then its a single octet identifier.
            if (tag == 0x1F)
                // if true, its a multiple octet identifier.
                tag = DecodeTagNumber(stream);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Identifier" /> class.
        /// </summary>
        public Asn1Identifier()
        {
        }

        /// <summary>
        /// Decode an Asn1Identifier directly from an InputStream and
        /// save the encoded length of the Asn1Identifier, but reuse the object.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Reset(Stream stream)
        {
            _encodedLength = 0;
            var r = stream.ReadByte();
            _encodedLength++;
            if (r < 0)
                throw new EndOfStreamException("BERDecoder: decode: EOF in Identifier");
            _tagClass = r >> 6;
            _constructed = (r & 0x20) != 0;
            tag = r & 0x1F; // if tag < 30 then its a single octet identifier.
            if (tag == 0x1F)
                // if true, its a multiple octet identifier.
                tag = DecodeTagNumber(stream);
        }

        /// <summary>
        /// In the case that we have a tag number that is greater than 30, we need
        /// to decode a multiple octet tag number.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>Int octet</returns>
        private int DecodeTagNumber(Stream stream)
        {
            var n = 0;
            while (true)
            {
                var r = stream.ReadByte();
                _encodedLength++;
                if (r < 0)
                    throw new EndOfStreamException("BERDecoder: decode: EOF in tag number");
                n = (n << 7) + (r & 0x7F);
                if ((r & 0x80) == 0)
                    break;
            }
            return n;
        }

        /// <summary>
        /// Creates a duplicate, not a true clone, of this object and returns
        /// a reference to the duplicate.
        /// </summary>
        /// <returns>Cloned object</returns>
        public object Clone()
        {
            try
            {
                return MemberwiseClone();
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }
    }

    /// <summary> This is the base class for all other Asn1 types.</summary>
    internal abstract class Asn1Object
    {
        private Asn1Identifier id;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Object"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        protected Asn1Object(Asn1Identifier id)
        {
            this.id = id;
        }

        /// <summary>
        /// Abstract method that must be implemented by each child
        /// class to encode itself ( an Asn1Object) directly intto
        /// a output stream.
        /// </summary>
        /// <param name="enc">The enc.</param>
        /// <param name="stream">The stream.</param>
        public abstract void Encode(Asn1Encoder enc, Stream stream);

        /// <summary>
        ///     Returns the identifier for this Asn1Object as an Asn1Identifier.
        ///     This Asn1Identifier object will include the CLASS, FORM and TAG
        ///     for this Asn1Object.
        /// </summary>
        /// <returns>Asn1 Identifier</returns>
        public virtual Asn1Identifier GetIdentifier()
        {
            return id;
        }

        /// <summary>
        ///     Sets the identifier for this Asn1Object. This is helpful when
        ///     creating implicit Asn1Tagged types.
        /// </summary>
        /// <param name="id">
        ///     An Asn1Identifier object representing the CLASS,
        ///     FORM and TAG)
        /// </param>
        public virtual void SetIdentifier(Asn1Identifier id)
        {
            this.id = id;
        }

        /// <summary>
        ///     This method returns a byte array representing the encoded
        ///     Asn1Object.  It in turn calls the encode method that is
        ///     defined in Asn1Object but will usually be implemented
        ///     in the child Asn1 classes.
        /// </summary>
        /// <returns>Byte array</returns>
        public sbyte[] GetEncoding(Asn1Encoder enc)
        {
            using (var stream = new MemoryStream())
            {
                try
                {
                    Encode(enc, stream);
                }
                catch (IOException e)
                {
                    // Should never happen - the current Asn1Object does not have
                    // a encode method. 
                    throw new Exception("IOException while encoding to byte array: " + e);
                }
                return stream.ToArray().ToSByteArray();
            }
        }

        /// <summary> Return a String representation of this Asn1Object.</summary>
        /// <returns>String object</returns>
        public override string ToString()
        {
            string[] classTypes = { "[UNIVERSAL ", "[APPLICATION ", "[", "[PRIVATE " };
            var sb = new StringBuilder();
            var id = GetIdentifier(); // could be overridden.
            sb.Append(classTypes[id.Asn1Class]).Append(id.Tag).Append("] ");
            return sb.ToString();
        }
    }

    /// <summary>
    ///     This interface defines the methods for encoding each of the ASN.1 types.
    ///     Encoders which implement this interface may be used to encode any of the
    ///     IAsn1Object data types.
    ///     This package also provides the BEREncoder class that can be used to
    ///     BER encode ASN.1 classes.  However an application might chose to use
    ///     its own encoder class.
    ///     This interface thus allows an application to use this package to
    ///     encode ASN.1 objects using other encoding rules if needed.
    ///     Note that Ldap packets are required to be BER encoded. Since this package
    ///     includes a BER encoder no application provided encoder is needed for
    ///     building Ldap packets.
    /// </summary>
    internal interface Asn1Encoder
    {
        /// <summary>
        ///     Encode an Asn1Boolean directly into the provided output stream.
        /// </summary>
        /// <param name="b">
        ///     The Asn1Boolean object to encode
        /// </param>
        /// <param name="out">
        ///     The output stream onto which the ASN.1 object is
        ///     to be encoded
        /// </param>
        void encode(Asn1Boolean b, Stream stream);

        /// <summary>
        ///     Encode an Asn1Numeric directly to a stream.
        ///     Use a two's complement representation in the fewest number of octets
        ///     possible.
        ///     Can be used to encode both INTEGER and ENUMERATED values.
        /// </summary>
        /// <param name="n">
        ///     The Asn1Numeric object to encode
        /// </param>
        /// <param name="out">
        ///     The output stream onto which the ASN.1 object is
        ///     to be encoded
        /// </param>
        void encode(Asn1Numeric n, Stream stream);

        /// <summary>
        /// Encode an Asn1Null directly to a stream.
        /// </summary>
        /// <param name="n">The Asn1Null object to encode</param>
        /// <param name="stream">The stream.</param>
        void encode(Asn1Null n, Stream stream);

        /// <summary>
        /// Encode an Asn1OctetString directly to a stream.
        /// </summary>
        /// <param name="os">The Asn1OctetString object to encode</param>
        /// <param name="stream">The stream.</param>
        void encode(Asn1OctetString os, Stream stream);

        /// <summary>
        /// Encode an Asn1Structured directly to a stream.
        /// </summary>
        /// <param name="c">The Asn1Structured object to encode</param>
        /// <param name="stream">The stream.</param>
        void encode(Asn1Structured c, Stream stream);

        /// <summary>
        /// Encode an Asn1Tagged directly to a stream.
        /// </summary>
        /// <param name="t">The Asn1Tagged object to encode</param>
        /// <param name="stream">The stream.</param>
        void encode(Asn1Tagged t, Stream stream);

        /// <summary>
        /// Encode an Asn1Identifier directly to a stream.
        /// </summary>
        /// <param name="id">The Asn1Identifier object to encode</param>
        /// <param name="stream">The stream.</param>
        void encode(Asn1Identifier id, Stream stream);
    }

    /// <summary> This class encapsulates the OCTET STRING type.</summary>
    internal class Asn1OctetString : Asn1Object
    {
        private readonly sbyte[] _content;

        /// <summary> ASN.1 OCTET STRING tag definition.</summary>
        public const int TAG = 0x04;

        /// <summary>
        ///     ID is added for Optimization.
        ///     Id needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        protected internal static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, false, TAG);

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1OctetString"/> class.
        ///     Call this constructor to construct an Asn1OctetString
        ///     object from a byte array.
        /// </summary>
        /// <param name="content">
        ///     A byte array representing the string that
        ///     will be contained in the this Asn1OctetString object
        /// </param>
        public Asn1OctetString(sbyte[] content) : base(ID)
        {
            _content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1OctetString"/> class.
        ///     Call this constructor to construct an Asn1OctetString
        ///     object from a String object.
        /// </summary>
        /// <param name="content">
        ///     A string value that will be contained
        ///     in the this Asn1OctetString object
        /// </param>
        public Asn1OctetString(string content) : base(ID)
        {
            try
            {
                _content = Encoding.UTF8.GetBytes(content).ToSByteArray();
            }
            catch (IOException uee)
            {
                throw new Exception(uee.ToString());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1OctetString"/> class.
        /// Constructs an Asn1OctetString object by decoding data from an
        /// input stream.
        /// </summary>
        /// <param name="dec">The decoder object to use when decoding the
        /// input stream.  Sometimes a developer might want to pass
        /// in his/her own decoder object</param>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        public Asn1OctetString(Asn1Decoder dec, Stream stream, int len) : base(ID)
        {
            _content = len > 0 ? (sbyte[])dec.decodeOctetString(stream, len) : new sbyte[0];
        }

        /// <summary>
        ///     Call this method to encode the current instance into the
        ///     specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">
        ///     Encoder object to use when encoding self.
        /// </param>
        /// <param name="out">
        ///     The output stream onto which the encoded byte
        ///     stream is written.
        /// </param>
        public override void Encode(Asn1Encoder enc, Stream stream)
        {
            enc.encode(this, stream);
        }

        /// <summary>
        /// Returns the content of this Asn1OctetString as a byte array.
        /// </summary>
        /// <returns></returns>
        public sbyte[] ByteValue() => _content;

        /// <summary>
        /// Returns the content of this Asn1OctetString as a String.
        /// </summary>
        /// <returns>String</returns>
        /// <exception cref="Exception">IO Exception</exception>
        public string StringValue()
        {
            try
            {
                var encoder = Encoding.UTF8;
                var dchar = encoder.GetChars(_content.ToByteArray());
                return new string(dchar);
            }
            catch (IOException uee)
            {
                throw new Exception(uee.ToString());
            }
        }

        /// <summary>
        /// Return a String representation of this Asn1Object.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return base.ToString() + "OCTET STRING: " + StringValue();
        }
    }

    /// <summary>
    ///     The Asn1Tagged class can hold a base Asn1Object with a distinctive tag
    ///     describing the type of that base object. It also maintains a boolean value
    ///     indicating whether the value should be encoded by EXPLICIT or IMPLICIT
    ///     means. (Explicit is true by default.)
    ///     If the type is encoded IMPLICITLY, the base types form, length and content
    ///     will be encoded as usual along with the class type and tag specified in
    ///     the constructor of this Asn1Tagged class.
    ///     If the type is to be encoded EXPLICITLY, the base type will be encoded as
    ///     usual after the Asn1Tagged identifier has been encoded.
    /// </summary>
    internal class Asn1Tagged : Asn1Object
    {
        /// <summary> Sets the Asn1Object tagged value</summary>
        public virtual Asn1Object TaggedValue
        {
            set
            {
                _content = value;
                if (!_explicitRenamed)
                {
                    // replace object's id with new tag.
                    value?.SetIdentifier(GetIdentifier());
                }
            }
        }

        /// <summary>
        ///     Returns a boolean value indicating if this object uses
        ///     EXPLICIT tagging.
        /// </summary>
        public virtual bool Explicit => _explicitRenamed;

        private readonly bool _explicitRenamed;
        private Asn1Object _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Tagged"/> class.
        ///     Constructs an Asn1Tagged object using the provided
        ///     AN1Identifier and the Asn1Object.
        ///     The explicit flag defaults to true as per the spec.
        /// </summary>
        public Asn1Tagged(Asn1Identifier identifier, Asn1Object objectRenamed) : this(identifier, objectRenamed, true)
        {
        }

        /// <summary> Constructs an Asn1Tagged object.</summary>
        public Asn1Tagged(Asn1Identifier identifier, Asn1Object objectRenamed, bool explicitRenamed)
            : base(identifier)
        {
            _content = objectRenamed;
            _explicitRenamed = explicitRenamed;
            if (!explicitRenamed && _content != null)
            {
                // replace object's id with new tag.
                _content.SetIdentifier(identifier);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Tagged"/> class.
        /// Constructs an Asn1Tagged object by decoding data from an
        /// input stream.
        /// </summary>
        /// <param name="dec">The decoder object to use when decoding the
        /// input stream.  Sometimes a developer might want to pass
        /// in his/her own decoder object</param>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        /// <param name="identifier">The identifier.</param>
        public Asn1Tagged(Asn1Decoder dec, Stream stream, int len, Asn1Identifier identifier)
            : base(identifier)
        {
            // If we are decoding an implicit tag, there is no way to know at this
            // low level what the base type really is. We can place the content
            // into an Asn1OctetString type and pass it back to the application who
            // will be able to create the appropriate ASN.1 type for this tag.
            _content = new Asn1OctetString(dec, stream, len);
        }

        /// <summary>
        /// Call this method to encode the current instance into the
        /// specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">Encoder object to use when encoding self.</param>
        /// <param name="stream">The stream.</param>
        public override void Encode(Asn1Encoder enc, Stream stream)
        {
            enc.encode(this, stream);
        }

        /// <summary>
        /// Returns the Asn1Object stored in this Asn1Tagged object
        /// </summary>
        /// <returns>Ans1Object object</returns>
        public Asn1Object taggedValue() => _content;

        /// <summary>
        /// Return a String representation of this Asn1Object.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (_explicitRenamed)
            {
                return base.ToString() + _content;
            }

            // implicit tagging
            return _content.ToString();
        }
    }

    /// <summary>
    ///     This class serves as the base type for all ASN.1
    ///     structured types.
    /// </summary>
    internal abstract class Asn1Structured : Asn1Object
    {
        private Asn1Object[] _content;
        private int _contentIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Structured" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        protected internal Asn1Structured(Asn1Identifier id) : this(id, 10)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Structured" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="size">The size.</param>
        protected internal Asn1Structured(Asn1Identifier id, int size) : base(id)
        {
            _content = new Asn1Object[size];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Structured" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="newContent">The new content.</param>
        /// <param name="size">The size.</param>
        protected internal Asn1Structured(Asn1Identifier id, Asn1Object[] newContent, int size) : base(id)
        {
            _content = newContent;
            _contentIndex = size;
        }

        /// <summary>
        ///     Encodes the contents of this Asn1Structured directly to an output
        ///     stream.
        /// </summary>
        public override void Encode(Asn1Encoder enc, Stream stream)
        {
            enc.encode(this, stream);
        }

        /// <summary> Decode an Asn1Structured type from an InputStream.</summary>
        protected internal void DecodeStructured(Asn1Decoder dec, Stream stream, int len)
        {
            var componentLen = new int[1]; // collects length of component
            while (len > 0)
            {
                Add(dec.decode(stream, componentLen));
                len -= componentLen[0];
            }
        }

        /// <summary>
        ///     Returns an array containing the individual ASN.1 elements
        ///     of this Asn1Structed object.
        /// </summary>
        /// <returns>
        ///     an array of Asn1Objects
        /// </returns>
        public Asn1Object[] ToArray()
        {
            var cloneArray = new Asn1Object[_contentIndex];
            Array.Copy(_content, 0, cloneArray, 0, _contentIndex);
            return cloneArray;
        }

        /// <summary>
        ///     Adds a new Asn1Object to the end of this Asn1Structured
        ///     object.
        /// </summary>
        /// <param name="value">
        ///     The Asn1Object to add to this Asn1Structured
        ///     object.
        /// </param>
        public void Add(Asn1Object valueRenamed)
        {
            if (_contentIndex == _content.Length)
            {
                // Array too small, need to expand it, double length
                var newSize = _contentIndex + _contentIndex;
                var newArray = new Asn1Object[newSize];
                Array.Copy(_content, 0, newArray, 0, _contentIndex);
                _content = newArray;
            }
            _content[_contentIndex++] = valueRenamed;
        }

        /// <summary>
        ///     Replaces the Asn1Object in the specified index position of
        ///     this Asn1Structured object.
        /// </summary>
        /// <param name="index">
        ///     The index into the Asn1Structured object where
        ///     this new ANS1Object will be placed.
        /// </param>
        /// <param name="value">
        ///     The Asn1Object to set in this Asn1Structured
        ///     object.
        /// </param>
        public void Set(int index, Asn1Object value)
        {
            if (index >= _contentIndex || index < 0)
            {
                throw new IndexOutOfRangeException($"Asn1Structured: get: index {index}, size {_contentIndex}");
            }
            _content[index] = value;
        }

        /// <summary>
        ///     Gets a specific Asn1Object in this structred object.
        /// </summary>
        /// <param name="index">
        ///     The index of the Asn1Object to get from
        ///     this Asn1Structured object.
        /// </param>
        public Asn1Object Get(int index)
        {
            if (index >= _contentIndex || index < 0)
            {
                throw new IndexOutOfRangeException($"Asn1Structured: set: index {index}, size {_contentIndex}");
            }
            return _content[index];
        }

        /// <summary>
        ///     Returns the number of Asn1Obejcts that have been encoded
        ///     into this Asn1Structured class.
        /// </summary>
        public int Size()
        {
            return _contentIndex;
        }

        /// <summary>
        ///     Creates a String representation of this Asn1Structured.
        ///     object.
        /// </summary>
        /// <param name="type">
        ///     the Type to put in the String representing this structured object
        /// </param>
        /// <returns>
        ///     the String representation of this object.
        /// </returns>
        public string ToString(string type)
        {
            var sb = new StringBuilder();
            sb.Append(type);
            for (var i = 0; i < _contentIndex; i++)
            {
                sb.Append(_content[i]);
                if (i != _contentIndex - 1)
                    sb.Append(", ");
            }

            sb.Append(" }");
            return base.ToString() + sb;
        }
    }

    /// <summary>
    /// This interface defines the methods for decoding each of the ASN.1 types.
    /// Decoders which implement this interface may be used to decode any of the
    /// Asn1Object data types.
    /// This package also provides the BERDecoder class that can be used to
    /// BER decode ASN.1 classes.  However an application might chose to use
    /// its own decoder class.
    /// This interface thus allows an application to use this package to
    /// decode ASN.1 objects using other decoding rules if needed.
    /// Note that Ldap packets are required to be BER encoded. Since this package
    /// includes a BER decoder no application provided decoder is needed for
    /// building Ldap packets.
    /// </summary>
    internal interface Asn1Decoder
    {
        /// <summary>
        ///     Decode an encoded value into an Asn1Object from a byte array.
        /// </summary>
        /// <param name="value">
        ///     A byte array that points to the encoded Asn1 data
        /// </param>
        Asn1Object decode(sbyte[] valueRenamed);

        /// <summary>
        ///     Decode an encoded value into an Asn1Object from an InputStream.
        /// </summary>
        /// <param name="in">
        ///     An input stream containing the encoded ASN.1 data.
        /// </param>
        Asn1Object decode(Stream stream);

        /// <summary>
        ///     Decode an encoded value into an Asn1Object from an InputStream.
        /// </summary>
        /// <param name="length">
        ///     The decoded components encoded length. This value is
        ///     handy when decoding structured types. It allows you to accumulate
        ///     the number of bytes decoded, so you know when the structured
        ///     type has decoded all of its components.
        /// </param>
        /// <param name="in">
        ///     An input stream containig the encoded ASN.1 data.
        /// </param>
        /// <returns></returns>
        Asn1Object decode(Stream stream, int[] length);

        /// <summary>
        /// Decode a BOOLEAN directly from a stream. Call this method when you
        /// know that the next ASN.1 encoded element is a BOOLEAN
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="len">Length in bytes</param>
        /// <returns>Decoded Boolean</returns>
        object decodeBoolean(Stream stream, int len);

        /// <summary>
        /// Decode a Numeric value directly from a stream.  Call this method when you
        /// know that the next ASN.1 encoded element is a Numeric
        /// Can be used to decodes INTEGER and ENUMERATED types.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="len">Length in bytes</param>
        /// <returns>Decoded Numeric</returns>
        object decodeNumeric(Stream stream, int len);

        /// <summary>
        ///     Decode an OCTET_STRING directly from a stream. Call this method when you
        ///     know that the next ASN.1 encoded element is a OCTET_STRING.
        /// </summary>
        /// <param name="in">
        ///     An input stream containig the encoded ASN.1 data.
        /// </param>
        /// <param name="len">
        ///     Length in bytes
        /// </param>
        object decodeOctetString(Stream stream, int len);

        /* Asn1 TYPE NOT YET SUPPORTED  
                            * Decode an OBJECT_IDENTIFIER directly from a stream.
                            * public Object decodeObjectIdentifier(InputStream in, int len)
                            * throws IOException;
                            */

        /// <summary>
        ///     Decode a CharacterString directly from a stream.
        ///     Decodes any of the specialized character strings.
        /// </summary>
        /// <param name="in">
        ///     An input stream containig the encoded ASN.1 data.
        /// </param>
        /// <param name="len">
        ///     Length in bytes
        /// </param>
        object decodeCharacterString(Stream stream, int len);

        /* No Decoders for ASN.1 structured types. A structured type's value is a
                * collection of other types.
                */
        /* Decoders for ASN.1 useful types
        */
        /* Asn1 TYPE NOT YET SUPPORTED  
        * Decode a GENERALIZED_TIME directly from a stream.
        * public Object decodeGeneralizedTime(InputStream in, int len)
        * throws IOException;
        */
        /* Asn1 TYPE NOT YET SUPPORTED  
        * Decode a UNIVERSAL_TIME directly from a stream.
        * public Object decodeUniversalTime(InputStream in, int len)
        * throws IOException;
        */
        /* Asn1 TYPE NOT YET SUPPORTED  
        * Decode an EXTERNAL directly from a stream.
        * public Object decodeExternal(InputStream in, int len)
        * throws IOException;
        */
        /* Asn1 TYPE NOT YET SUPPORTED  
        * Decode an OBJECT_DESCRIPTOR directly from a stream.
        * public Object decodeObjectDescriptor(InputStream in, int len)
        * throws IOException;
        */
    }

    /// <summary> This class encapsulates the ASN.1 BOOLEAN type.</summary>
    internal class Asn1Boolean : Asn1Object
    {
        private readonly bool _content;

        /// <summary> ASN.1 BOOLEAN tag definition.</summary>
        public const int TAG = 0x01;

        /// <summary>
        ///     ID is added for Optimization.
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, false, TAG);

        /* Constructors for Asn1Boolean
                        */

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Boolean"/> class.
        /// Call this constructor to construct an Asn1Boolean
        /// object from a boolean value.
        /// </summary>
        /// <param name="content">The boolean value to be contained in the
        /// this Asn1Boolean object</param>
        public Asn1Boolean(bool content)
            : base(ID)
        {
            _content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Boolean"/> class.
        /// Constructs an Asn1Boolean object by decoding data from an
        /// input stream.
        /// </summary>
        /// <param name="dec">The decoder object to use when decoding the
        /// input stream.  Sometimes a developer might want to pass
        /// in his/her own decoder object</param>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        public Asn1Boolean(Asn1Decoder dec, Stream stream, int len)
            : base(ID)
        {
            _content = (bool)dec.decodeBoolean(stream, len);
        }

        /* Asn1Object implementation
                */

        /// <summary>
        ///     Encode the current instance into the
        ///     specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">
        ///     Encoder object to use when encoding self.
        /// </param>
        /// <param name="out">
        ///     The output stream onto which the encoded byte
        ///     stream is written.
        /// </param>
        public override void Encode(Asn1Encoder enc, Stream stream)
        {
            enc.encode(this, stream);
        }

        /// <summary>
        /// Returns the content of this Asn1Boolean as a boolean.
        /// </summary>
        /// <returns>Asn1Boolean as a boolean</returns>
        public bool BooleanValue() => _content;

        /// <summary> Returns a String representation of this Asn1Boolean object.</summary>
        public override string ToString()
        {
            return base.ToString() + "BOOLEAN: " + _content;
        }
    }

    /// <summary> This class represents the ASN.1 NULL type.</summary>
    internal class Asn1Null : Asn1Object
    {
        /// <summary> ASN.1 NULL tag definition.</summary>
        public const int TAG = 0x05;

        /// <summary> ID is added for Optimization.</summary>
        /// <summary>
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, false, TAG);

        /* Constructor for Asn1Null
                        */

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Null"/> class.
        /// Call this constructor to construct a new Asn1Null
        /// object.
        /// </summary>
        public Asn1Null()
            : base(ID)
        {
        }

        /// <summary>
        ///     Call this method to encode the current instance into the
        ///     specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">
        ///     Encoder object to use when encoding self.
        /// </param>
        /// <param name="out">
        ///     The output stream onto which the encoded byte
        ///     stream is written.
        /// </param>
        public override void Encode(Asn1Encoder enc, Stream stream)
        {
            enc.encode(this, stream);
        }

        /// <summary> Return a String representation of this Asn1Null object.</summary>
        /// <returns>Asn1Null string</returns>
        public override string ToString()
        {
            return base.ToString() + "NULL: \"\"";
        }
    }

    /// <summary>
    ///     This abstract class is the base class
    ///     for all Asn1 numeric (integral) types. These include
    ///     Asn1Integer and Asn1Enumerated.
    /// </summary>
    internal abstract class Asn1Numeric : Asn1Object
    {
        private readonly long _content;

        internal Asn1Numeric(Asn1Identifier id, int valueRenamed) 
            : base(id)
        {
            _content = valueRenamed;
        }

        internal Asn1Numeric(Asn1Identifier id, long valueRenamed)
            : base(id)
        {
            _content = valueRenamed;
        }

        /// <summary> Returns the content of this Asn1Numeric object as an int.</summary>
        /// <returns>Asn1Numeric</returns>
        public int IntValue()
        {
            return (int)_content;
        }

        /// <summary> Returns the content of this Asn1Numeric object as a long.</summary>
        public long LongValue()
        {
            return _content;
        }
    }

    /// <summary>
    ///     This class provides a means to manipulate ASN.1 Length's. It will
    ///     be used by Asn1Encoder's and Asn1Decoder's by composition.
    /// </summary>
    internal class Asn1Length
    {
        /// <summary> Returns the length of this Asn1Length.</summary>
        public virtual int Length => _length;

        /// <summary> Returns the encoded length of this Asn1Length.</summary>
        public virtual int EncodedLength => _encodedLength;

        private int _length;
        private int _encodedLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Length"/> class. Constructs an empty Asn1Length.  Values are added by calling reset</summary>
        public Asn1Length()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Length"/> class. Constructs an Asn1Length</summary>
        public Asn1Length(int length)
        {
            _length = length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Length"/> class.
        /// Constructs an Asn1Length object by decoding data from an
        /// input stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public Asn1Length(Stream stream)
        {
            var r = stream.ReadByte();
            _encodedLength++;
            if (r == 0x80)
                _length = -1;
            else if (r < 0x80)
                _length = r;
            else
            {
                _length = 0;
                for (r = r & 0x7F; r > 0; r--)
                {
                    var part = stream.ReadByte();
                    _encodedLength++;
                    if (part < 0)
                        throw new EndOfStreamException("BERDecoder: decode: EOF in Asn1Length");
                    _length = (_length << 8) + part;
                }
            }
        }

        /// <summary>
        /// Resets an Asn1Length object by decoding data from an
        /// input stream.
        /// Note: this was added for optimization of Asn1.LBERdecoder.decode()
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Reset(Stream stream)
        {
            _encodedLength = 0;
            var r = stream.ReadByte();
            _encodedLength++;
            if (r == 0x80)
                _length = -1;
            else if (r < 0x80)
                _length = r;
            else
            {
                _length = 0;
                for (r = r & 0x7F; r > 0; r--)
                {
                    var part = stream.ReadByte();
                    _encodedLength++;
                    if (part < 0)
                        throw new EndOfStreamException("BERDecoder: decode: EOF in Asn1Length");
                    _length = (_length << 8) + part;
                }
            }
        }
    }

    /// <summary>
    ///     The Asn1Sequence class can hold an ordered collection of components with
    ///     distinct type.
    ///     This class inherits from the Asn1Structured class which
    ///     provides functionality to hold multiple Asn1 components.
    /// </summary>
    internal class Asn1Sequence : Asn1Structured
    {
        /// <summary> ASN.1 SEQUENCE tag definition.</summary>
        public const int TAG = 0x10;

        /// <summary>
        ///     ID is added for Optimization.
        ///     id needs only be one Value for every instance Thus we create it only once.
        /// </summary>
        private static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, true, TAG);

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Sequence"/> class.
        ///     Constructs an Asn1Sequence object with no actual Asn1Objects in it.
        ///     Assumes a default size of 10 elements.
        /// </summary>
        public Asn1Sequence()
            : base(ID, 10)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Sequence"/> class.
        ///     Constructs an Asn1Sequence object with the specified
        ///     number of placeholders for Asn1Objects.
        ///     It should be noted there are no actual Asn1Objects in this
        ///     SequenceOf object.
        /// </summary>
        /// <param name="size">
        ///     Specifies the initial size of the collection.
        /// </param>
        public Asn1Sequence(int size)
            : base(ID, size)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Sequence"/> class.
        ///     Constructs an Asn1Sequence object with an array representing an
        ///     Asn1 sequence.
        /// </summary>
        /// <param name="newContent">
        ///     the array containing the Asn1 data for the sequence
        /// </param>
        /// <param name="size">
        ///     Specifies the number of items in the array
        /// </param>
        public Asn1Sequence(Asn1Object[] newContent, int size)
            : base(ID, newContent, size)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Sequence"/> class.
        ///     Constructs an Asn1Sequence object by decoding data from an
        ///     input stream.
        /// </summary>
        /// <param name="dec">
        ///     The decoder object to use when decoding the
        ///     input stream.  Sometimes a developer might want to pass
        ///     in his/her own decoder object
        /// </param>
        /// <param name="in">
        ///     A byte stream that contains the encoded ASN.1
        /// </param>
        public Asn1Sequence(Asn1Decoder dec, Stream stream, int len) 
            : base(ID)
        {
            DecodeStructured(dec, stream, len);
        }

        /* Asn1Sequence specific methods
                */

        /// <summary> Return a String representation of this Asn1Sequence.</summary>
        public override string ToString()
        {
            return ToString("SEQUENCE: { ");
        }
    }

    /// <summary>
    /// The Asn1Set class can hold an unordered collection of components with
    /// distinct type. This class inherits from the Asn1Structured class
    /// which already provides functionality to hold multiple Asn1 components.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Structured" />
    internal class Asn1Set : Asn1Structured
    {
        /// <summary> ASN.1 SET tag definition.</summary>
        public const int TAG = 0x11;

        /// <summary> ID is added for Optimization.</summary>
        /// <summary>
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, true, TAG);

        /* Constructors for Asn1Set
                        */

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Set"/> class.
        ///     Constructs an Asn1Set object with no actual
        ///     Asn1Objects in it. Assumes a default size of 5 elements.
        /// </summary>
        public Asn1Set() 
            : base(ID)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Set"/> class.
        ///     Constructs an Asn1Set object with the specified
        ///     number of placeholders for Asn1Objects. However there
        ///     are no actual Asn1Objects in this SequenceOf object.
        /// </summary>
        /// <param name="size">
        ///     Specifies the initial size of the collection.
        /// </param>
        public Asn1Set(int size) 
            : base(ID, size)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Set"/> class.
        ///     Constructs an Asn1Set object by decoding data from an
        ///     input stream.
        /// </summary>
        /// <param name="dec">
        ///     The decoder object to use when decoding the
        ///     input stream.  Sometimes a developer might want to pass
        ///     in his/her own decoder object
        /// </param>
        /// <param name="in">
        ///     A byte stream that contains the encoded ASN.1
        /// </param>
        public Asn1Set(Asn1Decoder dec, Stream stream, int len) 
            : base(ID)
        {
            DecodeStructured(dec, stream, len);
        }

        /* Asn1Set specific methods
                */

        /// <summary> Returns a String representation of this Asn1Set.</summary>
        public override string ToString()
        {
            return ToString("SET: { ");
        }
    }

    /// <summary> This class encapsulates the ASN.1 INTEGER type.</summary>
    internal class Asn1Integer : Asn1Numeric
    {
        /// <summary> ASN.1 INTEGER tag definition.</summary>
        public const int TAG = 0x02;

        /// <summary> ID is added for Optimization.</summary>
        /// <summary>
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, false, TAG);

        /* Constructors for Asn1Integer
                        */

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Integer"/> class.
        ///     Call this constructor to construct an Asn1Integer
        ///     object from an integer value.
        /// </summary>
        /// <param name="content">
        ///     The integer value to be contained in the
        ///     this Asn1Integer object
        /// </param>
        public Asn1Integer(int content) 
            : base(ID, content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Integer"/> class.
        ///     Call this constructor to construct an Asn1Integer
        ///     object from a long value.
        /// </summary>
        /// <param name="content">
        ///     The long value to be contained in the
        ///     this Asn1Integer object
        /// </param>
        public Asn1Integer(long content) 
            : base(ID, content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Integer"/> class.
        ///     Constructs an Asn1Integer object by decoding data from an
        ///     input stream.
        /// </summary>
        /// <param name="dec">
        ///     The decoder object to use when decoding the
        ///     input stream.  Sometimes a developer might want to pass
        ///     in his/her own decoder object
        /// </param>
        /// <param name="in">
        ///     A byte stream that contains the encoded ASN.1
        /// </param>
        public Asn1Integer(Asn1Decoder dec, Stream stream, int len)
            : base(ID, (long)dec.decodeNumeric(stream, len))
        {
        }

        /* Asn1Object implementation
                */

        /// <summary>
        ///     Call this method to encode the current instance into the
        ///     specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">
        ///     Encoder object to use when encoding self.
        /// </param>
        /// <param name="out">
        ///     The output stream onto which the encoded byte
        ///     stream is written.
        /// </param>
        public override void Encode(Asn1Encoder enc, Stream stream)
        {
            enc.encode(this, stream);
        }

        /* Asn1Integer specific methods
                */

        /// <summary> Returns a String representation of this Asn1Integer object.</summary>
        public override string ToString()
        {
            return base.ToString() + "INTEGER: " + LongValue();
        }
    }

    /// <summary> This class encapsulates the ASN.1 ENUMERATED type.</summary>
    internal class Asn1Enumerated : Asn1Numeric
    {
        /// <summary> ASN.1 tag definition for ENUMERATED</summary>
        public const int TAG = 0x0a;

        /// <summary>
        ///     ID is added for Optimization.
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, false, TAG);

        /* Constructors for Asn1Enumerated
                        */

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Enumerated"/> class.
        ///     Call this constructor to construct an Asn1Enumerated
        ///     object from an integer value.
        /// </summary>
        /// <param name="content">
        ///     The integer value to be contained in the
        ///     this Asn1Enumerated object
        /// </param>
        public Asn1Enumerated(int content) 
            : base(ID, content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Enumerated"/> class.
        ///     Call this constructor to construct an Asn1Enumerated
        ///     object from a long value.
        /// </summary>
        /// <param name="content">
        ///     The long value to be contained in the
        ///     this Asn1Enumerated object
        /// </param>
        public Asn1Enumerated(long content)
            : base(ID, content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Enumerated"/> class.
        /// Constructs an Asn1Enumerated object by decoding data from an
        /// input stream.
        /// </summary>
        /// <param name="dec">The decoder object to use when decoding the
        /// input stream.  Sometimes a developer might want to pass
        /// in his/her own decoder object</param>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        public Asn1Enumerated(Asn1Decoder dec, Stream stream, int len)
            : base(ID, (long)dec.decodeNumeric(stream, len))
        {
        }

        /// <summary>
        ///     Call this method to encode the current instance into the
        ///     specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">
        ///     Encoder object to use when encoding self.
        /// </param>
        /// <param name="out">
        ///     The output stream onto which the encoded byte
        ///     stream is written.
        /// </param>
        public override void Encode(Asn1Encoder enc, Stream stream)
        {
            enc.encode(this, stream);
        }

        /// <summary> Return a String representation of this Asn1Enumerated.</summary>
        public override string ToString()
        {
            return base.ToString() + "ENUMERATED: " + LongValue();
        }
    }

    /// <summary>
    ///     The Asn1SequenceOf class is used to hold an ordered collection
    ///     of components with identical type.  This class inherits
    ///     from the Asn1Structured class which already provides
    ///     functionality to hold multiple Asn1 components.
    /// </summary>
    internal class Asn1SequenceOf : Asn1Structured
    {
        /// <summary> ASN.1 SEQUENCE OF tag definition.</summary>
        public const int TAG = 0x10;

        /// <summary>
        ///     ID is added for Optimization.
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, true, TAG);

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1SequenceOf"/> class.
        ///     Constructs an Asn1SequenceOf object with no actual
        ///     Asn1Objects in it. Assumes a default size of 5 elements.
        /// </summary>
        public Asn1SequenceOf() 
            : base(ID)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1SequenceOf"/> class.
        /// Constructs an Asn1SequenceOf object with the specified
        /// number of placeholders for Asn1Objects. However there
        /// are no actual Asn1Objects in this SequenceOf object.
        /// </summary>
        /// <param name="size">Specifies the initial size of the collection.</param>
        public Asn1SequenceOf(int size) 
            : base(ID, size)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1SequenceOf"/> class.
        ///     A copy constructor which creates an Asn1SequenceOf from an
        ///     instance of Asn1Sequence.
        ///     Since SEQUENCE and SEQUENCE_OF have the same identifier, the decoder
        ///     will always return a SEQUENCE object when it detects that identifier.
        ///     In order to take advantage of the Asn1SequenceOf type, we need to be
        ///     able to construct this object when knowingly receiving an
        ///     Asn1Sequence.
        /// </summary>
        public Asn1SequenceOf(Asn1Sequence sequence) 
            : base(ID, sequence.ToArray(), sequence.Size())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1SequenceOf"/> class.
        /// Constructs an Asn1SequenceOf object by decoding data from an
        /// input stream.
        /// </summary>
        /// <param name="dec">The decoder object to use when decoding the
        /// input stream.  Sometimes a developer might want to pass
        /// in his/her own decoder object</param>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        public Asn1SequenceOf(Asn1Decoder dec, Stream stream, int len) 
            : base(ID)
        {
            DecodeStructured(dec, stream, len);
        }

        /// <summary>
        /// Returns a String representation of this Asn1SequenceOf object
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString("SEQUENCE OF: { ");
        }
    }
}

#endif