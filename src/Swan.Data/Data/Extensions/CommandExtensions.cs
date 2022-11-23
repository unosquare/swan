#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
#pragma warning disable CA1031 // Do not catch general exception types
namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DbCommand"/> objects.
/// </summary>
public static class CommandExtensions
{
    /// <summary>
    /// Tries to prepare a command on the server side.
    /// Useful when executing the command multiple times by varying argument values.
    /// </summary>
    /// <typeparam name="T">The compatible command type.</typeparam>
    /// <param name="command">The command object.</param>
    /// <param name="exception">When prepare fails, the associated exception.</param>
    /// <returns>True if prepare succeeded. False otherwise.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exception is consumed by the API call user.")]
    public static bool TryPrepare<T>(this T command, [NotNullWhen(false)] out Exception? exception)
        where T : DbCommand
    {
        exception = null;

        if (command is null)
        {
            exception = new ArgumentNullException(nameof(command));
            return false;
        }

        try
        {
            command.Prepare();
            return true;
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }
    }

    /// <summary>
    /// Tries to prepare a command on the server side.
    /// Useful when executing a command multiple times by varying argument values.
    /// </summary>
    /// <typeparam name="T">The compatible command type.</typeparam>
    /// <param name="command">The command object.</param>
    /// <returns>True if prepare succeeded. False otherwise.</returns>
    public static bool TryPrepare<T>(this T command)
        where T : DbCommand => command.TryPrepare(out _);

    /// <summary>
    /// Tries to prepare a command on the server side.
    /// Useful when executing a command multiple times by varying argument values.
    /// </summary>
    /// <param name="command">The command object.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if prepare succeeded. False otherwise.</returns>
    public static async Task<bool> TryPrepareAsync(this DbCommand command, CancellationToken ct = default)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        try
        {
            await command.PrepareAsync(ct).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tries to find a parameter within the command parameter collection using the given name.
    /// The search is case-insensitive and the name can optionally start with a
    /// parameter prefix.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="command">The command to search.</param>
    /// <param name="name">The name to find.</param>
    /// <param name="parameter">The parameter output (if found).</param>
    /// <returns>True if the parameter was found. False otherwise.</returns>
    public static bool TryFindParameter<T>(this T command, string name, [MaybeNullWhen(false)] out IDbDataParameter parameter)
        where T : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(Library.CommandConnectionErrorMessage, nameof(command));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        var provider = command.Connection.Provider();
        var quotedName = provider.QuoteParameter(name);
        var unquotedName = provider.UnquoteParameter(name);

        parameter = command.GetNamedParameters().FirstOrDefault((kvp) =>
            kvp.Key.Equals(unquotedName, StringComparison.Ordinal) ||
            kvp.Key.Equals(quotedName, StringComparison.Ordinal)).Value;

        return parameter is not null;
    }

