namespace Swan.Data;

/// <summary>
/// Represents a table and schema that is bound to a specific connection.
/// </summary>
public interface ITableContext : IDbTable, IConnected
{
    // placeholder
}

public interface ITableContext<T> : ITableContext
    where T : class
{
    // placeholder
}
