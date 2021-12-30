namespace Swan.Data;

/// <summary>
/// Provides extension methods for connected database commands.
/// </summary>
public static class DbCommandExtensions
{
    private const string CommandConnectionErrorMessage = $"The {nameof(IDbCommand)}.{nameof(IDbCommand.Connection)} cannot be null.";

    /// <summary>
    /// Tries to find a parameter within the command using the given name.
    /// The search is case-insensitive and the name can optionally start with a
    /// parameter prefix.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="command">The command to search.</param>
    /// <param name="name">The name to find.</param>
    /// <param name="parameter">The parameter output (if found).</param>
    /// <returns>True if the paramater was found. False otherwise.</returns>
    public static bool TryFindParameter<T>(this T command, string name, [MaybeNullWhen(false)] out IDbDataParameter parameter)
        where T : IDbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(CommandConnectionErrorMessage, nameof(command));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        var provider = command.Connection.Provider();
        var quotedName = provider.QuoteParameter(name);
        var unquotedName = provider.UnquoteParameter(name);

        parameter = default;
        foreach (IDbDataParameter p in command.Parameters)
        {
            var parameterName = p.ParameterName;

            if (string.IsNullOrWhiteSpace(parameterName))
                continue;

            if (string.Equals(unquotedName, parameterName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(quotedName, parameterName, StringComparison.OrdinalIgnoreCase))
            {
                parameter = p;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// If the given parameter name does not exist, adds the parameter with the specified name and value,
    /// atomatically specifying the parameter's <see cref="IDataParameter.DbType"/> if not provided.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="command">The command to add the parameter to.</param>
    /// <param name="name">The quoted or unquoted parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <param name="dbType">The optional database type.</param>
    /// <param name="direction">The parameter direction. Typically input.</param>
    /// <returns>The provided command for fluent API compatibility.</returns>
    public static T SetParameter<T>(this T command, string name, object? value, DbType? dbType = default, ParameterDirection direction = ParameterDirection.Input)
        where T : IDbCommand => command.SetParameterWithType(name, (value?.GetType() ?? typeof(DBNull)), value, dbType, direction);

    /// <summary>
    /// If the given parameter name does not exist, adds the parameter with the specified name and value,
    /// atomatically specifying the parameter's <see cref="IDataParameter.DbType"/> if not provided.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TValue">The CLR type of the value.</typeparam>
    /// <param name="command">The command to add the parameter to.</param>
    /// <param name="name">The quoted or unquoted parameter name.</param>
    /// <param name="value">The parameter value.</param>
    /// <param name="dbType">The optional database type.</param>
    /// <param name="direction">The parameter direction. Typically input.</param>
    /// <returns>The provided command for fluent API compatibility.</returns>
    public static TCommand SetParameter<TCommand, TValue>(this TCommand command, string name, TValue? value, DbType? dbType = default, ParameterDirection direction = ParameterDirection.Input)
        where TCommand : IDbCommand => command.SetParameterWithType(name, typeof(TValue), value, dbType, direction);

    /// <summary>
    /// This is the internal call to resolve the easy adding of command parameters.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="command">The command to add the parameter to.</param>
    /// <param name="name">The quoted or unquoted parameter name.</param>
    /// <param name="valueType">The enforced CLR type of the value.</param>
    /// <param name="value">The parameter value.</param>
    /// <param name="dbType">The optional database type.</param>
    /// <param name="direction">The parameter direction. Typically input.</param>
    /// <returns>The provided command for fluent API compatibility.</returns>
    private static T SetParameterWithType<T>(this T command, string name, Type valueType, object? value, DbType? dbType = default, ParameterDirection direction = ParameterDirection.Input)
        where T : IDbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(CommandConnectionErrorMessage, nameof(command));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(name);

        var isNullValue = Equals(value, null) || Equals(value, DBNull.Value);
        object dataValue = isNullValue ? DBNull.Value : value!;

        // Case 1: The parameter exists. Simply update the parameter value,
        // and if dbtype is specified, update it as well.
        if (command.TryFindParameter(name, out var parameter))
        {
            parameter.Value = dataValue;
            if (dbType.HasValue)
                parameter.DbType = dbType.Value;

            return command;
        }

        // Case 2: DbType was specified
        if (dbType.HasValue)
        {
            parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Direction = direction;
            parameter.DbType = dbType.Value;
            parameter.Value = dataValue;
            command.Parameters.Add(parameter);
            return command;
        }

        var provider = command.Connection.Provider();

        // Case 3: Can use AddWithValue via reflection and compiled delegate
        if (!isNullValue && provider.AddWithValueMethod != null)
        {
            parameter = provider.AddWithValueMethod!.Invoke(command.Parameters, name, dataValue);
            parameter.Direction = direction;
            return command;
        }

        // Case 4: Can use the type mapper
        if (provider.TypeMapper.TryGetDbTypeFor(valueType, out var mappedType) && mappedType.HasValue)
        {
            parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Direction = direction;
            parameter.DbType = mappedType.Value;
            parameter.Value = dataValue;
            command.Parameters.Add(parameter);
            return command;
        }

        // Case 5: Worst case, just use a string and convert the value to string
        parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Direction = direction;
        parameter.DbType = DbType.String;
        parameter.Value = isNullValue ? DBNull.Value : $"{dataValue}";
        command.Parameters.Add(parameter);

        return command;
    }


    public static TCommand SetParameters<TCommand, TValue>(this TCommand command, TValue parameters)
        where TCommand : IDbCommand
        where TValue : class
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(CommandConnectionErrorMessage, nameof(command));

        if (parameters is null)
            return command;

        var typeInfo = parameters.GetType().TypeInfo();
        var provider = command.Connection.Provider();

        foreach (var (propertyName, property) in typeInfo.Properties)
        {
            if (!property.CanRead || !property.HasPublicGetter || propertyName.Contains('.', StringComparison.Ordinal))
                continue;

            var parameterName = provider.QuoteParameter(propertyName);
            if (!string.IsNullOrWhiteSpace(command.CommandText) &&
                !command.CommandText.Contains(parameterName, StringComparison.InvariantCulture))
            {
                continue;
            }

            if (property.TryRead(parameters, out var value))
                command.SetParameterWithType(propertyName, property.PropertyType.NativeType, value);
        }

        return command;
    }

    /// <summary>
    /// Executes a data reader in the underlying stream as a single result-set
    /// and provides a foward-only enumerable set which can then be processed by
    /// iterating over items, one at a time.
    /// </summary>
    /// <typeparam name="T">The type of elements to return.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="behavior">The command behavior.</param>
    /// <param name="deserialize">The deserialization method used to produce the items based on the records.</param>
    /// <returns>An enumerable, forward-only data source.</returns>
    public static IEnumerable<T> Query<T>(this IDbCommand command, CommandBehavior behavior, Func<IDataReader, T> deserialize)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(CommandConnectionErrorMessage, nameof(command));

        if (deserialize == null)
            throw new ArgumentNullException(nameof(deserialize));

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
                    catch { /* don't spoil the existing exception */ }
                }
                reader.Dispose();
            }

            command.Parameters?.Clear();
            command.Dispose();
        }
    }

    public static IEnumerable<T> Query<T>(this IDbCommand command) =>
        command.Query(CommandBehavior.Default, (reader) => DataRecordExtensions.ExtractObject<T>(reader));

    public static IEnumerable<dynamic> Query(this IDbCommand command) =>
        command.Query(CommandBehavior.Default, (reader) => DataRecordExtensions.ExtractExpando(reader));

    private static IDataReader ExecuteOptimizedReader(this IDbCommand command, CommandBehavior requiredFlags = CommandBehavior.Default)
    {
        const CommandBehavior OptimizedBahavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult;
        IDataReader? reader = null;

        try
        {
            reader = command.ExecuteReader(OptimizedBahavior | requiredFlags);
            return reader;
        }
        catch (ArgumentException)
        {
            reader = command.ExecuteReader(requiredFlags);
            return reader;
        }
    }
}
