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
        
        public object Clone() => MemberwiseClone();
        
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
        
        protected Asn1Object(Asn1Identifier id = null)
        {
            _id = id;
        }
        
        public virtual Asn1Identifier GetIdentifier() => _id;
        
        public virtual void SetIdentifier(Asn1Identifier identifier) => _id = identifier;
        
        public override string ToString()
        {
            var identifier = GetIdentifier();

            return $"{ClassTypes[(int) identifier.Asn1Class]}{identifier.Tag}]";
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
        
        public Asn1OctetString(Stream stream, int len)
            : base(Id)
        {
            _content = len > 0 ? (sbyte[]) LberDecoder.DecodeOctetString(stream, len) : new sbyte[0];
        }
        
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
        
        public Asn1Tagged(Stream stream, int len, Asn1Identifier identifier)
            : base(identifier)
        {
            // If we are decoding an implicit tag, there is no way to know at this
            // low level what the base type really is. We can place the content
            // into an Asn1OctetString type and pass it back to the application who
            // will be able to create the appropriate ASN.1 type for this tag.
            _content = new Asn1OctetString(stream, len);
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
                var newArray = new Asn1Object[_contentIndex + _contentIndex];
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
        
        protected internal void DecodeStructured(Stream stream, int len)
        {
            var componentLen = new int[1]; // collects length of component

            while (len > 0)
            {
                Add(LberDecoder.Decode(stream, componentLen));
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
        
        public Asn1Boolean(bool content)
            : base(Id)
        {
            _content = content;
        }
        
        public Asn1Boolean(Stream stream, int len)
            : base(Id)
        {
            _content = LberDecoder.DecodeBoolean(stream, len);
        }
        
        public bool BooleanValue() => _content;
        
        public override string ToString() => $"{base.ToString()}BOOLEAN: {_content}";
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
        
        public Asn1Null()
            : base(Id)
        {
        }
        
        public override string ToString() => $"{base.ToString()}NULL: \"\"";
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

        public int Length { get; }

        public int EncodedLength { get; }
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

        public Asn1Sequence(int size)
            : base(Id, size)
        {
        }

        public Asn1Sequence(Stream stream, int len)
            : base(Id)
        {
            DecodeStructured(stream, len);
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
        
        public Asn1Set(Stream stream, int len)
            : base(Id)
        {
            DecodeStructured(stream, len);
        }

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
        
        public Asn1Integer(int content)
            : base(Id, content)
        {
        }
        
        public Asn1Integer(Stream stream, int len)
            : base(Id, LberDecoder.DecodeNumeric(stream, len))
        {
        }

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
        
        public Asn1Enumerated(LdapScope content)
            : base(Id, (int) content)
        {
        }

        public Asn1Enumerated(int content)
            : base(Id, content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asn1Enumerated"/> class.
        /// Constructs an Asn1Enumerated object by decoding data from an
        /// input stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        public Asn1Enumerated(Stream stream, int len)
            : base(Id, LberDecoder.DecodeNumeric(stream, len))
        {
        }
        
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
        
        public Asn1SequenceOf(Stream stream, int len)
            : base(Id)
        {
            DecodeStructured(stream, len);
        }
        
        public override string ToString() => ToString("SEQUENCE OF: { ");
    }
}

#endif