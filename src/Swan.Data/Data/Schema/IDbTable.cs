namespace Swan.Data.Schema;

/// <summary>
/// Represents table structure information from the backing data store.
/// </summary>
public interface IDbTable
{
    /// <summary>
    /// Gets the ssociated databse provider.
    /// </summary>
    DbProvider Provider { get; }

    /// <summary>
    /// Gets the database name (catalog) this table belongs to.
    /// </summary>
    string Database { get; }

    /// <summary>
    /// Gets the schema name this table belongs to. Returns
    /// an empty string if provider does not support schemas.
    /// </summary>
    string Schema { get; }

    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    string TableName { get; }

    /// <summary>
    /// Gets the list of columns contained in this table.
    /// </summary>
    public IReadOnlyList<IDbColumn> Columns { get; }
}

