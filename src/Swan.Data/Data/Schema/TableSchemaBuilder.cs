namespace Swan.Data.Schema;

/// <summary>
/// A connected wrapper to create stand-alone Table DDL commands
/// based on types.
/// </summary>
public class TableSchemaBuilder : IConnected
{
    private readonly List<DbColumnSchema> _columns = new(64);

    internal TableSchemaBuilder(DbConnection connection, string tableName, string? schemaName = default)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentNullException(nameof(tableName));

        Connection = connection;
        Provider = Connection.Provider();
        Table = new(tableName, schemaName ?? Provider.DefaultSchemaName);
    }

    /// <summary>
    /// Gets the full table identifier. Name and schema name.
    /// </summary>
    public TableIdentifier Table { get; }

    /// <summary>
    /// Provides access to the associated columns.
    /// </summary>
    public IList<IDbColumnSchema> Columns => _columns.Cast<IDbColumnSchema>().ToList();

    /// <summary>
    /// Gets the associated provider.
    /// </summary>
    public DbProvider Provider { get; }

    /// <summary>
    /// Gets the associated connection.
    /// </summary>
    public DbConnection Connection { get; }

    /// <summary>
    /// Creates a DDL command to create the table if it does not exist.
    /// </summary>
    /// <returns>The DDL command.</returns>
    public DbCommand CreateDdlCommand()
    {
        if (Provider.Kind == ProviderKind.Unknown)
            throw new NotSupportedException("Cannot generate DDL code for unknown provider.");

        if (!_columns.Any())
            throw new InvalidOperationException("Cannot generate DDL code with no provided columns.");

        var schemaName = string.IsNullOrWhiteSpace(Table.Schema) ? Provider.DefaultSchemaName : Table.Schema;
        var quotedTableName = Provider.QuoteTable(Table.Name, schemaName);
        var orderedFields = _columns.OrderBy(c => c.Ordinal).ThenBy(c => c.Name);
        var builder = (Provider.Kind == ProviderKind.SqlServer
            ? new StringBuilder($"IF OBJECT_ID('{quotedTableName}') IS NULL\r\nCREATE TABLE {quotedTableName} (\r\n")
            : new StringBuilder($"CREATE TABLE IF NOT EXISTS {quotedTableName} (\r\n"))
            .Append(string.Join($",\r\n", orderedFields.Select(c => $"    {Provider.GetColumnDdlString(c)}").ToArray()));

        if (Provider.Kind == ProviderKind.MySql &&
            _columns.FirstOrDefault(c => c is IDbColumnSchema s && s.IsIdentity) is IDbColumnSchema identityCol)
        {
            var constraint = $",\r\n    PRIMARY KEY ({Provider.QuoteField(identityCol.Name)})";
            builder.Append(constraint);
        }

        builder.AppendLine("\r\n);");

        return Connection
            .BeginCommandText(builder.ToString())
            .EndCommandText();
    }

    /// <summary>
    /// Executes the DDL command that creates the table if it does not exist.
    /// </summary>
    /// <returns>The number of affected records.</returns>
    public int ExecuteDdlCommand()
    {
        using var command = CreateDdlCommand();
        return command.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes the DDL command that creates the table if it does not exist.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of affected records.</returns>
    public async Task<int> ExecuteDdlCommandAsync(CancellationToken ct = default)
    {
        await using var command = CreateDdlCommand();
        return await command.ExecuteNonQueryAsync(ct);
    }

    internal TableSchemaBuilder WithIdentity(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentNullException(nameof(columnName));

        foreach (var col in _columns)
        {
            col.IsKey = false;
            col.IsAutoIncrement = false;
        }

        var column = _columns.FirstOrDefault(c => c.Name.ToUpperInvariant().Equals(c.Name.ToUpperInvariant(), StringComparison.Ordinal));

        if (column is null)
        {
            column = new()
            {
                Name = columnName,
                DataType = typeof(int),
                Ordinal = _columns.Count
            };

            _columns.Add(column);
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

        var identityCandidate = _columns.Where(
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
        var column = _columns.FirstOrDefault(c => c.Name.ToUpperInvariant().Equals(columnName.ToUpperInvariant(), StringComparison.Ordinal));

        if (column is null)
        {
            column = new()
            {
                Ordinal = _columns.Count,
            };

            _columns.Add(column);
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

