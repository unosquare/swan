namespace Swan.Extensions;

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
        const ulong kiloByte = 1024;
        const ulong megaByte = kiloByte * kiloByte;
        const ulong gigaByte = megaByte * kiloByte;
        const ulong teraByte = gigaByte * kiloByte;

        return byteLength switch
        {
            < kiloByte => $"{byteLength} bytes",
            >= teraByte => $"{byteLength / Convert.ToDouble(teraByte):0.##} TB",
            >= gigaByte => $"{byteLength / Convert.ToDouble(gigaByte):0.##} GB",
            >= megaByte => $"{byteLength / Convert.ToDouble(megaByte):0.##} MB",
            >= kiloByte => $"{byteLength / Convert.ToDouble(kiloByte):0.##} KB"
        };
    }

    /// <summary>
    /// Formats the specified bytes count as a human readable size in bytes.
    /// In the context of this method, one kilobyte is 1024 bytes.
    /// </summary>
    /// <param name="byteLength">The length in number of bytes.</param>
    /// <returns>
    /// The string representing a human readable size in bytes.
    /// </returns>
    public static string FormatByteSize(this long byteLength) =>
        byteLength < 0
            ? $"-{Convert.ToUInt64(Math.Abs(byteLength)).FormatByteSize()}"
            : Convert.ToUInt64(byteLength).FormatByteSize();

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
