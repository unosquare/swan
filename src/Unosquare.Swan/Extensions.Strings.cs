namespace Unosquare.Swan
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    partial class Extensions
    {
        private static readonly Lazy<MD5> MD5Hasher = new Lazy<MD5>(() => { return MD5.Create(); }, true);

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

        /// <summary>
        /// Returns a string that represents the given item
        /// It tries to use InvariantCulture if the ToString(IFormatProvider)
        /// overload exists.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static string ToStringInvariant(this object item)
        {
            if (item == null)
                return string.Empty;

            var itemType = item.GetType();

            if (itemType == typeof(string))
                return item as string;

            return Constants.BasicTypesInfo.ContainsKey(itemType) ?
                Constants.BasicTypesInfo[itemType].ToStringInvariant(item) :
                item.ToString();
        }

        /// <summary>
        /// Returns a string that represents the given item
        /// It tries to use InvariantCulture if the ToString(IFormatProvider)
        /// overload exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static string ToStringInvariant<T>(this T item)
        {
            if (typeof(string) == typeof(T))
                return item == null ? string.Empty : item as string;

            return ToStringInvariant(item as object);
        }
    }
}
