namespace Unosquare.Swan
{
    using System;
    using System.Globalization;
    using System.Text;

    partial class Extensions
    {
        /// <summary>
        /// Converts an array of bytes to its lower-case, hexadecimal representation
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="addPrefix">if set to <c>true</c> add the 0x prefix tot he output.</param>
        /// <returns></returns>
        public static string ToLowerHex(this byte[] bytes, bool addPrefix = false)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var item in bytes)
                sb.Append(item.ToString("x2", CultureInfo.InvariantCulture));

            return $"{(addPrefix ? "0x" : string.Empty)}{sb.ToString()}";
        }

        /// <summary>
        /// Converts an array of bytes to its upper-case, hexadecimal representation
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="addPrefix">if set to <c>true</c> [add prefix].</param>
        /// <returns></returns>
        public static string ToUpperHex(this byte[] bytes, bool addPrefix = false)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var item in bytes)
                sb.Append(item.ToString("X2", CultureInfo.InvariantCulture));

            return $"{(addPrefix ? "0x" : string.Empty)}{sb.ToString()}";
        }

        /// <summary>
        /// Converts an array of bytes to a sequence of dash-separated, hexadecimal,
        /// uppercase characters
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string ToDashedHex(this byte[] bytes)
        {
            return BitConverter.ToString(bytes);
        }

        /// <summary>
        /// Converts an array of bytes to a base-64 encoded string
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        public static string ToBase64(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
    }
}
