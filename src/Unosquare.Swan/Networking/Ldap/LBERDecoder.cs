﻿#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System.IO;
    using System.Text;

    /// <summary>
    /// This class provides LBER decoding routines for ASN.1 Types. LBER is a
    /// subset of BER as described in the following taken from 5.1 of RFC 2251:
    /// 5.1. Mapping Onto BER-based Transport Services
    /// The protocol elements of Ldap are encoded for exchange using the
    /// Basic Encoding Rules (BER) [11] of ASN.1 [3]. However, due to the
    /// high overhead involved in using certain elements of the BER, the
    /// following additional restrictions are placed on BER-encodings of Ldap
    /// protocol elements:
    /// <li>(1) Only the definite form of length encoding will be used.</li><li>(2) OCTET STRING values will be encoded in the primitive form only.</li><li>
    /// (3) If the value of a BOOLEAN type is true, the encoding MUST have
    /// its contents octets set to hex "FF".
    /// </li><li>
    /// (4) If a value of a type is its default value, it MUST be absent.
    /// Only some BOOLEAN and INTEGER types have default values in this
    /// protocol definition.
    /// These restrictions do not apply to ASN.1 types encapsulated inside of
    /// OCTET STRING values, such as attribute values, unless otherwise
    /// noted.
    /// </li>
    /// [3] ITU-T Rec. X.680, "Abstract Syntax Notation One (ASN.1) -
    /// Specification of Basic Notation", 1994.
    /// [11] ITU-T Rec. X.690, "Specification of ASN.1 encoding rules: Basic,
    /// Canonical, and Distinguished Encoding Rules", 1994.
    /// </summary>
    /// <seealso cref="IAsn1Decoder" />
    internal class LBERDecoder : IAsn1Decoder
    {
        public LBERDecoder()
        {
            asn1ID = new Asn1Identifier();
            asn1Len = new Asn1Length();
        }

        // used to speed up decode, so it doesn't need to recreate an identifier every time
        // instead just reset is called CANNOT be static for multiple connections
        private readonly Asn1Identifier asn1ID;
        private readonly Asn1Length asn1Len;

        /// <summary>
        /// Decode an LBER encoded value into an Asn1Type from a byte array.
        /// </summary>
        /// <param name="value_Renamed">The value renamed.</param>
        /// <returns>Decoded Asn1Object</returns>
        public virtual Asn1Object Decode(sbyte[] value_Renamed)
        {
            Asn1Object asn1 = null;

            var stream = new MemoryStream(value_Renamed.ToByteArray());
            try
            {
                asn1 = Decode(stream);
            }
            catch (IOException)
            {
                // Ignore
            }
            return asn1;
        }

        /// <summary>
        /// Decode an LBER encoded value into an Asn1Type from an InputStream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>
        /// Decoded Asn1Object
        /// </returns>
        public virtual Asn1Object Decode(Stream stream)
        {
            var len = new int[1];
            return Decode(stream, len);
        }

        /// <summary>
        /// Decode an LBER encoded value into an Asn1Object from an InputStream.
        /// This method also returns the total length of this encoded
        /// Asn1Object (length of type + length of length + length of content)
        /// in the parameter len. This information is helpful when decoding
        /// structured types.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="len">The length.</param>
        /// <returns>
        /// Decoded Asn1Obect
        /// </returns>
        /// <exception cref="EndOfStreamException">Unknown tag</exception>
        public virtual Asn1Object Decode(Stream stream, int[] len)
        {
            asn1ID.Reset(stream);
            asn1Len.Reset(stream);

            var length = asn1Len.Length;
            len[0] = asn1ID.EncodedLength + asn1Len.EncodedLength + length;

            if (asn1ID.Universal == false)
                return new Asn1Tagged(this, stream, length, (Asn1Identifier) asn1ID.Clone());

            switch (asn1ID.Tag)
            {
                case Asn1Sequence.TAG:
                    return new Asn1Sequence(this, stream, length);

                case Asn1Set.TAG:
                    return new Asn1Set(this, stream, length);

                case Asn1Boolean.TAG:
                    return new Asn1Boolean(this, stream, length);

                case Asn1Integer.TAG:
                    return new Asn1Integer(this, stream, length);

                case Asn1OctetString.TAG:
                    return new Asn1OctetString(this, stream, length);

                case Asn1Enumerated.TAG:
                    return new Asn1Enumerated(this, stream, length);

                case Asn1Null.TAG:
                    return new Asn1Null(); // has no content to decode.

                default:
                    throw new EndOfStreamException("Unknown tag"); // !!! need a better exception
            }
        }

        /// <summary>
        /// Decode a boolean directly from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="len">Length in bytes</param>
        /// <returns>
        /// Decoded boolean object
        /// </returns>
        /// <exception cref="EndOfStreamException">LBER: BOOLEAN: decode error: EOF</exception>
        public object DecodeBoolean(Stream stream, int len)
        {
            var lber = new sbyte[len];

            var i = stream.ReadInput(ref lber, 0, lber.Length);

            if (i != len)
                throw new EndOfStreamException("LBER: BOOLEAN: decode error: EOF");

            return lber[0] != 0x00;
        }

        /// <summary>
        /// Decode a Numeric type directly from a stream. Decodes INTEGER
        /// and ENUMERATED types.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="len">Length in bytes</param>
        /// <returns>
        /// Decoded numeric object
        /// </returns>
        /// <exception cref="EndOfStreamException">
        /// LBER: NUMERIC: decode error: EOF
        /// or
        /// LBER: NUMERIC: decode error: EOF
        /// </exception>
        public object DecodeNumeric(Stream stream, int len)
        {
            long l = 0;
            var r = stream.ReadByte();

            if (r < 0)
                throw new EndOfStreamException("LBER: NUMERIC: decode error: EOF");

            if ((r & 0x80) != 0)
            {
                // check for negative number
                l = -1;
            }

            l = (l << 8) | r;

            for (var i = 1; i < len; i++)
            {
                r = stream.ReadByte();
                if (r < 0)
                    throw new EndOfStreamException("LBER: NUMERIC: decode error: EOF");
                l = (l << 8) | r;
            }

            return l;
        }

        /// <summary>
        /// Decode an OctetString directly from a stream.
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <param name="len">Length in bytes</param>
        /// <returns>Decoded octet </returns>
        public object DecodeOctetString(Stream stream, int len)
        {
            var octets = new sbyte[len];
            var totalLen = 0;

            while (totalLen < len)
            {
                // Make sure we have read all the data
                var inLen = stream.ReadInput(ref octets, totalLen, len - totalLen);
                totalLen += inLen;
            }

            return octets;
        }

        /// <summary>
        /// Decode a CharacterString directly from a stream.
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <param name="len">Length in bytes</param>
        /// <returns>Decoded character string</returns>
        /// <exception cref="EndOfStreamException">LBER: CHARACTER STRING: decode error: EOF</exception>
        public object DecodeCharacterString(Stream stream, int len)
        {
            var octets = new sbyte[len];

            for (var i = 0; i < len; i++)
            {
                var ret = stream.ReadByte(); // blocks
                if (ret == -1)
                    throw new EndOfStreamException("LBER: CHARACTER STRING: decode error: EOF");
                octets[i] = (sbyte)ret;
            }

            var encoder = Encoding.UTF8;
            var dchar = encoder.GetChars(octets.ToByteArray());
            var rval = new string(dchar);

            return rval;
        }
    }
}

#endif