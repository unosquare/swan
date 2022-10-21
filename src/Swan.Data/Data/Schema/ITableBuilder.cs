namespace Swan.Data.Schema;

/// <summary>
/// Provides a table builder with a backing schema typically used to issue Table DDL commands.
/// </summary>
public interface ITableBuilder : IDbTableSchema, IDbConnected
{
    /// <summary>
    /// Using the table schema, builds a DDL command to create the table in the database
    /// if it does not exist.
    /// </summary>
    /// <param name="transaction">The optional transaction.</param>
    /// <returns>The command.</returns>
    DbCommand BuildDdlCommand(DbTransaction? transaction = default);

    /// <summary>
    /// Executes the DDL command that creates the table if it does not exist.
    /// </summary>
    /// <param name="transaction">The optional transaction.</param>
    /// <returns>The number of affected records.</returns>
    int ExecuteDdlCommand(DbTransaction? transaction = default);

    /// <summary>
    /// Executes the DDL command that creates the table if it does not exist.
    /// </summary>
    /// <param name="transaction">The optional transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> ExecuteDdlCommandAsync(DbTransaction? transaction = default, CancellationToken ct = default);

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
