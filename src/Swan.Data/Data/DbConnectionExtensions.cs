namespace Swan.Data;

/// <summary>
/// Provides <see cref="DbConnection"/> extension methods.
/// </summary>
public static class DbConnectionExtensions
{
    public static ProviderMetadata Provider(this DbConnection connection) => connection is null
        ? throw new ArgumentNullException(nameof(connection))
        : ProviderMetadata.FromConnection(connection);

    public static async Task<IReadOnlyList<string>> TableNames(this DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        await connection.EnsureIsValidAsync();

        var tables = new List<string>();
        var dt = connection.GetSchema("Tables");
        foreach (DataRow row in dt.Rows)
        {
            string tablename = (string)row[2];
            tables.Add(tablename);
        }
        return tables;
    }

    public static async Task<DbTableContext> TableCommandAsync(this DbConnection connection, string tableName, string? schemaName = default)
    {
        var tableMeta = await TableMetadata.AcquireAsync(connection, tableName, schemaName);
        await connection.EnsureIsValidAsync();
        return new DbTableContext(connection, tableMeta);
    }

    public static DbTableContext TableCommand(this DbConnection connection, string tableName, string? schemaName = default) =>
        connection.TableCommandAsync(tableName, schemaName).ConfigureAwait(false).GetAwaiter().GetResult();

    /// <summary>
    /// Ensures the connection state is open and that the <see cref="DbConnection.Database"/> property has been set.
    /// </summary>
    /// <param name="connection">The connection to check for validity.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An awaitable task.</returns>
    public static async Task EnsureIsValidAsync(this DbConnection connection, CancellationToken ct = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct);

        if (string.IsNullOrWhiteSpace(connection.Database))
            throw new InvalidOperationException($"{nameof(connection)}.{nameof(connection.Database)} must be set.");
    }

    /// <summary>
    /// Ensures the connection state is open and that the <see cref="DbConnection.Database"/> property has been set.
    /// </summary>
    /// <param name="connection">The connection to check for validity.</param>
    public static void EnsureIsValid(this DbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        if (connection.State != ConnectionState.Open)
            connection.Open();

        if (string.IsNullOrWhiteSpace(connection.Database))
            throw new InvalidOperationException($"{nameof(connection)}.{nameof(connection.Database)} must be set.");
    }

    public static CommandDefinition BeginCommand(this DbConnection connection) => connection is null
        ? throw new ArgumentNullException(nameof(connection))
        : new(connection);

    public static T AddParameter<T>(this T command, string name, object? value, DbType? dbType = default, ParameterDirection direction = ParameterDirection.Input)
        where T : IDbCommand
    {
        IDbDataParameter? parameter = default;
        foreach (IDbDataParameter param in command.Parameters)
        {
            if (param.ParameterName == name)
            {
                parameter = param;
                break;
            }
        }

        if (parameter == null)
        {
            parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Direction = direction;

            if (dbType.HasValue)
                parameter.DbType = dbType.Value;
            else if (value != null && TypeMap.TryGetValue(value.GetType(), out var mappedType) && mappedType.HasValue)
                parameter.DbType = mappedType.Value;

            command.Parameters.Add(parameter);
        }

        parameter.Value = value ?? DBNull.Value;
        return command;
    }

    private static readonly IReadOnlyDictionary<Type, DbType?> TypeMap = new Dictionary<Type, DbType?>(37)
    {
        [typeof(byte)] = DbType.Byte,
        [typeof(sbyte)] = DbType.SByte,
        [typeof(short)] = DbType.Int16,
        [typeof(ushort)] = DbType.UInt16,
        [typeof(int)] = DbType.Int32,
        [typeof(uint)] = DbType.UInt32,
        [typeof(long)] = DbType.Int64,
        [typeof(ulong)] = DbType.UInt64,
        [typeof(float)] = DbType.Single,
        [typeof(double)] = DbType.Double,
        [typeof(decimal)] = DbType.Decimal,
        [typeof(bool)] = DbType.Boolean,
        [typeof(string)] = DbType.String,
        [typeof(char)] = DbType.StringFixedLength,
        [typeof(Guid)] = DbType.Guid,
        [typeof(DateTime)] = DbType.DateTime2,
        [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
        [typeof(TimeSpan)] = null,
        [typeof(byte[])] = DbType.Binary,
        [typeof(byte?)] = DbType.Byte,
        [typeof(sbyte?)] = DbType.SByte,
        [typeof(short?)] = DbType.Int16,
        [typeof(ushort?)] = DbType.UInt16,
        [typeof(int?)] = DbType.Int32,
        [typeof(uint?)] = DbType.UInt32,
        [typeof(long?)] = DbType.Int64,
        [typeof(ulong?)] = DbType.UInt64,
        [typeof(float?)] = DbType.Single,
        [typeof(double?)] = DbType.Double,
        [typeof(decimal?)] = DbType.Decimal,
        [typeof(bool?)] = DbType.Boolean,
        [typeof(char?)] = DbType.StringFixedLength,
        [typeof(Guid?)] = DbType.Guid,
        [typeof(DateTime?)] = DbType.DateTime2,
        [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
        [typeof(TimeSpan?)] = null,
        [typeof(object)] = DbType.Object
    };
}
