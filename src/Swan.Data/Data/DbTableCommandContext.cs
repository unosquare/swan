namespace Swan.Data;
internal class DbTableCommandContext : DbCommandContext
{

    /// <summary>
    /// Gets the table metadata.
    /// </summary>
    public TableMetadata Table { get; }
}


