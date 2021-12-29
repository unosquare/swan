namespace Swan.Data;

using Swan.Extensions;

/// <summary>
/// Provides a fluent database command context to issue
/// commands to the associated table.
/// </summary>
public sealed class DbTableContext : IDisposable
{
    private const int DefaultSkip = default;
    private const int DefaultTake = int.MaxValue;

    private readonly ProviderMetadata Provider;
    private bool IsDisposed;
    private int _Skip = DefaultSkip;
    private int _Take = DefaultTake;

    /// <summary>
    /// Creates a new instance of the <see cref="DbTableContext"/> class.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="tableMeta"></param>
    internal DbTableContext(DbConnection connection, TableMetadata tableMeta)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        Provider = connection.Provider();
        Connection = connection;
        Table = tableMeta;
        Command = connection.CreateCommand();
    }

    /// <summary>
    /// Gets the associated table for this command.
    /// </summary>
    public TableMetadata Table { get; }

    /// <summary>
    /// Gets the command being built and that is ultimately issued via the connection.
    /// </summary>
    private DbCommand? Command { get; set; }

    /// <summary>
    /// Gets the associated connection.
    /// </summary>
    private DbConnection? Connection { get; set; }

    /// <summary>
    /// Associates the current command context to a transaction.
    /// </summary>
    /// <param name="transaction">The associated transaction</param>
    /// <returns>The fluent API context.</returns>
    public DbTableContext WithTransaction(DbTransaction transaction)
    {
        if (Command is null)
            throw new ObjectDisposedException(nameof(Command));

        Command.Transaction = transaction;
        return this;
    }

    /// <summary>
    /// Sets the command text via a method that provides table metadata.
    /// </summary>
    /// <param name="textFactory">The factory method that produces the command text.</param>
    /// <returns>A fluent API context.</returns>
    public DbTableContext WithText(Func<TableMetadata, string> textFactory)
    {
        if (Command is null)
            throw new ObjectDisposedException(nameof(Command));

        Command.CommandText = textFactory(Table);
        return this;
    }

    /// <summary>
    /// Sets the command text to an arbitary value.
    /// </summary>
    /// <param name="commandText"></param>
    /// <returns></returns>
    public DbTableContext WithText(string commandText)
    {
        if (Command is null)
            throw new ObjectDisposedException(nameof(Command));

        Command.CommandText = commandText ?? string.Empty;
        return this;
    }

    /// <summary>
    /// Sets the command timeout.
    /// </summary>
    /// <param name="timeout">The command timeout as a TimeSpan.</param>
    /// <returns>A fluent API context.</returns>
    public DbTableContext WithTimeout(TimeSpan timeout)
    {
        if (Command is null)
            throw new ObjectDisposedException(nameof(Command));

        Command.CommandTimeout = Convert.ToInt32(timeout.TotalSeconds);
        return this;
    }

    /// <summary>
    /// Sets the command timeout.
    /// </summary>
    /// <param name="timeout">The command timeout in seconds.</param>
    /// <returns>A fluent API context.</returns>
    public DbTableContext WithTimeout(int timeout)
    {
        if (Command is null)
            throw new ObjectDisposedException(nameof(Command));

        Command.CommandTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Adds or updates an existing parameter providing a name and value.
    /// </summary>
    /// <param name="name">The case-sensitive parameter name, with or without the prefix. We'll fix it for you.</param>
    /// <param name="value">The parameter name.</param>
    /// <param name="direction">The parameter direction.</param>
    /// <returns>A fluent API context.</returns>
    public DbTableContext WithParameter(string name, object? value, ParameterDirection direction = ParameterDirection.Input)
    {
        if (Command is null)
            throw new ObjectDisposedException(nameof(Command));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        var parameterName = NormalizeParameterName(name);
        if (!Command.CommandText.Contains(parameterName, StringComparison.Ordinal))
            return this;

        var parameter = FindParameter(parameterName);
        if (parameter != null)
        {
            parameter.Value = value ?? DBNull.Value;
            return this;
        }

        var columnNameSearch = !string.IsNullOrWhiteSpace(Provider.ParameterPrefix)
            ? parameterName[Provider.ParameterPrefix.Length..]
            : parameterName;

        var column = Table.ContainsKey(columnNameSearch)
            ? Table[columnNameSearch]
            : null;

        parameter = Command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Direction = direction;
        parameter.Value = value ?? DBNull.Value;

        if (column != null)
        {
            parameter.SourceColumn = column.ColumnName;
            parameter.Value = value == null
                ? DBNull.Value
                : !TypeManager.TryChangeType(value, column.DataType, out var convertedValue)
                    ? throw new InvalidCastException($"Unable to set {value} as {column.DataType} for column {column.ColumnName}")
                    : convertedValue;

            parameter.DbType = (DbType)column.NonVersionedProviderType;
        }

        Command.Parameters.Add(parameter);
        return this;
    }

    /// <summary>
    /// Adds or updates parameters by reading object properties.
    /// </summary>
    /// <param name="parametersObject">The object containing readable properties and matching parameter names.</param>
    /// <returns>A fluent API context.</returns>
    public DbTableContext WithParameters(object parametersObject)
    {
        if (parametersObject is null)
            throw new ArgumentNullException(nameof(parametersObject));

        var properties = parametersObject.GetType().Properties();
        foreach (var p in properties)
        {
            if (p.TryRead(parametersObject, out var value))
                WithParameter(p.PropertyName, value, ParameterDirection.Input);
        }

        return this;
    }

    /// <summary>
    /// Configures execution of the ToList methods to append a record offset clause
    /// to the existing command text upon execution.
    /// </summary>
    /// <param name="count">The number of records to skip or offset.</param>
    /// <returns>A fluent API context.</returns>
    public DbTableContext Skip(int count)
    {
        _Skip = count.ClampMin(0);
        return this;
    }

    /// <summary>
    /// Configures the execution of the ToList methods to append a limit clause
    /// to the existing command text upon execution.
    /// </summary>
    /// <param name="count">The number of records to return.</param>
    /// <returns>A fluent API context.</returns>
    public DbTableContext Take(int count)
    {
        _Take = count.ClampMin(1);
        return this;
    }

    /// <summary>
    /// Unbinds this command context from the connection, marks it as disposed
    /// and returns the current <see cref="DbCommand"/> that has been built so far.
    /// </summary>
    /// <returns>The current internal command of this context.</returns>
    public DbCommand Extract()
    {
        var command = Command;
        Command = null;
        Connection = null;
        IsDisposed = true;

        return command!;
    }

    /// <summary>
    /// Issues the command text (or a default SELECT) and materializes the records as objects.
    /// </summary>
    /// <typeparam name="T">The type to read the objects into.</typeparam>
    /// <param name="typeFactory">An optional factory method to create the object of the given type.</param>
    /// <returns>A list of objects from the set that was read.</returns>
    public async Task<IList<T>> ToListAsync<T>(Func<T>? typeFactory = default)
        where T : class
    {
        if (Command is null)
            throw new ObjectDisposedException(nameof(Command));

        typeFactory ??= () => (typeof(T).CreateInstance() as T)!;
        var skipTakeClause = GetSkipTakeClause();

        if (string.IsNullOrWhiteSpace(Command.CommandText))
        {
            Command.CommandText = $"SELECT * FROM {Table.QuotedName} {skipTakeClause}";
            Command.Parameters.Clear();
        }
        else
        {
            Command.CommandText = $"{Command.CommandText} {skipTakeClause}";
        }

        var isAutoSkipTake = !string.IsNullOrWhiteSpace(skipTakeClause);
        var limit = isAutoSkipTake ? DefaultTake : _Take;
        var offset = isAutoSkipTake ? DefaultSkip : _Skip;

        var result = new List<T>();
        var rowNumber = 0;

        try
        {
            using var reader = await Command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                rowNumber++;
                if (rowNumber <= offset)
                    continue;

                result.Add(reader.ReadObject(typeFactory));

                if (result.Count >= limit)
                    break;
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            Dispose();
        }

        return result;
    }

    /// <summary>
    /// Issues the command text (or a default SELECT) and materializes the records as
    /// dynamic <see cref="ExpandoObject"/> objects..
    /// </summary>
    /// <returns>A list of objects from the set that was read.</returns>
    public async Task<IList<dynamic>> ToListAsync()
    {
        if (Command is null)
            throw new ObjectDisposedException(nameof(Command));

        var skipTakeClause = GetSkipTakeClause();

        if (string.IsNullOrWhiteSpace(Command.CommandText))
        {
            Command.CommandText = $"SELECT * FROM {Table.QuotedName} {skipTakeClause}";
            Command.Parameters.Clear();
        }
        else
        {
            Command.CommandText = $"{Command.CommandText} {skipTakeClause}";
        }

        var isAutoSkipTake = !string.IsNullOrWhiteSpace(skipTakeClause);
        var limit = isAutoSkipTake ? DefaultTake : _Take;
        var offset = isAutoSkipTake ? DefaultSkip : _Skip;

        var result = new List<dynamic>();
        var rowNumber = 0;

        try
        {
            using var reader = await Command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                rowNumber++;
                if (rowNumber <= offset)
                    continue;

                result.Add(reader.ReadObject());

                if (result.Count >= limit)
                    break;
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            Dispose();
        }

        return result;
    }

    /// <summary>
    /// Issues the command text (or a default SELECT) and materializes the first records as an object.
    /// </summary>
    /// <typeparam name="T">The type to read the objects into.</typeparam>
    /// <param name="typeFactory">An optional factory method to create the object of the given type.</param>
    /// <returns>An object of the given type or null if no objects were retrieved.</returns>
    public async Task<T?> FirstOrDefaultAsync<T>(Func<T>? typeFactory = default)
        where T : class
    {
        Skip(0).Take(1);
        var result = await ToListAsync(typeFactory);
        return result.Count > 0 ? result[0] : default;
    }

    /// <summary>
    /// Issues the command text (or a default SELECT) and materializes the first records as a
    /// dynamic <see cref="ExpandoObject"/> object.
    /// </summary>
    /// <returns>An dynamic object or null if no objects were retrieved.</returns>
    public async Task<dynamic?> FirstOrDefaultAsync()
    {
        Skip(0).Take(1);
        var result = await ToListAsync();
        return result.Count > 0 ? result[0] : default;
    }

    public async Task<T?> FirstOrDefaultAsync<T>(object keyObject)
        where T : class
    {
        Skip(0).Take(1);
        var keyColumnClause = string.Join(" AND ",
            Table.KeyColumns.Select(c => $"{Provider.QuotePrefix}{c.ColumnName}{Provider.QuoteSuffix} = {Provider.ParameterPrefix}{c.ColumnName}"));
        Command.CommandText = $"SELECT * FROM {Table.QuotedName} WHERE {keyColumnClause} {GetSkipTakeClause()}";
        WithParameters(keyObject);

        return await FirstOrDefaultAsync<T>();
    }

    public async Task<dynamic> FirstOrDefaultAsync(object keyObject)
    {
        Skip(0).Take(1);
        var keyColumnClause = string.Join(" AND ",
            Table.KeyColumns.Select(c => $"{Provider.QuotePrefix}{c.ColumnName}{Provider.QuoteSuffix} = {Provider.ParameterPrefix}{c.ColumnName}"));
        Command.CommandText = $"SELECT * FROM {Table.QuotedName} WHERE {keyColumnClause} {GetSkipTakeClause()}";
        WithParameters(keyObject);

        return await FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteAsync(object keyObject)
    {
        var keyObjects = new[] { keyObject };
        return await DeleteManyAsync(keyObjects) > 0;
    }

    public async Task<int> DeleteManyAsync(IEnumerable keyObjects)
    {
        var keyColumnClause = string.Join(" AND ",
            Table.KeyColumns.Select(c => $"{Provider.QuotePrefix}{c.ColumnName}{Provider.QuoteSuffix} = {Provider.ParameterPrefix}{c.ColumnName}"));

        Command.CommandText = $"DELETE FROM {Table.QuotedName} WHERE {keyColumnClause}";
        var hasTriedPrepare = false;
        var affected = 0;

        try
        {
            foreach (var keyObject in keyObjects)
            {
                WithParameters(keyObject);

                if (!hasTriedPrepare)
                {
                    await TryPrepareCommand();
                    hasTriedPrepare = true;
                }

                affected += await Command.ExecuteNonQueryAsync();
            }
        }
        finally
        {
            Dispose();
        }

        return affected;
    }

    public async Task<T> InsertAsync<T>(T item)
        where T : class
    {
        var colNames = Table.Values
            .Where(c => !c.IsAutoIncrement && !c.IsIdentity && !c.IsReadOnly && !c.IsExpression)
            .OrderBy(c => c.ColumnOrdinal)
            .Select(c => c.ColumnName)
            .ToArray();

        var fields = string.Join(", ", colNames.Select(c => $"{Provider.QuotePrefix}{c}{Provider.QuoteSuffix}"));
        var values = string.Join(", ", colNames.Select(c => $"{Provider.ParameterPrefix}{c}"));

        Command.CommandText = $"INSERT INTO {Table.QuotedName} ({fields}) OUTPUT INSERTED.* VALUES ({values})";
        WithParameters(item);
        return await FirstOrDefaultAsync<T>();
    }

    public async Task<int> UpdateManyAsync<T>(IEnumerable<T> items)
        where T : new()
    {
        var keyColumns = Table.KeyColumns.Select(c => c.ColumnName);
        var keyColumnClause = string.Join(" AND ",
            Table.KeyColumns.Select(c => $"{Provider.QuotePrefix}{c.ColumnName}{Provider.QuoteSuffix} = {Provider.ParameterPrefix}{c.ColumnName}"));

        var targetColumns = Table.Values.Where(c => !keyColumns.Contains(c.ColumnName) && !c.IsExpression && !c.IsReadOnly);
        var targetColumnCaluse = string.Join(", ",
            targetColumns.Select(c => $"{c.ColumnName} = {Provider.ParameterPrefix}{c.ColumnName}"));

        Command.CommandText = $"UPDATE {Table.QuotedName} SET {targetColumnCaluse} WHERE {keyColumnClause}";

        var prepared = false;
        var affected = 0;
        foreach (var item in items)
        {
            WithParameters(item);

            if (!prepared)
            {
                await TryPrepareCommand();
                prepared = true;
            }

            affected += await Command.ExecuteNonQueryAsync();
        }

        return affected;
    }

    public async Task<bool> UpdateAsync<T>(T item)
        where T : new() =>
        await UpdateManyAsync(new[] { item }) > 0;

    /// <summary>
    /// Trims the provided name and adds the corresponding
    /// <see cref="TableMetadata.ParameterPrefix"/>
    /// </summary>
    /// <param name="name">The name identifier.</param>
    /// <returns></returns>
    private string NormalizeParameterName(string name)
    {
        return string.IsNullOrWhiteSpace(Provider.ParameterPrefix)
            ? name.Trim()
            : !name.StartsWith(Provider.ParameterPrefix, StringComparison.Ordinal)
            ? $"{Provider.ParameterPrefix}{name.Trim()}"
            : name.Trim();
    }

    private string GetSkipTakeClause()
    {
        var builder = new StringBuilder(256);

        if (_Skip == DefaultSkip && _Take == DefaultTake)
            return string.Empty;

        switch (Provider.Kind)
        {
            case ProviderKind.SqlServer:
                if (!Command.CommandText.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase))
                    builder.Append(" ORDER BY CURRENT_TIMESTAMP ");
                builder.Append(CultureInfo.InvariantCulture, $"OFFSET {_Skip} ROWS FETCH NEXT {_Take} ROWS ONLY");
                break;
            case ProviderKind.MySql:
            case ProviderKind.Sqlite:
                builder.Append(CultureInfo.InvariantCulture, $"LIMIT {_Take} OFFSET {_Skip}");
                break;
        }

        return builder.ToString().Trim();
    }

    private IDataParameter? FindParameter(string name)
    {
        var parameterName = NormalizeParameterName(name);
        foreach (IDataParameter queryParameter in Command.Parameters)
        {
            if (queryParameter.ParameterName == parameterName)
                return queryParameter;
        }

        return null;
    }

    private async Task<bool> TryPrepareCommand()
    {
        try
        {
            await Command.PrepareAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheridoc />
    public void Dispose()
    {
        if (IsDisposed)
            return;

        try
        {
            Command?.Parameters?.Clear();
            Command?.Dispose();
            Command = null;
            Connection = null;
        }
        finally
        {
            IsDisposed = true;
        }
    }
}
