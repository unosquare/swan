namespace Swan.Data.SqlBulkOps;
/// <summary>
/// Defines constants for bulk operations.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The default notification frequency in number of rows copied.
    /// </summary>
    public const int DefaultNotifyAfter = 100;

    /// <summary>
    /// The minimum notification frequency in number of rows copied.
    /// </summary>
    public const int MinNotifyAfter = 10;

    /// <summary>
    /// The maximum notification frequency in number of rows copied.
    /// </summary>
    public const int MaxNotifyAfter = 1000;

    /// <summary>
    /// The default batch size in number of rows copied.
    /// </summary>
    public const int DefaultBatchSize = 1000;

    /// <summary>
    /// The minimum batch size in number of rows copied.
    /// </summary>
    public const int MinBatchSize = 10;

    /// <summary>
    /// The maximum batch size in number of rows copied.
    /// </summary>
    public const int MaxBatchSize = 10000;

    /// <summary>
    /// Represents an indefinite wait time for the bulk copy operation to complete.
    /// </summary>
    public const int InfiniteTimeoutSeconds = 0;
}
