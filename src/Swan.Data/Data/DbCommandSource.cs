#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
namespace Swan.Data;

/// <summary>
/// A simple helper class to define commands that can be used in the context
/// of a specified connection. It's just a <see cref="StringBuilder" /> wrapper
/// with functionality that is useful to build SQL commands along with their basic
/// properties.
/// </summary>
public sealed class DbCommandSource : IDbConnected
{
    private StringBuilder? _commandText;
    private DbConnection? _connection;

    /// <summary>
    /// Creates a new instance of the <see cref="DbCommandSource"/> class.
    /// </summary>
    /// <param name="connection">The associated connection.</param>
    /// <param name="initialText">An optional initial command text.</param>
    internal DbCommandSource(DbConnection connection, string? initialText = default)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Provider = connection.Provider();

        _commandText = !string.IsNullOrWhiteSpace(initialText)
            ? new(initialText)
            : new();
    }

    /// <inheritdoc />
    public DbConnection Connection => _connection ?? throw new InvalidOperationException(Library.NoConnectionErrorMessage);

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
    /// Gets a value indicating whether the current command text is empty.
    /// </summary>
    public bool IsEmpty => (_commandText?.Length ?? 0) <= 0;

    /// <summary>
    /// Appends the specified text to the command. Automatically pre-appends
    /// a space if the current text does not end with a whitespace character.
    /// Nothing is appended if the text provided is null or whitespace.
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <returns>This instance for fluent API support.</returns>
    public DbCommandSource AppendText(string? text)
    {
        if (_commandText is null)
            throw new InvalidOperationException(Library.NoConnectionErrorMessage);

        if (string.IsNullOrWhiteSpace(text))
            return this;

        if (_commandText.Length > 0 && !char.IsWhiteSpace(_commandText[^1]))
            _commandText.Append(' ');

        _commandText.Append(text);
        return this;
    }

    /// <summary>
    /// Converts the current definition into a connection-bound <see cref="DbCommand"/> object.
    /// </summary>
    /// <returns>The actual command that can be executed.</returns>
    public DbCommand EndCommandText()
    {
        if (_connection is null)
            throw new InvalidOperationException(Library.NoConnectionErrorMessage);

        if (_commandText is null)
            throw new InvalidOperationException(Library.NoConnectionErrorMessage);

        var command = _connection.CreateCommand();
        command.Connection = _connection;
        command.CommandText = _commandText.ToString();
        command.CommandType = CommandType.Text;
        command.CommandTimeout = Convert.ToInt32(Provider.DefaultCommandTimeout.TotalSeconds).ClampMin(0);
        command.ConfigureAwait(false);

        _connection = null;
        _commandText.Clear();
        _commandText = null;
        return command;
    }
}
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
