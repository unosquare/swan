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
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over items, one at a time.
    /// </summary>
    /// <param name="trailingSql">The optional sql statements appended after the basic SELECT clause.</param>
    /// <param name="param">The optional parameters object.</param>
    /// <returns>The enumerable to iterate over.</returns>
    IEnumerable<T> Query(string? trailingSql = default, object? param = default);

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over items, one at a time.
    /// </summary>
    /// <param name="trailingSql">The optional sql statements appended after the basic SELECT clause.</param>
    /// <param name="param">The optional parameters object.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The enumerable to iterate over.</returns>
    IAsyncEnumerable<T> QueryAsync(string? trailingSql = default, object? param = default, CancellationToken ct = default);

    /// <summary>
    /// Inserts an item of the given type to the database
    /// and if the table has defined a single, auto incremental key
    /// column (identity column), returns the inserted item.
    /// </summary>
    /// <param name="item">The item to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The newly inserted item whenever possible.</returns>
    T? InsertOne(T item, DbTransaction? transaction = null);

    /// <summary>
    /// Inserts an item of the given type to the database
    /// and if the table has defined a single, auto incremental key
    /// column (identity column), returns the inserted item.
    /// </summary>
    /// <param name="item">The item to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The newly inserted item whenever possible.</returns>
    Task<T?> InsertOneAsync(T item, DbTransaction? transaction = null, CancellationToken ct = default);

    /// <summary>
    /// Inserts a set of records of the given type to the table.
    /// By defualt, this implementation does not represent a bulk insert operation.
    /// </summary>
    /// <param name="items">The items to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The number of records affected.</returns>
    int InsertMany(IEnumerable<T> items, DbTransaction? transaction = null);

    /// <summary>
    /// Inserts a set of records of the given type to the table.
    /// By defualt, this implementation does not represent a bulk insert operation.
    /// </summary>
    /// <param name="items">The items to insert.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of records affected.</returns>
    Task<int> InsertManyAsync(IEnumerable<T> items, DbTransaction? transaction = null, CancellationToken ct = default);

    /// <summary>
    /// Updates a single item. Key values must be correctly set in the passed object.
    /// </summary>
    /// <param name="item">The item to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <returns>The number of affected records.</returns>
    int UpdateOne(T item, DbTransaction? transaction = null);

    /// <summary>
    /// Updates a single item. Key values must be correctly set in the passed object.
    /// </summary>
    /// <param name="item">The item to update.</param>
    /// <param name="transaction">The optional associated transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    Task<int> UpdateOneAsync(T item, DbTransaction? transaction = null, CancellationToken ct = default);

    /// <summary>
    /// Updates 
    /// </summary>
    /// <param name="items"></param>
    /// <param name="transaction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    int UpdateMany(IEnumerable<T> items, DbTransaction? transaction = null);

    bool TryFind(T key, out T item, DbTransaction? transaction = null);

    T DeleteOne(T item, DbTransaction? transaction = null);

    T DeleteMany(IEnumerable<T> items, DbTransaction? transaction = null);
}
