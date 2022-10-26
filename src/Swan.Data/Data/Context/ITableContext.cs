namespace Swan.Data.Context;

/// <summary>
/// Represents a table and schema that is bound to a specific connection.
/// </summary>
public interface ITableContext : IDbTableSchema, IDbConnected
{
    /// <summary>
    /// Gets the current number of rows in the table.
    /// </summary>
    /// <param name="trailingSql">The SQL statements after the table name.</param>
    /// <param name="param">The optional parameters.</param>
    /// <param name="transaction">The optional transaction.</param>
    /// <returns>The computed table count.</returns>
    long Count(string? trailingSql = default, object? param = default, DbTransaction? transaction = default);

    /// <summary>
    /// Gets the current number of rows in the table.
    /// </summary>
    /// <param name="trailingSql">The SQL statements after the table name.</param>
    /// <param name="param">The optional parameters.</param>
    /// <param name="transaction">The optional transaction.</param>
    /// <param name="ct">The optional cancellation token.</param>
    /// <returns>The computed table count.</returns>
    Task<long> CountAsync(string? trailingSql = default, object? param = default, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a forward-only enumerable set which can then be processed by
    /// iterating over items, one at a time.
    /// </summary>
    /// <param name="deserializer">The object deserializer.</param>
    /// <param name="trailingSql">The optional sql statements appended after the basic SELECT clause.</param>
    /// <param name="param">The optional parameters object.</param>
    /// <param name="transaction">The associated transaction.</param>
    /// <returns>The enumerable to iterate over.</returns>
    IEnumerable Query(Func<IDataRecord, object> deserializer, string? trailingSql = default, object? param = default, DbTransaction? transaction = default);

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a forward-only enumerable set which can then be processed by
    /// iterating over items, one at a time.
    /// </summary>
    /// <param name="deserializer"></param>
    /// <param name="trailingSql">The optional sql statements appended after the basic SELECT clause.</param>
    /// <param name="param">The optional parameters object.</param>
    /// <param name="transaction">The associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The enumerable to iterate over.</returns>
    IAsyncEnumerable<object> QueryAsync(Func<IDataRecord, object> deserializer, string? trailingSql = default, object? param = default, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and a single row and returns the parsed object from the first row.
    /// </summary>
    /// <param name="deserializer">The object deserializer</param>
    /// <param name="trailingSql">The optional sql statements appended after the basic SELECT clause.</param>
    /// <param name="param">The optional parameters object.</param>
    /// <param name="transaction">The associated transaction.</param>
    /// <returns>The parsed object. A default value if no rows are retrieved.</returns>
    object? FirstOrDefault(Func<IDataRecord, object> deserializer, string? trailingSql = default, object? param = default, DbTransaction? transaction = default);

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and a single row and returns the parsed object from the first row.
    /// </summary>
    /// <param name="deserializer">The object deserializer</param>
    /// <param name="trailingSql">The optional sql statements appended after the basic SELECT clause.</param>
    /// <param name="param">The optional parameters object.</param>
    /// <param name="transaction">The associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The parsed object. A default value if no rows are retrieved.</returns>
    Task<object?> FirstOrDefaultAsync(
        Func<IDataRecord, object> deserializer,
        string? trailingSql = default, object? param = default, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Inserts an item of the given type to the database
    /// and if the table has defined a single, auto incremental key
    /// column (identity column), returns the inserted item.
    /// </summary>
    /// <param name="deserializer">The object deserializer.</param>
    /// <param name="item">The item to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The newly inserted item whenever possible.</returns>
    object? InsertOne(Func<IDataRecord, object> deserializer, object item, DbTransaction? transaction = default);

    /// <summary>
    /// Inserts an item of the given type to the database
    /// and if the table has defined a single, auto incremental key
    /// column (identity column), returns the inserted item.
    /// </summary>
    /// <param name="deserializer">The object deserializer.</param>
    /// <param name="item">The item to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The newly inserted item whenever possible.</returns>
    Task<object?> InsertOneAsync(Func<IDataRecord, object> deserializer, object item, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Inserts a set of records of the given type to the table.
    /// By default, this implementation does not represent a bulk insert operation.
    /// </summary>
    /// <param name="items">The items to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The number of records affected.</returns>
    int InsertMany(IEnumerable items, DbTransaction? transaction = default);

    /// <summary>
    /// Inserts a set of records of the given type to the table.
    /// By default, this implementation does not represent a bulk insert operation.
    /// </summary>
    /// <param name="items">The items to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of records affected.</returns>
    Task<int> InsertManyAsync(IEnumerable items, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Updates a single item. Key values must be correctly set in the passed object.
    /// </summary>
    /// <param name="item">The item to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The number of affected records.</returns>
    int UpdateOne(object item, DbTransaction? transaction = default);

    /// <summary>
    /// Updates a single item. Key values must be correctly set for the passed object.
    /// </summary>
    /// <param name="item">The item to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> UpdateOneAsync(object item, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Updates the provided set of items. Key values must be correctly set for the
    /// objects in the passed enumerable.
    /// </summary>
    /// <param name="items">The items to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The number of affected records.</returns>
    int UpdateMany(IEnumerable items, DbTransaction? transaction = default);

    /// <summary>
    /// Updates the provided set of items. Key values must be correctly set for the
    /// objects in the passed enumerable.
    /// </summary>
    /// <param name="items">The items to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> UpdateManyAsync(IEnumerable items, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Deletes the provided item. Key values must be correctly set for the passed object.
    /// </summary>
    /// <param name="item">The item to delete.</param>
    /// <param name="transaction">The associated transaction.</param>
    /// <returns>The number of affected records.</returns>
    int DeleteOne(object item, DbTransaction? transaction = default);

    /// <summary>
    /// Deletes the provided item. Key values must be correctly set for the passed object.
    /// </summary>
    /// <param name="item">The item to delete.</param>
    /// <param name="transaction">The associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> DeleteOneAsync(object item, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Deletes the provided set of items. Key values must be correctly set for the
    /// objects in the passed enumerable.
    /// </summary>
    /// <param name="items">The items to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The number of affected records.</returns>
    int DeleteMany(IEnumerable items, DbTransaction? transaction = default);

    /// <summary>
    /// Deletes the provided set of items. Key values must be correctly set for the
    /// objects in the passed enumerable.
    /// </summary>
    /// <param name="items">The items to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> DeleteManyAsync(IEnumerable items, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Builds a command and its parameters that can be used to insert
    /// a row of data into this table.
    /// </summary>
    /// <param name="transaction">An optional transaction.</param>
    /// <returns>The command.</returns>
    DbCommand BuildInsertCommand(DbTransaction? transaction = default);

    /// <summary>
    /// Creates a command where a data row is found via its key column values
    /// and updated based on object property values.
    /// </summary>
    /// <param name="transaction">The optional transaction.</param>
    /// <returns>The command.</returns>
    DbCommand BuildUpdateCommand(DbTransaction? transaction = default);

    /// <summary>
    /// Creates a command where a data row is found via its key column values
    /// and deleted based on object property values.
    /// </summary>
    /// <param name="transaction">The optional transaction.</param>
    /// <returns>The command.</returns>
    DbCommand BuildDeleteCommand(DbTransaction? transaction = default);

    /// <summary>
    /// Creates a command where a data row is retrieved via its key column values.
    /// </summary>
    /// <param name="transaction">The optional transaction.</param>
    /// <returns>The command.</returns>
    DbCommand BuildSelectCommand(DbTransaction? transaction = default);
}
