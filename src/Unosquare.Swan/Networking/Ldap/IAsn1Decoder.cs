#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System.IO;

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
    internal interface IAsn1Decoder
    {
        /// <summary>
        /// Decode an encoded value into an Asn1Object from an InputStream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="length">The decoded components encoded length. This value is
        /// handy when decoding structured types. It allows you to accumulate
        /// the number of bytes decoded, so you know when the structured
        /// type has decoded all of its components.</param>
        /// <returns>Decoded object</returns>
        Asn1Object Decode(Stream stream, int[] length);

        /// <summary>
        /// Decode a BOOLEAN directly from a stream. Call this method when you
        /// know that the next ASN.1 encoded element is a BOOLEAN
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="len">Length in bytes</param>
        /// <returns>Decoded Boolean</returns>
        object DecodeBoolean(Stream stream, int len);

        /// <summary>
        /// Decode a Numeric value directly from a stream.  Call this method when you
        /// know that the next ASN.1 encoded element is a Numeric
        /// Can be used to decodes INTEGER and ENUMERATED types.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="len">Length in bytes</param>
        /// <returns>Decoded Numeric</returns>
        object DecodeNumeric(Stream stream, int len);

        /// <summary>
        /// Decode an OCTET_STRING directly from a stream. Call this method when you
        /// know that the next ASN.1 encoded element is a OCTET_STRING.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="len">Length in bytes</param>
        /// <returns>Decoded string</returns>
        object DecodeOctetString(Stream stream, int len);
    }
}
#endif