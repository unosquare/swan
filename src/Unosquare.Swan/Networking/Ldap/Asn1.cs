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
        /// <summary>
        /// ASN.1 SET OF tag definition.
        /// </summary>
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
        /// <param name="set">The set.</param>
        public Asn1SetOf(Asn1Set set)
            : base(ID, set.ToArray(), set.Size())
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
        private Asn1Object _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Choice" /> class.
        /// Constructs an Asn1Choice object using an Asn1Object value.
        /// </summary>
        /// <param name="content">The Asn1Object that this Asn1Choice will
        /// encode.  Since all Asn1 objects are derived from Asn1Object
        /// any basic type can be passed in.</param>
        public Asn1Choice(Asn1Object content)
            : base(null)
        {
            _content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Choice" /> class.
        /// No arg Constructor. This is used by Filter, who subsequently sets the
        /// content after parsing the RFC 2254 Search Filter String.
        /// </summary>
        protected internal Asn1Choice()
            : base(null)
        {
            _content = null;
        }

        /// <summary>
        /// Sets the CHOICE value stored in this Asn1Choice.
        /// </summary>
        /// <value>
        /// The choice value.
        /// </value>
        protected internal virtual Asn1Object ChoiceValue
        {
            get => _content;
            set => _content = value;
        }

        /// <summary>
        /// Call this method to encode the contents of this Asn1Choice
        /// instance into the specified output stream using the
        /// specified encoder object.
        /// </summary>
        /// <param name="enc">Encoder object to use when encoding self.</param>
        /// <param name="stream">The stream.</param>
        public override void Encode(IAsn1Encoder enc, Stream stream) => _content.Encode(enc, stream);
        
        /// <summary>
        /// This method will return the Asn1Identifier of the
        /// encoded Asn1Object.We  override the parent method
        /// as the identifier of an Asn1Choice depends on the
        /// type of the object encoded by this Asn1Choice.
        /// </summary>
        /// <returns>
        /// Asn1 Identifier
        /// </returns>
        public override Asn1Identifier GetIdentifier() => _content.GetIdentifier();

        /// <summary>
        /// Sets the identifier of the contained Asn1Object. We
        /// override the parent method as the identifier of
        /// an Asn1Choice depends on the type of the object
        /// encoded by this Asn1Choice.
        /// </summary>
        /// <param name="id">An Asn1Identifier object representing the CLASS,
        /// FORM and TAG)</param>
        public override void SetIdentifier(Asn1Identifier id) => _content.SetIdentifier(id);

        /// <summary>
        /// Return a String representation of this Asn1Object.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => _content.ToString();
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
        /// Returns a boolean value indicating whether or not this Asn1Identifier
        /// has a TAG CLASS of CONTEXT-SPECIFIC.
        /// </summary>
        /// <value>
        ///   <c>true</c> if context; otherwise, <c>false</c>.
        /// </value>
        /// <seealso cref="CONTEXT"></seealso>
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
            {
                // if true, its a multiple octet identifier.
                tag = DecodeTagNumber(stream);
            }
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
            {
                // if true, its a multiple octet identifier.
                tag = DecodeTagNumber(stream);
            }
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
                if ((r & 0x80) == 0) break;
            }

            return n;
        }
    }

    /// <summary>
    /// This is the base class for all other Asn1 types.
    /// </summary>
    internal abstract class Asn1Object
    {
        private static readonly string[] ClassTypes = { "[UNIVERSAL ", "[APPLICATION ", "[", "[PRIVATE " };

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
        public abstract void Encode(IAsn1Encoder enc, Stream stream);

        /// <summary>
        ///     Returns the identifier for this Asn1Object as an Asn1Identifier.
        ///     This Asn1Identifier object will include the CLASS, FORM and TAG
        ///     for this Asn1Object.
        /// </summary>
        /// <returns>Asn1 Identifier</returns>
        public virtual Asn1Identifier GetIdentifier() => id;

        /// <summary>
        /// Sets the identifier for this Asn1Object. This is helpful when
        /// creating implicit Asn1Tagged types.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        public virtual void SetIdentifier(Asn1Identifier identifier) => id = identifier;

        /// <summary>
        /// This method returns a byte array representing the encoded
        /// Asn1Object.  It in turn calls the encode method that is
        /// defined in Asn1Object but will usually be implemented
        /// in the child Asn1 classes.
        /// </summary>
        /// <param name="enc">The enc.</param>
        /// <returns>
        /// Byte array
        /// </returns>
        /// <exception cref="Exception">IOException while encoding to byte array: " + e</exception>
        public sbyte[] GetEncoding(IAsn1Encoder enc)
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

        /// <summary>
        /// Return a String representation of this Asn1Object.
        /// </summary>
        /// <returns>
        /// String object
        /// </returns>
        public override string ToString()
        {
            var identifier = GetIdentifier(); // could be overridden.
            return new StringBuilder()
                .Append(ClassTypes[identifier.Asn1Class]).Append(identifier.Tag).Append("] ")
                .ToString();
        }
    }

    /// <summary>
    /// This class encapsulates the OCTET STRING type.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Object" />
    internal class Asn1OctetString : Asn1Object
    {
        private readonly sbyte[] _content;

        /// <summary> ASN.1 OCTET STRING tag definition.</summary>
        public const int TAG = 0x04;

        /// <summary>
        /// ID is added for Optimization.
        /// Id needs only be one Value for every instance,
        /// thus we create it only once.
        /// </summary>
        protected internal static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, false, TAG);

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1OctetString" /> class.
        /// Call this constructor to construct an Asn1OctetString
        /// object from a byte array.
        /// </summary>
        /// <param name="content">A byte array representing the string that
        /// will be contained in the this Asn1OctetString object</param>
        public Asn1OctetString(sbyte[] content) 
            : base(ID)
        {
            _content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1OctetString" /> class.
        /// Call this constructor to construct an Asn1OctetString
        /// object from a String object.
        /// </summary>
        /// <param name="content">A string value that will be contained
        /// in the this Asn1OctetString object</param>
        /// <exception cref="Exception"></exception>
        public Asn1OctetString(string content) 
            : base(ID)
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
        public Asn1OctetString(IAsn1Decoder dec, Stream stream, int len) 
            : base(ID)
        {
            _content = len > 0 ? (sbyte[])dec.DecodeOctetString(stream, len) : new sbyte[0];
        }

        /// <summary>
        /// Call this method to encode the current instance into the
        /// specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">Encoder object to use when encoding self.</param>
        /// <param name="stream">The stream.</param>
        public override void Encode(IAsn1Encoder enc, Stream stream) => enc.Encode(this, stream);

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
        public override string ToString() => base.ToString() + "OCTET STRING: " + StringValue();
    }

    /// <summary>
    /// The Asn1Tagged class can hold a base Asn1Object with a distinctive tag
    /// describing the type of that base object. It also maintains a boolean value
    /// indicating whether the value should be encoded by EXPLICIT or IMPLICIT
    /// means. (Explicit is true by default.)
    /// If the type is encoded IMPLICITLY, the base types form, length and content
    /// will be encoded as usual along with the class type and tag specified in
    /// the constructor of this Asn1Tagged class.
    /// If the type is to be encoded EXPLICITLY, the base type will be encoded as
    /// usual after the Asn1Tagged identifier has been encoded.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Object" />
    internal class Asn1Tagged : Asn1Object
    {
        private readonly bool _explicitRenamed;
        private Asn1Object _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Tagged" /> class.
        /// Constructs an Asn1Tagged object using the provided
        /// AN1Identifier and the Asn1Object.
        /// The explicit flag defaults to true as per the spec.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="objectRenamed">The object renamed.</param>
        public Asn1Tagged(Asn1Identifier identifier, Asn1Object objectRenamed) 
            : this(identifier, objectRenamed, true)
        {
        }

        /// <summary>
        /// Constructs an Asn1Tagged object.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="objectRenamed">The object renamed.</param>
        /// <param name="explicitRenamed">if set to <c>true</c> [explicit renamed].</param>
        public Asn1Tagged(Asn1Identifier identifier, Asn1Object objectRenamed, bool explicitRenamed)
            : base(identifier)
        {
            _content = objectRenamed;
            _explicitRenamed = explicitRenamed;
            if (!explicitRenamed)
            {
                // replace object's id with new tag.
                _content?.SetIdentifier(identifier);
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
        public Asn1Tagged(IAsn1Decoder dec, Stream stream, int len, Asn1Identifier identifier)
            : base(identifier)
        {
            // If we are decoding an implicit tag, there is no way to know at this
            // low level what the base type really is. We can place the content
            // into an Asn1OctetString type and pass it back to the application who
            // will be able to create the appropriate ASN.1 type for this tag.
            _content = new Asn1OctetString(dec, stream, len);
        }

        /// <summary>
        /// Sets the Asn1Object tagged value
        /// </summary>
        /// <value>
        /// The tagged value.
        /// </value>
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

            get => _content;
        }

        /// <summary>
        /// Returns a boolean value indicating if this object uses
        /// EXPLICIT tagging.
        /// </summary>
        /// <value>
        ///   <c>true</c> if explicit; otherwise, <c>false</c>.
        /// </value>
        public virtual bool Explicit => _explicitRenamed;

        /// <summary>
        /// Call this method to encode the current instance into the
        /// specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">Encoder object to use when encoding self.</param>
        /// <param name="stream">The stream.</param>
        public override void Encode(IAsn1Encoder enc, Stream stream) => enc.Encode(this, stream);
        
        /// <summary>
        /// Return a String representation of this Asn1Object.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => _explicitRenamed ? base.ToString() + _content : _content.ToString();
    }

    /// <summary>
    /// This class serves as the base type for all ASN.1
    /// structured types.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Object" />
    internal abstract class Asn1Structured : Asn1Object
    {
        private Asn1Object[] _content;
        private int _contentIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Structured" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        protected internal Asn1Structured(Asn1Identifier id) 
            : this(id, 10)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Structured" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="size">The size.</param>
        protected internal Asn1Structured(Asn1Identifier id, int size) 
            : base(id)
        {
            _content = new Asn1Object[size];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Structured" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="newContent">The new content.</param>
        /// <param name="size">The size.</param>
        protected internal Asn1Structured(Asn1Identifier id, Asn1Object[] newContent, int size) 
            : base(id)
        {
            _content = newContent;
            _contentIndex = size;
        }

        /// <summary>
        /// Encodes the contents of this Asn1Structured directly to an output
        /// stream.
        /// </summary>
        /// <param name="enc">The enc.</param>
        /// <param name="stream">The stream.</param>
        public override void Encode(IAsn1Encoder enc, Stream stream) => enc.Encode(this, stream);

        /// <summary>
        /// Decode an Asn1Structured type from an InputStream.
        /// </summary>
        /// <param name="dec">The decimal.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        protected internal void DecodeStructured(IAsn1Decoder dec, Stream stream, int len)
        {
            var componentLen = new int[1]; // collects length of component

            while (len > 0)
            {
                Add(dec.Decode(stream, componentLen));
                len -= componentLen[0];
            }
        }

        /// <summary>
        /// Returns an array containing the individual ASN.1 elements
        /// of this Asn1Structed object.
        /// </summary>
        /// <returns>
        /// an array of Asn1Objects
        /// </returns>
        public Asn1Object[] ToArray()
        {
            var cloneArray = new Asn1Object[_contentIndex];
            Array.Copy(_content, 0, cloneArray, 0, _contentIndex);
            return cloneArray;
        }

        /// <summary>
        /// Adds a new Asn1Object to the end of this Asn1Structured
        /// object.
        /// </summary>
        /// <param name="valueRenamed">The value renamed.</param>
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
        /// Gets a specific Asn1Object in this structred object.
        /// </summary>
        /// <param name="index">The index of the Asn1Object to get from
        /// this Asn1Structured object.</param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Asn1Object Get(int index)
        {
            if (index >= _contentIndex || index < 0)
            {
                throw new IndexOutOfRangeException($"Asn1Structured: set: index {index}, size {_contentIndex}");
            }

            return _content[index];
        }

        /// <summary>
        /// Returns the number of Asn1Obejcts that have been encoded
        /// into this Asn1Structured class.
        /// </summary>
        /// <returns></returns>
        public int Size() => _contentIndex;

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

        public Asn1Object SetRenamed(int index, Asn1Object valueRenamed)
        {
            if (index>= _contentIndex || index < 0)
            {
                throw new IndexOutOfRangeException($"Asn1Structured: set: index {index}, size {_contentIndex}");
            }

            return _content[index] = valueRenamed;
        }

        public Asn1Object GetRenamed(int index)
        {
            if (index >= _contentIndex || index < 0)
            {
                throw new IndexOutOfRangeException($"Asn1Structured: set: index {index}, size {_contentIndex}");
            }

            return _content[index];
        }
    }

    /// <summary>
    /// This class encapsulates the ASN.1 BOOLEAN type.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Object" />
    internal class Asn1Boolean : Asn1Object
    {
        private readonly bool _content;

        /// <summary> ASN.1 BOOLEAN tag definition.</summary>
        public const int TAG = 0x01;

        /// <summary>
        /// ID is added for Optimization.
        /// ID needs only be one Value for every instance,
        /// thus we create it only once.
        /// </summary>
        public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, false, TAG);
        
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
        public Asn1Boolean(IAsn1Decoder dec, Stream stream, int len)
            : base(ID)
        {
            _content = (bool)dec.DecodeBoolean(stream, len);
        }
        
        /// <summary>
        /// Encode the current instance into the
        /// specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">Encoder object to use when encoding self.</param>
        /// <param name="stream">The stream.</param>
        public override void Encode(IAsn1Encoder enc, Stream stream) => enc.Encode(this, stream);

        /// <summary>
        /// Returns the content of this Asn1Boolean as a boolean.
        /// </summary>
        /// <returns>Asn1Boolean as a boolean</returns>
        public bool BooleanValue() => _content;

        /// <summary>
        /// Returns a String representation of this Asn1Boolean object.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => base.ToString() + "BOOLEAN: " + _content;
    }

    /// <summary>
    /// This class represents the ASN.1 NULL type.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Object" />
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
        /// Call this method to encode the current instance into the
        /// specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">Encoder object to use when encoding self.</param>
        /// <param name="stream">The stream.</param>
        public override void Encode(IAsn1Encoder enc, Stream stream) => enc.Encode(this, stream);

        /// <summary> Return a String representation of this Asn1Null object.</summary>
        /// <returns>Asn1Null string</returns>
        public override string ToString() => base.ToString() + "NULL: \"\"";
    }

    /// <summary>
    /// This abstract class is the base class
    /// for all Asn1 numeric (integral) types. These include
    /// Asn1Integer and Asn1Enumerated.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Object" />
    internal abstract class Asn1Numeric : Asn1Object
    {
        private readonly long _content;

        internal Asn1Numeric(Asn1Identifier id, int numericValue) 
            : base(id)
        {
            _content = numericValue;
        }

        internal Asn1Numeric(Asn1Identifier id, long numericValue)
            : base(id)
        {
            _content = numericValue;
        }

        /// <summary> Returns the content of this Asn1Numeric object as an int.</summary>
        /// <returns>Asn1Numeric</returns>
        public int IntValue() => (int)_content;

        /// <summary>
        /// Returns the content of this Asn1Numeric object as a long.
        /// </summary>
        /// <returns></returns>
        public long LongValue() => _content;
    }

    /// <summary>
    /// This class provides a means to manipulate ASN.1 Length's. It will
    /// be used by Asn1Encoder's and Asn1Decoder's by composition.
    /// </summary>
    internal class Asn1Length
    {
        private int _length;
        private int _encodedLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Length"/> class. Constructs an empty Asn1Length.  Values are added by calling reset</summary>
        public Asn1Length()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Length" /> class. Constructs an Asn1Length
        /// </summary>
        /// <param name="length">The length.</param>
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
            {
                _length = -1;
            }
            else if (r < 0x80)
            {
                _length = r;
            }
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

        /// <summary> Returns the length of this Asn1Length.</summary>
        public virtual int Length => _length;

        /// <summary> Returns the encoded length of this Asn1Length.</summary>
        public virtual int EncodedLength => _encodedLength;

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
            {
                _length = -1;
            }
            else if (r < 0x80)
            {
                _length = r;
            }
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
        /// Initializes a new instance of the <see cref="Asn1Sequence" /> class.
        /// Constructs an Asn1Sequence object with an array representing an
        /// Asn1 sequence.
        /// </summary>
        /// <param name="newContent">the array containing the Asn1 data for the sequence</param>
        /// <param name="size">Specifies the number of items in the array</param>
        public Asn1Sequence(Asn1Object[] newContent, int size)
            : base(ID, newContent, size)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Sequence" /> class.
        /// Constructs an Asn1Sequence object by decoding data from an
        /// input stream.
        /// </summary>
        /// <param name="dec">The decoder object to use when decoding the
        /// input stream.  Sometimes a developer might want to pass
        /// in his/her own decoder object</param>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        public Asn1Sequence(IAsn1Decoder dec, Stream stream, int len) 
            : base(ID)
        {
            DecodeStructured(dec, stream, len);
        }

        /// <summary>
        /// Return a String representation of this Asn1Sequence.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => ToString("SEQUENCE: { ");
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Set" /> class.
        /// Constructs an Asn1Set object with no actual
        /// Asn1Objects in it. Assumes a default size of 5 elements.
        /// </summary>
        public Asn1Set() 
            : base(ID)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Set" /> class.
        /// Constructs an Asn1Set object with the specified
        /// number of placeholders for Asn1Objects. However there
        /// are no actual Asn1Objects in this SequenceOf object.
        /// </summary>
        /// <param name="size">Specifies the initial size of the collection.</param>
        public Asn1Set(int size) 
            : base(ID, size)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Set" /> class.
        /// Constructs an Asn1Set object by decoding data from an
        /// input stream.
        /// </summary>
        /// <param name="dec">The decoder object to use when decoding the
        /// input stream.  Sometimes a developer might want to pass
        /// in his/her own decoder object</param>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        public Asn1Set(IAsn1Decoder dec, Stream stream, int len) 
            : base(ID)
        {
            DecodeStructured(dec, stream, len);
        }
        
        /// <summary>
        /// Returns a String representation of this Asn1Set.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => ToString("SET: { ");
    }

    /// <summary>
    /// This class encapsulates the ASN.1 INTEGER type.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Numeric" />
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Integer" /> class.
        /// Call this constructor to construct an Asn1Integer
        /// object from an integer value.
        /// </summary>
        /// <param name="content">The integer value to be contained in the
        /// this Asn1Integer object</param>
        public Asn1Integer(int content) 
            : base(ID, content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Integer" /> class.
        /// Call this constructor to construct an Asn1Integer
        /// object from a long value.
        /// </summary>
        /// <param name="content">The long value to be contained in the
        /// this Asn1Integer object</param>
        public Asn1Integer(long content) 
            : base(ID, content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Integer" /> class.
        /// Constructs an Asn1Integer object by decoding data from an
        /// input stream.
        /// </summary>
        /// <param name="dec">The decoder object to use when decoding the
        /// input stream.  Sometimes a developer might want to pass
        /// in his/her own decoder object</param>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        public Asn1Integer(IAsn1Decoder dec, Stream stream, int len)
            : base(ID, (long)dec.DecodeNumeric(stream, len))
        {
        }

        /// <summary>
        /// Call this method to encode the current instance into the
        /// specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">Encoder object to use when encoding self.</param>
        /// <param name="stream">The stream.</param>
        public override void Encode(IAsn1Encoder enc, Stream stream) => enc.Encode(this, stream);

        /// <summary>
        /// Returns a String representation of this Asn1Integer object.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => base.ToString() + "INTEGER: " + LongValue();
    }

    /// <summary>
    /// This class encapsulates the ASN.1 ENUMERATED type.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Numeric" />
    internal class Asn1Enumerated : Asn1Numeric
    {
        /// <summary>
        /// ASN.1 tag definition for ENUMERATED
        /// </summary>
        public const int TAG = 0x0a;

        /// <summary>
        ///     ID is added for Optimization.
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, false, TAG);

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Enumerated" /> class.
        /// Call this constructor to construct an Asn1Enumerated
        /// object from an integer value.
        /// </summary>
        /// <param name="content">The integer value to be contained in the
        /// this Asn1Enumerated object</param>
        public Asn1Enumerated(int content) 
            : base(ID, content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Enumerated" /> class.
        /// Call this constructor to construct an Asn1Enumerated
        /// object from a long value.
        /// </summary>
        /// <param name="content">The long value to be contained in the
        /// this Asn1Enumerated object</param>
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
        public Asn1Enumerated(IAsn1Decoder dec, Stream stream, int len)
            : base(ID, (long)dec.DecodeNumeric(stream, len))
        {
        }

        /// <summary>
        /// Call this method to encode the current instance into the
        /// specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">Encoder object to use when encoding self.</param>
        /// <param name="stream">The stream.</param>
        public override void Encode(IAsn1Encoder enc, Stream stream) => enc.Encode(this, stream);

        /// <summary>
        /// Return a String representation of this Asn1Enumerated.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => base.ToString() + "ENUMERATED: " + LongValue();
    }

    /// <summary>
    /// The Asn1SequenceOf class is used to hold an ordered collection
    /// of components with identical type.  This class inherits
    /// from the Asn1Structured class which already provides
    /// functionality to hold multiple Asn1 components.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Structured" />
    internal class Asn1SequenceOf : Asn1Structured
    {
        /// <summary>
        /// ASN.1 SEQUENCE OF tag definition.
        /// </summary>
        public const int TAG = 0x10;

        /// <summary>
        ///     ID is added for Optimization.
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        public static readonly Asn1Identifier ID = new Asn1Identifier(Asn1Identifier.UNIVERSAL, true, TAG);

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1SequenceOf" /> class.
        /// Constructs an Asn1SequenceOf object with no actual
        /// Asn1Objects in it. Assumes a default size of 5 elements.
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
        /// Initializes a new instance of the <see cref="Asn1SequenceOf" /> class.
        /// A copy constructor which creates an Asn1SequenceOf from an
        /// instance of Asn1Sequence.
        /// Since SEQUENCE and SEQUENCE_OF have the same identifier, the decoder
        /// will always return a SEQUENCE object when it detects that identifier.
        /// In order to take advantage of the Asn1SequenceOf type, we need to be
        /// able to construct this object when knowingly receiving an
        /// Asn1Sequence.
        /// </summary>
        /// <param name="sequence">The sequence.</param>
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
        public Asn1SequenceOf(IAsn1Decoder dec, Stream stream, int len) 
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
        public override string ToString() => ToString("SEQUENCE OF: { ");
    }
}

#endif