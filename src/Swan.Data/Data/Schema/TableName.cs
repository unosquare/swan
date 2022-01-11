namespace Swan.Data.Schema;

/// <summary>
/// Represents a table and schema name.
/// </summary>
/// <param name="Name">The table name.</param>
/// <param name="Schema">The schema name this table belongs to.</param>
public record TableName(string Name, string Schema)
{
    /// <summary>
    /// Creates a new instance of the <see cref="TableName"/> record.
    /// </summary>
    public TableName() : this(string.Empty, string.Empty) { }
}

