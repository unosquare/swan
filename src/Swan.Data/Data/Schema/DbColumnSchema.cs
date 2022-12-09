namespace Swan.Data.Schema;

internal sealed record DbColumnSchema : IDbColumnSchema
{
    public DbColumnSchema()
    {
        // placeholder
    }

    public string ColumnName { get; set; } = string.Empty;

    public Type DataType { get; set; } = typeof(string);

    public string DataTypeName { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public int ColumnOrdinal { get; set; }

    public int ColumnSize { get; set; }

    public int NumericPrecision { get; set; }

    public int NumericScale { get; set; }

    public bool IsLong { get; set; }

    public bool AllowDBNull { get; set; }

    public bool IsReadOnly { get; set; }

    public bool IsUnique { get; set; }

    public bool IsKey { get; set; }

    public bool IsAutoIncrement { get; set; }

    public string BaseCatalogName { get; set; } = string.Empty;

    public string BaseSchemaName { get; set; } = string.Empty;

    public string BaseTableName { get; set; } = string.Empty;

    public string BaseColumnName { get; set; } = string.Empty;

    object ICloneable.Clone() => this with { };
}
