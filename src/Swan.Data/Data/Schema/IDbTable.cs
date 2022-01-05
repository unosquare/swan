namespace Swan.Data.Schema;

/// <summary>
/// Represents table structure information from the backing data store.
/// </summary>
public interface IDbTable
{
    /// <summary>
    /// Gets the column schema data for the given column name.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <returns>The column schema.</returns>
    IDbColumn? this[string name]
    {
        get;
    }

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

    /// <summary>
    /// Adds a column to the table schema.
    /// Column name is mandatory.
    /// </summary>
    /// <param name="column">The column to add.</param>
    void AddColumn(IDbColumn column);

    /// <summary>
    /// Removes a column from the table schema by its column name.
    /// </summary>
    /// <param name="column">The name of the column to remove.</param>
    void RemoveColumn(string column);
}

