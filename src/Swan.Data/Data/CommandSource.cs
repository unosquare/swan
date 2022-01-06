namespace Swan.Data;

/// <summary>
/// A simple helper class to define commands that can be used in the context
/// of a specified connection. It's just a <see cref="StringBuilder" /> wrapper
/// with functionality that is useful to build SQL commands along with their basic
/// properties.
/// </summary>
public sealed class CommandSource : IConnected
{
    private StringBuilder? _commandText;
    private IDbConnection? _connection;

    /// <summary>
    /// Creates a new instance of the <see cref="CommandSource"/> class.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="initialText">An optional initial command text.</param>
    internal CommandSource(IDbConnection connection, string? initialText = default)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        _connection = connection;
        Provider = connection.Provider();

        _commandText = !string.IsNullOrWhiteSpace(initialText)
            ? new(initialText)
            : new();
    }

    /// <inheritdoc />
    public IDbConnection Connection => _connection ?? throw new InvalidOperationException(Library.NoConnectionErrorMessage);

    /// <summary>
    /// Gets the resolved provider associated with the connection.
    /// </summary>
    public DbProvider Provider { get; }

    /// <summary>
    /// Gets the text contained up to this point in
    /// the internal string builder.
    /// </summary>
    public string CommandText => _commandText is null
        ? throw new InvalidOperationException(Library.NoConnectionErrorMessage)
        : _commandText.ToString();

    /// <summary>
    /// Appends the specified text to the command. Automatically pre-appends
    /// a space if the current text does not end with a whitespace character.
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <returns>This instance for fluent API support.</returns>
    public CommandSource AppendText(string text)
    {
        if (_commandText is null)
            throw new InvalidOperationException(Library.NoConnectionErrorMessage);

        if (_commandText.Length > 0 && !char.IsWhiteSpace(_commandText[^1]))
            _commandText.Append(' ');

        _commandText.Append(text);
        return this;
    }

    /// <summary>
    /// Converts the current definition into a connection-bound <see cref="IDbCommand"/> object.
    /// </summary>
    /// <returns>The actual command that can be executed.</returns>
    public IDbCommand EndCommandText()
    {
        if (_connection is null)
            throw new InvalidOperationException(Library.NoConnectionErrorMessage);
        
        if (_commandText is null)
            throw new InvalidOperationException(Library.NoConnectionErrorMessage);

        var command = _connection.CreateCommand();
        command.CommandText = _commandText.ToString();
        command.CommandType = CommandType.Text;
        command.CommandTimeout = Convert.ToInt32(Provider.DefaultCommandTimeout.TotalSeconds).ClampMin(0);

        _connection = null;
        _commandText.Clear();
        _commandText = null;
        return command;
    }
}
