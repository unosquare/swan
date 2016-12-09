namespace Unosquare.Swan
{
    using System.Security.Cryptography;
    using System.Text;

    partial class Extensions
    {
        static private readonly MD5 MD5Hasher = MD5.Create();

        /// <summary>
        /// Computes the MD5 of the string and outputs it ina a hexadecimal, uppercase string representation.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns></returns>
        public static string ComputeMD5(this string inputString)
        {
            var inputBytes = Encoding.UTF8.GetBytes(inputString.Trim().ToLower());
            var hash = MD5Hasher.ComputeHash(inputBytes);

            var sb = new StringBuilder();

            foreach (var t in hash)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
