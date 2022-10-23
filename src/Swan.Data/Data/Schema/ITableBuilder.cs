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
    DbCommand BuildTableCommand(DbTransaction? transaction = default);

    /// <summary>
    /// Executes the DDL command that creates the table if it does not exist.
    /// </summary>
    /// <param name="transaction">The optional transaction.</param>
    /// <returns>The resulting table context from executing the DDL command.</returns>
    ITableContext ExecuteTableCommand(DbTransaction? transaction = default);

    /// <summary>
    /// Executes the DDL command that creates the table if it does not exist.
    /// </summary>
    /// <param name="transaction">The optional transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The resulting table context from executing the DDL command.</returns>
    Task<ITableContext> ExecuteTableCommandAsync(DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Adds a column to the table schema.
    /// Column name is mandatory.
    /// </summary>
    /// <param name="column">The column to add.</param>
    ITableBuilder AddColumn(IDbColumnSchema column);

    /// <summary>
    /// Removes a column from the table schema by its column name.
    /// </summary>
    /// <param name="columnName">The name of the column to remove.</param>
    ITableBuilder RemoveColumn(string columnName);
}

/// <summary>
/// Represents a generic table builder.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface ITableBuilder<T> : ITableBuilder
    where T : class
{
    /// <summary>
    /// Executes the DDL command that creates the table if it does not exist.
    /// </summary>
    /// <param name="transaction">The optional transaction.</param>
    /// <returns>The resulting table context from executing the DDL command.</returns>
    new ITableContext<T> ExecuteTableCommand(DbTransaction? transaction = default);

    /// <summary>
    /// Executes the DDL command that creates the table if it does not exist.
    /// </summary>
    /// <param name="transaction">The optional transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The resulting table context from executing the DDL command.</returns>
    new Task<ITableContext<T>> ExecuteTableCommandAsync(DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Adds a column to the table schema.
    /// Column name is mandatory.
    /// </summary>
    /// <param name="column">The column to add.</param>
    new ITableBuilder<T> AddColumn(IDbColumnSchema column);

    /// <summary>
    /// Removes a column from the table schema by its column name.
    /// </summary>
    /// <param name="columnName">The name of the column to remove.</param>
    new ITableBuilder<T> RemoveColumn(string columnName);
}
