namespace Swan.Data.Schema;

/// <summary>
/// Represents a table and schema name.
/// </summary>
/// <param name="Name">The table name.</param>
/// <param name="Schema">The schema name this table belongs to.</param>
/// <param name="IsView">Sets whether the table is a view.</param>
public sealed record TableIdentifier(string Name, string Schema, bool IsView)
{
    /// <summary>
    /// Creates a new instance of the <see cref="TableIdentifier"/> record.
    /// </summary>
    public TableIdentifier() : this(string.Empty, string.Empty, false) { }
}
