namespace Swan.Data.Extensions;

/// <summary>
/// Provides extension methods that generate code.
/// </summary>
public static class CodeGenExtensions
{
    private static readonly Dictionary<Type, string> CSharpTypeNames = new()
    {
        [typeof(bool)] = "bool",
        [typeof(byte)] = "byte",
        [typeof(sbyte)] = "sbyte",
        [typeof(char)] = "char",
        [typeof(decimal)] = "decimal",
        [typeof(double)] = "double",
        [typeof(float)] = "float",
        [typeof(int)] = "int",
        [typeof(uint)] = "uint",
        [typeof(nint)] = "nint",
        [typeof(nuint)] = "nuint",
        [typeof(long)] = "long",
        [typeof(ulong)] = "ulong",
        [typeof(short)] = "short",
        [typeof(ushort)] = "ushort",
        [typeof(string)] = "string",
    };
    private static readonly CultureInfo ci = CultureInfo.InvariantCulture;

    /// <summary>
    /// Provides a very simple record class code generator (C#) that matches
    /// a table schema definition. Useful if you want to get started quickly
    /// without having to write a ton of POCO classes.
    /// </summary>
    /// <param name="table">The table to extract column schema info from.</param>
    /// <param name="entityName">The optional name of the generated record class.</param>
    /// <returns>A string containing code that defines a record class.</returns>
    public static string GeneratePocoCode(this IDbTableSchema table, string? entityName = default)
    {
        if (table is null)
            throw new ArgumentNullException(nameof(table));

        if (table.ColumnCount == 0)
            throw new InvalidOperationException("Unable to define properties for empty column set.");

        var hasSchemaName = !string.IsNullOrWhiteSpace(table.Schema);
        var fullTableName = !hasSchemaName ? table.TableName : $"{table.Schema}.{table.TableName}";

        entityName ??= table.TableName;
        return new StringBuilder()
            .Append("/// <summary>\r\n")
            .Append(ci, $"/// Represents a record that maps to the {fullTableName} table.\r\n")
            .Append("/// </summary>\r\n")
            .Append(ci,
                $"[Table(\"{table.TableName}\"{(hasSchemaName ? $", Schema = \"{table.Schema}\"" : string.Empty)})]\r\n")
            .Append(ci, $"public record {entityName}\r\n")
            .Append("{\r\n")
            .Append("    /// <summary>\r\n")
            .Append(ci, $"    /// Creates a new instance of the <see cref=\"{entityName}\" /> class.\r\n")
            .Append("    /// </summary>\r\n")
            .Append(ci, $"    public {entityName}() ").Append("{ /* placeholder */ }\r\n\r\n")
            .Append(string.Join("\r\n\r\n", table.Columns.Select(c => c.ToPropertyCode())))
            .Append("\r\n}\r\n")
            .ToString();
    }

    /// <summary>
    /// Converts columns schema information to a compatible, annotated propery code.
    /// </summary>
    /// <param name="column">The column schema to convert.</param>
    /// <returns>The string represntation of the compatible property.</returns>
    private static string ToPropertyCode(this IDbColumnSchema column)
    {
        var builder = new StringBuilder(1024);
        var typeAlias = column.DataType.GetTypeAlias();

        builder
            .Append("    /// <summary>\r\n")
            .Append(ci, $"    /// Gets or sets a value for {column.Name.Humanize()}.\r\n")
            .Append("    /// </summary>\r\n");

        if (column.IsKey)
            builder.Append(ci, $"    [Key]\r\n");

        if (column.MaxLength > 0 && column.Precision <= 0 && column.Scale <= 0 && typeAlias != "bool")
            builder.Append(ci, $"    [MaxLength({column.MaxLength})]\r\n");

        if (column.IsAutoIncrement && column.IsKey)
            builder.Append(ci, $"    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]\r\n");

        builder
            .Append(ci,
                $"    [Column(nameof({column.Name}){(column.Ordinal >= 0 ? $", Order = {column.Ordinal}" : string.Empty)})]\r\n")
            .Append(ci, $"    public {typeAlias}{(column.AllowsDBNull ? "? " : " ")}{column.Name}")
            .Append("{ get; set; }");
        return builder.ToString();
    }

    /// <summary>
    /// Gets a short-hand type alias. For example, for <see cref="Int32"/>, returns int.
    /// </summary>
    /// <param name="type">The type alias.</param>
    /// <returns>The string representation of the type alias.</returns>
    private static string GetTypeAlias(this Type type)
    {
        var baseType = Nullable.GetUnderlyingType(type) ?? type;
        return CSharpTypeNames.TryGetValue(baseType, out var alias)
            ? alias
            : baseType.Name;
    }
}
