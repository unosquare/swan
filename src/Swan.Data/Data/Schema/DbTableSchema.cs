namespace Swan.Data.Schema;

using Swan.Data.Extensions;

/// <summary>
/// Represents table structure information from the backing data store.
/// </summary>
internal class DbTableSchema : IDbTableSchema
{
    private const StringComparison TableNameComparison = StringComparison.OrdinalIgnoreCase;
    private const CommandBehavior SchemaTableBehavior = CommandBehavior.SchemaOnly | CommandBehavior.SingleResult | CommandBehavior.KeyInfo;
    private const CommandBehavior SchemaViewBehavior = CommandBehavior.SchemaOnly | CommandBehavior.SingleResult;

    /// <summary>
    /// Holds the column collection as a dictionary where column names are case-insensitive.
    /// </summary>
    private readonly Dictionary<string, IDbColumnSchema> _columnsByName = new(128, StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Holds the column collection as a List for index addressing.
    /// </summary>
    private readonly List<IDbColumnSchema> _columnList = new(128);

    /// <summary>
    /// Holds column names by index.
    /// </summary>
    private readonly Dictionary<string, int> _columnOrdinals = new(128, StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Creates a new instance of the <see cref="DbTableSchema"/> class.
    /// </summary>
    public DbTableSchema(string database, string tableName, string schema, IEnumerable<IDbColumnSchema>? columns = default)
    {
        if (string.IsNullOrWhiteSpace(database))
            throw new ArgumentNullException(nameof(database));

        Database = database;
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        Schema = schema ?? throw new ArgumentNullException(nameof(schema));

        if (columns is null)
            return;

        foreach (var column in columns)
            AddColumn(column);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="DbTableSchema"/> class.
    /// </summary>
    /// <param name="other">The table schema object to copy.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DbTableSchema(IDbTableSchema other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other));

        Database = other.Database;
        TableName = other.TableName;
        Schema = other.Schema;

        foreach (var column in other.Columns)
            AddColumn(column);
    }

    /// <inheritdoc />
    public IDbColumnSchema? this[string name] => _columnsByName.TryGetValue(name, out var column) ? column : null;

    /// <inheritdoc />
    public IDbColumnSchema? this[int index] => _columnList[index];

    /// <inheritdoc />
    public string Database { get; }

    /// <inheritdoc />
    public string Schema { get; }

    /// <inheritdoc />
    public string TableName { get; }

    /// <inheritdoc />
    public IReadOnlyList<IDbColumnSchema> Columns => _columnList;

    /// <inheritdoc />
    public IReadOnlyList<IDbColumnSchema> KeyColumns => _columnList.Where(c => c.IsKey).ToArray();

    /// <inheritdoc />
    public IDbColumnSchema? IdentityKeyColumn => _columnList.FirstOrDefault(c => c.IsKey && c.IsAutoIncrement);

    /// <inheritdoc />
    public bool HasKeyIdentityColumn => IdentityKeyColumn != null;

    /// <inheritdoc />
    public IReadOnlyList<IDbColumnSchema> InsertableColumns => _columnList
        .Where(c => !c.IsAutoIncrement && !c.IsReadOnly)
        .ToArray();

    /// <inheritdoc />
    public IReadOnlyList<IDbColumnSchema> UpdateableColumns => _columnList
        .Where(c => !c.IsKey && !c.IsAutoIncrement && !c.IsReadOnly)
        .ToArray();

    /// <inheritdoc />
    public int ColumnCount => _columnList.Count;

    /// <inheritdoc />
    public IDbTableSchema AddColumn(IDbColumnSchema column)
    {
        if (column is null)
            throw new ArgumentNullException(nameof(column));

        if (string.IsNullOrWhiteSpace(column.ColumnName))
            throw new ArgumentException("The column name is empty but it must be specified.", nameof(column));

        if (_columnsByName.ContainsKey(column.ColumnName))
            throw new ArgumentException($"A column with the same name '{column.ColumnName}' has already been added.", nameof(column));

        if (column.Clone() is not IDbColumnSchema columnCopy)
            throw new ArgumentException($"The {nameof(ICloneable.Clone)} method did not return a {nameof(IDbColumnSchema)}", nameof(column));

        _columnsByName[columnCopy.ColumnName] = columnCopy;
        _columnList.Add(columnCopy);

        var ordinal = _columnList.Count - 1;
        _columnOrdinals[columnCopy.ColumnName] = ordinal;
        if (columnCopy.ColumnOrdinal != ordinal)
            _ = columnCopy.GetType().TypeInfo().TryWriteProperty(columnCopy, nameof(IDbColumnSchema.ColumnOrdinal), ordinal);

        return this;
    }

    /// <inheritdoc />
    public IDbTableSchema RemoveColumn(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return this;

        var removalTarget = this[columnName];
        if (removalTarget is not null)
        {
            _columnList.Remove(removalTarget);
            _columnsByName.Remove(columnName);
            _columnOrdinals.Clear();

            for (var ordinal = 0; ordinal < _columnList.Count; ordinal++)
            {
                var column = _columnList[ordinal];
                _columnOrdinals[column.ColumnName] = ordinal;
                if (column.ColumnOrdinal != ordinal)
                    _ = column.GetType().TypeInfo().TryWriteProperty(column, nameof(IDbColumnSchema.ColumnOrdinal), ordinal);
            }
        }

        return this;
    }

    /// <inheritdoc />
    public int GetColumnOrdinal(string columnName) => string.IsNullOrWhiteSpace(columnName)
            ? throw new ArgumentNullException(nameof(columnName))
            : !_columnOrdinals.TryGetValue(columnName, out var index)
            ? throw new KeyNotFoundException($"Column with name '{columnName}' could not be found.")
            : index;

    /// <summary>
    /// Loads table schema information from a database connection.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The optional schema name.</param>
    /// <param name="transaction">The optional transaction.</param>
    /// <returns>A populated table schema.</returns>
    public static IDbTableSchema Load(DbConnection connection, string tableName, string? schema, DbTransaction? transaction = default)
    {
        connection.EnsureConnected();
        var provider = connection.Provider();
        schema ??= provider.DefaultSchemaName;

        TableIdentifier? targetTable = default;
        var useCacheOptions = new bool[] { true, false };
        foreach (var useCache in useCacheOptions)
        {
            var tables = connection.GetTableNames(transaction, useCache);
            targetTable = tables.FirstOrDefault(c => c.Name.Equals(tableName, TableNameComparison) &&
                (string.IsNullOrWhiteSpace(schema) || c.Schema.Equals(schema, TableNameComparison)));

            if (targetTable is not null)
                break;
        }

        targetTable ??= new(tableName, schema, false);

        using var schemaCommand = connection.BeginCommandText()
            .Select().Fields().From(targetTable.Name, schema).Where("1 = 2").EndCommandText()
            .WithTransaction(transaction);

        using var schemaReader = schemaCommand
            .ExecuteReader(targetTable.IsView ? SchemaViewBehavior : SchemaTableBehavior);

        using var schemaTable = schemaReader.GetSchemaTable();

        if (schemaTable is null)
            throw new InvalidOperationException("Could not retrieve table schema.");

        var columnInstance = provider.ColumnSchemaFactory.Invoke();
        var columnType = columnInstance.GetType().TypeInfo();
        var columns = schemaTable.Query(r => DeserializeColumn(r, columnType)).ToList();
        return new DbTableSchema(connection.Database, tableName, schema, columns);
    }

    /// <summary>
    /// Loads table schema information from a database connection.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schema">The optional schema name.</param>
    /// <param name="transaction">The optional transaction.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A populated table schema.</returns>
    public static async Task<IDbTableSchema> LoadAsync(DbConnection connection, string tableName, string? schema, DbTransaction? transaction = default, CancellationToken ct = default)
    {
        await connection.EnsureConnectedAsync(ct).ConfigureAwait(false);
        var provider = connection.Provider();
        schema ??= provider.DefaultSchemaName;

        TableIdentifier? targetTable = default;
        var useCacheOptions = new bool[] { true, false };
        foreach (var useCache in useCacheOptions)
        {
            var tables = await connection.GetTableNamesAsync(transaction, useCache, ct).ConfigureAwait(false);
            targetTable = tables.FirstOrDefault(c => c.Name.Equals(tableName, TableNameComparison) &&
                (string.IsNullOrWhiteSpace(schema) || c.Schema.Equals(schema, TableNameComparison)));

            if (targetTable is not null)
                break;
        }

        targetTable ??= new(tableName, schema, false);

        await using var schemaCommand = connection.BeginCommandText()
            .Select().Fields().From(targetTable.Name, schema).Where("1 = 2").EndCommandText()
            .WithTransaction(transaction);

        await using var schemaReader = await schemaCommand
            .ExecuteReaderAsync(targetTable.IsView ? SchemaViewBehavior : SchemaTableBehavior, ct);

        using var schemaTable = await schemaReader.GetSchemaTableAsync(ct);

        if (schemaTable is null)
            throw new InvalidOperationException("Could not retrieve table schema.");

        var columnInstance = provider.ColumnSchemaFactory.Invoke();
        var columnType = columnInstance.GetType().TypeInfo();
        var columns = schemaTable.Query(r => DeserializeColumn(r, columnType)).ToList();
        return new DbTableSchema(connection.Database, tableName, schema, columns);
    }

    private static IDbColumnSchema DeserializeColumn(DataRow columnInfo, ITypeInfo columnType) =>
        (columnInfo.ToDataRecord().ParseObject(columnType.NativeType, columnType.CreateInstance) as IDbColumnSchema)!;
}