    /// <summary>
    /// Provides a way to iterate over parameters as key-value pairs.
    /// Keys are the parameter names.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>An enumerable source of parameters.</returns>
    public static IEnumerable<KeyValuePair<string, IDbDataParameter>> GetNamedParameters<T>(this T command)
        where T : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Parameters is null)
            yield break;

        foreach (var item in command.Parameters)
        {
            if (item is not IDbDataParameter p)
                continue;

            if (string.IsNullOrWhiteSpace(p.ParameterName))
                continue;

            yield return new(p.ParameterName, p);
        }
    }

    /// <summary>
    /// Adds or updates a parameter in the command's parameter collection without setting a value.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="name">The parameter name to add or update.</param>
    /// <param name="dbType">The database type.</param>
    /// <param name="direction">The direction.</param>
    /// <param name="size">The direction.</param>
    /// <param name="precision">The numeric precision.</param>
    /// <param name="scale">The numeric scale.</param>
    /// <param name="isNullable">Whether the parameter accepts database nulls.</param>
    /// <returns>The added or updated parameter object.</returns>
    public static IDbDataParameter DefineParameter(this DbCommand command, string name, DbType dbType,
        ParameterDirection direction = ParameterDirection.Input, int size = default, int precision = default, int scale = default, bool isNullable = default)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        var needsAdding = false;
        if (!command.TryFindParameter(name, out var parameter))
        {
            needsAdding = true;
            parameter = command.CreateParameter();
            parameter.ParameterName = command.Connection?.Provider().QuoteParameter(name) ?? name;
        }

        parameter.DbType = dbType;
        parameter.Direction = direction;
        parameter.Size = (size == default && dbType == DbType.String) ? 4000 : size;
        parameter.Precision = Convert.ToByte(precision.Clamp(0, byte.MaxValue));
        parameter.Scale = Convert.ToByte(scale.Clamp(0, byte.MaxValue));

        if (isNullable)
        {
            parameter.GetType().TypeInfo().TryWriteProperty(
                parameter, nameof(IDataParameter.IsNullable), true);
        }

        if (needsAdding)
            command.Parameters.Add(parameter);

        return parameter;
    }

    /// <summary>
    /// Adds or updates a parameter in the command's parameter collection without setting a value.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="name">The parameter name to add or update.</param>
    /// <param name="clrType">The CLR type that will map to the <see cref="DbType"/>.</param>
    /// <param name="direction">The direction.</param>
    /// <param name="size">The direction.</param>
    /// <param name="precision">The numeric precision.</param>
    /// <param name="scale">The numeric scale.</param>
    /// <param name="isNullable">Whether the parameter accepts database nulls.</param>
    /// <returns>The added or updated parameter object.</returns>
    public static IDbDataParameter DefineParameter(this DbCommand command, string name, Type clrType,
        ParameterDirection direction = ParameterDirection.Input, int size = default, int precision = default, int scale = default, bool isNullable = default)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(Library.CommandConnectionErrorMessage, nameof(command));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (clrType is null)
            throw new ArgumentNullException(nameof(clrType));

        var provider = command.Connection.Provider();
        if (!provider.TypeMapper.TryGetProviderTypeFor(clrType, out var dbType))
            dbType = DbType.String;

        return command.DefineParameter(name, dbType.GetValueOrDefault(DbType.String), direction, size, precision, scale, isNullable);
    }

    /// <summary>
    /// Adds or updates a parameter in the command's parameter collection without setting a value.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="column">The column information used to create or update the parameter definition.</param>
    /// <param name="direction">The optional parameter direction. The default is input.</param>
    /// <returns>The added or updated parameter object.</returns>
    public static IDbDataParameter DefineParameter(this DbCommand command, IDbColumnSchema column,
        ParameterDirection direction = ParameterDirection.Input) =>
        column is null
            ? throw new ArgumentNullException(nameof(column))
            : command.DefineParameter(column.ColumnName, column.DataType, direction,
                column.ColumnSize, column.NumericPrecision, column.NumericScale, column.AllowDBNull);

    /// <summary>
    /// Adds or updates multiple parameters in the command's parameter collection
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <param name="command"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TCommand DefineParameters<TCommand>(this TCommand command, IEnumerable<IDbColumnSchema> columns)
        where TCommand : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (columns is null)
            throw new ArgumentNullException(nameof(columns));

        foreach (var column in columns)
            command.DefineParameter(column);

        return command;
    }

    /// <summary>
    /// Adds or updates a parameter definition, and sets the parameter's value.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="command">The command.</param>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <param name="size">The parameter size.</param>
    /// <returns>The command with the updated parameter.</returns>
    public static TCommand SetParameter<TCommand, TValue>(this TCommand command, string name, TValue value, int? size = default)
        where TCommand : DbCommand => command.SetParameter(name, value, typeof(TValue), size);

    /// <summary>
    /// Adds or updates a parameter definition, and sets the parameter's value.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="command">The command.</param>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <param name="clrType">The CLR type that maps to the <see cref="DbType"/>.</param>
    /// <param name="size">The parameter size.</param>
    /// <returns>The command with the updated parameter.</returns>
    public static TCommand SetParameter<TCommand>(this TCommand command, string name, object? value, Type clrType, int? size = default)
        where TCommand : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(name);

        var isNullValue = Equals(value, null) || Equals(value, DBNull.Value);
        var dataValue = isNullValue ? DBNull.Value : value!;

        // Let's update the parameter if it already exists.
        if (command.TryFindParameter(name, out var parameter))
        {
            parameter.Value = dataValue;
            if (size.HasValue)
                parameter.Size = size.Value;

            return command;
        }

        parameter = command.DefineParameter(name, clrType, size: size.GetValueOrDefault());
        parameter.Value = dataValue;
        return command;
    }

    /// <summary>
    /// Adds or updates a parameter definition, and sets the parameter's value.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="command">The command.</param>
    /// <param name="name">The parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <param name="dbType">The database type of the parameter.</param>
    /// <param name="size">The parameter size.</param>
    /// <returns>The command with the updated parameter.</returns>
    public static TCommand SetParameter<TCommand>(this TCommand command, string name, object? value, DbType dbType, int? size = default)
        where TCommand : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(name);

        var isNullValue = Equals(value, null) || Equals(value, DBNull.Value);
        var dataValue = isNullValue ? DBNull.Value : value!;

        // Update the parameter if it exists.
        if (command.TryFindParameter(name, out var parameter))
        {
            parameter.Value = dataValue;
            parameter.DbType = dbType;

            if (size.HasValue)
                parameter.Size = size.Value;

            return command;
        }

        parameter = command.DefineParameter(name, dbType, size: size.GetValueOrDefault());
        parameter.Value = dataValue;
        return command;
    }

    /// <summary>
    /// Takes the given parameters object, extracts its publicly visible properties and values
    /// and adds the to the command's parameter collection. If the command text is set, it looks
    /// for the parameters within the command text before adding them.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="command">The command.</param>
    /// <param name="parameters">The parameters contained in an object as properties.</param>
    /// <param name="typeInfo">The option type of the parameters object.</param>
    /// <returns>The same command for fluent API support.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    internal static TCommand SetParameters<TCommand>(this TCommand command, object? parameters, ITypeInfo? typeInfo)
        where TCommand : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (parameters is null)
            return command;

        if (command.Connection is null)
            throw new ArgumentException(Library.CommandConnectionErrorMessage, nameof(command));

        typeInfo ??= parameters.GetType().TypeInfo();
        var provider = command.Connection.Provider();
        var columnMap = typeInfo.GetColumnMap();

        var hasCommandText = !string.IsNullOrWhiteSpace(command.CommandText);
        var commandText = hasCommandText
            ? command.CommandText.AsSpan()
            : Array.Empty<char>().AsSpan();

        foreach (var (columnName, property) in columnMap)
        {
            if (!property.CanRead)
                continue;

            if (!provider.TypeMapper.TryGetProviderTypeFor(property.PropertyType.NativeType, out _))
                continue;

            var parameterName = provider.QuoteParameter(columnName);
            var containsParameter = hasCommandText
                && commandText.Contains(parameterName, StringComparison.InvariantCultureIgnoreCase);

            if (hasCommandText && !containsParameter)
                continue;

            if (property.TryRead(parameters, out var value))
                command.SetParameter(parameterName, value, property.PropertyType.BackingType.NativeType);
        }

        return command;
    }

    /// <summary>
    /// Takes the given parameters object, extracts its publicly visible properties and values
    /// and adds the to the command's parameter collection. If the command text is set, it looks
    /// for the parameters within the command text before adding them.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="command">The command.</param>
    /// <param name="parameters">The parameters contained in an object as properties.</param>
    /// <returns>The same command for fluent API support.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static TCommand SetParameters<TCommand>(this TCommand command, object? parameters)
        where TCommand : DbCommand => command is null
            ? throw new ArgumentNullException(nameof(command))
            : command.SetParameters(parameters, null);

    /// <summary>
    /// Sets the command's basic properties. Properties with null values will not be set.
    /// Calling this method with default argument only will result in no modification of the
    /// command object.
    /// </summary>
    /// <typeparam name="TCommand">The compatible command type.</typeparam>
    /// <param name="command">The command object.</param>
    /// <param name="commandText">The optional command text.</param>
    /// <param name="commandType">The optional command type.</param>
    /// <param name="dbTransaction">The optional associated transaction.</param>
    /// <param name="timeout">The optional command timeout.</param>
    /// <returns>The modified command object.</returns>
    public static TCommand WithProperties<TCommand>(this TCommand command, string? commandText = default, CommandType? commandType = default, DbTransaction? dbTransaction = default, TimeSpan? timeout = default)
        where TCommand : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (commandType.HasValue)
            command.CommandType = commandType.Value;

        if (dbTransaction != default)
            command.Transaction = dbTransaction;

        if (timeout.HasValue)
            command.CommandTimeout = Convert.ToInt32(timeout.Value.TotalSeconds).ClampMin(0);

        if (!string.IsNullOrWhiteSpace(commandText))
            command.CommandText = commandText;

        return command;
    }

    /// <summary>
    /// Appends the specified text to the <see cref="DbCommand.CommandText"/>.
    /// Automatic spacing is enabled by default, and therefore, if the command text does not end with
    /// whitespace, it automatically adds a space between the existing command text and the appended
    /// one so you don't have to.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="command">The command to append text to.</param>
    /// <param name="text">The text to append.</param>
    /// <param name="autoSpace">The auto-spacing flag.</param>
    /// <returns>The command with the modified command text.</returns>
    public static TCommand AppendText<TCommand>(this TCommand command, string text, bool autoSpace = true)
        where TCommand : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.CommandText is null)
        {
            command.CommandText = text;
            return command;
        }

        var whitespace = autoSpace && command.CommandText.Length > 0 && !char.IsWhiteSpace(command.CommandText[0])
            ? " "
            : string.Empty;

        command.CommandText = $"{command.CommandText}{whitespace}{text}";
        return command;
    }

    /// <summary>
    /// Sets a command text to the provided command.
    /// </summary>
    /// <typeparam name="TCommand">The compatible command type.</typeparam>
    /// <param name="command">The command object.</param>
    /// <param name="commandText">The command text.</param>
    /// <returns>The modified command object.</returns>
    public static TCommand WithText<TCommand>(this TCommand command, string? commandText)
        where TCommand : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        command.CommandText = commandText ?? string.Empty;
        return command;
    }

    /// <summary>
    /// Sets a transaction (or null value) to the provided command.
    /// </summary>
    /// <typeparam name="TCommand">The compatible command type.</typeparam>
    /// <param name="command">The command object.</param>
    /// <param name="transaction">The transaction.</param>
    /// <returns>The modified command object.</returns>
    public static TCommand WithTransaction<TCommand>(this TCommand command, DbTransaction? transaction)
        where TCommand : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        command.Transaction = transaction;
        return command;
    }

    /// <summary>
    /// Sets a command execution timeout.
    /// The timeout includes both, execution of the command and
    /// transfer of the results packets over the network.
    /// </summary>
    /// <param name="command">The command object.</param>
    /// <param name="timeout">The timeout value.</param>
    /// <returns>The modified command object.</returns>
    public static TCommand WithTimeout<TCommand>(this TCommand command, TimeSpan timeout)
        where TCommand : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        command.CommandTimeout = Convert.ToInt32(timeout.TotalSeconds).ClampMin(0);
        return command;
    }

    /// <summary>
    /// Sets a command execution timeout.
    /// The timeout includes both, execution of the command and
    /// transfer of the results packets over the network.
    /// </summary>
    /// <param name="command">The command object.</param>
    /// <param name="seconds">The timeout value in seconds.</param>
    /// <returns>The modified command object.</returns>
    public static TCommand WithTimeout<TCommand>(this TCommand command, int seconds)
        where TCommand : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        command.CommandTimeout = seconds.ClampMin(0);
        return command;
    }

    /// <summary>
    /// Sets the command type. Typically text or stored procedure.
    /// </summary>
    /// <param name="command">The command object.</param>
    /// <param name="commandType">The command type.</param>
    /// <returns>The modified command object.</returns>
    public static TCommand WithCommandType<TCommand>(this TCommand command, CommandType commandType)
        where TCommand : DbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        command.CommandType = commandType;
        return command;
    }

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a forward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// The associated command is automatically disposed after iterating over the elements.
    /// </summary>
    /// <typeparam name="T">The type of elements to return.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="deserialize">The deserialization function used to produce the typed items based on the records.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<T> Query<T>(this DbCommand command,
        Func<IDataRecord, T>? deserialize = default,
        CommandBehavior behavior = CommandBehavior.Default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(Library.CommandConnectionErrorMessage, nameof(command));

        command.Connection.EnsureConnected();
        var typeInfo = typeof(T).TypeInfo();
        deserialize ??= (r) => r.ParseObject<T>(typeInfo);
        var reader = command.ExecuteOptimizedReader(behavior);

        try
        {
            if (reader.FieldCount == 0)
                yield break;

            while (reader.Read())
            {
                yield return deserialize(reader);
            }

            // skip the following result sets.
            while (reader.NextResult()) { }
            reader.Dispose();
            reader = null;
        }
        finally
        {
            if (reader != null)
            {
                if (!reader.IsClosed)
                {
                    try { command.Cancel(); }
                    catch { /* ignore */ }
                }
                reader.Dispose();
            }

            command.Parameters?.Clear();
            command.Dispose();
        }
    }

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a forward-only enumerable set which can then be processed by
    /// iterating over records, one at a time.
    /// The associated command is automatically disposed after iterating over the elements.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<dynamic> Query(this DbCommand command, CommandBehavior behavior = CommandBehavior.Default) =>
        command.Query((r) => r.ParseExpando(), behavior);

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a forward-only enumerable set which can then be processed by
    /// iterating over records, one at a time. The command is automatically disposed
    /// after the iteration completes.
    /// </summary>
    /// <typeparam name="T">The type of elements to return.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="deserialize">The deserialization function used to produce the typed items based on the records.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static async IAsyncEnumerable<T> QueryAsync<T>(this DbCommand command,
        Func<IDataRecord, T>? deserialize = default,
        CommandBehavior behavior = CommandBehavior.Default,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(Library.CommandConnectionErrorMessage, nameof(command));

        await command.Connection.EnsureConnectedAsync(ct).ConfigureAwait(false);
        var typeInfo = typeof(T).TypeInfo();
        deserialize ??= (r) => r.ParseObject<T>(typeInfo);
        var reader = await command.ExecuteOptimizedReaderAsync(behavior, ct).ConfigureAwait(false);

        try
        {
            if (reader.FieldCount <= 0)
                yield break;

            while (await reader.ReadAsync(ct).ConfigureAwait(false))
                yield return deserialize(reader);

            // Skip the following result sets.
            while (await reader.NextResultAsync(ct).ConfigureAwait(false)) { }

            // Gracefully dispose the reader.
            await reader.DisposeAsync().ConfigureAwait(false);
            reader = null;
        }
        finally
        {
            if (reader is not null)
            {
                if (!reader.IsClosed)
                {
                    try { command.Cancel(); }
                    catch { /* ignore */ }
                }

                await reader.DisposeAsync().ConfigureAwait(false);
            }

            command.Parameters?.Clear();
            await command.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// and provides a forward-only enumerable set which can then be processed by
    /// iterating over records, one at a time. The command is automatically disposed
    /// after the iteration completes.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static async IAsyncEnumerable<dynamic> QueryAsync(this DbCommand command,
        CommandBehavior behavior = CommandBehavior.Default,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var enumerable = command.QueryAsync((r) => r.ParseExpando(), behavior, ct);
        await foreach (var item in enumerable.WithCancellation(ct).ConfigureAwait(false))
            yield return item;
    }

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// containing a single row of data.
    /// The associated command is automatically disposed after returning the first element.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="deserialize">The deserialization function used to produce the typed items based on the records.</param>
    /// <returns>The parse object.</returns>
    public static T? FirstOrDefault<T>(this DbCommand command,
        Func<IDataRecord, T>? deserialize = default,
        CommandBehavior behavior = CommandBehavior.SingleRow) =>
        command.Query(deserialize, behavior).FirstOrDefault();

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result set
    /// containing a single row of data.
    /// The associated command is automatically disposed after returning the first element.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <returns>The parse object.</returns>
    public static dynamic? FirstOrDefault(this DbCommand command, CommandBehavior behavior = CommandBehavior.SingleRow) =>
        command.Query(behavior).FirstOrDefault();

    /// <summary>
    /// Retrieves the first result from a query command and parses it as an
    /// object of the given type.
    /// </summary>
    /// <typeparam name="T">The type of element to return.</typeparam>
    /// <param name="command">The command.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="deserialize">The deserialization callback.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The element to return or null of not found.</returns>
    public static async Task<T?> FirstOrDefaultAsync<T>(this DbCommand command,
        Func<IDataRecord, T>? deserialize = default,
        CommandBehavior behavior = CommandBehavior.SingleRow,
        CancellationToken ct = default)
    {
        var enumerable = command.QueryAsync(deserialize, behavior, ct);
        return await enumerable.FirstOrDefaultAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the first result from a query command and parses it as a
    /// <see cref="ExpandoObject"/>.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The element to return or null of not found.</returns>
    public static async Task<dynamic?> FirstOrDefaultAsync(this DbCommand command,
        CommandBehavior behavior = CommandBehavior.SingleRow,
        CancellationToken ct = default)
    {
        var enumerable = command.QueryAsync(behavior, ct);
        return await enumerable.FirstOrDefaultAsync(ct).ConfigureAwait(false);
    }
}

#pragma warning restore CA1031 // Do not catch general exception types
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
