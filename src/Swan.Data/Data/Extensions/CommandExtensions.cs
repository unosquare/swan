namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IDbCommand"/> objects.
/// </summary>
public static partial class CommandExtensions
{
    /// <summary>
    /// Tries to find a parameter within the command parameter collection using the given name.
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
            throw new ArgumentException(Library.CommandConnectionErrorMessage, nameof(command));

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
        where T : IDbCommand => command.SetParameter(name, (value?.GetType() ?? typeof(DBNull)), value, dbType, direction);

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
        where TCommand : IDbCommand => command.SetParameter(name, typeof(TValue), value, dbType, direction);

    /// <summary>
    /// If the given parameter name does not exist, adds the parameter with the specified name and value,
    /// atomatically specifying the parameter's <see cref="IDataParameter.DbType"/> if not provided.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    /// <param name="command">The command to add the parameter to.</param>
    /// <param name="name">The quoted or unquoted parameter name.</param>
    /// <param name="valueType">The enforced CLR type of the value.</param>
    /// <param name="value">The parameter value.</param>
    /// <param name="dbType">The optional database type.</param>
    /// <param name="direction">The parameter direction. Typically input.</param>
    /// <returns>The provided command for fluent API compatibility.</returns>
    public static T SetParameter<T>(this T command, string name, Type valueType, object? value, DbType? dbType = default, ParameterDirection direction = ParameterDirection.Input)
        where T : IDbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(Library.CommandConnectionErrorMessage, nameof(command));

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

    /// <summary>
    /// Takes the given parameters object, extracts its publicly visible properties and values
    /// and adds the to the command's parameter collection. If the command text is set, it looks
    /// for the parameters within the command text before adding them.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <param name="command"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static TCommand SetParameters<TCommand>(this TCommand command, object parameters)
        where TCommand : IDbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException(Library.CommandConnectionErrorMessage, nameof(command));

        if (parameters is null)
            return command;

        var typeInfo = parameters.GetType().TypeInfo();
        var provider = command.Connection.Provider();

        foreach (var (propertyName, property) in typeInfo.Properties)
        {
            if (!property.CanRead || !property.HasPublicGetter || !property.PropertyType.IsBasicType ||
                propertyName.Contains('.', StringComparison.Ordinal))
                continue;

            var parameterName = provider.QuoteParameter(propertyName);
            if (!string.IsNullOrWhiteSpace(command.CommandText) &&
                !command.CommandText.Contains(parameterName, StringComparison.InvariantCulture))
            {
                continue;
            }

            if (property.TryRead(parameters, out var value))
                command.SetParameter(propertyName, property.PropertyType.NativeType, value);
        }

        return command;
    }

    public static TCommand WithTransaction<TCommand>(this TCommand command, IDbTransaction? transaction)
        where TCommand : IDbCommand
    {
        command.Transaction = transaction;
        return command;
    }


}
