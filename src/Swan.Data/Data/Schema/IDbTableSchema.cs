namespace Swan.Data.Schema;

/// <summary>
/// Represents table structure information from the backing data store.
/// </summary>
public interface IDbTableSchema
{
    /// <summary>
    /// Gets the column schema data for the given column name.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <returns>The column schema.</returns>
    IDbColumnSchema? this[string name] { get; }

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
    IReadOnlyList<IDbColumnSchema> Columns { get; }

    /// <summary>
    /// Gets a list of key columns by qhich records are uniquely identified.
    /// </summary>
    IReadOnlyList<IDbColumnSchema> KeyColumns { get; }

    /// <summary>
    /// Gets the identity column (if any) for this table.
    /// Returns null if no identity column is found.
    /// </summary>
    IDbColumnSchema? IdentityKeyColumn { get; }

    /// <summary>
    /// Determines if the table has an identity column.
    /// </summary>
    bool HasKeyIdentityColumn { get; }

    /// <summary>
    /// Gets the columns that can be used for Insert statements.
    /// These are columns that are not read-only or automatically set by the RDBMS.
    /// </summary>
    IReadOnlyList<IDbColumnSchema> InsertableColumns { get; }

    /// <summary>
    /// Gets the columns that can be used for update statements.
    /// </summary>
    IReadOnlyList<IDbColumnSchema> UpdateableColumns { get; }

    /// <summary>
    /// Adds a column to the table schema.
    /// Column name is mandatory.
    /// </summary>
    /// <param name="column">The column to add.</param>
    IDbTableSchema AddColumn(IDbColumnSchema column);

    /// <summary>
    /// Removes a column from the table schema by its column name.
    /// </summary>
    /// <param name="columnName">The name of the column to remove.</param>
    IDbTableSchema RemoveColumn(string columnName);
}
