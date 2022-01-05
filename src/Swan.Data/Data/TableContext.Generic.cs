namespace Swan.Data;

/// <summary>
/// A table context that maps between a given type and a data store.
/// </summary>
/// <typeparam name="T">The type this table context maps to.</typeparam>
internal class TableContext<T> : TableContext, ITableContext<T>
    where T : class
{
    public TableContext(IDbConnection connection, string tableName, string? schema = null)
        : base(connection, tableName, schema)
    {
        Deserializer = new((r) => r.ParseObject<T>());
    }

    /// <inheritdoc />
    public Func<IDataRecord, T>? Deserializer { get; set; }
}
