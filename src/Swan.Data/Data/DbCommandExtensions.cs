namespace Swan.Data;

public static class DbCommandExtensions
{
    public static T AddParameter<T>(this T command, string name, object? value, DbType? dbType = default, ParameterDirection direction = ParameterDirection.Input)
        where T : IDbCommand
    {
        if (command is null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection is null)
            throw new ArgumentException($"Parameter {nameof(command)} must have an associated {nameof(IDbConnection)}", nameof(command));

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
            var provider = command.Connection.Provider();

            if (dbType.HasValue)
            {
                parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Direction = direction;
                parameter.DbType = dbType.Value;
                parameter.Value = value ?? DBNull.Value;
                command.Parameters.Add(parameter);
                return command;
            }
            
            if (value != null && provider.AddWithValueMethod != null)
            {
                provider.AddWithValueMethod(command.Parameters, name, value);
                if (command.Parameters[name] is IDbDataParameter p)
                    p.Direction = direction;

                return command;
            }

            if (value != null && provider.TypeMapper.TryGetDbTypeFor(value.GetType(), out var mappedType) && mappedType.HasValue)
            {
                parameter = command.CreateParameter();
                parameter.ParameterName = name;
                parameter.Direction = direction;
                parameter.DbType = mappedType.Value;
                parameter.Value = value;
                command.Parameters.Add(parameter);
                return command;
            }
        }

        
        return command;
    }

    public static IEnumerable<T> Query<T>(this IDbCommand command)
    {
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            yield return reader.ReadObject<T>();
        }
    }

    public static IEnumerable<dynamic> Query(this IDbCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        if (command.Connection == null)
            throw new ArgumentException("Command does not have an associated connection.", nameof(command));

        var reader = command.ExecuteOptimizedReader();

        try
        {
            if (reader.FieldCount == 0)
                yield break;

            while (reader.Read())
            {
                yield return reader.ReadObject();
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

