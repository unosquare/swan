#if !UWP
namespace Unosquare.Swan.Networking.Ldap
{
    using System.IO;
    
    /// <summary>
    /// This interface defines the methods for encoding each of the ASN.1 types.
    /// Encoders which implement this interface may be used to encode any of the
    /// IAsn1Object data types.
    /// This package also provides the BEREncoder class that can be used to
    /// BER encode ASN.1 classes.  However an application might chose to use
    /// its own encoder class.
    /// This interface thus allows an application to use this package to
    /// encode ASN.1 objects using other encoding rules if needed.
    /// Note that Ldap packets are required to be BER encoded. Since this package
    /// includes a BER encoder no application provided encoder is needed for
    /// building Ldap packets.
    /// </summary>
    internal interface IAsn1Encoder
    {
        /// <summary>
        /// Encode an Asn1Boolean directly into the provided output stream.
        /// </summary>
        /// <param name="b">The Asn1Boolean object to encode</param>
        /// <param name="stream">The stream.</param>
        void Encode(Asn1Boolean b, Stream stream);

        /// <summary>
        /// Encode an Asn1Numeric directly to a stream.
        /// Use a two's complement representation in the fewest number of octets
        /// possible.
        /// Can be used to encode both INTEGER and ENUMERATED values.
        /// </summary>
        /// <param name="n">The Asn1Numeric object to encode</param>
        /// <param name="stream">The stream.</param>
        void Encode(Asn1Numeric n, Stream stream);

        /// <summary>
        /// Encode an Asn1Null directly to a stream.
        /// </summary>
        /// <param name="n">The Asn1Null object to encode</param>
        /// <param name="stream">The stream.</param>
        void Encode(Asn1Null n, Stream stream);

        /// <summary>
        /// Encode an Asn1OctetString directly to a stream.
        /// </summary>
        /// <param name="os">The Asn1OctetString object to encode</param>
        /// <param name="stream">The stream.</param>
        void Encode(Asn1OctetString os, Stream stream);

        /// <summary>
        /// Encode an Asn1Structured directly to a stream.
        /// </summary>
        /// <param name="c">The Asn1Structured object to encode</param>
        /// <param name="stream">The stream.</param>
        void Encode(Asn1Structured c, Stream stream);

        /// <summary>
        /// Encode an Asn1Tagged directly to a stream.
        /// </summary>
        /// <param name="t">The Asn1Tagged object to encode</param>
        /// <param name="stream">The stream.</param>
        void Encode(Asn1Tagged t, Stream stream);

        /// <summary>
        /// Encode an Asn1Identifier directly to a stream.
        /// </summary>
        /// <param name="id">The Asn1Identifier object to encode</param>
        /// <param name="stream">The stream.</param>
        void Encode(Asn1Identifier id, Stream stream);
    }
}
#endif