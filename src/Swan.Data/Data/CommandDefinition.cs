namespace Swan.Data;

using Swan.Extensions;

public sealed class CommandDefinition
{
    private readonly DbProvider _provider;

    private IDbCommand _command;
    private StringBuilder _commandText = new();

    internal CommandDefinition(IDbConnection connection)
    {
        if (connection is null)
            throw new ArgumentNullException(nameof(connection));

        _provider = connection.Provider();
        _command = connection.CreateCommand();
        _command.Connection ??= connection;
        _command.CommandType = CommandType.Text;
        _command.CommandTimeout = Convert.ToInt32(_provider.DefaultCommandTimeout.TotalSeconds).ClampMin(0);
    }

    public CommandDefinition WithTransaction(IDbTransaction transaction)
    {
        _command.Transaction = transaction;
        return this;
    }

    public CommandDefinition WithCommandTimeout(TimeSpan timeout)
    {
        _command.CommandTimeout = Convert.ToInt32(_provider.DefaultCommandTimeout.TotalSeconds).ClampMin(0);
        return this;
    }

    public CommandDefinition WithCommandType(CommandType commandType)
    {
        _command.CommandType = commandType;
        return this;
    }

    public CommandDefinition WithText(string text)
    {
        _commandText.Clear();
        _commandText.Append(text);
        return this;
    }

    public CommandDefinition AppendText(string text)
    {
        if (_commandText.Length > 0 && !char.IsWhiteSpace(_commandText[_commandText.Length - 1]))
            _commandText.Append(' ');

        _commandText.Append(text);
        return this;
    }

    public CommandDefinition Select(params string[] fields)
    {
        AppendText("SELECT");
        return fields != null && fields.Length > 0
            ? FieldNames(fields)
            : this;
    }

    public CommandDefinition FieldName(string item)
    {
        return !string.IsNullOrWhiteSpace(item)
        ? AppendText(_provider.QuoteField(item))
        : this;
    }

    public CommandDefinition FieldNames(params string[] items)
    {
        var quotedNames = items != null && items.Length > 0
            ? string.Join(", ", items.Select(f => _provider.QuoteField(f)))
            : "*";
        return AppendText($"{quotedNames}");
    }

    public CommandDefinition From(string? tableName = default, string? schemaName = default)
    {
        AppendText("FROM");
        return !string.IsNullOrEmpty(tableName)
            ? TableName(tableName, schemaName)
            : this;
    }

    public CommandDefinition TableName(string tableName, string? schemaName = default) =>
        AppendText(_provider.QuoteTable(tableName, schemaName));

    public CommandDefinition Where() => AppendText("WHERE");

    public CommandDefinition IsBetween() => AppendText("BETWEEN");

    public CommandDefinition Or() => AppendText("OR");

    public CommandDefinition And() => AppendText("AND");

    public CommandDefinition ParameterName(string item)
    {
        return !string.IsNullOrWhiteSpace(item)
            ? AppendText(_provider.QuoteParameter(item))
            : this;
    }

    public CommandDefinition ParameterNames(params string[] items)
    {
        var quotedNames = items != null && items.Length > 0
            ? string.Join(", ", items.Select(f => _provider.QuoteParameter(f)))
            : string.Empty;

        return AppendText($"{quotedNames}");
    }

    public CommandDefinition ParameterPairs(string pairSeparator, params string[] items)
    {
        var quotedNames = items != null && items.Length > 0
            ? string.Join(", ", items.Select(f => $"{_provider.QuoteField(f)} = {_provider.QuoteParameter(f)}"))
            : string.Empty;

        return AppendText($"{quotedNames}");
    }

    public CommandDefinition OrderBy(params string[] items)
    {
        if (items != null && items.Length > 0)
        {
            AppendText("ORDER BY");
            return FieldNames(items);
        }

        return this;

    }

    public CommandDefinition Limit(int skip = default, int take = int.MaxValue)
    {
        const int DefaultSkip = 0;
        const int DefaultTake = int.MaxValue;

        var builder = new StringBuilder(256);

        if (skip == DefaultSkip && take == DefaultTake)
            return this;

        switch (_provider.Kind)
        {
            case ProviderKind.SqlServer:
                builder.Append(CultureInfo.InvariantCulture, $"OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY");
                break;
            case ProviderKind.MySql:
            case ProviderKind.Sqlite:
                builder.Append(CultureInfo.InvariantCulture, $"LIMIT {take} OFFSET {skip}");
                break;
        }

        return AppendText(builder.ToString());
    }

    public IDbCommand FinishCommand()
    {
        _command.CommandText = _commandText.ToString();
        var result = _command;
        _command = null;
        _commandText.Clear();
        _commandText = null;
        return result;
    }

}
