#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System.IO;

    /// <summary>
    /// This class provides LBER encoding routines for ASN.1 Types. LBER is a
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
    internal static class LberEncoder
    {
        /// <summary>
        /// BER Encode an Asn1Boolean directly into the specified output stream.
        /// </summary>
        /// <param name="b">The Asn1Boolean object to encode</param>
        /// <param name="stream">The stream.</param>
        public static void Encode(Asn1Boolean b, Stream stream)
        {
            Encode(b.GetIdentifier(), stream);
            stream.WriteByte(0x01);
            stream.WriteByte((byte) (b.BooleanValue() ? 0xff : 0x00));
        }

        /// <summary>
        /// Encode an Asn1Numeric directly into the specified outputstream.
        /// Use a two's complement representation in the fewest number of octets
        /// possible.
        /// Can be used to encode INTEGER and ENUMERATED values.
        /// </summary>
        /// <param name="n">The Asn1Numeric object to encode</param>
        /// <param name="stream">The stram</param>
        public static void Encode(Asn1Numeric n, Stream stream)
        {
            var octets = new sbyte[8];
            sbyte len;
            var longValue = n.LongValue();
            long endValue = longValue < 0 ? -1 : 0;
            var endSign = endValue & 0x80;

            for (len = 0; len == 0 || longValue != endValue || (octets[len - 1] & 0x80) != endSign; len++)
            {
                octets[len] = (sbyte)(longValue & 0xFF);
                longValue >>= 8;
            }

            Encode(n.GetIdentifier(), stream);
            stream.WriteByte((byte)len);

            for (var i = len - 1; i >= 0; i--)
            {
                stream.WriteByte((byte) octets[i]);
            }
        }

        /// <summary>
        /// Encode an Asn1OctetString directly into the specified outputstream.
        /// </summary>
        /// <param name="os">The Asn1OctetString object to encode</param>
        /// <param name="stream">The stream.</param>
        public static void Encode(Asn1OctetString os, Stream stream)
        {
            Encode(os.GetIdentifier(), stream);
            EncodeLength(os.ByteValue().Length, stream);
            var tempSbyteArray = os.ByteValue();
            stream.Write(tempSbyteArray.ToByteArray(), 0, tempSbyteArray.Length);
        }

        public static void Encode(Asn1Object obj, Stream stream)
        {
            switch (obj)
            {
                case Asn1Boolean b:
                    Encode(b, stream);
                    break;
                case Asn1Numeric n:
                    Encode(n, stream);
                    break;
                case Asn1Null n:
                    Encode(n.GetIdentifier(), stream);
                    stream.WriteByte(0x00); // Length (with no Content)
                    break;
                case Asn1OctetString n:
                    Encode(n, stream);
                    break;
                case Asn1Structured n:
                    Encode(n, stream);
                    break;
                case Asn1Tagged n:
                    Encode(n, stream);
                    break;
                case Asn1Choice n:
                    Encode(n.ChoiceValue, stream);
                    break;
                default:
                    throw new InvalidDataException();
            }
        }

        /// <summary>
        /// Encode an Asn1Structured into the specified outputstream.  This method
        /// can be used to encode SET, SET_OF, SEQUENCE, SEQUENCE_OF
        /// </summary>
        /// <param name="c">The Asn1Structured object to encode</param>
        /// <param name="stream">The stream.</param>
        public static void Encode(Asn1Structured c, Stream stream)
        {
            Encode(c.GetIdentifier(), stream);

            var arrayValue = c.ToArray();

            using (var output = new MemoryStream())
            {
                foreach (var obj in arrayValue)
                {
                    Encode(obj, output);
                }

                EncodeLength((int) output.Length, stream);

                var tempSbyteArray = output.ToArray();
                stream.Write(tempSbyteArray, 0, tempSbyteArray.Length);
            }
        }

        /// <summary>
        /// Encode an Asn1Tagged directly into the specified outputstream.
        /// </summary>
        /// <param name="t">The Asn1Tagged object to encode</param>
        /// <param name="stream">The stream.</param>
        public static void Encode(Asn1Tagged t, Stream stream)
        {
            if (!t.Explicit)
            {
                Encode(t.TaggedValue, stream);
                return;
            }

            Encode(t.GetIdentifier(), stream);

            // determine the encoded length of the base type.
            using (var encodedContent = new MemoryStream())
            {
                Encode(t.TaggedValue, encodedContent);

                EncodeLength((int) encodedContent.Length, stream);
                var tempSbyteArray = encodedContent.ToArray().ToSByteArray();
                stream.Write(tempSbyteArray.ToByteArray(), 0, tempSbyteArray.Length);
            }
        }

        /// <summary>
        /// Encode an Asn1Identifier directly into the specified outputstream.
        /// </summary>
        /// <param name="id">The Asn1Identifier object to encode</param>
        /// <param name="stream">The stream.</param>
        public static void Encode(Asn1Identifier id, Stream stream)
        {
            var c = (int) id.Asn1Class;
            var t = id.Tag;
            var ccf = (sbyte)((c << 6) | (id.Constructed ? 0x20 : 0));

            if (t < 30)
            {
                stream.WriteByte((byte)(ccf | t));
            }
            else
            {
                stream.WriteByte((byte)(ccf | 0x1F));
                EncodeTagInteger(t, stream);
            }
        }

        /// <summary>
        /// Encodes the length.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="stream">The stream.</param>
        private static void EncodeLength(int length, Stream stream)
        {
            if (length < 0x80)
            {
                stream.WriteByte((byte)length);
            }
            else
            {
                var octets = new sbyte[4]; // 4 bytes sufficient for 32 bit int.
                sbyte n;
                for (n = 0; length != 0; n++)
                {
                    octets[n] = (sbyte)(length & 0xFF);
                    length >>= 8;
                }

                stream.WriteByte((byte)(0x80 | n));

                for (var i = n - 1; i >= 0; i--)
                    stream.WriteByte((byte)octets[i]);
            }
        }

        /// <summary>
        /// Encodes the provided tag into the outputstream.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="stream">The stream.</param>
        private static void EncodeTagInteger(int val, Stream stream)
        {
            var octets = new sbyte[5];
            int n;

            for (n = 0; val != 0; n++)
            {
                octets[n] = (sbyte)(val & 0x7F);
                val = val >> 7;
            }

            for (var i = n - 1; i > 0; i--)
            {
                stream.WriteByte((byte)(octets[i] | 0x80));
            }

            stream.WriteByte((byte)octets[0]);
        }
    }
}
#endif
