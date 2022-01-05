namespace Swan.Data;

/// <summary>
/// Represents a table and schema that is bound to a specific connection.
/// </summary>
public interface ITableContext : IDbTableSchema, IConnected
{
    // placeholder
}

/// <summary>
/// Represents a table and schema that is bound to a specific connection
/// and that also maps to a specific CLR type.
/// </summary>
/// <typeparam name="T">The CLR type to map the table to.</typeparam>
public interface ITableContext<T> : ITableContext
    where T : class
{
    // placeholder
}
