namespace Swan.Data.Extensions;

public static partial class SqlTextExtensions
{
    /// <summary>
    /// Appends the SELECT keyword to the command text.
    /// Optionally appends quoted fields names specified.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="fields">The optional set of unquoted field names.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource Select(this DbCommandSource @this, params string[]? fields)
    {
        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        @this.AppendText("SELECT");
        return fields != null && fields.Length > 0
            ? @this.Fields(fields)
            : @this;
    }

    /// <summary>
    /// Appends a field name to the command text by first quoting it.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="item">The field name to append.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource Field(this DbCommandSource @this, string item)
    {
        return @this is null
            ? throw new ArgumentNullException(nameof(@this))
            : !string.IsNullOrWhiteSpace(item)
            ? @this.AppendText(@this.Provider.QuoteField(item))
            : @this;
    }

    /// <summary>
    /// Appends a set of field names, quoting them and separating them by commas.
    /// If the list of fields names is null or empty, it appends a '*' character
    /// to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="items">The filed names to append.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource Fields(this DbCommandSource @this, params string[]? items)
    {
        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        var quotedNames = items != null && items.Length > 0
            ? string.Join(", ", items.Select(f => @this.Provider.QuoteField(f)))
            : "*";
        return @this.AppendText($"{quotedNames}");
    }

    /// <summary>
    /// Appends a FROM keyword to the command text, and optionally appends
    /// a table identifier to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="tableName">The name of the table</param>
    /// <param name="schemaName">The optional schema name.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource From(this DbCommandSource @this, string? tableName = default, string? schemaName = default)
    {
        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        @this.AppendText("FROM");
        return !string.IsNullOrEmpty(tableName)
            ? @this.Table(tableName, schemaName)
            : @this;
    }

    /// <summary>
    /// Appends the specified table identifier.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="tableName"></param>
    /// <param name="schemaName"></param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource Table(this DbCommandSource @this, string tableName, string? schemaName = default) => @this is null
        ? throw new ArgumentNullException(nameof(@this))
        : @this.AppendText(@this.Provider.QuoteTable(tableName, schemaName));

    /// <summary>
    /// Appends the WHERE keyword to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource Where(this DbCommandSource @this) => @this is null
        ? throw new ArgumentNullException(nameof(@this))
        : @this.AppendText("WHERE");

    /// <summary>
    /// Appends the BETWEEN keyword to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource IsBetween(this DbCommandSource @this) => @this is null
        ? throw new ArgumentNullException(nameof(@this))
        : @this.AppendText("BETWEEN");

    /// <summary>
    /// Appends the OR keyword to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource Or(this DbCommandSource @this) => @this is null
        ? throw new ArgumentNullException(nameof(@this))
        : @this.AppendText("OR");

    /// <summary>
    /// Appends the AND keyword to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource And(this DbCommandSource @this) => @this is null
        ? throw new ArgumentNullException(nameof(@this))
        : @this.AppendText("AND");

    /// <summary>
    /// Appends a parameter to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="item"></param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource Parameter(this DbCommandSource @this, string item) => @this is null
        ? throw new ArgumentNullException(nameof(@this))
        : !string.IsNullOrWhiteSpace(item)
        ? @this.AppendText(@this.Provider.QuoteParameter(item))
        : @this;

    /// <summary>
    /// Appends a comma-separated list of parameter names.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="items">The unquoted list of parameters.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource Parameters(this DbCommandSource @this, params string[]? items)
    {
        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        var quotedNames = items != null && items.Length > 0
            ? string.Join(", ", items.Select(f => @this.Provider.QuoteParameter(f)))
            : string.Empty;

        return @this.AppendText($"{quotedNames}");
    }


    /// <summary>
    /// Appends a set of fields names and matching parameter names.
    /// Useful when building UPDATE clauses and WHERE clauses.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="items">The unquoted list of parameters.</param>
    /// <param name="itemSeparator">The optional string that separates the field and parameter. Typically just a comma.</param>
    /// <param name="operatorSeparator">The optional string that separates the field name and parameter name. Typically just a '=' sign.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource FieldParameters(this DbCommandSource @this, string[] items, string itemSeparator = ", ", string operatorSeparator = " = ")
    {
        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        var quotedNames = items != null && items.Length > 0
            ? string.Join(itemSeparator, items.Select(f => $"{@this.Provider.QuoteField(f)}{operatorSeparator}{@this.Provider.QuoteParameter(f)}"))
            : string.Empty;

        return @this.AppendText($"{quotedNames}");
    }

    /// <summary>
    /// Appends the ORDER BY keywords, and optionally, a set of provided field names.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="items">The field names to append to the order by clause.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource OrderBy(this DbCommandSource @this, params string[]? items)
    {
        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        @this.AppendText("ORDER BY");

        if (items != null && items.Length > 0)
            return @this.Fields(items);

        return @this;
    }

    /// <summary>
    /// Used in SELECT statements, appends a provider-compatible clause that
    /// skips a given number of records and then retrieves up to a number of records.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static DbCommandSource Limit(this DbCommandSource @this, int skip = default, int take = int.MaxValue)
    {
        const int DefaultSkip = 0;
        const int DefaultTake = int.MaxValue;

        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        var builder = new StringBuilder(256);

        if (skip == DefaultSkip && take == DefaultTake)
            return @this;

        switch (@this.Provider.Kind)
        {
            case ProviderKind.SqlServer:
                builder.Append(CultureInfo.InvariantCulture, $"OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY");
                break;
            case ProviderKind.MySql:
            case ProviderKind.Sqlite:
                builder.Append(CultureInfo.InvariantCulture, $"LIMIT {take} OFFSET {skip}");
                break;
        }

        return @this.AppendText(builder.ToString());
    }
}
