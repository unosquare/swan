namespace Unosquare.Swan
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    partial class Extensions
    {
        static private readonly Lazy<MD5> MD5Hasher = new Lazy<MD5>(() => { return MD5.Create(); }, true);

        /// <summary>
        /// Computes the MD5 of the string and outputs it in a hexadecimal, uppercase string.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns></returns>
        public static string ComputeMD5(this string inputString)
        {
            var inputBytes = Encoding.UTF8.GetBytes(inputString.Trim().ToLower());
            var hash = MD5Hasher.Value.ComputeHash(inputBytes);

            var sb = new StringBuilder();

            foreach (var t in hash)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
