#if !UWP

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Unosquare.Swan.Networking.Ldap
{
    /// <summary>
    ///     Represents the Ldap Unbind request.
    ///     <pre>
    ///         UnbindRequest ::= [APPLICATION 2] NULL
    ///     </pre>
    /// </summary>
    public class RfcUnbindRequest : Asn1Null, RfcRequest
    {
        /// <summary>
        ///     Override getIdentifier to return an application-wide id.
        ///     <pre>
        ///         ID = CLASS: APPLICATION, FORM: PRIMITIVE, TAG: 2. (0x42)
        ///     </pre>
        /// </summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, false, LdapMessage.UNBIND_REQUEST);
        }
        public RfcRequest dupRequest(string @base, string filter, bool request)
        {
            throw new LdapException(ExceptionMessages.NO_DUP_REQUEST, new object[] { "unbind" },
                LdapException.Ldap_NOT_SUPPORTED, null);
        }
        public string getRequestDN()
        {
            return null;
        }
    }
    /// <summary>
    ///     An implementation of LdapAuthHandler must be able to provide an
    ///     LdapAuthProvider object at the time of a referral.  The class
    ///     encapsulates information that is used by the client for authentication
    ///     when following referrals automatically.
    /// </summary>
    /// <seealso cref="LdapAuthHandler">
    /// </seealso>
    /// <seealso cref="LdapBindHandler">
    /// </seealso>
    public class LdapAuthProvider
    {
        /// <summary>
        ///     Returns the distinguished name to be used for authentication on
        ///     automatic referral following.
        /// </summary>
        /// <returns>
        ///     The distinguished name from the object.
        /// </returns>
        public virtual string DN
        {
            get { return dn; }
        }
        /// <summary>
        ///     Returns the password to be used for authentication on automatic
        ///     referral following.
        /// </summary>
        /// <returns>
        ///     The byte[] value (UTF-8) of the password from the object.
        /// </returns>
        public virtual sbyte[] Password
        {
            get { return password; }
        }
        private readonly string dn;
        private readonly sbyte[] password;
        /// <summary>
        ///     Constructs information that is used by the client for authentication
        ///     when following referrals automatically.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name to use when authenticating to
        ///     a server.
        /// </param>
        /// <param name="password">
        ///     The password to use when authenticating to a server.
        /// </param>
        public LdapAuthProvider(string dn, sbyte[] password)
        {
            this.dn = dn;
            this.password = password;
        }
    }
    /// <summary>
    ///     Used to provide credentials for authentication when processing a
    ///     referral.
    ///     A programmer desiring to supply authentication credentials
    ///     to the API when automatically following referrals MUST
    ///     implement this interface. If LdapAuthHandler or LdapBindHandler are not
    ///     implemented, automatically followed referrals will use anonymous
    ///     authentication. Referral URLs of any type other than Ldap (i.e. a
    ///     referral URL other than ldap://something) are not chased automatically
    ///     by the API on automatic following.
    /// </summary>
    /// <seealso cref="LdapBindHandler">
    /// </seealso>
    /// <seealso cref="LdapConstraints.ReferralFollowing">
    /// </seealso>
    public interface LdapAuthHandler : LdapReferralHandler
    {
        /// <summary>
        ///     Returns an object which can provide credentials for authenticating to
        ///     a server at the specified host and port.
        /// </summary>
        /// <param name="host">
        ///     Contains a host name or the IP address (in dotted string
        ///     format) of a host running an Ldap server.
        /// </param>
        /// <param name="port">
        ///     Contains the TCP or UDP port number of the host.
        /// </param>
        /// <returns>
        ///     An object with authentication credentials to the specified
        ///     host and port.
        /// </returns>
        LdapAuthProvider getAuthProvider(string host, int port);
    }
    /// <summary>
    ///     The Base64 utility class performs base64 encoding and decoding.
    ///     The Base64 Content-Transfer-Encoding is designed to represent
    ///     arbitrary sequences of octets in a form that need not be humanly
    ///     readable.  The encoding and decoding algorithms are simple, but the
    ///     encoded data are consistently only about 33 percent larger than the
    ///     unencoded data.  The base64 encoding algorithm is defined by
    ///     RFC 2045.
    /// </summary>
    public class Base64
    {
        /// <summary>
        ///     Conversion table for encoding to base64.
        ///     emap is a six-bit value to base64 (8-bit) converstion table.
        ///     For example, the value of the 6-bit value 15
        ///     is mapped to 0x50 which is the ASCII letter 'P', i.e. the letter P
        ///     is the base64 encoded character that represents the 6-bit value 15.
        /// </summary>
        /*
        * 8-bit base64 encoded character                 base64       6-bit
        *                                                encoded      original
        *                                                character    binary value
        */
        private static readonly char[] emap =
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
            'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k',
            'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6',
            '7', '8', '9', '+', '/'
        }; // 4-9, + /;  56-63
        /// <summary>
        ///     conversion table for decoding from base64.
        ///     dmap is a base64 (8-bit) to six-bit value converstion table.
        ///     For example the ASCII character 'P' has a value of 80.
        ///     The value in the 80th position of the table is 0x0f or 15.
        ///     15 is the original 6-bit value that the letter 'P' represents.
        /// </summary>
        /*
        * 6-bit decoded value                            base64    base64
        *                                                encoded   character
        *                                                value
        *
        * Note: about half of the values in the table are only place holders
        */
        private static readonly sbyte[] dmap =
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3e, 0x00, 0x00, 0x00, 0x3f,
            0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12,
            0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e,
            0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30,
            0x31, 0x32, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00
        }; // 120-127 'xyz     '
        /// <summary>
        ///     Default constructor, don't allow instances of the
        ///     utility class to be created.
        /// </summary>
        private Base64()
        {
        }
        /// <summary>
        ///     Encodes the specified String into a base64 encoded String object.
        /// </summary>
        /// <param name="inputString">
        ///     The String object to be encoded.
        /// </param>
        /// <returns>
        ///     a String containing the encoded value of the input.
        /// </returns>
        public static string encode(string inputString)
        {
            try
            {
                var encoder = Encoding.GetEncoding("utf-8");
                var ibytes = encoder.GetBytes(inputString);
                var sbytes = ibytes.ToSByteArray();
                return encode(sbytes);
            }
            catch (IOException ue)
            {
                throw new Exception("US-ASCII String encoding not supported by JVM", ue);
            }
        }
        /// <summary>
        ///     Encodes the specified bytes into a base64 array of bytes.
        ///     Each byte in the return array represents a base64 character.
        /// </summary>
        /// <param name="inputBytes">
        ///     the byte array to be encoded.
        /// </param>
        /// <returns>
        ///     a String containing the base64 encoded data
        /// </returns>
        public static string encode(sbyte[] inputBytes)
        {
            int i, j, k;
            int t, t1, t2;
            int ntb; // number of three-bytes in inputBytes
            bool onePadding = false, twoPaddings = false;
            char[] encodedChars; // base64 encoded chars
            var len = inputBytes.Length;
            if (len == 0)
            {
                // No data, return no data.
                return new StringBuilder("").ToString();
            }
            // every three bytes will be encoded into four bytes
            if (len % 3 == 0)
            {
                ntb = len / 3;
            }
            // the last one or two bytes will be encoded into
            // four bytes with one or two paddings
            else
            {
                ntb = len / 3 + 1;
            }
            // need two paddings
            if (len % 3 == 1)
            {
                twoPaddings = true;
            }
            // need one padding
            else if (len % 3 == 2)
            {
                onePadding = true;
            }
            encodedChars = new char[ntb * 4];
            // map of decoded and encoded bits
            //     bits in 3 decoded bytes:   765432  107654  321076  543210
            //     bits in 4 encoded bytes: 76543210765432107654321076543210
            //       plain           "AAA":   010000  010100  000101  000001
            //       base64 encoded "QUFB": 00010000000101000000010100000001
            // one padding:
            //     bits in 2 decoded bytes:   765432  10 7654  3210
            //     bits in 4 encoded bytes: 765432107654 321076543210 '='
            //       plain            "AA":   010000  010100  0001
            //       base64 encoded "QUE=": 00010000000101000000010000111101
            // two paddings:
            //     bits in 1 decoded bytes:   765432  10
            //     bits in 4 encoded bytes: 7654321076543210 '=' '='
            //       plain             "A":   010000  01
            //       base64 encoded "QQ==": 00010000000100000011110100111101
            //
            // note: the encoded bits which have no corresponding decoded bits
            // are filled with zeros; '=' = 00111101.
            for (i = 0, j = 0, k = 1; i < len; i += 3, j += 4, k++)
            {
                // build encodedChars[j]
                t = 0x00ff & inputBytes[i];
                encodedChars[j] = emap[t >> 2];
                // build encodedChars[j+1]
                if (k == ntb && twoPaddings)
                {
                    encodedChars[j + 1] = emap[(t & 0x03) << 4];
                    encodedChars[j + 2] = '=';
                    encodedChars[j + 3] = '=';
                    break;
                }
                t1 = 0x00ff & inputBytes[i + 1];
                encodedChars[j + 1] = emap[((t & 0x03) << 4) + ((t1 & 0xf0) >> 4)];
                // build encodedChars[j+2]
                if (k == ntb && onePadding)
                {
                    encodedChars[j + 2] = emap[(t1 & 0x0f) << 2];
                    encodedChars[j + 3] = '=';
                    break;
                }
                t2 = 0x00ff & inputBytes[i + 2];
                encodedChars[j + 2] = emap[((t1 & 0x0f) << 2) | ((t2 & 0xc0) >> 6)];
                // build encodedChars[j+3]
                encodedChars[j + 3] = emap[t2 & 0x3f];
            }
            return new string(encodedChars);
        }
        public static void GetCharsFromString(string sourceString, int sourceStart, int sourceEnd,
        ref char[] destinationArray, int destinationStart)
        {
            int sourceCounter;
            int destinationCounter;
            sourceCounter = sourceStart;
            destinationCounter = destinationStart;
            while (sourceCounter < sourceEnd)
            {
                destinationArray[destinationCounter] = sourceString[sourceCounter];
                sourceCounter++;
                destinationCounter++;
            }
        }
        /// <summary>
        ///     Decodes the input base64 encoded String.
        ///     The resulting binary data is returned as an array of bytes.
        /// </summary>
        /// <param name="encodedString">
        ///     The base64 encoded String object.
        /// </param>
        /// <returns>
        ///     The decoded byte array.
        /// </returns>
        public static sbyte[] decode(string encodedString)
        {
            var c = new char[encodedString.Length];
            GetCharsFromString(encodedString, 0, encodedString.Length, ref c, 0);
            return decode(c);
        }
        /// <summary>
        ///     Decodes the input base64 encoded array of characters.
        ///     The resulting binary data is returned as an array of bytes.
        /// </summary>
        /// <param name="encodedChars">
        ///     The character array containing the base64 encoded data.
        /// </param>
        /// <returns>
        ///     A byte array object containing decoded bytes.
        /// </returns>
        public static sbyte[] decode(char[] encodedChars)
        {
            int i, j, k;
            var ecLen = encodedChars.Length; // length of encodedChars
            var gn = ecLen / 4; // number of four-byte groups in encodedChars
            int dByteLen; // length of decoded bytes, default is '0'
            bool onePad = false, twoPads = false;
            sbyte[] decodedBytes; // decoded bytes
            if (encodedChars.Length == 0)
            {
                return new sbyte[0];
            }
            // the number of encoded bytes should be multiple of 4
            if (ecLen % 4 != 0)
            {
                throw new Exception("Novell.Directory.Ldap.ldif_dsml." +
                                    "Base64Decoder: decode: mal-formatted encode value");
            }
            // every four-bytes in encodedString, except the last one if it in the
            // form of '**==' or '***=' ( can't be '*===' or '===='), will be
            // decoded into three bytes.
            if (encodedChars[ecLen - 1] == '=' && encodedChars[ecLen - 2] == '=')
            {
                // the last four bytes of encodedChars is in the form of '**=='
                twoPads = true;
                // the first two bytes of the last four-bytes of encodedChars will
                // be decoded into one byte.
                dByteLen = gn * 3 - 2;
                decodedBytes = new sbyte[dByteLen];
            }
            else if (encodedChars[ecLen - 1] == '=')
            {
                // the last four bytes of encodedChars is in the form of '***='
                onePad = true;
                // the first two bytes of the last four-bytes of encodedChars will
                // be decoded into two bytes.
                dByteLen = gn * 3 - 1;
                decodedBytes = new sbyte[dByteLen];
            }
            else
            {
                // the last four bytes of encodedChars is in the form of '****',
                // e.g. no pad.
                dByteLen = gn * 3;
                decodedBytes = new sbyte[dByteLen];
            }
            // map of encoded and decoded bits
            // no padding:
            //     bits in 4 encoded bytes: 76543210 76543210 76543210 76543210
            //     bits in 3 decoded bytes:   765432   107654   321076   543210
            //        base64  string "QUFB":00010000 00010100 000001010 0000001
            //          plain string  "AAA":   010000  010100  000101  000001
            // one padding:
            //     bits in 4 encoded bytes: 76543210 76543210 76543210 76543210
            //     bits in 2 decoded bytes:   765432   107654   3210
            //       base64  string "QUE=": 00010000 000101000 0000100 00111101
            //         plain string   "AA":   010000  010100  0001
            // two paddings:
            //     bits in 4 encoded bytes: 76543210 76543210 76543210 76543210
            //     bits in 1 decoded bytes:   765432   10
            //       base64  string "QQ==": 00010000 00010000 00111101 00111101
            //         plain string    "A":   010000  01
            for (i = 0, j = 0, k = 1; i < ecLen; i += 4, j += 3, k++)
            {
                // build decodedBytes[j].
                decodedBytes[j] = (sbyte)((dmap[encodedChars[i]] << 2) | ((dmap[encodedChars[i + 1]] & 0x30) >> 4));
                // build decodedBytes[j+1]
                if (k == gn && twoPads)
                {
                    break;
                }
                decodedBytes[j + 1] =
                    (sbyte)(((dmap[encodedChars[i + 1]] & 0x0f) << 4) | ((dmap[encodedChars[i + 2]] & 0x3c) >> 2));
                // build decodedBytes[j+2]
                if (k == gn && onePad)
                {
                    break;
                }
                decodedBytes[j + 2] =
                    (sbyte)(((dmap[encodedChars[i + 2]] & 0x03) << 6) | (dmap[encodedChars[i + 3]] & 0x3f));
            }
            return decodedBytes;
        }
        /// <summary>
        ///     Decodes a base64 encoded StringBuffer.
        ///     Decodes all or part of the input base64 encoded StringBuffer, each
        ///     Character value representing a base64 character. The resulting
        ///     binary data is returned as an array of bytes.
        /// </summary>
        /// <param name="encodedSBuf">
        ///     The StringBuffer object that contains base64
        ///     encoded data.
        /// </param>
        /// <param name="start">
        ///     The start index of the base64 encoded data.
        /// </param>
        /// <param name="end">
        ///     The end index + 1 of the base64 encoded data.
        /// </param>
        /// <returns>
        ///     The decoded byte array
        /// </returns>
        public static sbyte[] decode(StringBuilder encodedSBuf, int start, int end)
        {
            int i, j, k;
            var esbLen = end - start; // length of the encoded part
            var gn = esbLen / 4; // number of four-bytes group in ebs
            int dByteLen; // length of dbs, default is '0'
            bool onePad = false, twoPads = false;
            sbyte[] decodedBytes; // decoded bytes
            if (encodedSBuf.Length == 0)
            {
                return new sbyte[0];
            }
            // the number of encoded bytes should be multiple of number 4
            if (esbLen % 4 != 0)
            {
                throw new Exception("Novell.Directory.Ldap.ldif_dsml." +
                                    "Base64Decoder: decode error: mal-formatted encode value");
            }
            // every four-bytes in ebs, except the last one if it in the form of
            // '**==' or '***=' ( can't be '*===' or '===='), will be decoded into
            // three bytes.
            if (encodedSBuf[end - 1] == '=' && encodedSBuf[end - 2] == '=')
            {
                // the last four bytes of ebs is in the form of '**=='
                twoPads = true;
                // the first two bytes of the last four-bytes of ebs will be
                // decoded into one byte.
                dByteLen = gn * 3 - 2;
                decodedBytes = new sbyte[dByteLen];
            }
            else if (encodedSBuf[end - 1] == '=')
            {
                // the last four bytes of ebs is in the form of '***='
                onePad = true;
                // the first two bytes of the last four-bytes of ebs will be
                // decoded into two bytes.
                dByteLen = gn * 3 - 1;
                decodedBytes = new sbyte[dByteLen];
            }
            else
            {
                // the last four bytes of ebs is in the form of '****', eg. no pad.
                dByteLen = gn * 3;
                decodedBytes = new sbyte[dByteLen];
            }
            // map of encoded and decoded bits
            // no padding:
            //     bits in 4 encoded bytes: 76543210 76543210 76543210 76543210
            //     bits in 3 decoded bytes:   765432   107654   321076   543210
            //        base64  string "QUFB":00010000 00010100 000001010 0000001
            //          plain string  "AAA":   010000  010100  000101  000001
            // one padding:
            //     bits in 4 encoded bytes: 76543210 76543210 76543210 76543210
            //     bits in 2 decoded bytes:   765432   107654   3210
            //       base64  string "QUE=": 00010000 000101000 0000100 00111101
            //         plain string   "AA":   010000  010100  0001
            // two paddings:
            //     bits in 4 encoded bytes: 76543210 76543210 76543210 76543210
            //     bits in 1 decoded bytes:   765432   10
            //       base64  string "QQ==": 00010000 00010000 00111101 00111101
            //         plain string    "A":   010000  01
            for (i = 0, j = 0, k = 1; i < esbLen; i += 4, j += 3, k++)
            {
                // build decodedBytes[j].
                decodedBytes[j] =
                    (sbyte)((dmap[encodedSBuf[start + i]] << 2) | ((dmap[encodedSBuf[start + i + 1]] & 0x30) >> 4));
                // build decodedBytes[j+1]
                if (k == gn && twoPads)
                {
                    break;
                }
                decodedBytes[j + 1] =
                    (sbyte)
                    (((dmap[encodedSBuf[start + i + 1]] & 0x0f) << 4) | ((dmap[encodedSBuf[start + i + 2]] & 0x3c) >> 2));
                // build decodedBytes[j+2]
                if (k == gn && onePad)
                {
                    break;
                }
                decodedBytes[j + 2] =
                    (sbyte)
                    (((dmap[encodedSBuf[start + i + 2]] & 0x03) << 6) | (dmap[encodedSBuf[start + i + 3]] & 0x3f));
            }
            return decodedBytes;
        }
        /// <summary>
        ///     Checks if the input byte array contains only safe values, that is,
        ///     the data does not need to be encoded for use with LDIF.
        ///     The rules for checking safety are based on the rules for LDIF
        ///     (Ldap Data Interchange Format) per RFC 2849.  The data does
        ///     not need to be encoded if all the following are true:
        ///     The data cannot start with the following byte values:
        ///     <pre>
        ///         00 (NUL)
        ///         10 (LF)
        ///         13 (CR)
        ///         32 (SPACE)
        ///         58 (:)
        ///         60 (LESSTHAN)
        ///         Any character with value greater than 127
        ///         (Negative for a byte value)
        ///     </pre>
        ///     The data cannot contain any of the following byte values:
        ///     <pre>
        ///         00 (NUL)
        ///         10 (LF)
        ///         13 (CR)
        ///         Any character with value greater than 127
        ///         (Negative for a byte value)
        ///     </pre>
        ///     The data cannot end with a space.
        /// </summary>
        /// <param name="bytes">
        ///     the bytes to be checked.
        /// </param>
        /// <returns>
        ///     true if encoding not required for LDIF
        /// </returns>
        public static bool isLDIFSafe(sbyte[] bytes)
        {
            var len = bytes.Length;
            if (len > 0)
            {
                int testChar = bytes[0];
                // unsafe if first character is a NON-SAFE-INIT-CHAR
                if (testChar == 0x00 || testChar == 0x0A || testChar == 0x0D || testChar == 0x20 || testChar == 0x3A ||
                    testChar == 0x3C || testChar < 0)
                {
                    // non ascii (>127 is negative)
                    return false;
                }
                // unsafe if last character is a space
                if (bytes[len - 1] == ' ')
                {
                    return false;
                }
                // unsafe if contains any non safe character
                if (len > 1)
                {
                    for (var i = 1; i < bytes.Length; i++)
                    {
                        testChar = bytes[i];
                        if (testChar == 0x00 || testChar == 0x0A || testChar == 0x0D || testChar < 0)
                        {
                            // non ascii (>127 is negative)
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        /// <summary>
        ///     Checks if the input String contains only safe values, that is,
        ///     the data does not need to be encoded for use with LDIF.
        ///     The rules for checking safety are based on the rules for LDIF
        ///     (Ldap Data Interchange Format) per RFC 2849.  The data does
        ///     not need to be encoded if all the following are true:
        ///     The data cannot start with the following char values:
        ///     <pre>
        ///         00 (NUL)
        ///         10 (LF)
        ///         13 (CR)
        ///         32 (SPACE)
        ///         58 (:)
        ///         60 (LESSTHAN)
        ///         Any character with value greater than 127
        ///     </pre>
        ///     The data cannot contain any of the following char values:
        ///     <pre>
        ///         00 (NUL)
        ///         10 (LF)
        ///         13 (CR)
        ///         Any character with value greater than 127
        ///     </pre>
        ///     The data cannot end with a space.
        /// </summary>
        /// <param name="str">
        ///     the String to be checked.
        /// </param>
        /// <returns>
        ///     true if encoding not required for LDIF
        /// </returns>
        public static bool isLDIFSafe(string str)
        {
            try
            {
                var encoder = Encoding.GetEncoding("utf-8");
                var ibytes = encoder.GetBytes(str);
                var sbytes = ibytes.ToSByteArray();
                return isLDIFSafe(sbytes);
            }
            catch (IOException ue)
            {
                throw new Exception("UTF-8 String encoding not supported by JVM", ue);
            }
        }
        /* **************UTF-8 Validation methods and members*******************
        * The following text is taken from draft-yergeau-rfc2279bis-02 and explains
        * UTF-8 encoding:
        *
        *In UTF-8, characters are encoded using sequences of 1 to 6 octets.
        * If the range of character numbers is restricted to U+0000..U+10FFFF
        * (the UTF-16 accessible range), then only sequences of one to four
        * octets will occur.  The only octet of a "sequence" of one has the
        * higher-order bit set to 0, the remaining 7 bits being used to encode
        * the character number.  In a sequence of n octets, n>1, the initial
        * octet has the n higher-order bits set to 1, followed by a bit set to
        * 0.  The remaining bit(s) of that octet contain bits from the number
        * of the character to be encoded.  The following octet(s) all have the
        * higher-order bit set to 1 and the following bit set to 0, leaving 6
        * bits in each to contain bits from the character to be encoded.
        *
        * The table below summarizes the format of these different octet types.
        * The letter x indicates bits available for encoding bits of the
        * character number.
        *
        * <pre>
        * Char. number range  |        UTF-8 octet sequence
        *    (hexadecimal)    |              (binary)
        * --------------------+---------------------------------------------
        * 0000 0000-0000 007F | 0xxxxxxx
        * 0000 0080-0000 07FF | 110xxxxx 10xxxxxx
        * 0000 0800-0000 FFFF | 1110xxxx 10xxxxxx 10xxxxxx
        * 0001 0000-001F FFFF | 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
        * 0020 0000-03FF FFFF | 111110xx 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx
        * 0400 0000-7FFF FFFF | 1111110x 10xxxxxx ... 10xxxxxx
        * </pre>
        */
        /// <summary>
        ///     Given the first byte in a sequence, getByteCount returns the number of
        ///     additional bytes in a UTF-8 character sequence (not including the first
        ///     byte).
        /// </summary>
        /// <param name="b">
        ///     The first byte in a UTF-8 character sequence.
        /// </param>
        /// <returns>
        ///     the number of additional bytes in a UTF-8 character sequence.
        /// </returns>
        private static int getByteCount(sbyte b)
        {
            if (b > 0)
                return 0;
            if ((b & 0xE0) == 0xC0)
            {
                return 1; //one additional byte (2 bytes total)
            }
            if ((b & 0xF0) == 0xE0)
            {
                return 2; //two additional bytes (3 bytes total)
            }
            if ((b & 0xF8) == 0xF0)
            {
                return 3; //three additional bytes (4 bytes total)
            }
            if ((b & 0xFC) == 0xF8)
            {
                return 4; //four additional bytes (5 bytes total)
            }
            if ((b & 0xFF) == 0xFC)
            {
                return 5; //five additional bytes (6 bytes total)
            }
            return -1;
        }
        /// <summary>
        ///     Bit masks used to determine if a the value of UTF-8 byte sequence
        ///     is less than the minimum value.
        ///     If the value of a byte sequence is less than the minimum value then
        ///     the number should be encoded in fewer bytes and is invalid.  For example
        ///     If the first byte indicates that a sequence has three bytes in a
        ///     sequence. Then the top five bits cannot be zero.  Notice the index into
        ///     the array is one less than the number of bytes in a sequence.
        ///     A validity test for this could be:
        /// </summary>
        private static readonly sbyte[][] lowerBoundMask =
        {
            new sbyte[] {0, 0}, new[] {(sbyte) 0x1E, (sbyte) 0x00},
            new[] {(sbyte) 0x0F, (sbyte) 0x20}, new[] {(sbyte) 0x07, (sbyte) 0x30}, new[] {(sbyte) 0x02, (sbyte) 0x38},
            new[] {(sbyte) 0x01, (sbyte) 0x3C}
        };
        public static long Identity(long literal)
        {
            return literal;
        }
        /// <summary>mask to AND with a continuation byte: should equal continuationResult </summary>
        private static readonly sbyte continuationMask = (sbyte)Identity(0xC0);
        /// <summary>expected result of ANDing a continuation byte with continuationMask </summary>
        private static readonly sbyte continuationResult = (sbyte)Identity(0x80);
        /// <summary>
        ///     Determines if an array of bytes contains only valid UTF-8 characters.
        ///     UTF-8 is the standard encoding for Ldap strings.  If a value contains
        ///     data that is not valid UTF-8 then data is lost converting the
        ///     value to a Java String.
        ///     In addition, Java Strings currently use UCS2 (Unicode Code Standard
        ///     2-byte characters). UTF-8 can be encoded as USC2 and UCS4 (4-byte
        ///     characters).  Some valid UTF-8 characters cannot be represented as UCS2
        ///     characters. To determine if all UTF-8 sequences can be encoded into
        ///     UCS2 characters (a Java String), specify the <code>isUCS2Only</code>
        ///     parameter as <code>true</code>.
        /// </summary>
        /// <param name="array">
        ///     An array of bytes that are to be tested for valid UTF-8
        ///     encoding.
        /// </param>
        /// <param name="isUCS2Only">
        ///     true if the UTF-8 values must be restricted to fit
        ///     within UCS2 encoding (2 bytes)
        /// </param>
        /// <returns>
        ///     true if all values in the byte array are valid UTF-8
        ///     sequences.  If <code>isUCS2Only</code> is
        ///     <code>true</code>, the method returns false if a UTF-8
        ///     sequence generates any character that cannot be
        ///     represented as a UCS2 character (Java String)
        /// </returns>
        public static bool isValidUTF8(sbyte[] array, bool isUCS2Only)
        {
            var index = 0;
            while (index < array.Length)
            {
                var count = getByteCount(array[index]);
                if (count == 0)
                {
                    //anything that qualifies as count=0 is valid UTF-8
                    index++;
                    continue;
                }
                if (count == -1 || index + count >= array.Length || isUCS2Only && count >= 3)
                {
                    /* Any count that puts us out of bounds for the index is
                    * invalid.  Valid UCS2 characters can only have 2 additional
                    * bytes. (three total) */
                    return false;
                }
                /* Tests if the first and second byte are below the minimum bound */
                if ((lowerBoundMask[count][0] & array[index]) == 0 && (lowerBoundMask[count][1] & array[index + 1]) == 0)
                {
                    return false;
                }
                /* testing continuation on the second and following bytes */
                for (var i = 1; i <= count; i++)
                {
                    if ((array[index + i] & continuationMask) != continuationResult)
                    {
                        return false;
                    }
                }
                index += count + 1;
            }
            return true;
        }
    }
    /// <summary>
    ///     The name and values of one attribute of a directory entry.
    ///     LdapAttribute objects are used when searching for, adding,
    ///     modifying, and deleting attributes from the directory.
    ///     LdapAttributes are often used in conjunction with an
    ///     {@link LdapAttributeSet} when retrieving or adding multiple
    ///     attributes to an entry.
    /// </summary>
    /// <seealso cref="LdapEntry">
    /// </seealso>
    /// <seealso cref="LdapAttributeSet">
    /// </seealso>
    /// <seealso cref="LdapModification">
    /// </seealso>
    public class LdapAttribute
    {
        /// <summary>
        ///     Returns an enumerator for the values of the attribute in byte format.
        /// </summary>
        /// <returns>
        ///     The values of the attribute in byte format.
        ///     Note: All string values will be UTF-8 encoded. To decode use the
        ///     String constructor. Example: new String( byteArray, "UTF-8" );
        /// </returns>
        /*public virtual IEnumerator ByteValues
        {
            get { return ByteValueArray; }
        }
*/
        /// <summary>
        ///     Returns an enumerator for the string values of an attribute.
        /// </summary>
        /// <returns>
        ///     The string values of an attribute.
        /// </returns>
        /*
        public virtual IEnumerator StringValues
        {
            get { return new ArrayEnumeration(); }
        }
*/
        /// <summary>
        ///     Returns the values of the attribute as an array of bytes.
        /// </summary>
        /// <returns>
        ///     The values as an array of bytes or an empty array if there are
        ///     no values.
        /// </returns>
        public virtual sbyte[][] ByteValueArray
        {
            get
            {
                if (null == values)
                    return new sbyte[0][];
                var size = values.Length;
                var bva = new sbyte[size][];
                // Deep copy so application cannot change values
                for (int i = 0, u = size; i < u; i++)
                {
                    bva[i] = new sbyte[((sbyte[])values[i]).Length];
                    Array.Copy((Array)values[i], 0, bva[i], 0, bva[i].Length);
                }
                return bva;
            }
        }
        /// <summary>
        ///     Returns the values of the attribute as an array of strings.
        /// </summary>
        /// <returns>
        ///     The values as an array of strings or an empty array if there are
        ///     no values
        /// </returns>
        public virtual string[] StringValueArray
        {
            get
            {
                if (null == values)
                    return new string[0];
                var size = values.Length;
                var sva = new string[size];
                for (var j = 0; j < size; j++)
                {
                    try
                    {
                        var encoder = Encoding.GetEncoding("utf-8");
                        var dchar = encoder.GetChars(((sbyte[])values[j]).ToByteArray());
                        //						char[] dchar = encoder.GetChars((byte[])values[j]);
                        sva[j] = new string(dchar);
                        //						sva[j] = new String((sbyte[]) values[j], "UTF-8");
                    }
                    catch (IOException uee)
                    {
                        // Exception should NEVER get thrown but just in case it does ...
                        throw new Exception(uee.ToString());
                    }
                }
                return sva;
            }
        }
        /// <summary>
        ///     Returns the the first value of the attribute as a <code>String</code>.
        /// </summary>
        /// <returns>
        ///     The UTF-8 encoded<code>String</code> value of the attribute's
        ///     value.  If the value wasn't a UTF-8 encoded <code>String</code>
        ///     to begin with the value of the returned <code>String</code> is
        ///     non deterministic.
        ///     If <code>this</code> attribute has more than one value the
        ///     first value is converted to a UTF-8 encoded <code>String</code>
        ///     and returned. It should be noted, that the directory may
        ///     return attribute values in any order, so that the first
        ///     value may vary from one call to another.
        ///     If the attribute has no values <code>null</code> is returned
        /// </returns>
        public virtual string StringValue
        {
            get
            {
                string rval = null;
                if (values != null)
                {
                    try
                    {
                        var encoder = Encoding.GetEncoding("utf-8");
                        var dchar = encoder.GetChars(((sbyte[])values[0]).ToByteArray());
                        //						char[] dchar = encoder.GetChars((byte[]) this.values[0]);
                        rval = new string(dchar);
                    }
                    catch (IOException use)
                    {
                        throw new Exception(use.ToString());
                    }
                }
                return rval;
            }
        }
        /// <summary>
        ///     Returns the the first value of the attribute as a byte array.
        /// </summary>
        /// <returns>
        ///     The binary value of <code>this</code> attribute or
        ///     <code>null</code> if <code>this</code> attribute doesn't have a value.
        ///     If the attribute has no values <code>null</code> is returned
        /// </returns>
        public virtual sbyte[] ByteValue
        {
            get
            {
                sbyte[] bva = null;
                if (values != null)
                {
                    // Deep copy so app can't change the value
                    bva = new sbyte[((sbyte[])values[0]).Length];
                    Array.Copy((Array)values[0], 0, bva, 0, bva.Length);
                }
                return bva;
            }
        }
        /// <summary>
        ///     Returns the language subtype of the attribute, if any.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns the string, lang-ja.
        /// </summary>
        /// <returns>
        ///     The language subtype of the attribute or null if the attribute
        ///     has none.
        /// </returns>
        public virtual string LangSubtype
        {
            get
            {
                if (subTypes != null)
                {
                    for (var i = 0; i < subTypes.Length; i++)
                    {
                        if (subTypes[i].StartsWith("lang-"))
                        {
                            return subTypes[i];
                        }
                    }
                }
                return null;
            }
        }
        /// <summary>
        ///     Returns the name of the attribute.
        /// </summary>
        /// <returns>
        ///     The name of the attribute.
        /// </returns>
        public virtual string Name
        {
            get { return name; }
        }
        /// <summary>
        ///     Replaces all values with the specified value. This protected method is
        ///     used by sub-classes of LdapSchemaElement because the value cannot be set
        ///     with a contructor.
        /// </summary>
        protected internal virtual string Value
        {
            set
            {
                values = null;
                try
                {
                    var encoder = Encoding.GetEncoding("utf-8");
                    var ibytes = encoder.GetBytes(value);
                    var sbytes = ibytes.ToSByteArray();
                    add(sbytes);
                }
                catch (IOException ue)
                {
                    throw new Exception(ue.ToString());
                }
            }
        }
        private readonly string name; // full attribute name
        private readonly string baseName; // cn of cn;lang-ja;phonetic
        private readonly string[] subTypes; // lang-ja of cn;lang-ja
        private object[] values; // Array of byte[] attribute values
        /// <summary>
        ///     Constructs an attribute with copies of all values of the input
        ///     attribute.
        /// </summary>
        /// <param name="attr">
        ///     An LdapAttribute to use as a template.
        ///     @throws IllegalArgumentException if attr is null
        /// </param>
        public LdapAttribute(LdapAttribute attr)
        {
            if (attr == null)
            {
                throw new ArgumentException("LdapAttribute class cannot be null");
            }
            // Do a deep copy of the LdapAttribute template
            name = attr.name;
            baseName = attr.baseName;
            if (null != attr.subTypes)
            {
                subTypes = new string[attr.subTypes.Length];
                Array.Copy(attr.subTypes, 0, subTypes, 0, subTypes.Length);
            }
            // OK to just copy attributes, as the app only sees a deep copy of them
            if (null != attr.values)
            {
                values = new object[attr.values.Length];
                Array.Copy(attr.values, 0, values, 0, values.Length);
            }
        }
        /// <summary>
        ///     Constructs an attribute with no values.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        ///     @throws IllegalArgumentException if attrName is null
        /// </param>
        public LdapAttribute(string attrName)
        {
            if ((object)attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }
            name = attrName;
            baseName = getBaseName(attrName);
            subTypes = getSubtypes(attrName);
        }
        /// <summary>
        ///     Constructs an attribute with a byte-formatted value.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     @throws IllegalArgumentException if attrName or attrBytes is null
        /// </param>
        public LdapAttribute(string attrName, sbyte[] attrBytes) : this(attrName)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            // Make our own copy of the byte array to prevent app from changing it
            var tmp = new sbyte[attrBytes.Length];
            Array.Copy(attrBytes, 0, tmp, 0, attrBytes.Length);
            add(tmp);
        }
        /// <summary>
        ///     Constructs an attribute with a single string value.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrString">
        ///     Value of the attribute as a string.
        ///     @throws IllegalArgumentException if attrName or attrString is null
        /// </param>
        public LdapAttribute(string attrName, string attrString) : this(attrName)
        {
            if ((object)attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            try
            {
                var encoder = Encoding.GetEncoding("utf-8");
                var ibytes = encoder.GetBytes(attrString);
                var sbytes = ibytes.ToSByteArray();
                add(sbytes);
            }
            catch (IOException e)
            {
                throw new Exception(e.ToString());
            }
        }
        /// <summary>
        ///     Constructs an attribute with an array of string values.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrStrings">
        ///     Array of values as strings.
        ///     @throws IllegalArgumentException if attrName, attrStrings, or a member
        ///     of attrStrings is null
        /// </param>
        public LdapAttribute(string attrName, string[] attrStrings) : this(attrName)
        {
            if (attrStrings == null)
            {
                throw new ArgumentException("Attribute values array cannot be null");
            }
            for (int i = 0, u = attrStrings.Length; i < u; i++)
            {
                try
                {
                    if ((object)attrStrings[i] == null)
                    {
                        throw new ArgumentException("Attribute value " + "at array index " + i + " cannot be null");
                    }
                    var encoder = Encoding.GetEncoding("utf-8");
                    var ibytes = encoder.GetBytes(attrStrings[i]);
                    var sbytes = ibytes.ToSByteArray();
                    add(sbytes);
                    //					this.add(attrStrings[i].getBytes("UTF-8"));
                }
                catch (IOException e)
                {
                    throw new Exception(e.ToString());
                }
            }
        }
        /// <summary>
        ///     Returns a clone of this LdapAttribute.
        /// </summary>
        /// <returns>
        ///     clone of this LdapAttribute.
        /// </returns>
        public object Clone()
        {
            try
            {
                var newObj = MemberwiseClone();
                if (values != null)
                {
                    Array.Copy(values, 0, ((LdapAttribute)newObj).values, 0, values.Length);
                }
                return newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }
        /// <summary>
        ///     Adds a string value to the attribute.
        /// </summary>
        /// <param name="attrString">
        ///     Value of the attribute as a String.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void addValue(string attrString)
        {
            if ((object)attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            try
            {
                var encoder = Encoding.GetEncoding("utf-8");
                var ibytes = encoder.GetBytes(attrString);
                var sbytes = ibytes.ToSByteArray();
                add(sbytes);
                //				this.add(attrString.getBytes("UTF-8"));
            }
            catch (IOException ue)
            {
                throw new Exception(ue.ToString());
            }
        }
        /// <summary>
        ///     Adds a byte-formatted value to the attribute.
        /// </summary>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     @throws IllegalArgumentException if attrBytes is null
        /// </param>
        public virtual void addValue(sbyte[] attrBytes)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            add(attrBytes);
        }
        /// <summary>
        ///     Adds a base64 encoded value to the attribute.
        ///     The value will be decoded and stored as bytes.  String
        ///     data encoded as a base64 value must be UTF-8 characters.
        /// </summary>
        /// <param name="attrString">
        ///     The base64 value of the attribute as a String.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void addBase64Value(string attrString)
        {
            if ((object)attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            add(Base64.decode(attrString));
        }
        /// <summary>
        ///     Adds a base64 encoded value to the attribute.
        ///     The value will be decoded and stored as bytes.  Character
        ///     data encoded as a base64 value must be UTF-8 characters.
        /// </summary>
        /// <param name="attrString">
        ///     The base64 value of the attribute as a StringBuffer.
        /// </param>
        /// <param name="start">
        ///     The start index of base64 encoded part, inclusive.
        /// </param>
        /// <param name="end">
        ///     The end index of base encoded part, exclusive.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void addBase64Value(StringBuilder attrString, int start, int end)
        {
            if (attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            add(Base64.decode(attrString, start, end));
        }
        /// <summary>
        ///     Adds a base64 encoded value to the attribute.
        ///     The value will be decoded and stored as bytes.  Character
        ///     data encoded as a base64 value must be UTF-8 characters.
        /// </summary>
        /// <param name="attrChars">
        ///     The base64 value of the attribute as an array of
        ///     characters.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void addBase64Value(char[] attrChars)
        {
            if (attrChars == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            add(Base64.decode(attrChars));
        }
        /// <summary>
        ///     Returns the base name of the attribute.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns cn.
        /// </summary>
        /// <returns>
        ///     The base name of the attribute.
        /// </returns>
        public virtual string getBaseName()
        {
            return baseName;
        }
        /// <summary>
        ///     Returns the base name of the specified attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns cn.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute from which to extract the
        ///     base name.
        /// </param>
        /// <returns>
        ///     The base name of the attribute.
        ///     @throws IllegalArgumentException if attrName is null
        /// </returns>
        public static string getBaseName(string attrName)
        {
            if ((object)attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }
            var idx = attrName.IndexOf(';');
            if (-1 == idx)
            {
                return attrName;
            }
            return attrName.Substring(0, idx - 0);
        }
        /// <summary>
        ///     Extracts the subtypes from the attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns an array containing lang-ja and phonetic.
        /// </summary>
        /// <returns>
        ///     An array subtypes or null if the attribute has none.
        /// </returns>
        public virtual string[] getSubtypes()
        {
            return subTypes;
        }
        /// <summary>
        ///     Extracts the subtypes from the specified attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns an array containing lang-ja and phonetic.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute from which to extract
        ///     the subtypes.
        /// </param>
        /// <returns>
        ///     An array subtypes or null if the attribute has none.
        ///     @throws IllegalArgumentException if attrName is null
        /// </returns>
        public static string[] getSubtypes(string attrName)
        {
            if ((object)attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }
            var st = new Tokenizer(attrName, ";");
            string[] subTypes = null;
            var cnt = st.Count;
            if (cnt > 0)
            {
                st.NextToken(); // skip over basename
                subTypes = new string[cnt - 1];
                var i = 0;
                while (st.HasMoreTokens())
                {
                    subTypes[i++] = st.NextToken();
                }
            }
            return subTypes;
        }
        /// <summary>
        ///     Reports if the attribute name contains the specified subtype.
        ///     For example, if you check for the subtype lang-en and the
        ///     attribute name is cn;lang-en, this method returns true.
        /// </summary>
        /// <param name="subtype">
        ///     The single subtype to check for.
        /// </param>
        /// <returns>
        ///     True, if the attribute has the specified subtype;
        ///     false, if it doesn't.
        ///     @throws IllegalArgumentException if subtype is null
        /// </returns>
        public virtual bool hasSubtype(string subtype)
        {
            if ((object)subtype == null)
            {
                throw new ArgumentException("subtype cannot be null");
            }
            if (null != subTypes)
            {
                for (var i = 0; i < subTypes.Length; i++)
                {
                    if (subTypes[i].ToUpper().Equals(subtype.ToUpper()))
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        ///     Reports if the attribute name contains all the specified subtypes.
        ///     For example, if you check for the subtypes lang-en and phonetic
        ///     and if the attribute name is cn;lang-en;phonetic, this method
        ///     returns true. If the attribute name is cn;phonetic or cn;lang-en,
        ///     this method returns false.
        /// </summary>
        /// <param name="subtypes">
        ///     An array of subtypes to check for.
        /// </param>
        /// <returns>
        ///     True, if the attribute has all the specified subtypes;
        ///     false, if it doesn't have all the subtypes.
        ///     @throws IllegalArgumentException if subtypes is null or if array member
        ///     is null.
        /// </returns>
        public virtual bool hasSubtypes(string[] subtypes)
        {
            if (subtypes == null)
            {
                throw new ArgumentException("subtypes cannot be null");
            }
            for (var i = 0; i < subtypes.Length; i++)
            {
                for (var j = 0; j < subTypes.Length; j++)
                {
                    if ((object)subTypes[j] == null)
                    {
                        throw new ArgumentException("subtype " + "at array index " + i + " cannot be null");
                    }
                    if (subTypes[j].ToUpper().Equals(subtypes[i].ToUpper()))
                    {
                        goto gotSubType;
                    }
                }
                return false;
                gotSubType:
                ;
            }
            return true;
        }
        /// <summary>
        ///     Removes a string value from the attribute.
        /// </summary>
        /// <param name="attrString">
        ///     Value of the attribute as a string.
        ///     Note: Removing a value which is not present in the attribute has
        ///     no effect.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void removeValue(string attrString)
        {
            if (null == (object)attrString)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            try
            {
                var encoder = Encoding.GetEncoding("utf-8");
                var ibytes = encoder.GetBytes(attrString);
                var sbytes = ibytes.ToSByteArray();
                removeValue(sbytes);
                //				this.removeValue(attrString.getBytes("UTF-8"));
            }
            catch (IOException uee)
            {
                // This should NEVER happen but just in case ...
                throw new Exception(uee.ToString());
            }
        }
        /// <summary>
        ///     Removes a byte-formatted value from the attribute.
        /// </summary>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     Example: <code>String.getBytes("UTF-8");</code>
        ///     Note: Removing a value which is not present in the attribute has
        ///     no effect.
        ///     @throws IllegalArgumentException if attrBytes is null
        /// </param>
        public virtual void removeValue(sbyte[] attrBytes)
        {
            if (null == attrBytes)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }
            for (var i = 0; i < values.Length; i++)
            {
                if (equals(attrBytes, (sbyte[])values[i]))
                {
                    if (0 == i && 1 == values.Length)
                    {
                        // Optimize if first element of a single valued attr
                        values = null;
                        return;
                    }
                    if (values.Length == 1)
                    {
                        values = null;
                    }
                    else
                    {
                        var moved = values.Length - i - 1;
                        var tmp = new object[values.Length - 1];
                        if (i != 0)
                        {
                            Array.Copy(values, 0, tmp, 0, i);
                        }
                        if (moved != 0)
                        {
                            Array.Copy(values, i + 1, tmp, i, moved);
                        }
                        values = tmp;
                        tmp = null;
                    }
                    break;
                }
            }
        }
        /// <summary>
        ///     Returns the number of values in the attribute.
        /// </summary>
        /// <returns>
        ///     The number of values in the attribute.
        /// </returns>
        public virtual int size()
        {
            return null == values ? 0 : values.Length;
        }
        /// <summary>
        ///     Compares this object with the specified object for order.
        ///     Ordering is determined by comparing attribute names (see
        ///     {@link #getName() }) using the method compareTo() of the String class.
        /// </summary>
        /// <param name="attribute">
        ///     The LdapAttribute to be compared to this object.
        /// </param>
        /// <returns>
        ///     Returns a negative integer, zero, or a positive
        ///     integer as this object is less than, equal to, or greater than the
        ///     specified object.
        /// </returns>
        public virtual int CompareTo(object attribute)
        {
            return name.CompareTo(((LdapAttribute)attribute).name);
        }
        /// <summary>
        ///     Adds an object to <code>this</code> object's list of attribute values
        /// </summary>
        /// <param name="bytes">
        ///     Ultimately all of this attribute's values are treated
        ///     as binary data so we simplify the process by requiring
        ///     that all data added to our list is in binary form.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        /// </param>
        private void add(sbyte[] bytes)
        {
            if (null == values)
            {
                values = new object[] { bytes };
            }
            else
            {
                // Duplicate attribute values not allowed
                for (var i = 0; i < values.Length; i++)
                {
                    if (equals(bytes, (sbyte[])values[i]))
                    {
                        return; // Duplicate, don't add
                    }
                }
                var tmp = new object[values.Length + 1];
                Array.Copy(values, 0, tmp, 0, values.Length);
                tmp[values.Length] = bytes;
                values = tmp;
                tmp = null;
            }
        }
        /// <summary>
        ///     Returns true if the two specified arrays of bytes are equal to each
        ///     another.  Matches the logic of Arrays.equals which is not available
        ///     in jdk 1.1.x.
        /// </summary>
        /// <param name="e1">
        ///     the first array to be tested
        /// </param>
        /// <param name="e2">
        ///     the second array to be tested
        /// </param>
        /// <returns>
        ///     true if the two arrays are equal
        /// </returns>
        private bool equals(sbyte[] e1, sbyte[] e2)
        {
            // If same object, they compare true
            if (e1 == e2)
                return true;
            // If either but not both are null, they compare false
            if (e1 == null || e2 == null)
                return false;
            // If arrays have different length, they compare false
            var length = e1.Length;
            if (e2.Length != length)
                return false;
            // If any of the bytes are different, they compare false
            for (var i = 0; i < length; i++)
            {
                if (e1[i] != e2[i])
                    return false;
            }
            return true;
        }
        /// <summary>
        ///     Returns a string representation of this LdapAttribute
        /// </summary>
        /// <returns>
        ///     a string representation of this LdapAttribute
        /// </returns>
        public override string ToString()
        {
            var result = new StringBuilder("LdapAttribute: ");
            try
            {
                result.Append("{type='" + name + "'");
                if (values != null)
                {
                    result.Append(", ");
                    if (values.Length == 1)
                    {
                        result.Append("value='");
                    }
                    else
                    {
                        result.Append("values='");
                    }
                    for (var i = 0; i < values.Length; i++)
                    {
                        if (i != 0)
                        {
                            result.Append("','");
                        }
                        if (((sbyte[])values[i]).Length == 0)
                        {
                            continue;
                        }
                        var encoder = Encoding.GetEncoding("utf-8");
                        //						char[] dchar = encoder.GetChars((byte[]) values[i]);
                        var dchar = encoder.GetChars(((sbyte[])values[i]).ToByteArray());
                        var sval = new string(dchar);
                        //						System.String sval = new String((sbyte[]) values[i], "UTF-8");
                        if (sval.Length == 0)
                        {
                            // didn't decode well, must be binary
                            result.Append("<binary value, length:" + sval.Length);
                            continue;
                        }
                        result.Append(sval);
                    }
                    result.Append("'");
                }
                result.Append("}");
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
            return result.ToString();
        }
    }
    /// <summary>
    ///     A set of {@link LdapAttribute} objects.
    ///     An <code>LdapAttributeSet</code> is a collection of <code>LdapAttribute</code>
    ///     classes as returned from an <code>LdapEntry</code> on a search or read
    ///     operation. <code>LdapAttributeSet</code> may be also used to contruct an entry
    ///     to be added to a directory.  If the <code>add()</code> or <code>addAll()</code>
    ///     methods are called and one or more of the objects to be added is not
    ///     an <code>LdapAttribute, ClassCastException</code> is thrown (as discussed in the
    ///     documentation for <code>java.util.Collection</code>).
    /// </summary>
    /// <seealso cref="LdapAttribute">
    /// </seealso>
    /// <seealso cref="LdapEntry">
    /// </seealso>
    public class LdapAttributeSet : SetSupport //, SupportClass.SetSupport
    {
        /// <summary>
        ///     Returns the number of attributes in this set.
        /// </summary>
        /// <returns>
        ///     number of attributes in this set.
        /// </returns>
        public override int Count
        {
            get { return map.Count; }
        }

        /// <summary>
        ///     This is the underlying data structure for this set.
        ///     HashSet is similar to the functionality of this set.  The difference
        ///     is we use the name of an attribute as keys in the Map and LdapAttributes
        ///     as the values.  We also do not declare the map as transient, making the
        ///     map serializable.
        /// </summary>
        private readonly Hashtable map;

        /// <summary> Constructs an empty set of attributes.</summary>
        public LdapAttributeSet()
        {
            map = new Hashtable();
        }
        // ---  methods not defined in Set ---
        /// <summary>
        ///     Returns a deep copy of this attribute set.
        /// </summary>
        /// <returns>
        ///     A deep copy of this attribute set.
        /// </returns>
        public override object Clone()
        {
            try
            {
                var newObj = MemberwiseClone();
                var i = GetEnumerator();
                while (i.MoveNext())
                {
                    ((LdapAttributeSet)newObj).Add(((LdapAttribute)i.Current).Clone());
                }
                return newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }
        /// <summary>
        ///     Returns the attribute matching the specified attrName.
        ///     For example:
        ///     <ul>
        ///         <li><code>getAttribute("cn")</code>      returns only the "cn" attribute</li>
        ///         <li>
        ///             <code>getAttribute("cn;lang-en")</code> returns only the "cn;lang-en"
        ///             attribute.
        ///         </li>
        ///     </ul>
        ///     In both cases, <code>null</code> is returned if there is no exact match to
        ///     the specified attrName.
        ///     Note: Novell eDirectory does not currently support language subtypes.
        ///     It does support the "binary" subtype.
        /// </summary>
        /// <param name="attrName">
        ///     The name of an attribute to retrieve, with or without
        ///     subtype specifications. For example, "cn", "cn;phonetic", and
        ///     "cn;binary" are valid attribute names.
        /// </param>
        /// <returns>
        ///     The attribute matching the specified attrName, or <code>null</code>
        ///     if there is no exact match.
        /// </returns>
        public virtual LdapAttribute getAttribute(string attrName)
        {
            return (LdapAttribute)map[attrName.ToUpper()];
        }
        /// <summary>
        ///     Returns a single best-match attribute, or <code>null</code> if no match is
        ///     available in the entry.
        ///     Ldap version 3 allows adding a subtype specification to an attribute
        ///     name. For example, "cn;lang-ja" indicates a Japanese language
        ///     subtype of the "cn" attribute and "cn;lang-ja-JP-kanji" may be a subtype
        ///     of "cn;lang-ja". This feature may be used to provide multiple
        ///     localizations in the same directory. For attributes which do not vary
        ///     among localizations, only the base attribute may be stored, whereas
        ///     for others there may be varying degrees of specialization.
        ///     For example, <code>getAttribute(attrName,lang)</code> returns the
        ///     <code>LdapAttribute</code> that exactly matches attrName and that
        ///     best matches lang.
        ///     If there are subtypes other than "lang" subtypes included
        ///     in attrName, for example, "cn;binary", only attributes with all of
        ///     those subtypes are returned. If lang is <code>null</code> or empty, the
        ///     method behaves as getAttribute(attrName). If there are no matching
        ///     attributes, <code>null</code> is returned.
        ///     Assume the entry contains only the following attributes:
        ///     <ul>
        ///         <li>cn;lang-en</li>
        ///         <li>cn;lang-ja-JP-kanji</li>
        ///         <li>sn</li>
        ///     </ul>
        ///     Examples:
        ///     <ul>
        ///         <li><code>getAttribute( "cn" )</code>       returns <code>null</code>.</li>
        ///         <li><code>getAttribute( "sn" )</code>       returns the "sn" attribute.</li>
        ///         <li>
        ///             <code>getAttribute( "cn", "lang-en-us" )</code>
        ///             returns the "cn;lang-en" attribute.
        ///         </li>
        ///         <li>
        ///             <code>getAttribute( "cn", "lang-en" )</code>
        ///             returns the "cn;lang-en" attribute.
        ///         </li>
        ///         <li>
        ///             <code>getAttribute( "cn", "lang-ja" )</code>
        ///             returns <code>null</code>.
        ///         </li>
        ///         <li>
        ///             <code>getAttribute( "sn", "lang-en" )</code>
        ///             returns the "sn" attribute.
        ///         </li>
        ///     </ul>
        ///     Note: Novell eDirectory does not currently support language subtypes.
        ///     It does support the "binary" subtype.
        /// </summary>
        /// <param name="attrName">
        ///     The name of an attribute to retrieve, with or without
        ///     subtype specifications. For example, "cn", "cn;phonetic", and
        ///     cn;binary" are valid attribute names.
        /// </param>
        /// <param name="lang">
        ///     A language specification with optional subtypes
        ///     appended using "-" as separator. For example, "lang-en", "lang-en-us",
        ///     "lang-ja", and "lang-ja-JP-kanji" are valid language specification.
        /// </param>
        /// <returns>
        ///     A single best-match <code>LdapAttribute</code>, or <code>null</code>
        ///     if no match is found in the entry.
        /// </returns>
        public virtual LdapAttribute getAttribute(string attrName, string lang)
        {
            var key = attrName + ";" + lang;
            return (LdapAttribute)map[key.ToUpper()];
        }
        /// <summary>
        ///     Creates a new attribute set containing only the attributes that have
        ///     the specified subtypes.
        ///     For example, suppose an attribute set contains the following
        ///     attributes:
        ///     <ul>
        ///         <li>    cn</li>
        ///         <li>    cn;lang-ja</li>
        ///         <li>    sn;phonetic;lang-ja</li>
        ///         <li>    sn;lang-us</li>
        ///     </ul>
        ///     Calling the <code>getSubset</code> method and passing lang-ja as the
        ///     argument, the method returns an attribute set containing the following
        ///     attributes:
        ///     <ul>
        ///         <li>cn;lang-ja</li>
        ///         <li>sn;phonetic;lang-ja</li>
        ///     </ul>
        /// </summary>
        /// <param name="subtype">
        ///     Semi-colon delimited list of subtypes to include. For
        ///     example:
        ///     <ul>
        ///         <li> "lang-ja" specifies only Japanese language subtypes</li>
        ///         <li> "binary" specifies only binary subtypes</li>
        ///         <li>
        ///             "binary;lang-ja" specifies only Japanese language subtypes
        ///             which also are binary
        ///         </li>
        ///     </ul>
        ///     Note: Novell eDirectory does not currently support language subtypes.
        ///     It does support the "binary" subtype.
        /// </param>
        /// <returns>
        ///     An attribute set containing the attributes that match the
        ///     specified subtype.
        /// </returns>
        public virtual LdapAttributeSet getSubset(string subtype)
        {
            // Create a new tempAttributeSet
            var tempAttributeSet = new LdapAttributeSet();
            var i = GetEnumerator();
            // Cycle throught this.attributeSet
            while (i.MoveNext())
            {
                var attr = (LdapAttribute)i.Current;
                // Does this attribute have the subtype we are looking for. If
                // yes then add it to our AttributeSet, else next attribute
                if (attr.hasSubtype(subtype))
                    tempAttributeSet.Add(attr.Clone());
            }
            return tempAttributeSet;
        }
        // --- methods defined in set ---
        /// <summary>
        ///     Returns an iterator over the attributes in this set.  The attributes
        ///     returned from this iterator are not in any particular order.
        /// </summary>
        /// <returns>
        ///     iterator over the attributes in this set
        /// </returns>
        public override IEnumerator GetEnumerator()
        {
            return map.Values.GetEnumerator();
        }
        /// <summary>
        ///     Returns <code>true</code> if this set contains no elements
        /// </summary>
        /// <returns>
        ///     <code>true</code> if this set contains no elements
        /// </returns>
        public override bool IsEmpty()
        {
            return map.Count == 0;
        }
        /// <summary>
        ///     Returns <code>true</code> if this set contains an attribute of the same name
        ///     as the specified attribute.
        /// </summary>
        /// <param name="attr">
        ///     Object of type <code>LdapAttribute</code>
        /// </param>
        /// <returns>
        ///     true if this set contains the specified attribute
        ///     @throws ClassCastException occurs the specified Object
        ///     is not of type LdapAttribute.
        /// </returns>
        public override bool Contains(object attr)
        {
            var attribute = (LdapAttribute)attr;
            return map.ContainsKey(attribute.Name.ToUpper());
        }
        /// <summary>
        ///     Adds the specified attribute to this set if it is not already present.
        ///     If an attribute with the same name already exists in the set then the
        ///     specified attribute will not be added.
        /// </summary>
        /// <param name="attr">
        ///     Object of type <code>LdapAttribute</code>
        /// </param>
        /// <returns>
        ///     true if the attribute was added.
        ///     @throws ClassCastException occurs the specified Object
        ///     is not of type <code>LdapAttribute</code>.
        /// </returns>
        public override bool Add(object attr)
        {
            //We must enforce that attr is an LdapAttribute
            var attribute = (LdapAttribute)attr;
            var name = attribute.Name.ToUpper();
            if (map.ContainsKey(name))
                return false;
            map[name] = attribute;
            return true;
        }
        /// <summary>
        ///     Removes the specified object from this set if it is present.
        ///     If the specified object is of type <code>LdapAttribute</code>, the
        ///     specified attribute will be removed.  If the specified object is of type
        ///     <code>String</code>, the attribute with a name that matches the string will
        ///     be removed.
        /// </summary>
        /// <param name="object">
        ///     LdapAttribute to be removed or <code>String</code> naming
        ///     the attribute to be removed.
        /// </param>
        /// <returns>
        ///     true if the object was removed.
        ///     @throws ClassCastException occurs the specified Object
        ///     is not of type <code>LdapAttribute</code> or of type <code>String</code>.
        /// </returns>
        public override bool Remove(object object_Renamed)
        {
            string attributeName; //the name is the key to object in the HashMap
            if (object_Renamed is string)
            {
                attributeName = (string)object_Renamed;
            }
            else
            {
                attributeName = ((LdapAttribute)object_Renamed).Name;
            }
            if ((object)attributeName == null)
            {
                return false;
            }
            var e = map[attributeName.ToUpper()];
            map.Remove(e);
            return e != null;
        }
        /// <summary> Removes all of the elements from this set.</summary>
        public override void Clear()
        {
            map.Clear();
        }
        /// <summary>
        ///     Adds all <code>LdapAttribute</code> objects in the specified collection to
        ///     this collection.
        /// </summary>
        /// <param name="c">
        ///     Collection of <code>LdapAttribute</code> objects.
        ///     @throws ClassCastException occurs when an element in the
        ///     collection is not of type <code>LdapAttribute</code>.
        /// </param>
        /// <returns>
        ///     true if this set changed as a result of the call.
        /// </returns>
        public override bool AddAll(ICollection c)
        {
            var setChanged = false;
            var i = c.GetEnumerator();
            while (i.MoveNext())
            {
                // we must enforce that everything in c is an LdapAttribute
                // add will return true if the attribute was added
                if (Add(i.Current))
                {
                    setChanged = true;
                }
            }
            return setChanged;
        }
        /// <summary>
        ///     Returns a string representation of this LdapAttributeSet
        /// </summary>
        /// <returns>
        ///     a string representation of this LdapAttributeSet
        /// </returns>
        public override string ToString()
        {
            var retValue = new StringBuilder("LdapAttributeSet: ");
            var attrs = GetEnumerator();
            var first = true;
            while (attrs.MoveNext())
            {
                if (!first)
                {
                    retValue.Append(" ");
                }
                first = false;
                var attr = (LdapAttribute)attrs.Current;
                retValue.Append(attr);
            }
            return retValue.ToString();
        }
    }
    /// <summary>
    ///     Represents a single entry in a directory, consisting of
    ///     a distinguished name (DN) and zero or more attributes.
    ///     An instance of
    ///     LdapEntry is created in order to add an entry to a directory, and
    ///     instances of LdapEntry are returned on a search by enumerating an
    ///     LdapSearchResults.
    /// </summary>
    /// <seealso cref="LdapAttribute">
    /// </seealso>
    /// <seealso cref="LdapAttributeSet">
    /// </seealso>
    public class LdapEntry
    {
        /// <summary>
        ///     Returns the distinguished name of the entry.
        /// </summary>
        /// <returns>
        ///     The distinguished name of the entry.
        /// </returns>
        public virtual string DN
        {
            get { return dn; }
        }
        protected internal string dn;
        protected internal LdapAttributeSet attrs;
        /// <summary> Constructs an empty entry.</summary>
        public LdapEntry() : this(null, null)
        {
        }
        /// <summary>
        ///     Constructs a new entry with the specified distinguished name and with
        ///     an empty attribute set.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry. The
        ///     value is not validated. An invalid distinguished
        ///     name will cause operations using this entry to fail.
        /// </param>
        public LdapEntry(string dn) : this(dn, null)
        {
        }
        /// <summary>
        ///     Constructs a new entry with the specified distinguished name and set
        ///     of attributes.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the new entry. The
        ///     value is not validated. An invalid distinguished
        ///     name will cause operations using this entry to fail.
        /// </param>
        /// <param name="attrs">
        ///     The initial set of attributes assigned to the
        ///     entry.
        /// </param>
        public LdapEntry(string dn, LdapAttributeSet attrs)
        {
            if ((object)dn == null)
            {
                dn = "";
            }
            if (attrs == null)
            {
                attrs = new LdapAttributeSet();
            }
            this.dn = dn;
            this.attrs = attrs;
        }
        /// <summary>
        ///     Returns the attributes matching the specified attrName.
        /// </summary>
        /// <param name="attrName">
        ///     The name of the attribute or attributes to return.
        /// </param>
        /// <returns>
        ///     An array of LdapAttribute objects.
        /// </returns>
        public virtual LdapAttribute getAttribute(string attrName)
        {
            return attrs.getAttribute(attrName);
        }
        /// <summary>
        ///     Returns the attribute set of the entry.
        ///     All base and subtype variants of all attributes are
        ///     returned. The LdapAttributeSet returned may be
        ///     empty if there are no attributes in the entry.
        /// </summary>
        /// <returns>
        ///     The attribute set of the entry.
        /// </returns>
        public virtual LdapAttributeSet getAttributeSet()
        {
            return attrs;
        }
        /// <summary>
        ///     Returns an attribute set from the entry, consisting of only those
        ///     attributes matching the specified subtypes.
        ///     The getAttributeSet method can be used to extract only
        ///     a particular language variant subtype of each attribute,
        ///     if it exists. The "subtype" may be, for example, "lang-ja", "binary",
        ///     or "lang-ja;phonetic". If more than one subtype is specified, separated
        ///     with a semicolon, only those attributes with all of the named
        ///     subtypes will be returned. The LdapAttributeSet returned may be
        ///     empty if there are no matching attributes in the entry.
        /// </summary>
        /// <param name="subtype">
        ///     One or more subtype specification(s), separated
        ///     with semicolons. The "lang-ja" and
        ///     "lang-en;phonetic" are valid subtype
        ///     specifications.
        /// </param>
        /// <returns>
        ///     An attribute set from the entry with the attributes that
        ///     match the specified subtypes or an empty set if no attributes
        ///     match.
        /// </returns>
        public virtual LdapAttributeSet getAttributeSet(string subtype)
        {
            return attrs.getSubset(subtype);
        }
    }
    /// <summary>
    ///     Encapsulates a single search result that is in response to an asynchronous
    ///     search operation.
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    public class LdapSearchResult : LdapMessage
    {
        /// <summary>
        ///     Returns the entry of a server's search response.
        /// </summary>
        /// <returns>
        ///     The LdapEntry associated with this LdapSearchResult
        /// </returns>
        public virtual LdapEntry Entry
        {
            get
            {
                if (entry == null)
                {
                    var attrs = new LdapAttributeSet();
                    var attrList = ((RfcSearchResultEntry)message.Response).Attributes;
                    var seqArray = attrList.ToArray();
                    for (var i = 0; i < seqArray.Length; i++)
                    {
                        var seq = (Asn1Sequence)seqArray[i];
                        var attr = new LdapAttribute(((Asn1OctetString)seq.Get(0)).StringValue());
                        var Set = (Asn1Set)seq.Get(1);
                        object[] setArray = Set.ToArray();
                        for (var j = 0; j < setArray.Length; j++)
                        {
                            attr.addValue(((Asn1OctetString)setArray[j]).ByteValue());
                        }
                        attrs.Add(attr);
                    }
                    entry = new LdapEntry(((RfcSearchResultEntry)message.Response).ObjectName.StringValue(), attrs);
                }
                return entry;
            }
        }
        private LdapEntry entry;
        /// <summary>
        ///     Constructs an LdapSearchResult object.
        /// </summary>
        /// <param name="message">
        ///     The RfcLdapMessage with a search result.
        /// </param>
        /*package*/
        internal LdapSearchResult(RfcLdapMessage message) : base(message)
        {
        }
        /// <summary>
        ///     Constructs an LdapSearchResult object from an LdapEntry.
        /// </summary>
        /// <param name="entry">
        ///     the LdapEntry represented by this search result.
        /// </param>
        /// <param name="cont">
        ///     controls associated with the search result
        /// </param>
        public LdapSearchResult(LdapEntry entry, LdapControl[] cont)
        {
            if (entry == null)
            {
                throw new ArgumentException("Argument \"entry\" cannot be null");
            }
            this.entry = entry;
        }
        /// <summary>
        ///     Return a String representation of this object.
        /// </summary>
        /// <returns>
        ///     a String representing this object.
        /// </returns>
        public override string ToString()
        {
            string str;
            if (entry == null)
            {
                str = base.ToString();
            }
            else
            {
                str = entry.ToString();
            }
            return str;
        }
    }
    /// <summary>
    ///     Encapsulates an ID which uniquely identifies a particular extended
    ///     operation, known to a particular server, and the data associated
    ///     with that extended operation.
    /// </summary>
    /// <seealso cref="LdapConnection.ExtendedOperation">
    /// </seealso>
    public class LdapExtendedOperation
    {
        private string oid;
        private sbyte[] vals;
        /// <summary>
        ///     Constructs a new object with the specified object ID and data.
        /// </summary>
        /// <param name="oid">
        ///     The unique identifier of the operation.
        /// </param>
        /// <param name="vals">
        ///     The operation-specific data of the operation.
        /// </param>
        public LdapExtendedOperation(string oid, sbyte[] vals)
        {
            this.oid = oid;
            this.vals = vals;
        }
        /// <summary>
        ///     Returns a clone of this object.
        /// </summary>
        /// <returns>
        ///     clone of this object.
        /// </returns>
        public object Clone()
        {
            try
            {
                var newObj = MemberwiseClone();
                //				Array.Copy((System.Array)SupportClass.ToByteArray( this.vals), 0, (System.Array)SupportClass.ToByteArray( ((LdapExtendedOperation) newObj).vals), 0, this.vals.Length);
                Array.Copy(vals, 0, ((LdapExtendedOperation)newObj).vals, 0, vals.Length);
                return newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }
        /// <summary>
        ///     Returns the unique identifier of the operation.
        /// </summary>
        /// <returns>
        ///     The OID (object ID) of the operation.
        /// </returns>
        public virtual string getID()
        {
            return oid;
        }
        /// <summary>
        ///     Returns a reference to the operation-specific data.
        /// </summary>
        /// <returns>
        ///     The operation-specific data.
        /// </returns>
        public virtual sbyte[] getValue()
        {
            return vals;
        }
        /// <summary>
        ///     Sets the value for the operation-specific data.
        /// </summary>
        /// <param name="newVals">
        ///     The byte array of operation-specific data.
        /// </param>
        protected internal virtual void setValue(sbyte[] newVals)
        {
            vals = newVals;
        }
        /// <summary>
        ///     Resets the OID for the operation to a new value
        /// </summary>
        /// <param name="newoid">
        ///     The new OID for the operation
        /// </param>
        protected internal virtual void setID(string newoid)
        {
            oid = newoid;
        }
    }
    /// <summary>
    ///     Represents an Ldap Search Result Reference.
    ///     <pre>
    ///         SearchResultReference ::= [APPLICATION 19] SEQUENCE OF LdapURL
    ///     </pre>
    /// </summary>
    public class RfcSearchResultReference : Asn1SequenceOf
    {
        // Constructors for SearchResultReference
        /// <summary>
        ///     The only time a client will create a SearchResultReference is when it is
        ///     decoding it from an InputStream
        /// </summary>
        public RfcSearchResultReference(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
        }
        // Accessors
        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.SEARCH_RESULT_REFERENCE);
        }
    }
    /// <summary> Represnts an Ldap String.</summary>
    public class RfcLdapString : Asn1OctetString
    {
        public RfcLdapString(string s) : base(s)
        {
        }
        public RfcLdapString(sbyte[] ba) : base(ba)
        {
        }
        public RfcLdapString(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
        }
    }
    /// <summary>
    ///     Represents an Ldap DN.
    ///     <pre>
    ///         LdapDN ::= LdapString
    ///     </pre>
    /// </summary>
    public class RfcLdapDN : RfcLdapString
    {
        // Constructors for RfcLdapDN
        public RfcLdapDN(string s) : base(s)
        {
        }
        public RfcLdapDN(sbyte[] s) : base(s)
        {
        }
    }
    /// <summary>
    ///     Represents an Ldap Extended Response.
    ///     <pre>
    ///         ExtendedResponse ::= [APPLICATION 24] SEQUENCE {
    ///         COMPONENTS OF LdapResult,
    ///         responseName     [10] LdapOID OPTIONAL,
    ///         response         [11] OCTET STRING OPTIONAL }
    ///     </pre>
    /// </summary>
    public class RfcExtendedResponse : Asn1Sequence, RfcResponse
    {
        public virtual RfcLdapOID ResponseName
        {
            get { return responseNameIndex != 0 ? (RfcLdapOID)Get(responseNameIndex) : null; }
        }
        public virtual Asn1OctetString Response
        {
            get { return responseIndex != 0 ? (Asn1OctetString)Get(responseIndex) : null; }
        }
        /// <summary> Context-specific TAG for optional responseName.</summary>
        public const int RESPONSE_NAME = 10;
        /// <summary> Context-specific TAG for optional response.</summary>
        public const int RESPONSE = 11;
        private readonly int referralIndex;
        private readonly int responseNameIndex;
        private readonly int responseIndex;
        // Constructors for ExtendedResponse
        /// <summary>
        ///     The only time a client will create a ExtendedResponse is when it is
        ///     decoding it from an InputStream
        /// </summary>
        public RfcExtendedResponse(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
            // decode optional tagged elements
            if (Size() > 3)
            {
                for (var i = 3; i < Size(); i++)
                {
                    var obj = (Asn1Tagged)Get(i);
                    var id = obj.GetIdentifier();
                    switch (id.Tag)
                    {
                        case RfcLdapResult.REFERRAL:
                            var content = ((Asn1OctetString)obj.taggedValue()).ByteValue();
                            var bais = new MemoryStream(content.ToByteArray());
                            Set(i, new Asn1SequenceOf(dec, bais, content.Length));
                            referralIndex = i;
                            break;
                        case RESPONSE_NAME:
                            Set(i, new RfcLdapOID(((Asn1OctetString)obj.taggedValue()).ByteValue()));
                            responseNameIndex = i;
                            break;
                        case RESPONSE:
                            Set(i, obj.taggedValue());
                            responseIndex = i;
                            break;
                    }
                }
            }
        }
        // Accessors
        public Asn1Enumerated getResultCode()
        {
            return (Asn1Enumerated)Get(0);
        }
        public RfcLdapDN getMatchedDN()
        {
            return new RfcLdapDN(((Asn1OctetString)Get(1)).ByteValue());
        }
        public RfcLdapString getErrorMessage()
        {
            return new RfcLdapString(((Asn1OctetString)Get(2)).ByteValue());
        }
        public Asn1SequenceOf getReferral()
        {
            return referralIndex != 0 ? (Asn1SequenceOf)Get(referralIndex) : null;
        }
        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.EXTENDED_RESPONSE);
        }
    }
    /// <summary>
    ///     Represents and Ldap Bind Response.
    ///     <pre>
    ///         BindResponse ::= [APPLICATION 1] SEQUENCE {
    ///         COMPONENTS OF LdapResult,
    ///         serverSaslCreds    [7] OCTET STRING OPTIONAL }
    ///     </pre>
    /// </summary>
    public class RfcBindResponse : Asn1Sequence, RfcResponse
    {
        /// <summary>
        ///     Returns the OPTIONAL serverSaslCreds of a BindResponse if it exists
        ///     otherwise null.
        /// </summary>
        public virtual Asn1OctetString ServerSaslCreds
        {
            get
            {
                if (Size() == 5)
                    return (Asn1OctetString)((Asn1Tagged)Get(4)).taggedValue();
                if (Size() == 4)
                {
                    // could be referral or serverSaslCreds
                    var obj = Get(3);
                    if (obj is Asn1Tagged)
                        return (Asn1OctetString)((Asn1Tagged)obj).taggedValue();
                }
                return null;
            }
        }
        // Constructors for BindResponse
        /// <summary>
        ///     The only time a client will create a BindResponse is when it is
        ///     decoding it from an InputStream
        ///     Note: If serverSaslCreds is included in the BindResponse, it does not
        ///     need to be decoded since it is already an OCTET STRING.
        /// </summary>
        public RfcBindResponse(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
            // Decode optional referral from Asn1OctetString to Referral.
            if (Size() > 3)
            {
                var obj = (Asn1Tagged)Get(3);
                var id = obj.GetIdentifier();
                if (id.Tag == RfcLdapResult.REFERRAL)
                {
                    var content = ((Asn1OctetString)obj.taggedValue()).ByteValue();
                    var bais = new MemoryStream(content.ToByteArray());
                    Set(3, new Asn1SequenceOf(dec, bais, content.Length));
                }
            }
        }
        // Accessors
        public Asn1Enumerated getResultCode()
        {
            return (Asn1Enumerated)Get(0);
        }
        public RfcLdapDN getMatchedDN()
        {
            return new RfcLdapDN(((Asn1OctetString)Get(1)).ByteValue());
        }
        public RfcLdapString getErrorMessage()
        {
            return new RfcLdapString(((Asn1OctetString)Get(2)).ByteValue());
        }
        public Asn1SequenceOf getReferral()
        {
            if (Size() > 3)
            {
                var obj = Get(3);
                if (obj is Asn1SequenceOf)
                    return (Asn1SequenceOf)obj;
            }
            return null;
        }
        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.BIND_RESPONSE);
        }
    }
    /// <summary>
    ///     Represents an LDAP Intermediate Response.
    ///     IntermediateResponse ::= [APPLICATION 25] SEQUENCE {
    ///     COMPONENTS OF LDAPResult, note: only present on incorrectly
    ///     encoded response from
    ///     pre Falcon-sp1 server
    ///     responseName     [10] LDAPOID OPTIONAL,
    ///     responseValue    [11] OCTET STRING OPTIONAL }
    /// </summary>
    public class RfcIntermediateResponse : Asn1Sequence, RfcResponse
    {
        /**
         * Context-specific TAG for optional responseName.
         */
        public const int TAG_RESPONSE_NAME = 0;
        /**
         * Context-specific TAG for optional response.
         */
        public const int TAG_RESPONSE = 1;
        private int m_referralIndex;
        private readonly int m_responseNameIndex;
        private readonly int m_responseValueIndex;
        // Constructors for ExtendedResponse
        /**
         * The only time a client will create a IntermediateResponse is when it is
         * decoding it from an InputStream. The stream contains the intermediate
         * response sequence that follows the msgID in the PDU. The intermediate
         * response draft defines this as:
         *      IntermediateResponse ::= [APPLICATION 25] SEQUENCE {
         *             responseName     [0] LDAPOID OPTIONAL,
         *             responseValue    [1] OCTET STRING OPTIONAL }
         *
         * Until post Falcon sp1, the LDAP server was incorrectly encoding
         * intermediate response as:
         *      IntermediateResponse ::= [APPLICATION 25] SEQUENCE {
         *             Components of LDAPResult,
         *             responseName     [0] LDAPOID OPTIONAL,
         *             responseValue    [1] OCTET STRING OPTIONAL }
         *
         * where the Components of LDAPResult are
         *               resultCode      ENUMERATED {...}
         *               matchedDN       LDAPDN,
         *               errorMessage    LDAPString,
         *               referral        [3] Referral OPTIONAL }
         *
         *
         * (The components of LDAPResult never have the optional referral.)
         * This constructor is written to handle both cases.
         *
         * The sequence of this intermediate response will have the element
         * at index m_responseNameIndex set to an RfcLDAPOID containing the
         * oid of the response. The element at m_responseValueIndex will be set
         * to an ASN1OctetString containing the value bytes.
         */
        public RfcIntermediateResponse(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        //		throws IOException
        {
            //		super(dec, in, len);
            var i = 0;
            m_responseNameIndex = m_responseValueIndex = 0;
            // decode optional tagged elements. The parent class constructor will
            // have decoded these elements as ASN1Tagged objects with the value
            // stored as an ASN1OctectString object.
            if (Size() >= 3) //the incorrectly encoded case, LDAPResult contains 
                i = 3; //at least 3 components
            else
                i = 0; //correctly encoded case, can have zero components
            for (; i < Size(); i++)
            {
                var obj = (Asn1Tagged)Get(i);
                var id = obj.GetIdentifier();
                switch (id.Tag)
                {
                    case TAG_RESPONSE_NAME:
                        Set(i, new RfcLdapOID(
                            ((Asn1OctetString)obj.taggedValue()).ByteValue()));
                        m_responseNameIndex = i;
                        break;
                    case TAG_RESPONSE:
                        Set(i, obj.taggedValue());
                        m_responseValueIndex = i;
                        break;
                }
            }
        }
        public Asn1Enumerated getResultCode()
        {
            if (Size() > 3)
                return (Asn1Enumerated)Get(0);
            return null;
        }
        public RfcLdapDN getMatchedDN()
        {
            if (Size() > 3)
                return new RfcLdapDN(((Asn1OctetString)Get(1)).ByteValue());
            return null;
        }
        public RfcLdapString getErrorMessage()
        {
            if (Size() > 3)
                return new RfcLdapString(((Asn1OctetString)Get(2)).ByteValue());
            return null;
        }
        public Asn1SequenceOf getReferral()
        {
            return Size() > 3 ? (Asn1SequenceOf)Get(3) : null;
        }
        public RfcLdapOID getResponseName()
        {
            return m_responseNameIndex >= 0
                ? (RfcLdapOID)Get(m_responseNameIndex)
                : null;
        }
        public Asn1OctetString getResponse()
        {
            return m_responseValueIndex != 0
                ? (Asn1OctetString)Get(m_responseValueIndex)
                : null;
        }
        /**
         * Override getIdentifier to return an application-wide id.
         */
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true,
                LdapMessage.INTERMEDIATE_RESPONSE);
        }
    }
}
#endif