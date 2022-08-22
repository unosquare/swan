namespace Swan.Data.Context;

/// <summary>
/// Represents a table and schema that is bound to a specific connection
/// and that also maps to a specific CLR type.
/// </summary>
/// <typeparam name="T">The CLR type to map the table to.</typeparam>
public interface ITableContext<T> : ITableContext
    where T : class
{
    /// <summary>
    /// Specifies a callback function that turns a <see cref="IDataRecord"/>
    /// into an object of the mapped type.
    /// </summary>
    Func<IDataRecord, T> Deserializer { get; }

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a forward-only enumerable set which can then be processed by
    /// iterating over items, one at a time.
    /// </summary>
    /// <param name="trailingSql">The optional sql statements appended after the basic SELECT clause.</param>
    /// <param name="param">The optional parameters object.</param>
    /// <param name="transaction">The associated transaction.</param>
    /// <returns>The enumerable to iterate over.</returns>
    IEnumerable<T> Query(string? trailingSql = default, object? param = default, DbTransaction? transaction = default);

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a forward-only enumerable set which can then be processed by
    /// iterating over items, one at a time.
    /// </summary>
    /// <param name="trailingSql">The optional sql statements appended after the basic SELECT clause.</param>
    /// <param name="param">The optional parameters object.</param>
    /// <param name="transaction">The associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The enumerable to iterate over.</returns>
    IAsyncEnumerable<T> QueryAsync(string? trailingSql = default, object? param = default, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and a single row and returns the parsed object from the first row.
    /// </summary>
    /// <param name="trailingSql">The optional sql statements appended after the basic SELECT clause.</param>
    /// <param name="param">The optional parameters object.</param>
    /// <param name="transaction">The associated transaction.</param>
    /// <returns>The parsed object. A default value if no rows are retrieved.</returns>
    T? FirstOrDefault(string? trailingSql = default, object? param = default, DbTransaction? transaction = default);

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and a single row and returns the parsed object from the first row.
    /// </summary>
    /// <param name="trailingSql">The optional sql statements appended after the basic SELECT clause.</param>
    /// <param name="param">The optional parameters object.</param>
    /// <param name="transaction">The associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The parsed object. A default value if no rows are retrieved.</returns>
    Task<T?> FirstOrDefaultAsync(
        string? trailingSql = default, object? param = default, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Inserts an item of the given type to the database
    /// and if the table has defined a single, auto incremental key
    /// column (identity column), returns the inserted item.
    /// </summary>
    /// <param name="item">The item to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The newly inserted item whenever possible.</returns>
    T? InsertOne(T item, DbTransaction? transaction = default);

    /// <summary>
    /// Inserts an item of the given type to the database
    /// and if the table has defined a single, auto incremental key
    /// column (identity column), returns the inserted item.
    /// </summary>
    /// <param name="item">The item to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The newly inserted item whenever possible.</returns>
    Task<T?> InsertOneAsync(T item, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Inserts a set of records of the given type to the table.
    /// By default, this implementation does not represent a bulk insert operation.
    /// </summary>
    /// <param name="items">The items to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The number of records affected.</returns>
    int InsertMany(IEnumerable<T> items, DbTransaction? transaction = default);

    /// <summary>
    /// Inserts a set of records of the given type to the table.
    /// By default, this implementation does not represent a bulk insert operation.
    /// </summary>
    /// <param name="items">The items to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of records affected.</returns>
    Task<int> InsertManyAsync(IEnumerable<T> items, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Updates a single item. Key values must be correctly set in the passed object.
    /// </summary>
    /// <param name="item">The item to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The number of affected records.</returns>
    int UpdateOne(T item, DbTransaction? transaction = default);

    /// <summary>
    /// Updates a single item. Key values must be correctly set for the passed object.
    /// </summary>
    /// <param name="item">The item to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> UpdateOneAsync(T item, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Updates the provided set of items. Key values must be correctly set for the
    /// objects in the passed enumerable.
    /// </summary>
    /// <param name="items">The items to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The number of affected records.</returns>
    int UpdateMany(IEnumerable<T> items, DbTransaction? transaction = default);

    /// <summary>
    /// Updates the provided set of items. Key values must be correctly set for the
    /// objects in the passed enumerable.
    /// </summary>
    /// <param name="items">The items to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> UpdateManyAsync(IEnumerable<T> items, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Deletes the provided item. Key values must be correctly set for the passed object.
    /// </summary>
    /// <param name="item">The item to delete.</param>
    /// <param name="transaction">The associated transaction.</param>
    /// <returns>The number of affected records.</returns>
    int DeleteOne(T item, DbTransaction? transaction = default);

    /// <summary>
    /// Deletes the provided item. Key values must be correctly set for the passed object.
    /// </summary>
    /// <param name="item">The item to delete.</param>
    /// <param name="transaction">The associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> DeleteOneAsync(T item, DbTransaction? transaction = default, CancellationToken ct = default);

    /// <summary>
    /// Deletes the provided set of items. Key values must be correctly set for the
    /// objects in the passed enumerable.
    /// </summary>
    /// <param name="items">The items to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The number of affected records.</returns>
    int DeleteMany(IEnumerable<T> items, DbTransaction? transaction = default);

    /// <summary>
    /// Deletes the provided set of items. Key values must be correctly set for the
    /// objects in the passed enumerable.
    /// </summary>
    /// <param name="items">The items to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> DeleteManyAsync(IEnumerable<T> items, DbTransaction? transaction = default, CancellationToken ct = default);
}
