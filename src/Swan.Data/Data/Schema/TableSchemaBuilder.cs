namespace Swan.Data.Schema;

/// <summary>
/// A connected wrapper to create stand-alone Table DDL commands
/// based on types.
/// </summary>
public class TableSchemaBuilder : IConnected
{
    /// <summary>
    /// Creates a new instance of the <see cref="TableSchemaBuilder"/> class.
    /// </summary>
    /// <param name="connection">The connection object.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schemaName">The name of the schema.</param>
    internal TableSchemaBuilder(DbConnection connection, string tableName, string? schemaName = default)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentNullException(nameof(tableName));

        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        Connection = connection;
        Provider = Connection.Provider();
        Table = new DbTableSchema(connection.Database, tableName, schemaName ?? Provider.DefaultSchemaName);
    }

    /// <summary>
    /// Gets the table schema along with its columns.
    /// </summary>
    public IDbTableSchema Table { get; }

    /// <summary>
    /// Gets the associated provider.
    /// </summary>
    public DbProvider Provider { get; }

    /// <summary>
    /// Gets the associated connection.
    /// </summary>
    public DbConnection Connection { get; }

    /// <summary>
    /// Generates a provider-specific DDL command for thecurrent table schema.
    /// </summary>
    /// <returns>The database command that can be executed to crete the table.</returns>
    public DbCommand CreateDdlCommand() =>
        Provider.CreateTableDdlCommand(Connection, Table);

    /// <summary>
    /// Executes the DDL command that creates the table if it does not exist.
    /// </summary>
    /// <returns>The number of affected records.</returns>
    public int ExecuteDdlCommand()
    {
        Connection.EnsureConnected();
        using var command = Provider.CreateTableDdlCommand(Connection, Table);
        return command.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes the DDL command that creates the table if it does not exist.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    public async Task<int> ExecuteDdlCommandAsync(CancellationToken ct = default)
    {
        await Connection.EnsureConnectedAsync(ct);
        await using var command = Provider.CreateTableDdlCommand(Connection, Table);
        
        return await command.ExecuteNonQueryAsync(ct);
    }

    internal TableSchemaBuilder WithIdentity(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentNullException(nameof(columnName));

        foreach (DbColumnSchema col in Table.Columns)
        {
            col.IsKey = false;
            col.IsAutoIncrement = false;
        }

        if (Table[columnName] is not DbColumnSchema column)
        {
            column = new DbColumnSchema()
            {
                Name = columnName,
                DataType = typeof(int),
                Ordinal = Table.Columns.Count
            };

            Table.AddColumn(column);
        }

        column.IsKey = true;
        column.IsAutoIncrement = true;

        return this;
    }

    internal TableSchemaBuilder MapType(Type type)
    {
        var info = type.TypeInfo();

        foreach (var p in info.Properties.Values)
        {
            if (!p.CanRead || !p.CanWrite || !p.HasPublicGetter)
                continue;

            var dataType = p.PropertyType.BackingType.NativeType;
            var length = dataType == typeof(string)
                ? 512 : dataType == typeof(byte[])
                ? 4000 : 0;

            AddColumnInternal(p.PropertyName, p.PropertyType.NativeType, length, 0, 0);
        }

        var identityCandidate = Table.Columns.Where(
            c => !string.IsNullOrWhiteSpace(c.Name) &&
            c.Name.ToUpperInvariant().EndsWith("ID", StringComparison.Ordinal) &&
            c.DataType.TypeInfo().IsNumeric &&
            !c.DataType.TypeInfo().IsNullable)
            .OrderBy(c => c.Ordinal)
            .ThenBy(c => c.Name)
            .FirstOrDefault();

        return identityCandidate is not null
            ? WithIdentity(identityCandidate.Name)
            : this;
    }

    private TableSchemaBuilder AddColumnInternal(string name, Type type, int length, int precision, int scale)
    {
        var columnName = name.Trim();

        if (Table[columnName] is not DbColumnSchema column)
        {
            column = new DbColumnSchema()
            {
                Ordinal = Table.Columns.Count,
                Name = columnName,
            };

            Table.AddColumn(column);
        }

        var info = type.TypeInfo();
        column.Name = columnName;
        column.DataType = info.BackingType.NativeType;
        column.MaxLength = length;
        column.AllowsDBNull = info.IsNullable;
        column.Scale = Convert.ToByte(scale);
        column.Precision = Convert.ToByte(precision);

        return this;
    }
}

