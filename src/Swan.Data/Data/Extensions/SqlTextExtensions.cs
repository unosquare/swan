namespace Swan.Data.Extensions;

/// <summary>
/// Extension methods to build SQL command text using <see cref="CommandSource"/> objects.
/// </summary>
public static partial class SqlTextExtensions
{
    /// <summary>
    /// Appends the SELECT keyword to the command text.
    /// Optionally appends quoted fields names specified.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="fields">The optional set of unquoted field names.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource Select(this CommandSource @this, params string[]? fields)
    {
        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        @this.AppendText("SELECT");
        return fields is {Length: > 0}
            ? @this.Fields(fields)
            : @this;
    }

    /// <summary>
    /// Appends a SELECT statement with the table fields and defined columns
    /// to this command source.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="table">The table with columns.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource Select(this CommandSource @this, IDbTableSchema table) => table is not null
        ? @this.Select(table.Columns.Select(c => c.Name).ToArray()).From(table)
        : throw new ArgumentNullException(nameof(table));

    /// <summary>
    /// Appends a SELECT clause by taking in columns to this command source.
    /// It does not include the table name.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="columns">The columns.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource Select(this CommandSource @this, IEnumerable<IDbColumnSchema> columns) => columns is not null
        ? @this.Select(columns.Select(c => c.Name).ToArray())
        : throw new ArgumentNullException(nameof(columns));

    /// <summary>
    /// Appends a field name to the command text by first quoting it.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="item">The field name to append.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource Field(this CommandSource @this, string item) =>
        @this is null
            ? throw new ArgumentNullException(nameof(@this))
            : !string.IsNullOrWhiteSpace(item)
                ? @this.AppendText(@this.Provider.QuoteField(item))
                : @this;

    /// <summary>
    /// Appends a set of field names, quoting them and separating them by commas.
    /// If the list of fields names is null or empty, it appends a '*' character
    /// to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="items">The filed names to append.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource Fields(this CommandSource @this, params string[]? items)
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
    public static CommandSource From(this CommandSource @this, string? tableName = default, string? schemaName = default)
    {
        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        @this.AppendText("FROM");
        return !string.IsNullOrEmpty(tableName)
            ? @this.Table(tableName, schemaName)
            : @this;
    }

    /// <summary>
    /// Appends a FROM keyword to the command text, and appends
    /// a table identifier to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="table">The table.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource From(this CommandSource @this, IDbTableSchema table) => table is not null
        ? @this.From(table.TableName, table.Schema)
        : throw new ArgumentNullException(nameof(table));

    /// <summary>
    /// Appends an INSERT INTO clause to the command text, and optionally appends
    /// a table identifier to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="tableName">The name of the table</param>
    /// <param name="schemaName">The optional schema name.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource InsertInto(this CommandSource @this, string? tableName = default, string? schemaName = default)
    {
        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        @this.AppendText("INSERT INTO");
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
    public static CommandSource Table(this CommandSource @this, string tableName, string? schemaName = default) => @this is null
        ? throw new ArgumentNullException(nameof(@this))
        : @this.AppendText(@this.Provider.QuoteTable(tableName, schemaName));

    /// <summary>
    /// Appends the WHERE keyword to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="condition">An optional condition that follows the WHERE keyword.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource Where(this CommandSource @this, string? condition = default) => @this is null
        ? throw new ArgumentNullException(nameof(@this))
        : string.IsNullOrWhiteSpace(condition)
        ? @this.AppendText("WHERE")
        : @this.AppendText($"WHERE {condition}");

    /// <summary>
    /// Appends the BETWEEN keyword to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource IsBetween(this CommandSource @this) => @this is null
        ? throw new ArgumentNullException(nameof(@this))
        : @this.AppendText("BETWEEN");

    /// <summary>
    /// Appends the OR keyword to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource Or(this CommandSource @this) => @this is null
        ? throw new ArgumentNullException(nameof(@this))
        : @this.AppendText("OR");

    /// <summary>
    /// Appends the AND keyword to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource And(this CommandSource @this) => @this is null
        ? throw new ArgumentNullException(nameof(@this))
        : @this.AppendText("AND");

    /// <summary>
    /// Appends a parameter to the command text.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="item"></param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource Parameter(this CommandSource @this, string item) => @this is null
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
    public static CommandSource Parameters(this CommandSource @this, params string[]? items)
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
    public static CommandSource FieldsAndParameters(this CommandSource @this, string[] items, string itemSeparator = ",", string operatorSeparator = "=")
    {
        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        var quotedNames = items is {Length: > 0}
            ? string.Join($" {itemSeparator} ", items.Select(f => $"{@this.Provider.QuoteField(f)} {operatorSeparator} {@this.Provider.QuoteParameter(f)}"))
            : string.Empty;

        return @this.AppendText($"{quotedNames}");
    }

    /// <summary>
    /// Appends the ORDER BY keywords, and optionally, a set of provided field names.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="items">The field names to append to the order by clause.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource OrderBy(this CommandSource @this, params string[]? items)
    {
        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        @this.AppendText("ORDER BY");

        return items is {Length: > 0}
            ? @this.Fields(items)
            : @this;
    }

    /// <summary>
    /// Used in SELECT statements, appends a provider-compatible clause that
    /// skips a given number of records and then retrieves up to a number of records.
    /// </summary>
    /// <param name="this">The instance.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <returns>This instance for fluent API support.</returns>
    public static CommandSource Limit(this CommandSource @this, int skip = default, int take = int.MaxValue)
    {
        const int DefaultSkip = 0;
        const int DefaultTake = int.MaxValue;

        if (@this is null)
            throw new ArgumentNullException(nameof(@this));

        var builder = new StringBuilder(256);

        if (skip == DefaultSkip && take == DefaultTake)
            return @this;

        builder
            .Append(' ')
            .Append(@this.Provider.GetLimitClause(skip, take));
        
        return @this.AppendText(builder.ToString());
    }
}
