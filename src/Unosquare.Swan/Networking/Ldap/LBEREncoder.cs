#if !UWP
using System.IO;

namespace Unosquare.Swan.Networking.Ldap
{
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
    /// <seealso cref="Unosquare.Swan.Networking.Ldap.Asn1Encoder" />
    internal class LBEREncoder : Asn1Encoder
    {
        /// <summary>
        /// This method returns the literal value received
        /// </summary>
        /// <param name="literal">The literal.</param>
        /// <returns></returns>
        public static long Identity(long literal)
        {
            return literal;
        }
        
        /// <summary> BER Encode an Asn1Boolean directly into the specified output stream.</summary>
        public virtual void encode(Asn1Boolean b, Stream stream)
        {
            /* Encode the id */
            encode(b.GetIdentifier(), stream);

            /* Encode the length */
            stream.WriteByte(0x01);

            /* Encode the boolean content*/
            stream.WriteByte((byte)(b.BooleanValue() ? (sbyte)Identity(0xff) : (sbyte)0x00));
        }

        /// <summary>
        /// Encode an Asn1Numeric directly into the specified outputstream.
        /// Use a two's complement representation in the fewest number of octets
        /// possible.
        /// Can be used to encode INTEGER and ENUMERATED values.
        /// </summary>
        /// <param name="n">The Asn1Numeric object to encode</param>
        /// <param name="stream">The stram</param>
        public void encode(Asn1Numeric n, Stream stream)
        {
            var octets = new sbyte[8];
            sbyte len;
            var value_Renamed = n.LongValue();
            long endValue = value_Renamed < 0 ? -1 : 0;
            var endSign = endValue & 0x80;

            for (len = 0; len == 0 || value_Renamed != endValue || (octets[len - 1] & 0x80) != endSign; len++)
            {
                octets[len] = (sbyte)(value_Renamed & 0xFF);
                value_Renamed >>= 8;
            }

            encode(n.GetIdentifier(), stream);
            stream.WriteByte((byte)len); // Length
            for (var i = len - 1; i >= 0; i--)
                // Content
                stream.WriteByte((byte)octets[i]);
        }

        /// <summary>
        /// Encode an Asn1Null directly into the specified outputstream.
        /// </summary>
        /// <param name="n">The Asn1Null object to encode</param>
        /// <param name="stream">The stream.</param>
        public void encode(Asn1Null n, Stream stream)
        {
            encode(n.GetIdentifier(), stream);
            stream.WriteByte(0x00); // Length (with no Content)
        }

        /// <summary>
        /// Encode an Asn1OctetString directly into the specified outputstream.
        /// </summary>
        /// <param name="os">The Asn1OctetString object to encode</param>
        /// <param name="stream">The stream.</param>
        public void encode(Asn1OctetString os, Stream stream)
        {
            encode(os.GetIdentifier(), stream);
            encodeLength(os.ByteValue().Length, stream);
            var temp_sbyteArray = os.ByteValue();
            stream.Write(temp_sbyteArray.ToByteArray(), 0, temp_sbyteArray.Length);
        }

        /// <summary>
        /// Encode an Asn1Structured into the specified outputstream.  This method
        /// can be used to encode SET, SET_OF, SEQUENCE, SEQUENCE_OF
        /// </summary>
        /// <param name="c">The Asn1Structured object to encode</param>
        /// <param name="stream">The stream.</param>
        public void encode(Asn1Structured c, Stream stream)
        {
            encode(c.GetIdentifier(), stream);

            var value_Renamed = c.ToArray();

            var output = new MemoryStream();

            /* Cycle through each element encoding each element */
            for (var i = 0; i < value_Renamed.Length; i++)
            {
                value_Renamed[i].Encode(this, output);
            }

            /* Encode the length */
            encodeLength((int)output.Length, stream);

            /* Add each encoded element into the output stream */
            var temp_sbyteArray = output.ToArray().ToSByteArray();
            stream.Write(temp_sbyteArray.ToByteArray(), 0, temp_sbyteArray.Length);
        }

        /// <summary>
        /// Encode an Asn1Tagged directly into the specified outputstream.
        /// </summary>
        /// <param name="t">The Asn1Tagged object to encode</param>
        /// <param name="stream">The stream.</param>
        public void encode(Asn1Tagged t, Stream stream)
        {
            if (t.Explicit)
            {
                encode(t.GetIdentifier(), stream);

                /* determine the encoded length of the base type. */
                var encodedContent = new MemoryStream();
                t.taggedValue().Encode(this, encodedContent);

                encodeLength((int)encodedContent.Length, stream);
                var temp_sbyteArray = encodedContent.ToArray().ToSByteArray();
                stream.Write(temp_sbyteArray.ToByteArray(), 0, temp_sbyteArray.Length);
            }
            else
            {
                t.taggedValue().Encode(this, stream);
            }
        }

        /// <summary>
        /// Encode an Asn1Identifier directly into the specified outputstream.
        /// </summary>
        /// <param name="id">The Asn1Identifier object to encode</param>
        /// <param name="stream">The stream.</param>
        public void encode(Asn1Identifier id, Stream stream)
        {
            var c = id.Asn1Class;
            var t = id.Tag;
            var ccf = (sbyte)((c << 6) | (id.Constructed ? 0x20 : 0));

            if (t < 30)
            {
                /* single octet */
                stream.WriteByte((byte)(ccf | t));
            }
            else
            {
                /* multiple octet */
                stream.WriteByte((byte)(ccf | 0x1F));
                encodeTagInteger(t, stream);
            }
        }

        /// <summary>
        /// Encodes the length.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="stream">The stream.</param>
        private void encodeLength(int length, Stream stream)
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
        /// <param name="value_Renamed">The value renamed.</param>
        /// <param name="stream">The stream.</param>
        private void encodeTagInteger(int value_Renamed, Stream stream)
        {
            var octets = new sbyte[5];
            int n;
            for (n = 0; value_Renamed != 0; n++)
            {
                octets[n] = (sbyte)(value_Renamed & 0x7F);
                value_Renamed = value_Renamed >> 7;
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