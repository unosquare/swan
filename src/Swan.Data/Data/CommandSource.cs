namespace Swan.Data.Extensions;

/// <summary>
/// A simple helper class to define commands that can be used in the context
/// of a specified connection. It's just a <see cref="StringBuilder" /> wrapper
/// with functionality that is useful to build SQL commands along with their basic
/// properties.
/// </summary>
public sealed class CommandSource : IConnected
{
    private StringBuilder? _commandText = new();
    private IDbConnection _connection;

    /// <summary>
    /// Creates a new instance of the <see cref="CommandSource"/> class.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    internal CommandSource(IDbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        _connection = connection;
        Provider = connection.Provider();
        CommandType = CommandType.Text;
        CommandTimeout = TimeSpan.FromSeconds(Convert.ToInt32(Provider.DefaultCommandTimeout.TotalSeconds).ClampMin(0));
    }

    /// <inheritdoc />
    public IDbConnection Connection => _connection;

    /// <summary>
    /// Gets the resolved provider associated with the connection.
    /// </summary>
    public DbProvider Provider { get; }

    /// <summary>
    /// Gets the text contained up to this point in
    /// the internal string builder.
    /// </summary>
    public string CommandText => _commandText is null
        ? throw new ObjectDisposedException(nameof(_commandText))
        : _commandText.ToString();

    /// <summary>
    /// Gets the command type.
    /// </summary>
    public CommandType CommandType { get; private set; }

    /// <summary>
    /// Gets the command timeout.
    /// </summary>
    public TimeSpan CommandTimeout { get; private set; }

    /// <summary>
    /// Sets a command execution timeout. If no value is provided,
    /// the default provider timeout is applied.
    /// The timeout includes both, execution of the command and
    /// transfer of the results packets over the network.
    /// </summary>
    /// <param name="timeout">The timeout value.</param>
    /// <returns>This instance for fluent API support.</returns>
    public CommandSource WithTimeout(TimeSpan? timeout)
    {
        var value = timeout.HasValue ?
            timeout.Value.TotalSeconds :
            Provider.DefaultCommandTimeout.TotalSeconds;
        
        CommandTimeout = TimeSpan.FromSeconds(Convert.ToInt32(value).ClampMin(0));
        return this;
    }


    /// <summary>
    /// Sets the command type. Typically just text or stored procedure.
    /// If no value is provided, it's automatically set to text.
    /// </summary>
    /// <param name="commandType">The command type.</param>
    /// <returns>This instance for fluent API support.</returns>
    public CommandSource WithCommandType(CommandType? commandType)
    {
        CommandType = commandType ?? CommandType.Text;
        return this;
    }

    /// <summary>
    /// Clears the current command text and replaces it with the provided command text.
    /// If a null or empty string is provided, it simply clears the current text, setting
    /// the command text to an empty string.
    /// </summary>
    /// <param name="text">The sql text.</param>
    /// <returns>This instance for fluent API support.</returns>
    public CommandSource WithText(string? text)
    {
        if (_commandText is null)
            throw new ObjectDisposedException(nameof(_commandText));

        _commandText.Clear();
        if (string.IsNullOrWhiteSpace(text))
            return this;

        _commandText.Append(text);
        return this;
    }

    /// <summary>
    /// Appends the specified text to the command. Automatically pre-appends
    /// a space if the current text does not end with a whitespace character.
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <returns>This instance for fluent API support.</returns>
    public CommandSource AppendText(string text)
    {
        if (_commandText is null)
            throw new ObjectDisposedException(nameof(_commandText));

        if (_commandText.Length > 0 && !char.IsWhiteSpace(_commandText[^1]))
            _commandText.Append(' ');

        _commandText.Append(text);
        return this;
    }

    /// <summary>
    /// Converts the current definition into a connection-bound
    /// </summary>
    /// <param name="transaction"></param>
    /// <returns></returns>
    public IDbCommand FinishCommand(IDbTransaction? transaction = default)
    {
        if (_connection is null)
            throw new ObjectDisposedException(nameof(_connection));
        
        if (_commandText is null)
            throw new ObjectDisposedException(nameof(_commandText));

        var command = _connection.CreateCommand();
        command.CommandText = _commandText.ToString();
        command.CommandType = CommandType;
        command.CommandTimeout = Convert.ToInt32(CommandTimeout.TotalSeconds).ClampMin(0);
        if (transaction != null)
            command.Transaction = transaction;

        _connection = null;
        _commandText.Clear();
        _commandText = null;
        return command;
    }
}
