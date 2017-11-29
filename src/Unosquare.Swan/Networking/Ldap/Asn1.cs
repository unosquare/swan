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
        public const int Tag = 0x11;

        public static readonly Asn1Identifier Id = new Asn1Identifier(Asn1IdentifierTag.Universal, true, Tag);
        
        public Asn1SetOf(int size = 10)
            : base(Id, size)
        {
        }
        
        public override string ToString() => ToString("SET OF: { ");
    }

    /// <summary>
    /// The Asn1Choice object represents the choice of any Asn1Object. All
    /// Asn1Object methods are delegated to the object this Asn1Choice contains.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Object" />
    internal class Asn1Choice
        : Asn1Object
    {
        private Asn1Object _content;
        
        public Asn1Choice(Asn1Object content = null)
        {
            _content = content;
        }
        
        protected internal virtual Asn1Object ChoiceValue
        {
            get => _content;
            set => _content = value;
        }
        
        public override void Encode(IAsn1Encoder enc, Stream stream) => _content.Encode(enc, stream);
        
        public override Asn1Identifier GetIdentifier() => _content.GetIdentifier();
        
        public override void SetIdentifier(Asn1Identifier id) => _content.SetIdentifier(id);
        
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
    internal sealed class Asn1Identifier
    {
        public Asn1Identifier(Asn1IdentifierTag tagClass, bool constructed, int tag)
        {
            Asn1Class = tagClass;
            Constructed = constructed;
            Tag = tag;
        }
        
        public Asn1Identifier(LdapOperation tag)
            : this(Asn1IdentifierTag.Application, true, (int) tag)
        {
        }
        
        public Asn1Identifier(int contextTag, bool constructed = false)
            : this(Asn1IdentifierTag.Context, constructed, contextTag)
        {
        }
        
        public Asn1Identifier(Stream stream)
        {
            var r = stream.ReadByte();
            EncodedLength++;
            if (r < 0)
                throw new EndOfStreamException("BERDecoder: decode: EOF in Identifier");

            Asn1Class = (Asn1IdentifierTag) (r >> 6);
            Constructed = (r & 0x20) != 0;
            Tag = r & 0x1F; // if tag < 30 then its a single octet identifier.

            if (Tag == 0x1F)
            {
                // if true, its a multiple octet identifier.
                Tag = DecodeTagNumber(stream);
            }
        }
        
        public Asn1IdentifierTag Asn1Class { get; }
        
        public bool Constructed { get; }
        
        public int Tag { get; }
        
        public int EncodedLength { get; private set; }
        
        public bool Universal => Asn1Class == Asn1IdentifierTag.Universal;
        
        /// <summary>
        /// Creates a duplicate, not a true clone, of this object and returns
        /// a reference to the duplicate.
        /// </summary>
        /// <returns>Cloned object</returns>
        public object Clone() => MemberwiseClone();

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
                EncodedLength++;
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
        private static readonly string[] ClassTypes = {"[UNIVERSAL ", "[APPLICATION ", "[", "[PRIVATE "};

        private Asn1Identifier _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Object"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        protected Asn1Object(Asn1Identifier id = null)
        {
            _id = id;
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
        public virtual Asn1Identifier GetIdentifier() => _id;

        /// <summary>
        /// Sets the identifier for this Asn1Object. This is helpful when
        /// creating implicit Asn1Tagged types.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        public virtual void SetIdentifier(Asn1Identifier identifier) => _id = identifier;

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
        public sbyte[] GetEncoding(IAsn1Encoder enc)
        {
            using (var stream = new MemoryStream())
            {
                Encode(enc, stream);

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
                .Append(ClassTypes[(int) identifier.Asn1Class]).Append(identifier.Tag).Append("] ")
                .ToString();
        }
    }

    /// <summary>
    /// This class encapsulates the OCTET STRING type.
    /// </summary>
    /// <seealso cref="Asn1Object" />
    internal sealed class Asn1OctetString
        : Asn1Object
    {
        public const int Tag = 0x04;

        private readonly sbyte[] _content;

        private static readonly Asn1Identifier Id = new Asn1Identifier(Asn1IdentifierTag.Universal, false, Tag);
        
        public Asn1OctetString(sbyte[] content)
            : base(Id)
        {
            _content = content;
        }
        
        public Asn1OctetString(string content)
            : base(Id)
        {
            _content = Encoding.UTF8.GetSBytes(content);
        }
        
        public Asn1OctetString(IAsn1Decoder dec, Stream stream, int len)
            : base(Id)
        {
            _content = len > 0 ? (sbyte[]) dec.DecodeOctetString(stream, len) : new sbyte[0];
        }
        
        public override void Encode(IAsn1Encoder enc, Stream stream) => enc.Encode(this, stream);
        
        public sbyte[] ByteValue() => _content;
        
        public string StringValue() => Encoding.UTF8.GetString(_content);
        
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
        private Asn1Object _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Tagged"/> class.
        /// </summary>
        /// <param name="identifier">The identifier.</param>
        /// <param name="obj">The object renamed.</param>
        /// <param name="isExplicit">if set to <c>true</c> [explicit].</param>
        public Asn1Tagged(Asn1Identifier identifier, Asn1Object obj = null, bool isExplicit = true)
            : base(identifier)
        {
            _content = obj;
            Explicit = isExplicit;

            if (!isExplicit)
            {
                // replace object's id with new tag.
                _content?.SetIdentifier(identifier);
            }
        }

        public Asn1Tagged(Asn1Identifier identifier, sbyte[] vals)
            : base(identifier)
        {
            _content = new Asn1OctetString(vals);
            Explicit = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Tagged"/> class by decoding data from an
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
        
        public Asn1Object TaggedValue
        {
            get => _content;

            set
            {
                _content = value;
                if (!Explicit)
                {
                    // replace object's id with new tag.
                    value?.SetIdentifier(GetIdentifier());
                }
            }
        }
        
        public bool Explicit { get; }
        
        public override void Encode(IAsn1Encoder enc, Stream stream) => enc.Encode(this, stream);
        
        public override string ToString() => Explicit ? base.ToString() + _content : _content.ToString();
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
        
        protected internal Asn1Structured(Asn1Identifier id, int size = 10)
            : base(id)
        {
            _content = new Asn1Object[size];
        }
        
        protected internal Asn1Structured(Asn1Identifier id, Asn1Object[] newContent, int size)
            : base(id)
        {
            _content = newContent;
            _contentIndex = size;
        }
        
        public override void Encode(IAsn1Encoder enc, Stream stream) => enc.Encode(this, stream);
        
        public Asn1Object[] ToArray()
        {
            var cloneArray = new Asn1Object[_contentIndex];
            Array.Copy(_content, 0, cloneArray, 0, _contentIndex);
            return cloneArray;
        }

        public void Add(string s) => Add(new Asn1OctetString(s));
        
        public void Add(Asn1Object obj)
        {
            if (_contentIndex == _content.Length)
            {
                // Array too small, need to expand it, double length
                var newSize = _contentIndex + _contentIndex;
                var newArray = new Asn1Object[newSize];
                Array.Copy(_content, 0, newArray, 0, _contentIndex);
                _content = newArray;
            }

            _content[_contentIndex++] = obj;
        }
        
        public void Set(int index, Asn1Object value)
        {
            if (index >= _contentIndex || index < 0)
            {
                throw new IndexOutOfRangeException($"Asn1Structured: get: index {index}, size {_contentIndex}");
            }

            _content[index] = value;
        }
        
        public Asn1Object Get(int index)
        {
            if (index >= _contentIndex || index < 0)
            {
                throw new IndexOutOfRangeException($"Asn1Structured: set: index {index}, size {_contentIndex}");
            }

            return _content[index];
        }
        
        public int Size() => _contentIndex;
        
        public string ToString(string type)
        {
            var sb = new StringBuilder().Append(type);

            for (var i = 0; i < _contentIndex; i++)
            {
                sb.Append(_content[i]);
                if (i != _contentIndex - 1)
                    sb.Append(", ");
            }

            sb.Append(" }");

            return base.ToString() + sb;
        }
        
        protected internal void DecodeStructured(IAsn1Decoder dec, Stream stream, int len)
        {
            var componentLen = new int[1]; // collects length of component

            while (len > 0)
            {
                Add(dec.Decode(stream, componentLen));
                len -= componentLen[0];
            }
        }
    }

    /// <summary>
    /// This class encapsulates the ASN.1 BOOLEAN type.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Object" />
    internal class Asn1Boolean
        : Asn1Object
    {
        public const int Tag = 0x01;

        public static readonly Asn1Identifier Id = new Asn1Identifier(Asn1IdentifierTag.Universal, false, Tag);

        private readonly bool _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Boolean"/> class.
        /// Call this constructor to construct an Asn1Boolean
        /// object from a boolean value.
        /// </summary>
        /// <param name="content">The boolean value to be contained in the
        /// this Asn1Boolean object</param>
        public Asn1Boolean(bool content)
            : base(Id)
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
            : base(Id)
        {
            _content = (bool) dec.DecodeBoolean(stream, len);
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
    internal sealed class Asn1Null
        : Asn1Object
    {
        public const int Tag = 0x05;

        public static readonly Asn1Identifier Id = new Asn1Identifier(Asn1IdentifierTag.Universal, false, Tag);

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Null"/> class.
        /// Call this constructor to construct a new Asn1Null
        /// object.
        /// </summary>
        public Asn1Null()
            : base(Id)
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
        
        public int IntValue() => (int) _content;
        
        public long LongValue() => _content;
    }

    /// <summary>
    /// This class provides a means to manipulate ASN.1 Length's. It will
    /// be used by Asn1Encoder's and Asn1Decoder's by composition.
    /// </summary>
    internal sealed class Asn1Length
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Length"/> class. Constructs an empty Asn1Length.  Values are added by calling reset</summary>
        public Asn1Length()
        {
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
            EncodedLength++;
            if (r == 0x80)
            {
                Length = -1;
            }
            else if (r < 0x80)
            {
                Length = r;
            }
            else
            {
                Length = 0;
                for (r = r & 0x7F; r > 0; r--)
                {
                    var part = stream.ReadByte();
                    EncodedLength++;
                    if (part < 0)
                        throw new EndOfStreamException("BERDecoder: decode: EOF in Asn1Length");
                    Length = (Length << 8) + part;
                }
            }
        }

        public int Length { get; private set; }

        public int EncodedLength { get; private set; }

        /// <summary>
        /// Resets an Asn1Length object by decoding data from an
        /// input stream.
        /// Note: this was added for optimization of Asn1.LBERdecoder.decode()
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Reset(Stream stream)
        {
            EncodedLength = 0;
            var r = stream.ReadByte();
            EncodedLength++;

            if (r == 0x80)
            {
                Length = -1;
            }
            else if (r < 0x80)
            {
                Length = r;
            }
            else
            {
                Length = 0;
                for (r = r & 0x7F; r > 0; r--)
                {
                    var part = stream.ReadByte();
                    EncodedLength++;
                    if (part < 0)
                        throw new EndOfStreamException("BERDecoder: decode: EOF in Asn1Length");
                    Length = (Length << 8) + part;
                }
            }
        }
    }

    /// <summary>
    /// The Asn1Sequence class can hold an ordered collection of components with
    /// distinct type.
    /// This class inherits from the Asn1Structured class which
    /// provides functionality to hold multiple Asn1 components.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Structured" />
    internal class Asn1Sequence
        : Asn1Structured
    {
        public const int Tag = 0x10;

        private static readonly Asn1Identifier Id = new Asn1Identifier(Asn1IdentifierTag.Universal, true, Tag);

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
            : base(Id, size)
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
            : base(Id)
        {
            DecodeStructured(dec, stream, len);
        }

        public override string ToString() => ToString("SEQUENCE: { ");
    }

    /// <summary>
    /// The Asn1Set class can hold an unordered collection of components with
    /// distinct type. This class inherits from the Asn1Structured class
    /// which already provides functionality to hold multiple Asn1 components.
    /// </summary>
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Structured" />
    internal sealed class Asn1Set
        : Asn1Structured
    {
        public const int Tag = 0x11;

        public static readonly Asn1Identifier Id = new Asn1Identifier(Asn1IdentifierTag.Universal, true, Tag);

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
            : base(Id)
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
    internal class Asn1Integer
        : Asn1Numeric
    {
        public const int Tag = 0x02;

        public static readonly Asn1Identifier Id = new Asn1Identifier(Asn1IdentifierTag.Universal, false, Tag);

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Integer" /> class.
        /// Call this constructor to construct an Asn1Integer
        /// object from an integer value.
        /// </summary>
        /// <param name="content">The integer value to be contained in the
        /// this Asn1Integer object</param>
        public Asn1Integer(int content)
            : base(Id, content)
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
            : base(Id, (long) dec.DecodeNumeric(stream, len))
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
    internal sealed class Asn1Enumerated : Asn1Numeric
    {
        public const int Tag = 0x0a;

        public static readonly Asn1Identifier Id = new Asn1Identifier(Asn1IdentifierTag.Universal, false, Tag);

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Enumerated" /> class.
        /// Call this constructor to construct an Asn1Enumerated
        /// object from an integer value.
        /// </summary>
        /// <param name="content">The integer value to be contained in the
        /// this Asn1Enumerated object</param>
        public Asn1Enumerated(int content)
            : base(Id, content)
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
            : base(Id, (long) dec.DecodeNumeric(stream, len))
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
        public const int Tag = 0x10;

        public static readonly Asn1Identifier Id = new Asn1Identifier(Asn1IdentifierTag.Universal, true, Tag);
        
        public Asn1SequenceOf(int size)
            : base(Id, size)
        {
        }
        
        public Asn1SequenceOf(IAsn1Decoder dec, Stream stream, int len)
            : base(Id)
        {
            DecodeStructured(dec, stream, len);
        }
        
        public override string ToString() => ToString("SEQUENCE OF: { ");
    }
}

#endif