namespace Swan.Extensions
{
    using System;

    /// <summary>
    /// Provides ways to express various types as human readable strings.
    /// </summary>
    public static class FormattingExtensions
    {
        /// <summary>
        /// Formats the specified bytes count as a human readable size in bytes.
        /// In the context of this method, one kilobyte is 1024 bytes.
        /// </summary>
        /// <param name="byteLength">The length in number of bytes.</param>
        /// <returns>
        /// The string representing a human readable size in bytes.
        /// </returns>
        public static string FormatByteSize(this ulong byteLength)
        {
            const ulong KiloByte = 1024;
            const ulong MegaByte = KiloByte * KiloByte;
            const ulong GigaByte = MegaByte * KiloByte;
            const ulong TeraByte = GigaByte * KiloByte;

            switch (byteLength)
            {
                case < KiloByte:
                    return $"{byteLength} bytes";
                case >= TeraByte:
                    return $"{byteLength / Convert.ToDouble(TeraByte):0.##} TB";
                case >= GigaByte:
                    return $"{byteLength / Convert.ToDouble(GigaByte):0.##} GB";
                case >= MegaByte:
                    return $"{byteLength / Convert.ToDouble(MegaByte):0.##} MB";
                case >= KiloByte:
                    return $"{byteLength / Convert.ToDouble(KiloByte):0.##} KB";
            }
        }

        /// <summary>
        /// Formats the specified bytes count as a human readable size in bytes.
        /// In the context of this method, one kilobyte is 1024 bytes.
        /// </summary>
        /// <param name="byteLength">The length in number of bytes.</param>
        /// <returns>
        /// The string representing a human readable size in bytes.
        /// </returns>
        public static string FormatByteSize(this long byteLength)
        {
            return byteLength < 0
                ? $"-{Convert.ToUInt64(Math.Abs(byteLength)).FormatByteSize()}"
                : Convert.ToUInt64(byteLength).FormatByteSize();
        }

        /// <summary>
        /// Formats the specified bytes count as a human readable size in bytes.
        /// In the context of this method, one kilobyte is 1024 bytes.
        /// </summary>
        /// <param name="byteLength">The length in number of bytes.</param>
        /// <returns>
        /// The string representing a human readable size in bytes.
        /// </returns>
        public static string FormatByteSize(this int byteLength) => Convert.ToInt64(byteLength).FormatByteSize();
    }
}
